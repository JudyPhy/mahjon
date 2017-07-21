package roomMgr

import (
	"math/rand"
	"server/msgHandler"
	"server/pb"
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
	side       *pb.BattleSide
	isOwner    bool
	playerInfo *PlayerInfo
}

type CardList struct {
	playerId int32
	list     []*Card
	process  *ProcessStatus
}

type RoomInfo struct {
	roomId         string
	playerList     []*RoomPlayerInfo
	cardList_east  *CardList
	cardList_south *CardList
	cardList_west  *CardList
	cardList_north *CardList
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
	log.Debug("sendAddedRobotMember, roomId=%d", roomInfo.roomId)
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
	timer := time.NewTimer(time.Second * 5)
	go func() {
		<-timer.C
		if len(roomInfo.playerList) == 4 {
			log.Debug("need add robot")
			memberCount := len(roomInfo.playerList)
			for i := 0; i < 4-memberCount; i++ {
				addRobotToRoom(roomInfo, i)
			}
			sendAddedRobotMember(roomInfo)
		}
		if len(roomInfo.playerList) == 4 {
			startBattle(roomInfo)
			timer.Stop()
		}
	}()
}

func getCardListBySide(side *pb.BattleSide, roomInfo *RoomInfo) *CardList {
	if side == pb.BattleSide_east.Enum() {
		return roomInfo.cardList_east
	} else if side == pb.BattleSide_south.Enum() {
		return roomInfo.cardList_south
	} else if side == pb.BattleSide_west.Enum() {
		return roomInfo.cardList_west
	} else if side == pb.BattleSide_north.Enum() {
		return roomInfo.cardList_north
	}
	return nil
}

func getCardListByPlayerId(playerId int32, roomInfo *RoomInfo) *CardList {
	if roomInfo.cardList_east.playerId == playerId {
		return roomInfo.cardList_east
	} else if roomInfo.cardList_south.playerId == playerId {
		return roomInfo.cardList_south
	} else if roomInfo.cardList_west.playerId == playerId {
		return roomInfo.cardList_west
	} else if roomInfo.cardList_north.playerId == playerId {
		return roomInfo.cardList_north
	}
	return nil
}

func getLeftSideList(roomInfo *RoomInfo) []*pb.BattleSide {
	origList := []*pb.BattleSide{pb.BattleSide_east.Enum(), pb.BattleSide_south.Enum(), pb.BattleSide_west.Enum(), pb.BattleSide_north.Enum()}
	result := []*pb.BattleSide{}
	for n, value := range origList {
		log.Debug("n=", n)
		curSide := value
		isFind := false
		for i, player := range roomInfo.playerList {
			log.Debug("i=", i)
			if player.side == curSide {
				isFind = true
				break
			}
		}
		if !isFind {
			result = append(result, curSide)
		}
	}
	log.Debug("current side list count is ", len(result))
	return result
}

func getRandomSideBySideList(sideList []*pb.BattleSide) *pb.BattleSide {
	log.Debug("getRandomSideBySideList, left side list count=", len(sideList))
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
	player.Side = info.side
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
	log.Debug("add player to room=%d", roomInfo.roomId)

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
		log.Debug("n=", n)
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
	log.Debug("roomId=%d, dealerId=%d", roomInfo.roomId, dealerId)
	return dealerId
}

func startBattle(roomInfo *RoomInfo) {
	log.Debug("startBattle, roomId=%d", roomInfo.roomId)
	dealerId := reqDealer(roomInfo)
	// deal cards
	var allPlayerCards []*pb.CardInfo
	loadAllCards()
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		cardList := getCardListBySide(value.side, roomInfo)
		cardList = &CardList{}
		cardList.playerId = value.playerInfo.oid
		cardList.list = getCardListByBattleStart()
		cardList.process = ProcessStatus_DEFAULT.Enum()
		log.Debug("side=", value.side, ", deal card count=", len(cardList.list))
		for n := 0; n < len(cardList.list); n++ {
			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(value.playerInfo.oid)
			card.CardOid = proto.Int32(cardList.list[n].oid)
			card.CardId = proto.Int32(cardList.list[n].id)
			card.Status = pb.CardStatus_inHand.Enum()
			allPlayerCards = append(allPlayerCards, card)
		}
	}

	//prepare send
	log.Debug("battle start, dealed cards sum count=", len(allPlayerCards))
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CBattleStart(dealerId, allPlayerCards, value.agent)
		}
	}
}

func checkExchangeCardOver(roomInfo *RoomInfo) bool {
	log.Debug("checkExchangeCardOver")
	if roomInfo.cardList_east.process != ProcessStatus_EXCHANGE_OVER.Enum() {
		return false
	}
	if roomInfo.cardList_south.process != ProcessStatus_EXCHANGE_OVER.Enum() {
		return false
	}
	if roomInfo.cardList_west.process != ProcessStatus_EXCHANGE_OVER.Enum() {
		return false
	}
	if roomInfo.cardList_north.process != ProcessStatus_EXCHANGE_OVER.Enum() {
		return false
	}
	return true
}

func processExchangeCard(roomInfo *RoomInfo) {
	log.Debug("processExchangeCard")

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
	log.Debug("out room=", roomId)
	RoomManager.lock.Lock()
	if _, ok := RoomManager.roomMap[roomId]; ok {
		chanlist := RoomManager.roomMap[roomId].playerList
		log.Debug("before=>player count in room", roomId, " :", len(RoomManager.roomMap[roomId].playerList))
		for n, value := range chanlist {
			if value.agent == a {
				chanlist = append(chanlist[:n], chanlist[n+1:]...)
				log.Debug("after offline=>player count in room", roomId, " :", len(RoomManager.roomMap[roomId].playerList))

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
					log.Debug("n=", n)
					value.agent.WriteMsg(data)
				}
				break
			}
		}
	} else {
		log.Error("room ", roomId, " not exist.")
	}
	RoomManager.lock.Unlock()
}

func UpdateExchangeCard(m *pb.C2GSExchangeCard, a gate.Agent) {
	log.Debug("UpdateExchangeCard")
	exchangeCount := len(m.CardOid)
	if exchangeCount != 3 {
		log.Error("exchange card count[%d] is error", exchangeCount)
		msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_FAIL_CARD_COUNT_ERROR.Enum(), a)
		return
	}
	player := getPlayerBtAgent(a)
	if player != nil {
		log.Debug("exchange player nickName=%d, roomId=%d", player.nickName, player.roomId)
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			cardList := getCardListByPlayerId(player.oid, roomInfo)
			if cardList != nil {
				count := 0
				for i, oid := range m.CardOid {
					log.Debug("player[%s] exchange card[%d]", player.nickName, oid)
					if i == 0 {
					}
					for n, value := range cardList.list {
						if n == 0 {
						}
						if value.oid == oid {
							value.status = CardStatus_EXCHANGE.Enum()
							count++
							break
						}
					}
				}
				if count != 3 {
					log.Error("The exchanging 3 cards is not all in card list, just has [%d] card in list.", count)
					msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_FAIL.Enum(), a)
				} else {
					log.Debug("The exchanging card has update in list.")
					cardList.process = ProcessStatus_EXCHANGE_OVER.Enum()
					msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_SUCCESS.Enum(), a)
					if checkExchangeCardOver(roomInfo) {
						processExchangeCard(roomInfo)
					}
				}
			} else {
				log.Error("no cardList for player[%d]", player.oid)
			}
		} else {
			log.Error("no room[%s]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}
