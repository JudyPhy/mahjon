package roomMgr

import (
	"server/card"
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type SideInfo struct {
	isRobot   bool
	isOwner   bool
	side      pb.BattleSide
	playerOid int32
	roomId    string
	agent     gate.Agent
	lackType  pb.CardType
	cardList  []*card.Card
	process   ProcessStatus
}

func (sideInfo *SideInfo) ToPbBattlePlayerInfo() *pb.BattlePlayerInfo {
	ret := &pb.BattlePlayerInfo{}
	ret.Side = sideInfo.side.Enum()
	ret.IsOwner = proto.Bool(sideInfo.isOwner)
	ret.Player = &pb.PlayerInfo{}
	ret.Player.Oid = proto.Int32(sideInfo.playerOid)
	return ret
}

func (sideInfo *SideInfo) resetCardsData() {
	log.Debug("resetCardsData: player%v", sideInfo.playerOid)
	sideInfo.cardList = make([]*card.Card, 0)
}

func (sideInfo *SideInfo) playerProcDiscard(curTurnPlayerOid int32, discard *card.Card) {
	log.Debug("player:%v process discard:%v(%v)", sideInfo.playerOid, discard.OID, discard.ID)
	if curTurnPlayerOid == sideInfo.playerOid {
		log.Debug("player self turn, don't need process.")
		return
	}
	if sideInfo.process == ProcessStatus_GAME_OVER {
		log.Debug("player self has game over.")
		return
	}
	handCard := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_INHAND)
	handCard = append(handCard, int(discard.ID))
	dealCard := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_DEAL)
	pList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_PENG)
	gList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_GANG)

	if card.IsHu(dealCard, handCard, gList, pList) {
		log.Debug("player can Hu!")
		sideInfo.process = ProcessStatus_WAITING_HU
	} else {
		inhandDealList := append(handCard[:], dealCard[:]...)
		if card.CanGangOther(inhandDealList, discard) {
			log.Debug("player can Gang!")
			sideInfo.process = ProcessStatus_WAITING_GANG
		} else {
			if card.CanPeng(inhandDealList, discard) {
				log.Debug("player can Peng!")
				sideInfo.process = ProcessStatus_WAITING_PENG
			} else {
				log.Debug("player turn over.")
				sideInfo.process = ProcessStatus_TURN_OVER
			}
		}
	}
}

func (sideInfo *SideInfo) refreshCard() {
	log.Debug("refreshCard")
	for _, curCard := range sideInfo.cardList {
		if curCard.Status == card.CardStatus_DEAL {
			curCard.Status = card.CardStatus_INHAND
		}
	}
}

func (sideInfo *SideInfo) drawNewCard(newCard *card.Card) {
	log.Debug("切换操作方，摸牌%v(%v)", newCard.OID, newCard.ID)
	sideInfo.cardList = append(sideInfo.cardList, newCard)
}

func (sideInfo *SideInfo) addDiscardAsPeng(discard *card.Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的碰牌堆中", discard.OID, discard.ID, sideInfo.playerOid)
	discard.Status = card.CardStatus_PENG
	discard.FromOther = true
	sideInfo.cardList = append(sideInfo.cardList, discard)
	for i := 0; i < 2; i++ {
		for _, value := range sideInfo.cardList {
			if value.Status == card.CardStatus_INHAND && value.ID == discard.ID {
				value.Status = card.CardStatus_PENG
				break
			}
		}
	}
}

func (sideInfo *SideInfo) addDiscardAsGang(discard *card.Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的杠牌堆中", discard.OID, discard.ID, sideInfo.playerOid)
	discard.Status = card.CardStatus_GANG
	discard.FromOther = true
	sideInfo.cardList = append(sideInfo.cardList, discard)
	for i := 0; i < 3; i++ {
		for _, value := range sideInfo.cardList {
			if value.Status == card.CardStatus_INHAND && value.ID == discard.ID {
				value.Status = card.CardStatus_GANG
				break
			}
		}
	}
}

func (sideInfo *SideInfo) deleteDiscard(discard *card.Card) {
	log.Debug("将牌%v(%v)从玩家[%v]的牌堆中去除", discard.OID, discard.ID, sideInfo.playerOid)
	for i, value := range sideInfo.cardList {
		if value.OID == discard.OID {
			sideInfo.cardList = append(sideInfo.cardList[:i], sideInfo.cardList[i+1:]...)
			break
		}
	}
}

func (sideInfo *SideInfo) addDiscardAsHu(discard *card.Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的胡牌中", discard.OID, discard.ID, sideInfo.playerOid)
	discard.Status = card.CardStatus_HU
	discard.FromOther = true
	sideInfo.cardList = append(sideInfo.cardList, discard)
}

func (sideInfo *SideInfo) checkPengOk(discard *card.Card) bool {
	count := 0
	for _, curCard := range sideInfo.cardList {
		if curCard.Status == card.CardStatus_INHAND && curCard.ID == discard.ID {
			count++
		}
	}
	return count >= 2
}

func (sideInfo *SideInfo) updateCardInfoBySelfGang(gCardId int32) {
	log.Debug("updateCardInfoBySelfGang")
	for _, curCard := range sideInfo.cardList {
		if curCard.Status == card.CardStatus_INHAND && curCard.ID == gCardId {
			curCard.Status = card.CardStatus_GANG
		}
	}
}
