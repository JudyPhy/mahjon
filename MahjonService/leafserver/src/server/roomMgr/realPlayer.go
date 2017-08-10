package roomMgr

import (
	"server/card"
	"server/pb"
	"time"

	"github.com/name5566/leaf/log"
)

func (sideInfo *SideInfo) updateExchangeCards(cardOidList []int32) {
	log.Debug("player%v exchange card", sideInfo.playerOid)
	for _, clientCard := range cardOidList {
		isFind := false
		for _, serviceCard := range sideInfo.cardList {
			if clientCard == serviceCard.OID {
				serviceCard.Status = card.CardStatus_EXCHANGE
				isFind = true
				break
			}
		}
		if !isFind {
			log.Error("playerOid[%v]'s exchanged card is not in cardList.", sideInfo.playerOid)
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
		sendRealPlayerProc(sideInfo.roomId, sideInfo.playerOid, pb.ProcType_SelfHu, int32(0), int32(0))
	} else {
		log.Debug("can't self hu, check self gang.")
		inhandPDList := append(inhandList[:], dealList[:]...)
		inhandPDList = append(inhandPDList[:], pList[:]...)
		if card.CanSelfGang(inhandPDList) {
			sendRealPlayerProc(sideInfo.roomId, sideInfo.playerOid, pb.ProcType_SelfGang, int32(0), int32(0))
		} else {
			log.Debug("can't self gang, proc discard")
			sendRealPlayerProc(sideInfo.roomId, sideInfo.playerOid, pb.ProcType_Discard, int32(0), int32(0))
		}
	}
}

func (sideInfo *SideInfo) realPlayerProcOver(procType pb.ProcType, procCardId int32, curTurnSideInfo *SideInfo) {
	if procType == pb.ProcType_Peng {
		//peng
		sideInfo.realPlayerEnsurePeng(curTurnSideInfo)
	} else if procType == pb.ProcType_HuOther {
		//hu other
		sideInfo.realPlayerEnsureHuOther(curTurnSideInfo)
	} else if procType == pb.ProcType_SelfHu {
		//self hu
		sideInfo.process = ProcessStatus_GAME_OVER
		huCard := sideInfo.getDealCard()
		huCard.Status = card.CardStatus_HU
		sendRealPlayerCardListAfterProc(sideInfo.roomId, sideInfo.playerOid, 0)
	} else if procType == pb.ProcType_GangOther {
		//gang other
		sideInfo.realPlayerEnsureGang(curTurnSideInfo)
	} else if procType == pb.ProcType_SelfGang {
		//self gang
		for _, curCard := range sideInfo.cardList {
			if curCard.ID == procCardId {
				curCard.Status = card.CardStatus_GANG
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
	var curCard *card.Card
	for _, value := range sideInfo.cardList {
		if value.OID == cardOid {
			value.Status = card.CardStatus_PRE_DISCARD
			sideInfo.process = ProcessStatus_TURN_OVER
			isFind = true
			curCard = value
			break
		}
	}
	if isFind {
		log.Debug("玩家[%v]出牌[%v(%v)]成功", sideInfo.playerOid, curCard.OID, curCard.ID)
	} else {
		log.Debug("玩家出牌[%v]不在自己手牌中", cardOid)
	}
	return curCard
}

func (sideInfo *SideInfo) realPlayerEnsurePeng(curTurnSideInfo *SideInfo) {
	log.Debug("realPlayerEnsurePeng")
	preDiscard := curTurnSideInfo.getPreDiscard()
	if preDiscard == nil {
		log.Error("player%v need peng, but pre discard is nil.", sideInfo.playerOid)
		return
	}
	count := 0
	for _, curCard := range sideInfo.cardList {
		if (curCard.Status == card.CardStatus_INHAND || curCard.Status == card.CardStatus_DEAL) && curCard.ID == preDiscard.ID {
			curCard.Status = card.CardStatus_PENG
			count++
			if count == 2 {
				break
			}
		}
	}
	preDiscard.Status = card.CardStatus_PENG
	preDiscard.FromOther = true
	sideInfo.addDiscardAsPeng(preDiscard)
	curTurnSideInfo.deleteDiscard(preDiscard)

	sideInfo.process = ProcessStatus_TURN_OVER_PENG
	sendRealPlayerCardListAfterProc(sideInfo.roomId, sideInfo.playerOid, curTurnSideInfo.playerOid)

	timer := time.NewTimer(time.Second * 1)
	<-timer.C
	realPlayerTurnToSelfAfterPeng(sideInfo.roomId, sideInfo.playerOid)
}

func (sideInfo *SideInfo) getPreDiscard() *card.Card {
	for _, curCard := range sideInfo.cardList {
		if curCard.Status == card.CardStatus_PRE_DISCARD {
			return curCard
		}
	}
	return nil
}

func (sideInfo *SideInfo) realPlayerEnsureGang(curTurnSideInfo *SideInfo) {
	log.Debug("realPlayerEnsureGang")
	preDiscard := curTurnSideInfo.getPreDiscard()
	if preDiscard == nil {
		log.Error("player%v need gang, but pre discard is nil.", sideInfo.playerOid)
		return
	}
	for _, curCard := range sideInfo.cardList {
		if curCard.ID == preDiscard.ID {
			curCard.Status = card.CardStatus_GANG
		}
	}
	preDiscard.Status = card.CardStatus_GANG
	preDiscard.FromOther = true
	sideInfo.addDiscardAsPeng(preDiscard)
	curTurnSideInfo.deleteDiscard(preDiscard)

	sideInfo.process = ProcessStatus_TURN_OVER_PENG
	sendRealPlayerCardListAfterProc(sideInfo.roomId, sideInfo.playerOid, curTurnSideInfo.playerOid)

	timer := time.NewTimer(time.Second * 1)
	<-timer.C
	turnToSelfAfterGang(sideInfo.roomId, sideInfo.side)
}

func (sideInfo *SideInfo) realPlayerEnsureHuOther(curTurnSideInfo *SideInfo) {
	log.Debug("realPlayerEnsureHuOther")
	preDiscard := curTurnSideInfo.getPreDiscard()
	if preDiscard == nil {
		log.Error("player%v need hu, but pre discard is nil.", sideInfo.playerOid)
		return
	}
	huCard := &card.Card{}
	huCard.OID = preDiscard.OID
	huCard.ID = preDiscard.ID
	huCard.FromOther = true
	huCard.Status = card.CardStatus_HU
	sideInfo.addDiscardAsHu(huCard)
	sideInfo.process = ProcessStatus_GAME_OVER
	curTurnSideInfo.deleteDiscard(preDiscard)
	sendRealPlayerCardListAfterProc(sideInfo.roomId, sideInfo.playerOid, curTurnSideInfo.playerOid)
}
