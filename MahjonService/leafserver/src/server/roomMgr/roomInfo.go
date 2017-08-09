package roomMgr

import (
	"bytes"
	"math/rand"
	"server/card"
	"server/msgHandler"
	"server/pb"
	"server/player"
	"sort"
	"strconv"
	"sync"
	"time"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type SideInfoMap struct {
	lock sync.Mutex
	cMap map[int32]*SideInfo //playerOID : SideInfo
}

type RoomInfo struct {
	roomId      string
	dealerId    int32
	cardWall    []*card.Card
	sideInfoMap *SideInfoMap
}

var curTurnPlayerOid int32

func (roomInfo *RoomInfo) Init(roomId string) {
	log.Debug("Init roomInfo...")
	roomInfo.roomId = roomId
	roomInfo.sideInfoMap = &SideInfoMap{}
	roomInfo.sideInfoMap.cMap = make(map[int32]*SideInfo)
}

//------------------------------------------ add player into room ------------------------------------------
func (roomInfo *RoomInfo) addPlayerToRoom(a gate.Agent, isOwner bool) bool {
	log.Debug("add player to room=%v", roomInfo.roomId)

	basePlayer := player.GetPlayerBtAgent(a)
	if basePlayer == nil {
		log.Error("player has not logined, can't add.")
		return false
	}
	basePlayer.roomId = roomInfo.roomId

	sideInfo := &SideInfo{}
	sideInfo.isRobot = false
	sideInfo.isOwner = isOwner
	sideInfo.side = roomInfo.getSide()
	sideInfo.playerOid = basePlayer.oid
	sideInfo.roomId = roomInfo.roomId
	sideInfo.process = ProcessStatus_DEFAULT

	roomInfo.sideInfoMap.lock.Lock()
	roomInfo.sideInfoMap.cMap[basePlayer.oid] = sideInfo
	roomInfo.sideInfoMap.lock.Unlock()

	// send update room playr event
	log.Debug("send player%v into room", basePlayer.oid)
	pbPlayer := basePlayer.ToPbBattlePlayerInfo(sideInfo.side, isOwner)
	var players []*pb.BattlePlayerInfo
	players = append(players, pbPlayer)
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	for _, value := range roomInfo.sideInfoMap.cMap {
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
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
		memberCount := len(roomInfo.sideInfoMap.cMap)
		if memberCount < 4 {
			log.Debug("need add robot")
			var players []*pb.BattlePlayerInfo
			for i := 0; i < 4-memberCount; i++ {
				robot := &pb.BattlePlayerInfo{}
				robot.Oid = proto.Int32(i + 20000)

				logStr := "robot"
				buf := bytes.NewBufferString(logStr)
				str := strconv.Itoa(i + 20000)
				buf.Write([]byte(str))
				robot.NickName = proto.String(buf.String())

				robot.HeadIcon = proto.String("nil")
				robot.Gold = proto.Int32(0)
				robot.Diamond = proto.Int32(0)
				robot.Side = roomInfo.getSide().Enum()
				robot.IsOwner = proto.Bool(false)
				roomInfo.addRobotToRoom(robot)
				players = append(players, robot)
			}
			//send robot into room
			status := pb.GS2CUpdateRoomInfo_ADD.Enum()
			for _, value := range roomInfo.sideInfoMap.cMap {
				if !value.isRobot && value.agent != nil {
					msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
				}
			}
		}

		if len(roomInfo.sideInfoMap.cMap) == 4 {
			roomInfo.startBattle()
			over = true
		}
	}()
}

func (roomInfo *RoomInfo) addRobotToRoom(robot *pb.BattlePlayerInfo) {
	log.Debug("addRobotToRoom roomId%v, robotOid%v", roomInfo.roomId, robot.Oid)
	sideInfo := &SideInfo{}
	sideInfo.isRobot = true
	sideInfo.isOwner = false
	sideInfo.side = robot.GetSide()
	sideInfo.playerOid = robot.oid
	sideInfo.roomId = roomInfo.roomId

	roomInfo.sideInfoMap.lock.Lock()
	roomInfo.sideInfoMap.cMap[robot.oid] = sideInfo
	roomInfo.sideInfoMap.lock.Unlock()
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
		roomInfo.sideInfoMap.lock.Lock()
		for i, sideInfo := range roomInfo.sideInfoMap.cMap {
			if i == 0 {
			}
			//log.Debug("curSide=%v, player.side=%v", value, sideInfo.side)
			if sideInfo.side == value {
				isFind = true
				break
			}
		}
		roomInfo.sideInfoMap.lock.Unlock()
		if !isFind {
			result = append(result, value)
		}
	}
	log.Debug("current side list count is %v", len(result))
	return result
}

