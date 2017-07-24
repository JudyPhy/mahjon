package roomMgr

import (
	"bytes"
	"math/rand"
	"server/msgHandler"
	"server/pb"
	"sort"
	"strconv"
	"sync"
	"time"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type SideInfo struct {
	//player
	isRobot    bool
	agent      gate.Agent
	side       pb.BattleSide
	isOwner    bool
	playerInfo *PlayerInfo
	//card
	cardList []*Card
	process  ProcessStatus
}

type PlayerCardMap struct {
	lock sync.Mutex
	cMap map[int32]*SideInfo //playerOID : SideInfo
}

type RoomInfo struct {
	roomId   string
	cardWall []*Card
	cardMap  *PlayerCardMap
}

func (roomInfo *RoomInfo) Init(roomId string) {
	roomInfo.roomId = roomId
	roomInfo.cardMap.cMap = make(map[int32]*SideInfo)
}

func (roomInfo *RoomInfo) getSide() pb.BattleSide {
	leftSideList := roomInfo.getLeftSideList()
	if len(leftSideList) > 0 {
		rand.Seed(time.Now().Unix())
		rnd := rand.Intn(len(leftSideList))
		return leftSideList[rnd]
	} else {
		return pb.BattleSide_none
	}
}

func (roomInfo *RoomInfo) getLeftSideList() []pb.BattleSide {
	origList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	result := []pb.BattleSide{}
	for n, value := range origList {
		if n == 0 {
		}
		isFind := false
		roomInfo.cardMap.lock.Lock()
		for i, sideInfo := range roomInfo.cardMap.cMap {
			if i == 0 {
			}
			//log.Debug("curSide=%v, player.side=%v", value, sideInfo.side)
			if sideInfo.side == value {
				isFind = true
				break
			}
		}
		roomInfo.cardMap.lock.Unlock()
		if !isFind {
			result = append(result, value)
		}
	}
	log.Debug("current side list count is %v", len(result))
	return result
}

func sideInfoToPbBattlePlayerInfo(sideInfo *SideInfo) *pb.BattlePlayerInfo {
	result := &pb.BattlePlayerInfo{}
	result.Side = sideInfo.side.Enum()
	result.IsOwner = proto.Bool(sideInfo.isOwner)
	result.Player = &pb.PlayerInfo{}
	result.Player.Oid = proto.Int32(sideInfo.playerInfo.oid)
	result.Player.NickName = proto.String(sideInfo.playerInfo.nickName)
	result.Player.HeadIcon = proto.String(sideInfo.playerInfo.headIcon)
	result.Player.Gold = proto.Int32(sideInfo.playerInfo.gold)
	result.Player.Diamond = proto.Int32(sideInfo.playerInfo.diamond)
	return result
}

//添加真实玩家到房间中
func (roomInfo *RoomInfo) addPlayerToRoom(a gate.Agent, isOwner bool) bool {
	log.Debug("add player to room=%v", roomInfo.roomId)

	//roomPlayer
	basePlayer := getPlayerBtAgent(a)
	if basePlayer == nil {
		log.Error("player has not logined, can't add.")
		return false
	}
	basePlayer.roomId = roomInfo.roomId

	sideInfo := &SideInfo{}
	sideInfo.isRobot = false
	sideInfo.agent = a
	sideInfo.isOwner = isOwner
	sideInfo.side = roomInfo.getSide()
	sideInfo.playerInfo = basePlayer
	sideInfo.cardList = make([]*Card, 0)
	sideInfo.process = ProcessStatus_DEFAULT

	roomInfo.cardMap.lock.Lock()
	roomInfo.cardMap.cMap[basePlayer.oid] = sideInfo
	roomInfo.cardMap.lock.Unlock()

	// send update room playr event
	log.Debug("send add room player info to client")
	battlePlayer := sideInfoToPbBattlePlayerInfo(sideInfo)
	var players []*pb.BattlePlayerInfo
	players = append(players, battlePlayer)
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
	roomInfo.cardMap.lock.Unlock()
	return true
}

func (roomInfo *RoomInfo) waitingRoomOk() {
	log.Debug("waitingRoomOk")
	timer := time.NewTimer(time.Second)
	over := false
	go func() {
		<-timer.C
		if over {
			return
		}
		if len(roomInfo.cardMap.cMap) < 4 {
			log.Debug("need add robot")
			memberCount := len(roomInfo.cardMap.cMap)
			for i := 0; i < 4-memberCount; i++ {
				roomInfo.addRobotToRoom(int32(i + 20000))
			}
			roomInfo.sendAddedRobotMember()
		}
		if len(roomInfo.cardMap.cMap) == 4 {
			roomInfo.startBattle()
			over = true
		}
	}()
}

func (roomInfo *RoomInfo) addRobotToRoom(oid int32) {
	log.Debug("addRobotToRoom roomId=%v", roomInfo.roomId)
	sideInfo := &SideInfo{}
	sideInfo.isRobot = true
	sideInfo.agent = nil
	sideInfo.side = roomInfo.getSide()
	sideInfo.isOwner = false
	sideInfo.playerInfo = &PlayerInfo{}
	sideInfo.playerInfo.oid = oid
	sideInfo.playerInfo.nickName = "游客"
	sideInfo.playerInfo.headIcon = "nil"
	sideInfo.playerInfo.gold = 0
	sideInfo.playerInfo.diamond = 0
	sideInfo.playerInfo.roomId = roomInfo.roomId

	roomInfo.cardMap.lock.Lock()
	roomInfo.cardMap.cMap[oid] = sideInfo
	roomInfo.cardMap.lock.Unlock()
}

func (roomInfo *RoomInfo) sendAddedRobotMember() {
	log.Debug("sendAddedRobotMember, roomId=%v", roomInfo.roomId)
	var players []*pb.BattlePlayerInfo
	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if value.isRobot {
			battlePlayer := sideInfoToPbBattlePlayerInfo(value)
			players = append(players, battlePlayer)
		}
	}
	roomInfo.cardMap.lock.Unlock()
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
}

