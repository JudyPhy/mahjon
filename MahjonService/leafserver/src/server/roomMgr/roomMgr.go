package roomMgr

import (
	"bytes"
	"math/rand"
	"server/msgHandler"
	"server/pb"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type ProcessStatus int32

const (
	ProcessStatus_DEFAULT       ProcessStatus = 1
	ProcessStatus_EXCHANGE_OVER ProcessStatus = 2
)

func (x ProcessStatus) Enum() *ProcessStatus {
	p := new(ProcessStatus)
	*p = x
	return p
}

type RoomPlayerInfo struct {
	isRobot    bool
	agent      gate.Agent
	side       pb.BattleSide
	isOwner    bool
	playerInfo *PlayerInfo
}

type CardList struct {
	playerId int32
	list     []*Card
	process  ProcessStatus
}

type RoomInfo struct {
	roomId     string
	playerList []*RoomPlayerInfo
	cardWall   []*Card
	cardList   []*CardList
}

// ---------------------
// | roomId | RoomInfo |
// ---------------------
type mgrRoom struct {
	lock    sync.Mutex
	roomMap map[string]*RoomInfo
}

var RoomManager *mgrRoom

func sendAddedRobotMember(roomInfo *RoomInfo) {
	log.Debug("sendAddedRobotMember, roomId=%v", roomInfo.roomId)
	var players []*pb.BattlePlayerInfo
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if value.isRobot {
			battlePlayer := playerInfoToPbBattlePlayerInfo(value)
			players = append(players, battlePlayer)
		}
	}
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
}

func waitingRoomOk(roomInfo *RoomInfo) {
	log.Debug("waitingRoomOk")
	timer := time.NewTimer(time.Second)
	over := false
	go func() {
		<-timer.C
		if over {
			return
		}
		if len(roomInfo.playerList) < 4 {
			log.Debug("need add robot")
			memberCount := len(roomInfo.playerList)
			for i := 0; i < 4-memberCount; i++ {
				addRobotToRoom(roomInfo, i)
			}
			sendAddedRobotMember(roomInfo)
		}
		if len(roomInfo.playerList) == 4 {
			startBattle(roomInfo)
			over = true
		}
	}()
}

func getCardListByPlayerId(playerId int32, roomInfo *RoomInfo) *CardList {
	for n, value := range roomInfo.cardList {
		if n == 0 {
		}
		if value.playerId == playerId {
			return value
		}
	}
	return nil
}

func getLeftSideList(roomInfo *RoomInfo) []pb.BattleSide {
	origList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	result := []pb.BattleSide{}
	for n, value := range origList {
		if n == 0 {
		}
		curSide := value
		isFind := false
		for i, player := range roomInfo.playerList {
			if i == 0 {
			}
			//log.Debug("curSide=", curSide.String(), "player.side=", player.side.String())
			if player.side.String() == curSide.String() {
				isFind = true
				break
			}
		}
		if !isFind {
			result = append(result, curSide)
		}
	}
	log.Debug("current side list count is %v", len(result))
	return result
}

func getRandomSideBySideList(sideList []pb.BattleSide) pb.BattleSide {
	log.Debug("getRandomSideBySideList, left side list count=%v", len(sideList))
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(sideList))
	return sideList[rnd]
}

func getRandomRoomId(length int) string {
	log.Debug("getRandomRoomId")
	rand.Seed(time.Now().UnixNano())
	rs := make([]string, length)
	for start := 0; start < length; start++ {
		rs = append(rs, strconv.Itoa(rand.Intn(10)))
	}
	return strings.Join(rs, "") //使用""拼接rs切片
}

func playerInfoToPbPlayerInfo(info *PlayerInfo) *pb.PlayerInfo {
	player := &pb.PlayerInfo{}
	player.Oid = proto.Int32(info.oid)
	player.NickName = proto.String(info.nickName)
	player.HeadIcon = proto.String(info.headIcon)
	player.Gold = proto.Int32(info.gold)
	player.Diamond = proto.Int32(info.diamond)
	return player
}

// RoomPlayerInfo -> pb.BattlePlayerInfo
func playerInfoToPbBattlePlayerInfo(info *RoomPlayerInfo) *pb.BattlePlayerInfo {
	player := &pb.BattlePlayerInfo{}
	player.Side = info.side.Enum()
	player.IsOwner = proto.Bool(info.isOwner)
	player.Player = playerInfoToPbPlayerInfo(info.playerInfo)
	return player
}

