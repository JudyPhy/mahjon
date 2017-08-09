package roomMgr

import (
	"server/card"

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

//接口必须在摸牌后执行
func (sideInfo *SideInfo) playerTurnSwitch() {
	log.Debug("turn switch to real player%v", sideInfo.playerOid)
	inhandList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_INHAND)
	dealList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_DEAL)
	gList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_GANG)
	pList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_PENG)
	if card.IsHu(dealList, inhandList, gList, pList) {
		log.Debug("real player%v self Hu, game over!", sideInfo.playerOid)
		sendRealPlayerProc(sideInfo.roomId, sideInfo.playerOid, 0, pb.ProcType_SelfHu)
	} else {
		log.Debug("can't self hu, check self gang.")
		inhandPDList := append(inhandList[:], dealList[:]...)
		inhandPDList = append(inhandPDList[:], pList[:]...)
		if card.CanSelfGang(inhandPDList) {
			sendRealPlayerProc(sideInfo.roomId, sideInfo.playerOid, 0, pb.ProcType_SelfGang)
		} else {
			log.Debug("can't self gang, proc discard")
			sendRealPlayerProc(sideInfo.roomId, sideInfo.playerOid, 0, pb.ProcType_Discard)
		}
	}
}

func (sideInfo *SideInfo) realPlayerProcOver(procType pb.ProcType, procCardId int32) {
	if procType == pb.ProcType_Peng {
		//peng
		roomInfo.playerEnsurePeng(procPlayerOid)
	} else if procType == pb.ProcType_HuOther {
		//hu other
		roomInfo.playerEnsureHuOther(procPlayerOid)
	} else if procType == pb.ProcType_SelfHu {
		//self hu
		sideInfo.process = ProcessStatus_GAME_OVER
		huCard := sideInfo.getDealCard()
		huCard.status = card.CardStatus_HU
		sendRealPlayerCardListAfterProc(sideInfo.roomId, sideInfo.playerOid, 0)
		//client needs time to play ani of updating cards
		timer := time.NewTimer(time.Second * 1)
		<-timer.C
		turnToSelfAfterHu(sideInfo.roomId, []*SideInfo{sideInfo})
	} else if procType == pb.ProcType_GangOther {
		//gang other
		roomInfo.playerEnsureGang(procPlayerOid)
	} else if procType == pb.ProcType_SelfGang {
		//self gang
		for _, curCard := range sideInfo.cardList {
			if curCard.id == procCardId {
				curCard.status = card.CardStatus_GANG
			}
		}
		sideInfo.process = ProcessStatus_TURN_OVER_GANG
		sendRealPlayerCardListAfterProc(sideInfo.roomId, sideInfo.playerOid, 0)
		//client needs time to play ani of updating cards
		timer := time.NewTimer(time.Second * 1)
		<-timer.C
		turnToSelfAfterGang(sideInfo.roomId, sideInfo.side)
	}
}

func (sideInfo *SideInfo) realPlayerUpdateDiscardInfo(cardOid int32) *card.Card {
	isFind := false
	var card *card.Card
	for _, value := range sideInfo.cardList {
		if value.oid == cardOid {
			value.status = CardStatus_PRE_DISCARD
			sideInfo.process = ProcessStatus_TURN_OVER
			isFind = true
			card = value
			break
		}
	}
	if isFind {
		log.Debug("玩家[%v]出牌[%v(%v)]成功", sideInfo.playerInfo.oid, card.oid, card.id)
	} else {
		log.Debug("玩家出牌[%v]不在自己手牌中", cardOid)
	}
	return card
}
