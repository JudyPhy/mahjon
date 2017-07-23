package roomMgr

import (
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
	status CardStatus
}

var mjType MJType

func loadAllCards() []*Card {
	log.Debug("loadAllCards")
	mjType := MJType_XUEZHAN
	mjCardCount := MJCardCount_XUEZHAN
	if mjType == MJType_XUEZHAN {
		mjCardCount = MJCardCount_XUEZHAN
	}
	maxCount := int32(mjCardCount)
	log.Debug("max card count=%v", maxCount)

	var cardWall []*Card
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
		card.status = CardStatus_NODEAL
		cardWall = append(cardWall, card)
		//log.Debug("card oid=%v, id=%v", card.oid, card.id)
	}
	log.Debug("load all card over, count=%v", len(cardWall))
	return cardWall
}
