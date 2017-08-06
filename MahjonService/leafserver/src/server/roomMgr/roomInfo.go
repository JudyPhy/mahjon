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

type PlayerCardMap struct {
	lock sync.Mutex
	cMap map[int32]*SideInfo //playerOID : SideInfo
}

type RoomInfo struct {
	roomId   string
	dealerId int32
	cardWall []*Card
	cardMap  *PlayerCardMap
}

var curTurnPlayerOid int32

func (roomInfo *RoomInfo) Init(roomId string) {
	log.Debug("Init roomInfo...")
	roomInfo.roomId = roomId
	roomInfo.cardMap = &PlayerCardMap{}
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

func intToString() {

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
	buf := bytes.NewBufferString("游客")
	str := strconv.Itoa(int(oid))
	buf.Write([]byte(str))
	sideInfo.playerInfo.nickName = buf.String()
	sideInfo.playerInfo.headIcon = "nil"
	sideInfo.playerInfo.gold = 0
	sideInfo.playerInfo.diamond = 0
	sideInfo.playerInfo.roomId = roomInfo.roomId

	roomInfo.cardMap.cMap[oid] = sideInfo
}

func (roomInfo *RoomInfo) sendAddedRobotMember() {
	log.Debug("sendAddedRobotMember, roomId=%v", roomInfo.roomId)
	var players []*pb.BattlePlayerInfo
	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		if value.isRobot {
			battlePlayer := sideInfoToPbBattlePlayerInfo(value)
			players = append(players, battlePlayer)
		}
	}
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
	for i, value := range roomInfo.cardMap.cMap {
		if i == 0 {
		}
		playerOidList = append(playerOidList, value.playerInfo.oid)
	}

	count := len(playerOidList)
	rand.Seed(time.Now().UnixNano())
	index := rand.Intn(count)
	dealerId := playerOidList[index]
	log.Debug("roomId=%v, dealerId=%v", roomInfo.roomId, dealerId)
	return dealerId
}

func (roomInfo *RoomInfo) startBattle() {
	log.Debug("startBattle, roomId=%v", roomInfo.roomId)
	roomInfo.dealerId = 10000 //roomInfo.reqDealer()
	curTurnPlayerOid = roomInfo.dealerId
	var allPlayerCards []*pb.CardInfo
	roomInfo.cardWall = loadAllCards()

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
		if value.playerInfo.oid == roomInfo.dealerId {
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
			msgHandler.SendGS2CBattleStart(roomInfo.dealerId, allPlayerCards, value.agent)
		} else if value.isRobot {
			value.cardList = roomInfo.selectRobotExchangeCard(value.cardList)
			value.process = ProcessStatus_EXCHANGE_OVER
		}
	}

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

	for n, value := range roomInfo.cardMap.cMap {
		if n == 0 {
		}
		var playerCardOid []int
		var playerCardId []int
		for i := 0; i < len(value.cardList); i++ {
			allOid = append(allOid, int(value.cardList[i].oid))
			playerCardOid = append(playerCardOid, int(value.cardList[i].oid))
			if value.cardList[i].status == CardStatus_INHAND || value.cardList[i].status == CardStatus_PENG || value.cardList[i].status == CardStatus_GANG {
				playerCardId = append(playerCardId, int(value.cardList[i].id))
			}
		}
		sort.Ints(playerCardOid)
		sort.Ints(playerCardId)
		/*
			//oid
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
		*/
		//id
		logStr = "player["
		str1 := strconv.Itoa(int(value.playerInfo.oid))
		str2 := "] has card id: "
		buf := bytes.NewBufferString(logStr)
		buf.Write([]byte(str1))
		buf.Write([]byte(str2))
		for i := 0; i < len(playerCardId); i++ {
			str := strconv.Itoa(playerCardId[i])
			buf.Write([]byte(str))
			buf.Write([]byte(", "))
		}
		str3 := strconv.Itoa(len(playerCardId))
		buf.Write([]byte(str3))
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
		for j, value := range roomInfo.cardMap.cMap {
			if j == 0 {
			}
			if value.side == side {
				result = append(result, value.playerInfo.oid)
				break
			}
		}
	}
	return result
}

//牌交换完毕发送新的手牌信息到客户端
func (roomInfo *RoomInfo) sendCardInfoAfterExchange(exchangeType pb.ExchangeType) {
	//send exchanged card to client
	var allExchangedCardList []*pb.CardInfo
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
		} else if value.isRobot {
			value.selectLack()
		}
	}
}

