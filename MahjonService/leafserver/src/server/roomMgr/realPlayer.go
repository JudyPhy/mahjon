package roomMgr

import (
	"github.com/name5566/leaf/log"
)

func (sideInfo *SideInfo) updateExchangeCards(cardOidList []int32) {
	log.Debug("player%v exchange card", sideInfo.playerOid)
	for _, clientCard := range cardOidList {
		isFind := false
		for _, serviceCard := range sideInfo.cardList {
			if clientCard == serviceCard.oid {
				serviceCard.status = CardStatus_EXCHANGE
				isFind = true
				break
			}
		}
		if !isFind {
			log.Error("playerOid[%v]'s exchanged card is not in cardList.", playerOid)
		}
	}
	sideInfo.process = ProcessStatus_EXCHANGE_OVER
}
