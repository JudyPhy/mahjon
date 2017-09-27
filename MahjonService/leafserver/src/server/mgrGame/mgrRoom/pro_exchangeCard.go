package mgrRoom

import (
	"server/mgrGame/mgrCard"
	"server/mgrGame/mgrSide"
	"server/mgrPlayer"
	"server/pb"

	"github.com/name5566/leaf/log"

	"math/rand"
	"time"
)

func (room *RoomInfo) UpdateExchangeCard(exCardOidList []int32, pid int32) {
	sider := room.sideMap[pid]
	sider.SetExchangeCard(exCardOidList)
	if room.checkExchangeOver() {
		for _, sider := range room.sideMap {
			if sider.GetIsRobot() {
				sider.SelectRobotExchangeCard()
			}
		}
		room.exchangeCardProc()
	}
}

func (room *RoomInfo) checkExchangeOver() bool {
	for _, a := range room.agents {
		pid := mgrPlayer.GetPlayerByAgent(a).GetPlayerId()
		if room.sideMap[pid].GetProcess() != mgrSide.ProcessStatus_EXCHANGE_OVER {
			return false
		}
	}
	return true
}

func (room *RoomInfo) exchangeCardProc() {

	exchangeAllMap := make(map[string][]*mgrCard.CardInfo)
	for _, sider := range room.sideMap {
		exCardList := make([]*mgrCard.CardInfo, 0)
		newHandCards := make([]*mgrCard.CardInfo, 0)
		for _, card := range sider.GetHandCards() {
			if card.Status == mgrCard.CardStatus_Exchanged {
				card.Status = mgrCard.CardStatus_InHand
				exCardList = append(exCardList, card)
			} else {
				newHandCards = append(newHandCards, card)
			}
		}
		exchangeAllMap[sider.GetSide().String()] = exCardList
		sider.SetHandCardsByNew(newHandCards)
	}
	exchangeType := getExchangeType()
	log.Debug("exchangeType=%v", exchangeType)
	for _, sider := range room.sideMap {
		fromSideStr := getExchangeCardSide(exchangeType, sider.GetSide())
		sider.SetHandCards(exchangeAllMap[fromSideStr])
	}
	allSideCardList := make([]*mgrCard.CardInfo, 0)
	for _, sider := range room.sideMap {
		allSideCardList = append(allSideCardList, sider.GetHandCards()...)
	}
	room.sendExchangeCardRet(exchangeType, allSideCardList)

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

func getExchangeCardSide(exchangeType pb.ExchangeType, curSide pb.MahjonSide) string {
	var nextSideStr string
	switch exchangeType {
	case pb.ExchangeType_AntiClock: //逆时针
		nextsideId := pb.MahjonSide_value[curSide.String()] + 1
		if nextsideId == 6 {
			nextsideId = 2
		}
		nextSideStr = pb.MahjonSide_name[nextsideId]
	case pb.ExchangeType_ClockWise: //顺时针
		nextsideId := pb.MahjonSide_value[curSide.String()] - 1
		if nextsideId == 1 {
			nextsideId = 5
		}
		nextSideStr = pb.MahjonSide_name[nextsideId]
	case pb.ExchangeType_Opposite:
		nextsideId := pb.MahjonSide_value[curSide.String()] + 2
		if nextsideId == 7 {
			nextsideId = 3
		} else if nextsideId == 6 {
			nextsideId = 2
		}
		nextSideStr = pb.MahjonSide_name[nextsideId]
	}
	return nextSideStr

}