func reqNewRoom(a gate.Agent) *RoomInfo {
	log.Debug("ReqNewRoom")
	newRoomId := getRandomRoomId(6)
	for {
		RoomManager.lock.Lock()
		_, ok := RoomManager.roomMap[newRoomId]
		if ok {
			newRoomId = getRandomRoomId(6)
		} else {
			RoomManager.roomMap[newRoomId] = &RoomInfo{}
			break
		}
	}
	roomInfo := RoomManager.roomMap[newRoomId]
	roomInfo.roomId = newRoomId
	RoomManager.lock.Unlock()
	return roomInfo
}

//添加真实玩家到房间中
func addPlayerToRoom(roomInfo *RoomInfo, a gate.Agent, isOwner bool) bool {
	log.Debug("add player to room=%v", roomInfo.roomId)

	//roomPlayer
	basePlayer := getPlayerBtAgent(a)
	if basePlayer == nil {
		log.Error("player has not logined, can't add.")
		return false
	}
	basePlayer.roomId = roomInfo.roomId
	sideList := getLeftSideList(roomInfo)
	side := getRandomSideBySideList(sideList)
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = false
	roomPlayer.agent = a
	roomPlayer.side = side
	roomPlayer.isOwner = isOwner
	roomPlayer.playerInfo = basePlayer
	roomInfo.playerList = append(roomInfo.playerList, roomPlayer)

	// send update room playr event
	log.Debug("send add room player info to client")
	battlePlayer := playerInfoToPbBattlePlayerInfo(roomPlayer)
	var players []*pb.BattlePlayerInfo
	players = append(players, battlePlayer)
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
	return true
}

func reqDealer(roomInfo *RoomInfo) int32 {
	count := len(roomInfo.playerList)
	rand.Seed(time.Now().UnixNano())
	index := rand.Intn(count)
	dealerId := roomInfo.playerList[index].playerInfo.oid
	log.Debug("roomId=%v, dealerId=%v", roomInfo.roomId, dealerId)
	return dealerId
}

func startBattle(roomInfo *RoomInfo) {
	log.Debug("startBattle, roomId=%v", roomInfo.roomId)
	dealerId := reqDealer(roomInfo)
	// deal cards
	var allPlayerCards []*pb.CardInfo
	roomInfo.cardWall = loadAllCards()
	roomInfo.cardList = make([]*CardList, 4)
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		cardList := &CardList{}
		cardList.playerId = value.playerInfo.oid
		for i := 0; i < 13; i++ {
			//log.Debug("current wall len=%v", len(roomInfo.cardWall))
			rand.Seed(time.Now().Unix())
			rnd := rand.Intn(len(roomInfo.cardWall))
			roomInfo.cardWall[rnd].status = CardStatus_INHAND
			cardList.list = append(cardList.list, roomInfo.cardWall[rnd])
			roomInfo.cardWall = append(roomInfo.cardWall[:rnd], roomInfo.cardWall[rnd+1:]...)
		}
		//log.Debug("card list count=%v", len(cardList.list))
		if value.playerInfo.oid == dealerId {
			rand.Seed(time.Now().Unix())
			rnd := rand.Intn(len(roomInfo.cardWall))
			roomInfo.cardWall[rnd].status = CardStatus_INHAND
			cardList.list = append(cardList.list, roomInfo.cardWall[rnd])
			roomInfo.cardWall = append(roomInfo.cardWall[:rnd], roomInfo.cardWall[rnd+1:]...)
		}
		roomInfo.cardList[n] = cardList

		cardList.process = ProcessStatus_DEFAULT
		log.Debug("side=%v, deal card count=%v", value.side, len(cardList.list))
		for n := 0; n < len(cardList.list); n++ {
			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(value.playerInfo.oid)
			card.CardOid = proto.Int32(cardList.list[n].oid)
			card.CardId = proto.Int32(cardList.list[n].id)
			card.Status = pb.CardStatus_inHand.Enum()
			allPlayerCards = append(allPlayerCards, card)
		}
	}

	//log
	allCardLog(roomInfo)

	//prepare send
	log.Debug("battle start, dealed cards sum count=%v", len(allPlayerCards))
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CBattleStart(dealerId, allPlayerCards, value.agent)
		} else if value.isRobot {
			selectRobotExchangeCard(roomInfo, value.playerInfo.oid)
		}
	}
}

func checkExchangeCardOver(roomInfo *RoomInfo) bool {
	log.Debug("checkExchangeCardOver")
	for n, value := range roomInfo.cardList {
		if n == 0 {
		}
		if value.process != ProcessStatus_EXCHANGE_OVER {
			return false
		}
	}
	return true
}