func (roomInfo *RoomInfo) reqDealer() int32 {
	var playerOidList []int32
	roomInfo.cardMap.lock.Lock()
	for i, value := range roomInfo.cardMap.cMap {
		if i == 0 {
		}
		playerOidList = append(playerOidList, value.playerInfo.oid)
	}
	roomInfo.cardMap.lock.Unlock()

	count := len(playerOidList)
	rand.Seed(time.Now().UnixNano())
	index := rand.Intn(count)
	dealerId := playerOidList[index]
	log.Debug("roomId=%v, dealerId=%v", roomInfo.roomId, dealerId)
	return dealerId
}

func (roomInfo *RoomInfo) startBattle() {
	log.Debug("startBattle, roomId=%v", roomInfo.roomId)
	dealerId := roomInfo.reqDealer()
	var allPlayerCards []*pb.CardInfo
	roomInfo.cardWall = loadAllCards()

	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		for i := 0; i < 13; i++ {
			//log.Debug("current wall len=%v", len(roomInfo.cardWall))
			rand.Seed(time.Now().Unix())
			rnd := rand.Intn(len(roomInfo.cardWall))
			roomInfo.cardWall[rnd].status = CardStatus_INHAND
			value.cardList = append(value.cardList, roomInfo.cardWall[rnd])
			roomInfo.cardWall = append(roomInfo.cardWall[:rnd], roomInfo.cardWall[rnd+1:]...)
		}
		log.Debug("side=%v, card list count=%v, card wall count=%v", value.side, len(value.cardList), len(roomInfo.cardWall))
		if value.playerInfo.oid == dealerId {
			rand.Seed(time.Now().Unix())
			rnd := rand.Intn(len(roomInfo.cardWall))
			roomInfo.cardWall[rnd].status = CardStatus_INHAND
			value.cardList = append(value.cardList, roomInfo.cardWall[rnd])
			roomInfo.cardWall = append(roomInfo.cardWall[:rnd], roomInfo.cardWall[rnd+1:]...)
		}

		for n := 0; n < len(value.cardList); n++ {
			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(value.playerInfo.oid)
			card.CardOid = proto.Int32(value.cardList[n].oid)
			card.CardId = proto.Int32(value.cardList[n].id)
			card.Status = pb.CardStatus_inHand.Enum()
			allPlayerCards = append(allPlayerCards, card)
		}
	}

	//prepare send
	log.Debug("battle start, dealed cards sum count=%v", len(allPlayerCards))
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CBattleStart(dealerId, allPlayerCards, value.agent)
		} else if value.isRobot {
			value.cardList = roomInfo.selectRobotExchangeCard(value.cardList)
			value.process = ProcessStatus_EXCHANGE_OVER
		}
	}
	roomInfo.cardMap.lock.Unlock()

	//log
	roomInfo.allCardLog()
}

func (roomInfo *RoomInfo) updateExchangeCards(cardList []*pb.CardInfo, playerOid int32) bool {
	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if value.playerInfo.oid == playerOid {
			for i, clientCard := range cardList {
				if i == 0 {
				}
				isFind := false
				for j, serviceCard := range value.cardList {
					if j == 0 {
					}
					if clientCard.GetCardOid() == serviceCard.oid {
						serviceCard.status = CardStatus_EXCHANGE
						isFind = true
						break
					}
				}
				if !isFind {
					log.Error("playerOid[%v]'s exchanged card is not in cardList.", playerOid)
					return false
				}
			}
			value.process = ProcessStatus_EXCHANGE_OVER
			break
		}
	}
	roomInfo.cardMap.lock.Unlock()
	return true
}

func (roomInfo *RoomInfo) checkExchangeCardOver() bool {
	log.Debug("checkExchangeCardOver")
	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if value.process != ProcessStatus_EXCHANGE_OVER {
			return false
		}
	}
	roomInfo.cardMap.lock.Unlock()
	return true
}