//交换牌
//取出交换牌存于map中，根据交换类型将map中的交换牌重新添加到对应的玩家手牌中
func (roomInfo *RoomInfo) processExchangeCard() {
	log.Debug("processExchangeCard")
	exchangeAllMap := make(map[int32][]*Card)
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

	exchangeType := getExchangeType()
	log.Debug("exchangeType=%v", exchangeType)
	playerIdListSortBySide := roomInfo.getPlayerIdListSortBySide()
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

	roomInfo.sendCardInfoAfterExchange(exchangeType)

	//log
	roomInfo.allCardLog()

}

func (roomInfo *RoomInfo) outRoom(playerOid int32) {
	log.Debug("playerOid[%v] out room", playerOid)
	isFind := false
	var playerList []*pb.BattlePlayerInfo
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
	if !isFind {
		log.Error("playerOid[%v] is not in room[%v], can't kick out.", playerOid, roomInfo.roomId)
	}
}

//---------------------------------------- lack card ------------------------------------------
func (roomInfo *RoomInfo) updateLack(playerOid int32, lackType *pb.CardType) {
	sideInfo, ok := roomInfo.cardMap.cMap[playerOid]
	if ok {
		sideInfo.lackType = lackType
		sideInfo.process = ProcessStatus_LACK_OVER
	} else {
		log.Error("playerOid[%v] not in map.")
	}
}

func (roomInfo *RoomInfo) selectLackOver() bool {
	for i, value := range roomInfo.cardMap.cMap {
		if i == 0 {
		}
		if value.process != ProcessStatus_LACK_OVER {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) sendLackCard() {
	var result []*pb.LackCard
	for _, value := range roomInfo.cardMap.cMap {
		lack := &pb.LackCard{}
		lack.PlayerId = proto.Int32(value.playerInfo.oid)
		lack.Type = value.lackType
		result = append(result, lack)
	}
	log.Debug("lack over, turn switch")
	for _, value := range roomInfo.cardMap.cMap {
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CSelectLackRet(result, value.agent)
		}
	}
}

func sendUpdateCardInfoBySelfGang(roonmId string, procPlayerOid int32, list []*pb.CardInfo) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roonmId]
	RoomManager.lock.Unlock()
	if ok {
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {

			}
		}
	} else {
		log.Debug("sendUpdateCardInfo, room[%v] not exist.", roonmId)
	}
}

func (roomInfo *RoomInfo) dealerStart() {
	log.Debug("Dealer start ==========>>>>>>>>>")
	curTurnPlayerOid = roomInfo.dealerId
	for _, value := range roomInfo.cardMap.cMap {
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CTurnToNext(curTurnPlayerOid, nil, pb.TurnSwitchType_NotDrawCard.Enum(), value.agent)
		}
	}
	for _, value := range roomInfo.cardMap.cMap {
		if roomInfo.dealerId == value.playerInfo.oid {
			if value.isRobot {
				value.robotTurnSwitch()
			} else {
				value.playerTurnSwitch()
			}
			break
		}
	}
}

func sendCanDiscardProc(roomId string) {
	log.Debug("sendCanDiscardProc")
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CPlayerEnsureProc(curTurnPlayerOid, pb.ProcType_Discard.Enum(), 0, 0, sideInfo.agent)
			}
		}
	} else {
		log.Debug("sendCanDiscardProc, room[%v] not exist.", roomId)
	}
}

//---------------------------------------- discard ------------------------------------------
func (roomInfo *RoomInfo) recvDiscard(playerId int32, Oid int32) {
	log.Debug("recvDiscard, playerId%v, cardOid%v", playerId, Oid)
	sideInfo, ok := roomInfo.cardMap.cMap[playerId]
	if ok {
		discard := sideInfo.playerUpdateDiscardInfo(Oid)
		roomInfo.broadcastDiscard(discard)
		timer := time.NewTimer(time.Second * 1)
		<-timer.C
		//延迟1秒
		roomInfo.checkTurnOver()
	} else {
		log.Error("no player[%v]", playerId)
	}
}

func (roomInfo *RoomInfo) broadcastDiscard(card *Card) {
	log.Debug("broad discard info to everyone，discard=%v(%v)", card.oid, card.id)
	if card != nil {
		for _, sideInfo := range roomInfo.cardMap.cMap {
			log.Debug("sideInfo=%v", sideInfo.playerInfo.oid)
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CDiscardRet(card.oid, sideInfo.agent)
			}
			sideInfo.playerProcDiscard(card)
		}
	}
}