//获取随机交换类型
func getExchangeType() pb.ExchangeType {
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(3)
	if rnd == 0 {
		return pb.ExchangeType_ClockWise
	} else if rnd == 1 {
		return pb.ExchangeType_AntiClock
	} else {
		return pb.ExchangeType_Opposite
	}
}

//按照东南西北排序玩家
func getPlayerIdListSortBySide(RoomInfo *RoomInfo) []int32 {
	var result []int32
	sideList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	for i, side := range sideList {
		if i == 0 {
		}
		for j, player := range RoomInfo.playerList {
			if j == 0 {
			}
			if player.side == side {
				result = append(result, player.playerInfo.oid)
				break
			}
		}
	}
	return result
}

func allCardLog(roomInfo *RoomInfo) {
	var allOid []int
	var wallOid []int
	for n, value := range roomInfo.cardWall {
		if n == 0 {
		}
		allOid = append(allOid, int(value.oid))
		wallOid = append(wallOid, int(value.oid))
	}
	sort.Ints(wallOid)
	logStr := "wall card oid: "
	buf := bytes.NewBufferString(logStr)
	for i := 0; i < len(wallOid); i++ {
		str := strconv.Itoa(wallOid[i])
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	for n, value := range roomInfo.cardList {
		if n == 0 {
		}
		var playerCardOid []int
		for i := 0; i < len(value.list); i++ {
			allOid = append(allOid, int(value.list[i].oid))
			playerCardOid = append(playerCardOid, int(value.list[i].oid))
		}
		sort.Ints(playerCardOid)
		logStr := "player["
		str1 := strconv.Itoa(int(value.playerId))
		str2 := "] card oid: "
		buf := bytes.NewBufferString(logStr)
		buf.Write([]byte(str1))
		buf.Write([]byte(str2))
		for i := 0; i < len(playerCardOid); i++ {
			str := strconv.Itoa(playerCardOid[i])
			buf.Write([]byte(str))
			buf.Write([]byte(", "))
		}
		log.Debug(buf.String())
	}

	log.Debug("all card count=%v", len(allOid))
	sort.Ints(allOid)
	logStr = "all card oid: "
	buf = bytes.NewBufferString(logStr)
	for i := 0; i < len(allOid); i++ {
		str := strconv.Itoa(allOid[i])
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())
}

//交换牌
func processExchangeCard(roomInfo *RoomInfo) {
	log.Debug("processExchangeCard")
	exchangeAllMap := make(map[int32][]*Card)
	for i, value := range roomInfo.cardList {
		if i == 0 {
		}
		var list []*Card
		for j := 0; j < len(value.list); j++ {
			if value.list[j].status == CardStatus_EXCHANGE {
				value.list[j].status = CardStatus_INHAND //取出交换牌，更新为手牌状态
				list = append(list, value.list[j])
				value.list = append(value.list[:j], value.list[j+1:]...)
				j--
			}
		}
		log.Debug("player[%v] has %v exchange cards, left card list count=%v", value.playerId, len(list), len(value.list))
		exchangeAllMap[value.playerId] = list
	}
	exchangeType := getExchangeType()
	log.Debug("exchangeType=%v", exchangeType)
	playerIdListSortBySide := getPlayerIdListSortBySide(roomInfo)
	for i, value := range roomInfo.cardList {
		if i == 0 {
		}
		curPlayerId := value.playerId
		index := 0
		for j := 0; j < len(playerIdListSortBySide); j++ {
			if playerIdListSortBySide[j] == curPlayerId {
				index = j
				break
			}
		}
		switch exchangeType {
		case pb.ExchangeType_ClockWise:
			log.Debug("ClockWise:")
			index++
			if index > 3 {
				index = 0
			}
		case pb.ExchangeType_AntiClock:
			log.Debug("AntiClock:")
			index--
			if index < 0 {
				index = 3
			}
		case pb.ExchangeType_Opposite:
			log.Debug("Opposite:")
			for n := 0; n < 2; n++ {
				index++
				if index > 3 {
					index = 0
				}
			}
		}
		fromPlayerId := playerIdListSortBySide[index]
		log.Debug("player[%v] exchange with player[%v]", curPlayerId, fromPlayerId)
		value.list = append(value.list[:], exchangeAllMap[fromPlayerId][:]...)
		log.Debug("after exchange, card count=%v", len(value.list))
	}

	//log
	allCardLog(roomInfo)

	//send exchanged card to client
	var allExchangedCardList []*pb.CardInfo
	for n, value := range roomInfo.cardList {
		if n == 0 {
		}
		for i := 0; i < len(value.list); i++ {
			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(value.playerId)
			card.CardOid = proto.Int32(value.list[i].oid)
			card.CardId = proto.Int32(value.list[i].id)
			card.Status = pb.CardStatus_inHand.Enum()
			allExchangedCardList = append(allExchangedCardList, card)
		}
	}
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateCardInfoAfterExchange(exchangeType.Enum(), allExchangedCardList, value.agent)
		}
	}
}

