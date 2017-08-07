package roomMgr

import (
	"bytes"
	"server/card"
	"server/pb"
	"server/player"
	"server/robot"
	"strconv"

	"github.com/name5566/leaf/log"
)

type SideInfo struct {
	isRobot   bool
	isOwner   bool
	side      pb.BattleSide
	playerOid int32
	roomId    string
	cardList  []*card.Card
}

func (sideInfo *SideInfo) resetCardsData() {
	log.Debug("resetCardsData: player%v", sideInfo.playerOid)
	sideInfo.cardList = make([]*card.Card, 0)
}

func (sideInfo *SideInfo) selectLack() {
	typeCount := []int{0, 0, 0}
	for i, value := range sideInfo.cardList {
		if i == 0 {
		}
		if value.id > 0 && value.id < 10 {
			typeCount[0]++
		} else if value.id > 10 && value.id < 20 {
			typeCount[1]++
		} else if value.id > 20 && value.id < 30 {
			typeCount[2]++
		}
	}

	logStr := "type count: "
	buf := bytes.NewBufferString(logStr)
	for i := 0; i < len(typeCount); i++ {
		str := strconv.Itoa(typeCount[i])
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	countMin := 14
	typeIndex := 0
	for i := 0; i < len(typeCount); i++ {
		if typeCount[i] < countMin {
			typeIndex = i
			countMin = typeCount[i]
		}
	}

	if typeIndex == 0 {
		sideInfo.lackType = pb.CardType_Wan.Enum()
	} else if typeIndex == 1 {
		sideInfo.lackType = pb.CardType_Tiao.Enum()
	} else {
		sideInfo.lackType = pb.CardType_Tong.Enum()
	}
	log.Debug("playeroid[%v], lack type=%v", sideInfo.playerInfo.oid, sideInfo.lackType)
	sideInfo.process = ProcessStatus_LACK_OVER
}

func (sideInfo *SideInfo) procSelfHuPlayAndRobot() {
	log.Debug("player%v self hu", sideInfo.playerInfo.oid)
	curTurnPlayerSelfHu(sideInfo.playerInfo.roomId)
}

func (sideInfo *SideInfo) procSelfGangPlayerAndRobot() {
	log.Debug("player%v self gang", sideInfo.playerInfo.oid)
	curTurnPlayerSelfGang(sideInfo.playerInfo.roomId)
}

func (sideInfo *SideInfo) playerProcDiscard(discard *card.Card) {
	log.Debug("player:%v process discard:%v(%v)", sideInfo.playerInfo.oid, discard.oid, discard.id)
	if curTurnPlayerOid == sideInfo.playerInfo.oid {
		log.Debug("player self turn, don't need process.")
		return
	}
	if sideInfo.process == ProcessStatus_GAME_OVER {
		log.Debug("player self has game over.")
		return
	}
	handCard := getInHandCardIdList(sideInfo.cardList)
	handCard = append(handCard, int(discard.id))
	pList := getPengCardIdList(sideInfo.cardList)
	gList := getGangCardIdList(sideInfo.cardList)

	if IsHu(handCard, gList, pList) {
		log.Debug("player can Hu!")
		sideInfo.process = ProcessStatus_WAITING_HU
	} else {
		if canGang(handCard, discard) != 0 {
			log.Debug("player can Gang!")
			sideInfo.process = ProcessStatus_WAITING_GANG
		} else {
			if canPeng(handCard, discard) {
				log.Debug("player can Peng!")
				sideInfo.process = ProcessStatus_WAITING_PENG
			} else {
				log.Debug("player turn over.")
				sideInfo.process = ProcessStatus_TURN_OVER
			}
		}
	}
}

func (sideInfo *SideInfo) addDiscardAsPeng(card *card.Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的碰牌堆中", card.oid, card.id, sideInfo.playerInfo.oid)
	card.status = CardStatus_PENG
	card.fromOther = true
	sideInfo.cardList = append(sideInfo.cardList, card)
	for i := 0; i < 2; i++ {
		for _, value := range sideInfo.cardList {
			if value.status == CardStatus_INHAND && value.id == card.id {
				value.status = CardStatus_PENG
				break
			}
		}
	}
}

func (sideInfo *SideInfo) addDiscardAsGang(card *card.Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的杠牌堆中", card.oid, card.id, sideInfo.playerInfo.oid)
	card.status = CardStatus_GANG
	card.fromOther = true
	sideInfo.cardList = append(sideInfo.cardList, card)
	for i := 0; i < 3; i++ {
		for _, value := range sideInfo.cardList {
			if value.status == CardStatus_INHAND && value.id == card.id {
				value.status = CardStatus_GANG
				break
			}
		}
	}
}

func (sideInfo *SideInfo) deleteDiscard(card *card.Card) {
	log.Debug("将牌%v(%v)从玩家[%v]的牌堆中去除", card.oid, card.id, sideInfo.playerInfo.oid)
	for i, value := range sideInfo.cardList {
		if value.oid == card.oid {
			sideInfo.cardList = append(sideInfo.cardList[:i], sideInfo.cardList[i+1:]...)
			break
		}
	}
}

func (sideInfo *SideInfo) addDiscardAsHu(card *card.Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的胡牌中", card.oid, card.id, sideInfo.playerInfo.oid)
	card.status = CardStatus_HU
	card.fromOther = true
	sideInfo.cardList = append(sideInfo.cardList, card)
}

func (sideInfo *SideInfo) drawNewCard(newCard *card.Card) {
	log.Debug("切换操作方，摸牌%v(%v)", newCard.oid, newCard.id)
	sideInfo.cardList = append(sideInfo.cardList, newCard)
}

func (sideInfo *SideInfo) checkPengOk(discard *Card) bool {
	count := 0
	for _, card := range sideInfo.cardList {
		if card.status == CardStatus_INHAND && card.id == discard.id {
			count++
		}
	}
	return count >= 2
}

func (sideInfo *SideInfo) updateCardInfoBySelfGang(gCardId int32) {
	log.Debug("updateCardInfoBySelfGang")
	for _, card := range sideInfo.cardList {
		if card.status == CardStatus_INHAND && card.id == gCardId {
			card.status = CardStatus_GANG
		}
	}
}