//------------------------------------------ battle start ------------------------------------------
func (roomInfo *RoomInfo) startBattle() {
	log.Debug("startBattle, roomId=%v", roomInfo.roomId)
	if len(roomInfo.sideInfoMap.cMap) != 4 {
		log.Debug("member%v not enough, can't start game.", len(roomInfo.sideInfoMap.cMap))
		result
	}
	roomInfo.dealerId = 10000 //roomInfo.reqDealer()
	curTurnPlayerOid = roomInfo.dealerId
	var allPlayerCards []*pb.CardInfo
	roomInfo.cardWall = card.LoadAllCards()

	//deal card to everyone
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		sideInfo.resetCardsData()
		if sideInfo.playerOid == roomInfo.dealerId {
			sideInfo.cardList = append(sideInfo.cardList, roomInfo.cardWall[:14])
			roomInfo.cardWall = roomInfo.cardWall[14:]
		} else {
			sideInfo.cardList = append(sideInfo.cardList, roomInfo.cardWall[:13])
			roomInfo.cardWall = roomInfo.cardWall[13:]
		}
		log.Debug("battle start: player%v has %v card", sideInfo.playerOid, len(sideInfo.cardList))
	}

	sendCardListByBattleStart()

	//log
	roomInfo.allCardLog()
}

func (roomInfo *RoomInfo) sendCardListByBattleStart() {
	log.Debug("sendCardListByBattleStart")
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			curPlayerOid := sideInfo.playerOid

			allPlayerCards := make([]*pb.CardInfo, 0)
			for _, innerSideInfo := range roomInfo.sideInfoMap.cMap {
				for _, card := range innerSideInfo.cardList {
					card.status = CardStatus_INHAND
					pbCard := &pb.CardInfo{}
					pbCard.CardOid = proto.Int32(card.oid)
					if curPlayerOid != innerSideInfo.playerOid {
						pbCard.CardId = proto.Int32(0)
					} else {
						pbCard.CardId = proto.Int32(card.oid)
					}
					pbCard.Status = card.ToPbStatus(card.status).Enum()
					pbCard.FromOther = proto.Bool(card.fromOther)
					allPlayerCards = append(allPlayerCards, pbCard)
				}
			}
			log.Debug("send player%v %v cards", sideInfo.playerOid, len(allPlayerCards))

			msgHandler.SendGS2CBattleStart(roomInfo.dealerId, allPlayerCards, sideInfo.agent)
		}
	}
}

func (roomInfo *RoomInfo) reqDealer() int32 {
	var playerOidList []int32
	for _, value := range roomInfo.sideInfoMap.cMap {
		playerOidList = append(playerOidList, value.playerInfo.oid)
	}
	count := len(playerOidList)
	rand.Seed(time.Now().UnixNano())
	index := rand.Intn(count)
	dealerId := playerOidList[index]
	log.Debug("roomId=%v, dealerId=%v", roomInfo.roomId, dealerId)
	return dealerId
}

//------------------------------------------ exchange cards ------------------------------------------
func (roomInfo *RoomInfo) updateExchangeCards(cardOidList []int32, playerOid int32) {
	sideInfo, ok := roomInfo.sideInfoMap.cMap[playerOid]
	if ok {
		sideInfo.updateExchangeCards(cardOidList)
	} else {
		log.Error("player%v not in room%v", playerOid, roomInfo.roomId)
	}
	if roomInfo.isRealPlayerExchangeOver() {
		roomInfo.robotProExchange()
		roomInfo.processExchangeCard()
	}
}

func (roomInfo *RoomInfo) isRealPlayerExchangeOver() bool {
	log.Debug("isRealPlayerExchangeOver")
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.process != ProcessStatus_EXCHANGE_OVER {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) robotProExchange() {
	log.Debug("robotProExchange")
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.isRobot {
			sideInfo.robotSelectExchangeCard()
		}
	}
}