func (roomInfo *RoomInfo) allCardLog() {
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

	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		var playerCardOid []int
		for i := 0; i < len(value.cardList); i++ {
			allOid = append(allOid, int(value.cardList[i].oid))
			playerCardOid = append(playerCardOid, int(value.cardList[i].oid))
		}
		sort.Ints(playerCardOid)
		logStr := "player["
		str1 := strconv.Itoa(int(value.playerInfo.oid))
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
	roomInfo.cardMap.lock.Unlock()

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

//按照东南西北排序玩家OID
func (roomInfo *RoomInfo) getPlayerIdListSortBySide() []int32 {
	var result []int32
	sideList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	for i, side := range sideList {
		if i == 0 {
		}
		roomInfo.cardMap.lock.Lock()
		for j, value := range roomInfo.cardMap.cMap {
			if j == 0 {
			}
			if value.side == side {
				result = append(result, value.playerInfo.oid)
				break
			}
		}
		roomInfo.cardMap.lock.Unlock()
	}
	return result
}

func (roomInfo *RoomInfo) sendCardInfoAfterExchange(exchangeType pb.ExchangeType) {
	//send exchanged card to client
	var allExchangedCardList []*pb.CardInfo
	roomInfo.cardMap.lock.Lock()
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		for i, origCard := range value.cardList {
			if i == 0 {
			}
			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(value.playerInfo.oid)
			card.CardOid = proto.Int32(origCard.oid)
			card.CardId = proto.Int32(origCard.id)
			card.Status = pb.CardStatus_inHand.Enum()
			allExchangedCardList = append(allExchangedCardList, card)
		}
	}
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateCardInfoAfterExchange(exchangeType.Enum(), allExchangedCardList, value.agent)
		}
	}
	roomInfo.cardMap.lock.Unlock()
}

//交换牌
func (roomInfo *RoomInfo) processExchangeCard() {
	log.Debug("processExchangeCard")
	exchangeAllMap := make(map[int32][]*Card)
	roomInfo.cardMap.lock.Lock()
	for i, value := range roomInfo.cardMap.cMap {
		if i == 0 {
		}
		var list []*Card
		for j := 0; j < len(value.cardList); j++ {
			if value.cardList[j].status == CardStatus_EXCHANGE {
				value.cardList[j].status = CardStatus_INHAND //取出交换牌，更新为手牌状态
				list = append(list, value.cardList[j])
				value.cardList = append(value.cardList[:j], value.cardList[j+1:]...)
				j--
			}
		}
		log.Debug("player[%v] has %v exchange cards, left card list count=%v", value.playerInfo.oid, len(list), len(value.cardList))
		exchangeAllMap[value.playerInfo.oid] = list
	}
	roomInfo.cardMap.lock.Unlock()

	exchangeType := getExchangeType()
	log.Debug("exchangeType=%v", exchangeType)
	playerIdListSortBySide := roomInfo.getPlayerIdListSortBySide()
	roomInfo.cardMap.lock.Lock()
	for i, value := range roomInfo.cardMap.cMap {
		if i == 0 {
		}
		index := 0
		for j := 0; j < len(playerIdListSortBySide); j++ {
			if playerIdListSortBySide[j] == value.playerInfo.oid {
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
		log.Debug("player[%v] exchange with player[%v]", value.playerInfo.oid, fromPlayerId)
		value.cardList = append(value.cardList[:], exchangeAllMap[fromPlayerId][:]...)
		log.Debug("after exchange, card count=%v", len(value.cardList))
	}
	roomInfo.cardMap.lock.Unlock()

	roomInfo.sendCardInfoAfterExchange(exchangeType)

	//log
	roomInfo.allCardLog()

}

func (roomInfo *RoomInfo) outRoom(playerOid int32) {
	log.Debug("playerOid[%v] out room")
	isFind := false
	var playerList []*pb.BattlePlayerInfo
	roomInfo.cardMap.lock.Lock()
	for i, value := range roomInfo.cardMap.cMap {
		if value.playerInfo.oid == playerOid {
			delete(roomInfo.cardMap.cMap, i)
			isFind = true

			battlePlayer := sideInfoToPbBattlePlayerInfo(value)
			playerList = append(playerList, battlePlayer)
			break
		}
	}
	for i, value := range roomInfo.cardMap.cMap {
		if i == 0 {
		}
		if !value.isRobot && value.agent != nil {
			status := pb.GS2CUpdateRoomInfo_REMOVE.Enum()
			msgHandler.SendGS2CUpdateRoomInfo(playerList, status, value.agent)
		}
	}
	roomInfo.cardMap.lock.Unlock()
	if !isFind {
		log.Error("playerOid[%v] is not in room[%v], can't kick out.", playerOid, roomInfo.roomId)
	}
}
