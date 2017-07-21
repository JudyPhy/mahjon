package roomMgr

import (
	"math/rand"
	"time"

	"github.com/name5566/leaf/log"
)

type MJType int32

const (
	MJType_XUEZHAN MJType = 1
)

func (x MJType) Enum() *MJType {
	p := new(MJType)
	*p = x
	return p
}

type MJCardCount int32

const (
	MJCardCount_XUEZHAN MJCardCount = 108
)

func (x MJCardCount) Enum() *MJCardCount {
	p := new(MJCardCount)
	*p = x
	return p
}

type CardStatus int32

const (
	CardStatus_NODEAL   CardStatus = 1
	CardStatus_INHAND   CardStatus = 2
	CardStatus_PENG     CardStatus = 3
	CardStatus_GANG     CardStatus = 4
	CardStatus_EXCHANGE CardStatus = 5
)

func (x CardStatus) Enum() *CardStatus {
	p := new(CardStatus)
	*p = x
	return p
}

type Card struct {
	oid    int32
	id     int32
	status *CardStatus
}

var mjType MJType
var cardWall []*Card

func loadAllCards() {
	log.Debug("loadAllCards")
	mjType := MJType_XUEZHAN
	mjCardCount := MJCardCount_XUEZHAN
	if mjType == MJType_XUEZHAN {
		mjCardCount = MJCardCount_XUEZHAN
	}
	maxCount := int32(mjCardCount)
	log.Debug("max card count=%d", maxCount)
	cardWall = make([]*Card, maxCount)
	id := int32(0)
	for i := int32(0); i < maxCount; i++ {
		card := &Card{}
		card.oid = int32(i)
		if i%4 == 0 {
			id++
			if id%10 == 0 {
				id++
			}
		}
		card.id = id
		card.status = CardStatus_NODEAL.Enum()
		cardWall = append(cardWall, card)
		log.Debug("card oid=%d", card.oid, ", id=%d", card.id)
	}
}

func getCardListByBattleStart() []*Card {
	list := make([]*Card, 13)
	for i := 0; i < 13; i++ {
		rand.Seed(time.Now().Unix())
		rnd := rand.Intn(len(cardWall))
		cardWall[rnd].status = CardStatus_INHAND.Enum()
		list = append(list, cardWall[rnd])
		cardWall = append(cardWall[:rnd], cardWall[rnd+1:]...)
	}
	return list
}