//------------------------------------------------------------------------------
//								   public func
//------------------------------------------------------------------------------
func Init() {
	log.Debug("init player map.")
	ChanPlayerStruct = &ChanPlayer{}
	ChanPlayerStruct.aPlayerMap = make(map[gate.Agent]*PlayerInfo)

	log.Debug("init room map.")
	RoomManager = &mgrRoom{}
	RoomManager.roomMap = make(map[string]*RoomInfo)
}

func CreateRoomRet(a gate.Agent) {
	log.Debug("CreateRoomRet")
	mode := pb.GameMode_CreateRoom.Enum()
	roomInfo := reqNewRoom(a)
	result := addPlayerToRoom(roomInfo, a, true)
	errorCode := pb.GS2CEnterGameRet_SUCCESS.Enum()
	if !result {
		log.Error("add player to room fail.")
		errorCode = pb.GS2CEnterGameRet_FAIL.Enum()
	}
	msgHandler.SendGS2CEnterGameRet(errorCode, mode, roomInfo.roomId, a)

	//test: add other 3 robot to room
	waitingRoomOk(roomInfo)
}

func OutRoom(roomId string, a gate.Agent) {
	log.Debug("out room=%v", roomId)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		log.Debug("before=> player count in room[%v], current player count[%v]", roomId, len(roomInfo.playerList))
		for n, value := range roomInfo.playerList {
			if value.agent == a {
				roomInfo.playerList = append(roomInfo.playerList[:n], roomInfo.playerList[n+1:]...)
				log.Debug("after=> player count in room[%v], current player count[%v]", roomId, len(roomInfo.playerList))
				/*
					//send remove player event to client
					log.Debug("send remove room player info to client")
					battlePlayer := &pb.BattlePlayerInfo{}
					playerInfo := getPlayerBtAgent(a)
					battlePlayer.Player = &pb.PlayerInfo{}
					battlePlayer.Player.Oid = proto.Int32(playerInfo.oid)
					data := &pb.GS2CUpdateRoomInfo{}
					data.Player = append(data.Player, battlePlayer)
					data.Status = pb.GS2CUpdateRoomInfo_REMOVE.Enum()
					for n, value := range RoomManager.roomMap[roomId].playerList {
						if n == 0 {
						}
						value.agent.WriteMsg(data)
					}
					break
				*/
			}
		}
	} else {
		log.Error("room %v not exist.", roomId)
	}
}

func UpdateExchangeCard(m *pb.C2GSExchangeCard, a gate.Agent) {
	log.Debug("UpdateExchangeCard")
	exchangeCount := len(m.CardList)
	if exchangeCount != 3 {
		log.Error("exchange card count[%v] is error", exchangeCount)
		msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_FAIL_CARD_COUNT_ERROR.Enum(), a)
		return
	}
	player := getPlayerBtAgent(a)
	if player != nil {
		log.Debug("exchange player nickName=%v, roomId=%v", player.nickName, player.roomId)
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			cardList := getCardListByPlayerId(player.oid, roomInfo)
			if cardList != nil {
				count := 0
				for i, info := range m.CardList {
					log.Debug("player[%v] exchange card[%v]", player.nickName, info.GetCardOid())
					if i == 0 {
					}
					for n, value := range cardList.list {
						if n == 0 {
						}
						if value.oid == info.GetCardOid() {
							value.status = CardStatus_EXCHANGE
							count++
							break
						}
					}
				}
				if count != 3 {
					log.Error("The exchanging 3 cards is not all in card list, just has [%v] card in list.", count)
					msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_FAIL.Enum(), a)
				} else {
					log.Debug("The exchanging card has update in list.")
					cardList.process = ProcessStatus_EXCHANGE_OVER
					msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_SUCCESS.Enum(), a)
					if checkExchangeCardOver(roomInfo) {
						processExchangeCard(roomInfo)
					}
				}
			} else {
				log.Error("no cardList for player[%v]", player.oid)
			}
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}
