package mgrSide

import (
	"server/mgrGame/mgrCard"

	"github.com/name5566/leaf/log"
)

func (sideInfo *SideInfo) SetExchangeCard(cardOidList []int32) {
	for _, v := range cardOidList {
		for _, c := range sideInfo.GetHandCards() {
			if v == c.Oid {
				c.Status = mgrCard.CardStatus_Exchanged
				log.Debug("sideMgr==>SetExchangeCard==>玩家%v换的牌为:%v", sideInfo.player.GetPlayerId(), c)
			}
		}
	}
	sideInfo.process = ProcessStatus_EXCHANGE_OVER
}