//---------------------------------------- turn over ----------------------------------------
func (roomInfo *RoomInfo) isNormalTurnOver() bool {
	for _, v := range roomInfo.cardMap.cMap {
		if v.process != ProcessStatus_TURN_OVER {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isPengTurnOver() bool {
	for _, v := range roomInfo.cardMap.cMap {
		if v.process != ProcessStatus_TURN_OVER && v.process != ProcessStatus_TURN_OVER_PENG {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isHuTurnOver() bool {
	for _, v := range roomInfo.cardMap.cMap {
		if v.process != ProcessStatus_TURN_OVER && v.process != ProcessStatus_TURN_OVER_HU {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isGangTurnOver() bool {
	for _, v := range roomInfo.cardMap.cMap {
		if v.process != ProcessStatus_TURN_OVER && v.process != ProcessStatus_TURN_OVER_GANG {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isEveryoneProcDiscardOver() bool {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		log.Debug("player[%v] process=%v", sideInfo.playerInfo.oid, sideInfo.process)
		if sideInfo.process == ProcessStatus_TURN_OVER_HU || sideInfo.process == ProcessStatus_GAME_OVER {
			continue
		}
		if sideInfo.process != ProcessStatus_TURN_OVER && sideInfo.process != ProcessStatus_WAITING_GANG &&
			sideInfo.process != ProcessStatus_WAITING_HU && sideInfo.process != ProcessStatus_WAITING_PENG {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) getTurnOverType() TurnOverType {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_PENG {
			return TurnOverType_PENG
		}
	}
	return TurnOverType_NORMAL
}

func (roomInfo *RoomInfo) getPengTurnOverSideInfo() *SideInfo {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_PENG {
			return sideInfo
		}
	}
	return nil
}

func (roomInfo *RoomInfo) getGangTurnOverSideInfo() *SideInfo {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_GANG {
			return sideInfo
		}
	}
	return nil
}

func (roomInfo *RoomInfo) getHuTurnOverSideInfo() []*SideInfo {
	list := make([]*SideInfo, 0)
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_HU {
			list = append(list, sideInfo)
		}
	}
	return list
}

func (roomInfo *RoomInfo) checkTurnOver() {
	log.Debug("checkTurnOver")
	preDiscard := roomInfo.getPreDiscard()
	if roomInfo.isNormalTurnOver() {
		log.Debug("all is normal over.(no one need p、g、h proc)")
		if preDiscard != nil {
			preDiscard.status = CardStatus_DISCARD
		} else {
			log.Error("normal over error! preDiscard is nil.")
		}
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if sideInfo.process != ProcessStatus_GAME_OVER && sideInfo.process != ProcessStatus_TURN_OVER_HU {
				sideInfo.process = ProcessStatus_TURN_START
			}
		}
		roomInfo.turnToNextPlayer()
	} else if roomInfo.isHuTurnOver() {
		log.Debug("someone hu, and it proc hu over.")
		sideInfoList := roomInfo.getHuTurnOverSideInfo()
		for _, sideInfo := range sideInfoList {
			sideInfo.process = ProcessStatus_GAME_OVER
		}
		if len(sideInfoList) > 0 {
			roomInfo.sendHuTurnToNext(sideInfoList)
		} else {
			log.Error("hu turn over, but no one's process is ProcessStatus_TURN_OVER_HU!")
		}
	} else if roomInfo.isGangTurnOver() {
		log.Debug("only one can gang, and it proc gang over.")
		sideInfo := roomInfo.getGangTurnOverSideInfo()
		if sideInfo != nil {
			roomInfo.sendNormalTurnToNext(sideInfo.side)
		} else {
			log.Error("gang turn over, but no one's process is ProcessStatus_TURN_OVER_GANG!")
		}
	} else if roomInfo.isPengTurnOver() {
		log.Debug("only one can peng, and it proc peng over.")
		sideInfo := roomInfo.getPengTurnOverSideInfo()
		if sideInfo != nil {
			roomInfo.sendPengTurnToNext(sideInfo.side)
		} else {
			log.Error("peng turn over, but no one's process is ProcessStatus_TURN_OVER_PENG!")
		}
	} else {
		if !roomInfo.isEveryoneProcDiscardOver() {
			log.Debug("some player is processing discard, waiting...")
		} else {
			log.Debug("everyone has process discard, and someone need p、g、h.")
			var playerHuOid []int32
			for _, sideInfo := range roomInfo.cardMap.cMap {
				if sideInfo.process == ProcessStatus_WAITING_HU {
					playerHuOid = append(playerHuOid, sideInfo.playerInfo.oid)
				}
			}
			if len(playerHuOid) > 0 {
				log.Debug("player proc hu.")
				roomInfo.procHu(preDiscard, playerHuOid)
				roomInfo.allCardLog()
			} else {
				log.Debug("player proc p、g.")
				roomInfo.procPG(preDiscard)
				roomInfo.allCardLog()
			}
		}
	}
}

func (roomInfo *RoomInfo) procPG(preDiscard *Card) {
	log.Debug("proc robot or player p、g.")
	var proSideInfo *SideInfo
	var beProcSideInfo *SideInfo
	var procType pb.ProcType
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_WAITING_GANG {
			proSideInfo = sideInfo
			procType = pb.ProcType_GangOther
		} else if sideInfo.process == ProcessStatus_WAITING_PENG {
			proSideInfo = sideInfo
			procType = pb.ProcType_Peng
		}
		if curTurnPlayerOid == sideInfo.playerInfo.oid {
			beProcSideInfo = sideInfo
		}
	}
	if proSideInfo.isRobot {
		log.Debug("robot proc p、g")
		if procType == pb.ProcType_Peng {
			proSideInfo.addDiscardAsPeng(preDiscard)
		} else if procType == pb.ProcType_GangOther {
			proSideInfo.addDiscardAsGang(preDiscard)
		}
		beProcSideInfo.deleteDiscard(preDiscard)
		roomInfo.sendRobotProc(proSideInfo.playerInfo.oid, beProcSideInfo.playerInfo.oid, procType)
	} else {
		log.Debug("real player proc p、g")
		roomInfo.sendRealPlayerProc(proSideInfo.playerInfo.oid, beProcSideInfo.playerInfo.oid, procType, preDiscard.id)
	}
}

func (roomInfo *RoomInfo) procHu(preDiscard *Card, playerHuOid []int32) {
	hasRobotHu := false
	for _, playerOid := range playerHuOid {
		sideInfo, ok := roomInfo.cardMap.cMap[playerOid]
		if ok {
			if sideInfo.isRobot {
				sideInfo.addDiscardAsHu(preDiscard)
				hasRobotHu = true
				roomInfo.sendRobotProc(sideInfo.playerInfo.oid, curTurnPlayerOid, pb.ProcType_HuOther)
			} else {
				sideInfo.process = ProcessStatus_PROC_HU
				roomInfo.sendRealPlayerProc(sideInfo.playerInfo.oid, curTurnPlayerOid, pb.ProcType_HuOther, preDiscard.id)
			}
		} else {
			log.Error("player%v not in room%v", playerOid, roomInfo.roomId)
		}
	}
	if hasRobotHu {
		log.Debug("has robot hu, delete discard from current sideinfo.")
		curTurnSideInfo := roomInfo.cardMap.cMap[curTurnPlayerOid]
		curTurnSideInfo.deleteDiscard(preDiscard)
	} else {
		log.Debug("only real player hu, waiting proc ret.")
	}
}

//---------------------------------------- robot proc ----------------------------------------
func (roomInfo *RoomInfo) sendRobotProc(procPlayer int32, beProcPlayer int32, procType pb.ProcType) {
	log.Debug("sendRobotProc, procPlayer=%v, beProcPlayer=%v， procType=%v", procPlayer, beProcPlayer, procType)
	var cardList []*pb.CardInfo
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid == procPlayer || sideInfo.playerInfo.oid == beProcPlayer {
			for _, card := range sideInfo.cardList {
				pbCard := &pb.CardInfo{}
				pbCard.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
				pbCard.CardOid = proto.Int32(card.oid)
				pbCard.CardId = proto.Int32(card.id)
				pbCard.Status = cardStatusToPbCardStatus(card.status).Enum()
				pbCard.FromOther = proto.Bool(card.fromOther)
				cardList = append(cardList, pbCard)
			}
		}
	}
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CRobotProc(procPlayer, procType.Enum(), beProcPlayer, cardList, sideInfo.agent)
		}
	}
}

//---------------------------------------- real player proc ----------------------------------------
func (roomInfo *RoomInfo) sendRealPlayerProc(procPlayer int32, beProcPlayer int32, procType pb.ProcType, procCardId int32) {
	log.Debug("sendRealPlayerProc, procPlayer=%v, beProcPlayer=%v， procType=%v, procCardId=%v", procPlayer, beProcPlayer, procType, procCardId)
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CPlayerEnsureProc(procPlayer, procType.Enum(), beProcPlayer, procCardId, sideInfo.agent)
		}
	}
}

func (roomInfo *RoomInfo) procSelfGang() {
	log.Debug("procSelfGang(include real player and robot)")
	sideInfo, ok := roomInfo.cardMap.cMap[curTurnPlayerOid]
	if ok {
		if sideInfo.isRobot {
			sideInfo.robotSelfGang()
		} else {
			roomInfo.sendRealPlayerProc(curTurnPlayerOid, 0, pb.ProcType_SelfGang, 0)
		}
	} else {
		log.Error("current turn player%v is not in room%v", curTurnPlayerOid, roomInfo.roomId)
	}
}

func (roomInfo *RoomInfo) procSelfHu() {
	log.Debug("procSelfHu(include real player and robot)")
	sideInfo, ok := roomInfo.cardMap.cMap[curTurnPlayerOid]
	if ok {
		if sideInfo.isRobot {
			sendRobotSelfHuProc(sideInfo.playerInfo.roomId)
		} else {
			roomInfo.sendRealPlayerProc(curTurnPlayerOid, 0, pb.ProcType_SelfHu, 0)
		}
	} else {
		log.Error("current turn player%v is not in room%v", curTurnPlayerOid, roomInfo.roomId)
	}
}

//---------------------------------------- turn to next ----------------------------------------
func (roomInfo *RoomInfo) getSideByPlayerOid(playerOid int32) pb.BattleSide {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid == playerOid {
			return sideInfo.side
		}
	}
	return pb.BattleSide_none
}

func (roomInfo *RoomInfo) nextSideCanProc(nextSide pb.BattleSide) bool {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		log.Debug("player%v process=%v", sideInfo.playerInfo.oid, sideInfo.process)
		if sideInfo.side == nextSide && sideInfo.process != ProcessStatus_GAME_OVER && sideInfo.process != ProcessStatus_TURN_OVER_HU {
			return true
		}
	}
	return false
}

