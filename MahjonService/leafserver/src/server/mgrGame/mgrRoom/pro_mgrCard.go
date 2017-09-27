package mgrRoom

import (
	"server/mgrGame/mgrCard"

	"github.com/name5566/leaf/log"

	"math/rand"
	"time"
)

func (mj *RoomInfo) shuffle() {
	k := len(mj.allCards)
	r_seed := rand.New(rand.NewSource(time.Now().UnixNano()))
	for i := 0; i < len(mj.allCards); i++ {
		x := r_seed.Intn(k)

		t := mj.allCards[x].Id
		mj.allCards[x].Id = mj.allCards[k-1].Id
		mj.allCards[k-1].Id = t
		k--
	}
	log.Debug("shuffled")
}

func (mj *RoomInfo) dealStartBattle() []*mgrCard.CardInfo {

	for i := 0; i < 4; i++ {
		mj.dealcards(mj.curPlayerId, 13)
		mj.nomalTurnToNext()
	}
	mj.dealcards(mj.dealerId, 1)
	cardListSum := make([]*mgrCard.CardInfo, 0)
	for _, sider := range mj.sideMap {
		cardListSum = append(cardListSum, sider.GetHandCards()...)
	}

	return cardListSum
}

func (mj *RoomInfo) dealcards(playerId int32, num int) []*mgrCard.CardInfo {
	startId := mj.cardIndex
	endId := startId + num
	_cardSlices := mj.allCards[startId:endId]
	for _, v := range _cardSlices {
		v.PlayerId = playerId
		v.Status = mgrCard.CardStatus_Deal
	}
	mj.sideMap[playerId].SetHandCards(_cardSlices)
	mj.cardIndex = endId
	return _cardSlices
}