func (roomInfo *RoomInfo) processExchangeCard() {
	log.Debug("processExchangeCard")
	exchangeAllMap := make(map[int32][]*card.Card) //playerOid : cardList
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		list := make([]*card.Card, 0)
		for n, card := range sideInfo.cardList {
			if card.status == CardStatus_EXCHANGE {
				card.status = CardStatus_INHAND
				list = append(list, card)
				sideInfo.cardList = append(sideInfo.cardList[:n], sideInfo.cardList[n+1:]...)
			}
		}
		log.Debug("player[%v] has %v exchange cards, left card list count=%v", sideInfo.playerInfo.oid, len(list), len(sideInfo.cardList))
		exchangeAllMap[sideInfo.playerInfo.oid] = list
	}

	exchangeType := getExchangeType()
	log.Debug("exchangeType=%v", exchangeType)
	playerIdListSortBySide := roomInfo.getPlayerIdListSortBySide()
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		index := 0
		for j := 0; j < len(playerIdListSortBySide); j++ {
			if playerIdListSortBySide[j] == sideInfo.playerOid {
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
		sideInfo.cardList = append(sideInfo.cardList[:], exchangeAllMap[fromPlayerId][:]...)
	}

	roomInfo.sendCardInfoAfterExchange(exchangeType)

	//log
	roomInfo.allCardLog()
}

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

//east -> south -> west -> north
func (roomInfo *RoomInfo) getPlayerIdListSortBySide() []int32 {
	var result []int32
	sideList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	for _, side := range sideList {
		for _, sideInfo := range roomInfo.sideInfoMap.cMap {
			if sideInfo.side == side {
				result = append(result, sideInfo.playerOid)
				break
			}
		}
	}
	return result
}

func (roomInfo *RoomInfo) sendCardInfoAfterExchange(exchangeType pb.ExchangeType) {
	log.Debug("sendCardInfoAfterExchange")
	var allExchangedCardList []*pb.CardInfo
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		for _, origCard := range sideInfo.cardList {
			allExchangedCardList = append(allExchangedCardList, origCard.TopbCard(sideInfo.playerOid))
		}
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CUpdateCardInfoAfterExchange(exchangeType.Enum(), allExchangedCardList, sideInfo.agent)
		}
		/*else if sideInfo.isRobot {
			sideInfo.selectLack()
		}*/
	}
}

//---------------------------------------- lack card ------------------------------------------
func (roomInfo *RoomInfo) updateLack(playerOid int32, lackType pb.CardType) {
	sideInfo, ok := roomInfo.sideInfoMap.cMap[playerOid]
	if ok {
		sideInfo.lackType = lackType.String()
		sideInfo.process = ProcessStatus_LACK_OVER
	} else {
		log.Error("playerOid[%v] not in room%v.", playerOid, roomInfo.roomId)
	}
	if roomInfo.realPlayerSelectLackOver() {
		roomInfo.robotProcLack()
		roomInfo.sendLackCard()
	}
}

func (roomInfo *RoomInfo) realPlayerSelectLackOver() bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.process != ProcessStatus_LACK_OVER {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) robotProcLack() {
	log.Debug("robotProcLack")
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.isRobot {
			sideInfo.robotSelectLackCard()
		}
	}
}

func (roomInfo *RoomInfo) sendLackCard() {
	log.Debug("sendLackCard")
	result := make([]*pb.LackCard, 0)
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		lack := &pb.LackCard{}
		lack.PlayerId = proto.Int32(sideInfo.playerOid)
		lack.Type = sideInfo.lackType.Enum()
		result = append(result, lack)
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CSelectLackRet(result, sideInfo.agent)
		}
	}
	log.Debug("select lack over, game start------------->>>>>>>>>>>>>>")
	roomInfo.dealerStart()
}

//------------------------------------------------- game start -------------------------------------------------
func (roomInfo *RoomInfo) dealerStart() {
	log.Debug("dealerStart")
	curTurnPlayerOid = roomInfo.dealerId
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CTurnToNext(curTurnPlayerOid, nil, pb.TurnSwitchType_NotDrawCard.Enum(), sideInfo.agent)
		}
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if roomInfo.dealerId == sideInfo.playerInfo.oid {
			if sideInfo.isRobot {
				sideInfo.robotTurnSwitch()
			} else {
				sideInfo.playerTurnSwitch()
			}
			break
		}
	}
}

func (roomInfo *RoomInfo) sendRealPlayerProc(procPlayer int32, beProcPlayer int32, procType pb.ProcType, procCardId int32) {
	log.Debug("sendRealPlayerProc, procPlayer=%v, beProcPlayer=%v， procType=%v, procCardId=%v", procPlayer, beProcPlayer, procType, procCardId)
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CPlayerEnsureProc(procPlayer, procType.Enum(), beProcPlayer, procCardId, sideInfo.agent)
		}
	}
}