func (roomInfo *RoomInfo) turnToNextPlayer() {
	log.Debug("turnToNextPlayer")
	curSide := roomInfo.getSideByPlayerOid(curTurnPlayerOid)
	if curSide != pb.BattleSide_none {
		count := 0
		for {
			nextSide := pb.BattleSide_none
			if curSide == pb.BattleSide_east {
				nextSide = pb.BattleSide_south
			} else if curSide == pb.BattleSide_south {
				nextSide = pb.BattleSide_west
			} else if curSide == pb.BattleSide_west {
				nextSide = pb.BattleSide_north
			} else if curSide == pb.BattleSide_north {
				nextSide = pb.BattleSide_east
			}
			if roomInfo.nextSideCanProc(nextSide) {
				roomInfo.sendNormalTurnToNext(nextSide)
				break
			}
			count++
			if count >= 4 {
				break
			}
		}
		if count >= 4 {
			log.Debug("游戏结束")
		}
	} else {
		log.Error("当前操作方错误")
	}
}

func (roomInfo *RoomInfo) getPlayerOidBySide(side pb.BattleSide) int32 {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.side == side {
			roomInfo.cardMap.lock.Unlock()
			return sideInfo.playerInfo.oid
		}
	}
	return 0
}

func (roomInfo *RoomInfo) sendNormalTurnToNext(nextSide pb.BattleSide) {
	log.Debug("sendNormalTurnToNext[%v]", nextSide)
	if len(roomInfo.cardWall) <= 0 {
		log.Debug("牌已经摸完，游戏结束.")
		roomInfo.sendGameOver()
		return
	}
	newCard := &pb.CardInfo{}
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.side == nextSide {
			curTurnPlayerOid = sideInfo.playerInfo.oid
			rand.Seed(time.Now().Unix())
			rnd := rand.Intn(len(roomInfo.cardWall))
			roomInfo.cardWall[rnd].status = CardStatus_INHAND

			newCard.CardOid = proto.Int32(roomInfo.cardWall[rnd].oid)
			newCard.CardId = proto.Int32(roomInfo.cardWall[rnd].id)
			newCard.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
			newCard.FromOther = proto.Bool(false)
			newCard.Status = pb.CardStatus_inHand.Enum()

			sideInfo.drawNewCard(roomInfo.cardWall[rnd])
			roomInfo.cardWall = append(roomInfo.cardWall[:rnd], roomInfo.cardWall[rnd+1:]...)
			break
		}
	}
	//send real player first, because robot will sleep 2 seconds
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CTurnToNext(curTurnPlayerOid, newCard, pb.TurnSwitchType_Normal.Enum(), sideInfo.agent)
		}
	}

	roomInfo.allCardLog()

	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid == curTurnPlayerOid {
			if sideInfo.isRobot {
				log.Debug("cur turn is robot.")
				sideInfo.robotTurnSwitch()
				break
			} else {
				log.Debug("cur turn is player.")
				sideInfo.playerTurnSwitch()
				break
			}
		}
	}
}

