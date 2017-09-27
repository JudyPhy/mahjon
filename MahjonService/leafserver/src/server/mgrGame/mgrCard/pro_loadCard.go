package mgrCard

import (
	"github.com/name5566/leaf/log"
)

func LoadAllCards(gType string) []*CardInfo {

	var maxCount int
	//===================判断是否为血战===============>
	switch gType {
	case "XueZhan":
		maxCount = 108
	}

	cardWall := make([]*CardInfo, 0)
	id := int32(0)
	for i := 0; i < maxCount; i++ {
		card := &CardInfo{}
		card.Oid = int32(i)
		if i%4 == 0 {
			id++
			if id%10 == 0 {
				id++
			}
		}
		card.Id = id
		card.Status = CardStatus_Wall

		cardWall = append(cardWall, card)
	}
	log.Debug("load all card over, count=%v", len(cardWall))
	return cardWall
}