func (roomInfo *RoomInfo) sendRealPlayerCardListAfterProc(procPlayer int32, beProcPlayer int32) {
	log.Debug("sendRealPlayerCardListAfterProc, card count=%v", len(cardList))
	pbCardList := make([]*pb.CardInfo, 0)
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if procPlayer == sideInfo.playerOid {
			for _, curCard := range sideInfo.cardList {
				pbCard := curCard.TopbCard(sideInfo.playerOid)
				pbCardList = append(pbCardList, pbCard)
			}
		}
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CUpdateCardAfterPlayerProc(pbCardList, sideInfo.agent)
		}
	}
}

func (roomInfo *RoomInfo) sendRobotProc(procPlayer int32, procType pb.ProcType, beProcPlayer int32) {
	log.Debug("sendRobotProc, procPlayer=%v, procType=%v, beProcPlayer=%v", procPlayer, beProcPlayer, procType)
	var cardList []*pb.CardInfo
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.playerOid == procPlayer || sideInfo.playerOid == beProcPlayer {
			for _, card := range sideInfo.cardList {
				pbCard := card.TopbCard(sideInfo.playerOid)
				if pbCard.Status == pb.CardStatus_inHand || pbCard.Status == pb.CardStatus_deal {
					pbCard.CardId = proto.Int32(0)
				}
				cardList = append(cardList, pbCard)
			}
		}
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CRobotProc(procPlayer, procType.Enum(), beProcPlayer, cardList, sideInfo.agent)
		}
	}
}

func (roomInfo *RoomInfo) broadcastAndProcDiscard(card *card.Card) {
	log.Debug("broad discard to everyone，discard=%v(%v)", card.oid, card.id)
	if card != nil {
		for _, sideInfo := range roomInfo.sideInfoMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CDiscardRet(card.oid, sideInfo.agent)
			}
			sideInfo.playerProcDiscard(card)
		}
	}
}

//--------------------------------------- real player proc ret ---------------------------------------
func (roomInfo *RoomInfo) playerEnsureProc(procPlayerOid int32, procType pb.ProcType, procCardId int32) {
	log.Debug("player%v select proc=%v, procCardId=%v", procPlayerOid, procType, procCardId)
	sideInfo, ok := roomInfo.sideInfoMap.cMap[procPlayerOid]
	if ok {
		sideInfo.realPlayerProcOver(procType, procCardId)
	} else {
		log.Error("player%v not in room%v", procPlayerOid, roomInfo.roomId)
	}
}

func (roomInfo *RoomInfo) outRoom(playerOid int32) {
	log.Debug("playerOid[%v] out room", playerOid)
	isFind := false
	var playerList []*pb.BattlePlayerInfo
	for i, value := range roomInfo.sideInfoMap.cMap {
		if value.playerInfo.oid == playerOid {
			delete(roomInfo.sideInfoMap.cMap, i)
			isFind = true

			battlePlayer := sideInfoToPbBattlePlayerInfo(value)
			playerList = append(playerList, battlePlayer)
			break
		}
	}
	for i, value := range roomInfo.sideInfoMap.cMap {
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

func (roomInfo *RoomInfo) isEmptyRoom() bool {
	if len(roomInfo.sideInfoMap.cMap) <= 0 {
		return true
	}
	return false
}

//---------------------------------------- turn over ----------------------------------------
func (roomInfo *RoomInfo) checkTurnOver() {
	log.Debug("出牌后，checkTurnOver")
	preDiscard := roomInfo.getPreDiscard()
	if roomInfo.isNormalTurnOver() {
		log.Debug("all is normal over.(no one need p、g、h proc)")
		if preDiscard != nil {
			preDiscard.status = CardStatus_DISCARD
		} else {
			log.Error("normal over error! preDiscard is nil.")
		}
		nextSide := roomInfo.getNextSide(curPlayerOid)
		roomInfo.sendNormalTurnToNext(nextSide)
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
			for _, sideInfo := range roomInfo.sideInfoMap.cMap {
				if sideInfo.process == ProcessStatus_WAITING_HU {
					playerHuOid = append(playerHuOid, sideInfo.playerOid)
				}
			}
			if len(playerHuOid) > 0 {
				log.Debug("player proc hu.")
				roomInfo.procHuOther(preDiscard, playerHuOid)
				roomInfo.allCardLog()
			} else {
				log.Debug("player proc p、g.")
				roomInfo.procPGOther(preDiscard)
				roomInfo.allCardLog()
			}
		}
	}
}

func (roomInfo *RoomInfo) getPreDiscard() *card.Card {
	for _, sideInfo := range roomInfo.sideMap.cMap {
		for _, card := range sideInfo.cardList {
			if card.status == CardStatus_PRE_DISCARD {
				return card
			}
		}
	}
	return nil
}

func (roomInfo *RoomInfo) isNormalTurnOver() bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process != ProcessStatus_TURN_OVER {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isHuTurnOver() bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process != ProcessStatus_TURN_OVER && sideInfo.process != ProcessStatus_TURN_OVER_HU {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isGangTurnOver() bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process != ProcessStatus_TURN_OVER && sideInfo.process != ProcessStatus_TURN_OVER_GANG {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isPengTurnOver() bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process != ProcessStatus_TURN_OVER && sideInfo.process != ProcessStatus_TURN_OVER_PENG {
			return false
		}
	}
	return true
}

func (roomInfo *RoomInfo) isEveryoneProcDiscardOver() bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
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

func (roomInfo *RoomInfo) getHuTurnOverSideInfo() []*SideInfo {
	list := make([]*SideInfo, 0)
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_HU {
			list = append(list, sideInfo)
		}
	}
	return list
}

func (roomInfo *RoomInfo) getGangTurnOverSideInfo() *SideInfo {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_GANG {
			return sideInfo
		}
	}
	return nil
}

