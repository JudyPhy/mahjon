package mgrSide

import (
	"server/mgrGame/mgrCard"

	"github.com/name5566/leaf/log"
)

func (sider *SideInfo) GetHandCards() []*mgrCard.CardInfo {
	return sider.handCards
}

func (sider *SideInfo) SetHandCards(card []*mgrCard.CardInfo) {
	for _, c := range card {
		c.PlayerId = sider.player.GetPlayerId()
	}
	sider.handCards = append(sider.handCards, card...)
}

func (sideInfo *SideInfo) SetHandCardsByNew(cards []*mgrCard.CardInfo) {
	sideInfo.handCards = append(sideInfo.handCards[0:0], cards...)
	log.Debug("玩家%v换牌后手牌为:%v", sideInfo.player.GetPlayerId(), mgrCard.GetCardIdList(sideInfo.handCards))
}

func (sider *SideInfo) RemoveCardFromHandCards(card int32) {
	for k, c := range sider.handCards {
		if c.Oid == card {
			c.Status = mgrCard.CardStatus_DisCard
			sider.disCards = append(sider.disCards, c)
			sider.handCards = append(sider.handCards[:k], sider.handCards[k+1:]...)
		}
	}
}

func (sider *SideInfo) SortHandCards() {
	for _, c := range sider.handCards {
		if c.Status == mgrCard.CardStatus_Deal {
			c.Status = mgrCard.CardStatus_InHand
		}
	}
}

func (sider *SideInfo) UpdateCardAfterDiscard(drawCard *mgrCard.CardInfo) []*mgrCard.CardInfo {

	sider.RemoveCardFromHandCards(drawCard.Oid)
	sider.SortHandCards()
	sider.process = ProcessStatus_TURN_OVER

	afterCards := append(sider.disCards, sider.handCards...)
	return afterCards
}