func (roomInfo *RoomInfo) sendPengTurnToNext(nextSide pb.BattleSide) {
	log.Debug("sendPengTurnToNext, nextSide=[%v]", nextSide)
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if nextSide == sideInfo.side {
			curTurnPlayerOid = sideInfo.playerInfo.oid
		}
	}
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CTurnToNext(curTurnPlayerOid, nil, pb.TurnSwitchType_JustCanDiscard.Enum(), sideInfo.agent)
		}
	}
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if curTurnPlayerOid == sideInfo.playerInfo.oid && sideInfo.isRobot {
			sideInfo.robotTurnSwitchAfterPeng()
		}
	}
}

func (roomInfo *RoomInfo) sendHuTurnToNext(curHuList []*SideInfo) {
	log.Debug("sendHuTurnToNext, hu count=[%v]", len(curHuList))
	huPlayerCount := len(curHuList)
	if huPlayerCount == 1 {
		if curHuList[0].side != pb.BattleSide_none {
			count := 0
			for {
				nextSide := pb.BattleSide_none
				if curHuList[0].side == pb.BattleSide_east {
					nextSide = pb.BattleSide_south
				} else if curHuList[0].side == pb.BattleSide_south {
					nextSide = pb.BattleSide_west
				} else if curHuList[0].side == pb.BattleSide_west {
					nextSide = pb.BattleSide_north
				} else if curHuList[0].side == pb.BattleSide_north {
					nextSide = pb.BattleSide_east
				}
				if roomInfo.nextSideCanProc(nextSide) {
					roomInfo.sendNormalTurnToNext(nextSide)
					break
				}
				count++
				if count >= 4 {
					break
				}
			}
			if count >= 4 {
				log.Debug("Game over!")
			}
		} else {
			log.Error("current turn side is none!")
		}
	} else if huPlayerCount == 2 {
		isFindNext := false
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if sideInfo.playerInfo.oid != curTurnPlayerOid &&
				sideInfo.playerInfo.oid != curHuList[0].playerInfo.oid && sideInfo.playerInfo.oid != curHuList[1].playerInfo.oid {
				nextSide := sideInfo.side
				if roomInfo.nextSideCanProc(nextSide) {
					roomInfo.sendNormalTurnToNext(nextSide)
					isFindNext = true
					break
				}
			}
		}
		if !isFindNext {
			log.Debug("Game over!")
		}
	}
}

func (roomInfo *RoomInfo) getCurTurnSideInfo() *SideInfo {
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid == curTurnPlayerOid {
			return sideInfo
		}
	}
	return nil
}

func (roomInfo *RoomInfo) robotProcOver(robotOid int32, procType pb.ProcType) {
	log.Debug("robotProcOver, robotOid=%v, procType=%v", robotOid, procType)
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid == robotOid {
			sideInfo.robotProcOver(procType)
			break
		}
	}
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_PROC_HU {
			return
		}
	}
	roomInfo.checkTurnOver()
}

func (roomInfo *RoomInfo) getHuStatusCard() *Card {
	log.Debug("getHuStatusCard")
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_HU {
			for _, curCard := range sideInfo.cardList {
				if curCard.status == CardStatus_HU {
					return curCard
				}
			}
		}
	}
	return nil
}

func (roomInfo *RoomInfo) setOtherProcessBySelfProc(exceptPlayerOid int32, process ProcessStatus) {
	log.Debug("setOtherProcessBySelfProc")
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid != exceptPlayerOid {
			sideInfo.process = process
		}
	}
}

func (roomInfo *RoomInfo) sendGameOver() {
	log.Debug("sendGameOver")
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CGameOver(sideInfo.agent)
		}
	}
}