func (roomInfo *RoomInfo) getPengTurnOverSideInfo() *SideInfo {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process == ProcessStatus_TURN_OVER_PENG {
			return sideInfo
		}
	}
	return nil
}

//---------->>>>>>>> get next turn side
func (roomInfo *RoomInfo) getNextSide(fromPlayerOid int32) pb.BattleSide {
	log.Debug("turnToNextPlayer")
	curSide := roomInfo.getSideByPlayerOid(fromPlayerOid)
	if curSide != pb.BattleSide_none {
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
			if nextSide == curSide {
				return pb.BattleSide_none
			}
			if roomInfo.nextSideCanProc(nextSide) {
				return nextSide
			}
		}
	} else {
		log.Error("current side is none.")
	}
	return pb.BattleSide_none
}

func (roomInfo *RoomInfo) getSideByPlayerOid(playerOid int32) pb.BattleSide {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.playerOid == playerOid {
			return sideInfo.side
		}
	}
	return pb.BattleSide_none
}

func (roomInfo *RoomInfo) nextSideCanProc(nextSide pb.BattleSide) bool {
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		log.Debug("player%v process=%v", sideInfo.playerOid, sideInfo.process)
		if sideInfo.side == nextSide && sideInfo.process != ProcessStatus_GAME_OVER {
			return true
		}
	}
	return false
}

//---------->>>>>>>> proc h、g、p other
func (roomInfo *RoomInfo) procHuOther(preDiscard *card.Card, playerHuOid []int32) {
	log.Debug("proc robot or player hu other.")
	hasRealPlayerHu := false
	for _, playerOid := range playerHuOid {
		sideInfo, ok := roomInfo.sideInfoMap.cMap[playerOid]
		if ok {
			if !sideInfo.isRobot {
				sideInfo.process = ProcessStatus_PROC_HU
				roomInfo.sendRealPlayerProc(sideInfo.playerInfo.oid, curTurnPlayerOid, pb.ProcType_HuOther, preDiscard.id)
			}
		} else {
			log.Error("player%v not in room%v", playerOid, roomInfo.roomId)
		}
	}
}

func (roomInfo *RoomInfo) procPGOther(preDiscard *card.Card) {
	log.Debug("proc robot or player p、g other.")
	var proSideInfo *SideInfo
	var beProcSideInfo *SideInfo
	var procType pb.ProcType
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
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

func (roomInfo *RoomInfo) recvRealPlayerDiscard(playerId int32, Oid int32) {
	log.Debug("recvRealPlayerDiscard, player%v, cardOid%v", playerId, Oid)
	sideInfo, ok := roomInfo.sideInfoMap.cMap[playerId]
	if ok {
		discard := sideInfo.realPlayerUpdateDiscardInfo(Oid)
		roomInfo.broadcastAndProcDiscard(discard)
		roomInfo.checkTurnOver()
	} else {
		log.Error("no player[%v]", playerId)
	}
}

//---------------------------------------- turn to next ----------------------------------------
func (roomInfo *RoomInfo) sendNormalTurnToNext(nextSide pb.BattleSide) {
	log.Debug("sendNormalTurnToNext[%v]", nextSide)
	if len(roomInfo.cardWall) <= 0 {
		log.Debug("牌已经摸完，游戏结束.")
		roomInfo.sendGameOver()
		return
	}
	//deal_card -> inhand_card before switch turn
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.process != ProcessStatus_GAME_OVER {
			sideInfo.process = ProcessStatus_TURN_START
			sideInfo.refreshCard()
		}
	}
	newCard := &pb.CardInfo{}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.side == nextSide {
			curTurnPlayerOid = sideInfo.playerOid
			drawCard := roomInfo.cardWall[0]
			roomInfo.cardWall = roomInfo.cardWall[1:]
			drawCard.status = card.CardStatus_DEAL
			drawCard.TopbCard(sideInfo.playerOid)
			sideInfo.drawNewCard(drawCard)
			newCard = drawCard.TopbCard(curTurnPlayerOid)
			break
		}
	}
	//send real player first, because robot will sleep 1 seconds
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CTurnToNext(curTurnPlayerOid, newCard, pb.TurnSwitchType_Normal.Enum(), sideInfo.agent)
		}
	}

	roomInfo.allCardLog()

	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if sideInfo.playerInfo.oid == curTurnPlayerOid {
			if sideInfo.isRobot {
				sideInfo.robotTurnSwitch()
				break
			} else {
				sideInfo.playerTurnSwitch()
				break
			}
		}
	}
}

func (roomInfo *RoomInfo) sendHuTurnToNext(curHuSideInfoList []*SideInfo) {
	log.Debug("sendHuTurnToNext, hu count=[%v]", len(curHuSideInfoList))
	huPlayerCount := len(curHuSideInfoList)
	if huPlayerCount == 1 {
		if curHuSideInfoList[0].side != pb.BattleSide_none {
			nextSide := roomInfo.getNextSide(curHuSideInfoList[0].side)
			roomInfo.sendNormalTurnToNext(nextSide)
		} else {
			log.Error("current turn side is none!")
		}
	} else if huPlayerCount == 2 {
		isFindNext := false
		for _, sideInfo := range roomInfo.sideInfoMap.cMap {
			if sideInfo.playerInfo.oid != curTurnPlayerOid &&
				sideInfo.playerInfo.oid != curHuSideInfoList[0].playerInfo.oid && sideInfo.playerInfo.oid != curHuSideInfoList[1].playerInfo.oid {
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
			roomInfo.sendGameOver()
		}
	}
}

func (roomInfo *RoomInfo) sendPengTurnToNext(nextSide pb.BattleSide) {
	log.Debug("sendPengTurnToNext, nextSide=[%v]", nextSide)
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if nextSide == sideInfo.side {
			curTurnPlayerOid = sideInfo.playerInfo.oid
		}
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CTurnToNext(curTurnPlayerOid, nil, pb.TurnSwitchType_JustCanDiscard.Enum(), sideInfo.agent)
		}
	}
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if curTurnPlayerOid == sideInfo.playerOid && sideInfo.isRobot {
			timer := time.NewTimer(time.Second * 1)
			<-timer.C
			sideInfo.robotDiscard()
			break
		}
	}
}

func (roomInfo *RoomInfo) robotProcOver(robotOid int32, procType pb.ProcType) {
	log.Debug("robotProcOver, robotOid=%v, procType=%v", robotOid, procType)
	sideInfo, ok := roomInfo.sideInfoMap.cMap[robotOid]
	if ok {
		sideInfo.robotProcOver(procType)
	} else {
		log.Debug("robot%v not in room%v", robotOid, roomInfo.roomId)
	}
}

func (roomInfo *RoomInfo) getHuStatusCard() *card.Card {
	log.Debug("getHuStatusCard")
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
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

func (roomInfo *RoomInfo) sendGameOver() {
	log.Debug("sendGameOver")
	for _, sideInfo := range roomInfo.sideInfoMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CGameOver(sideInfo.agent)
		}
	}
}

//---------------------------------------- log ----------------------------------------
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

	for n, value := range roomInfo.sideInfoMap.cMap {
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
		buf.Write([]byte("["))
		str3 := strconv.Itoa(len(playerCardId))
		buf.Write([]byte(str3))
		buf.Write([]byte("]"))
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
