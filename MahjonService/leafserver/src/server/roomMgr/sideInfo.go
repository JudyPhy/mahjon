package roomMgr

import (
	"bytes"
	"server/pb"
	"strconv"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type SideInfo struct {
	//player
	isRobot    bool
	agent      gate.Agent
	side       pb.BattleSide
	isOwner    bool
	playerInfo *PlayerInfo
	//card
	lackType *pb.CardType
	cardList []*Card
	process  ProcessStatus
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

//接口必须在摸牌后执行
func (sideInfo *SideInfo) robotTurnSwitch() {
	log.Debug("机器人收到切换操作方消息，进入自己操作过程")
	inhandList := getInHandCardIdList(sideInfo.cardList)
	gList := getGangCardIdList(sideInfo.cardList)
	pList := getPengCardIdList(sideInfo.cardList)
	if IsHu(inhandList, gList, pList) {
		log.Debug("胡牌，游戏结束")
	} else {
		//未胡牌
		log.Debug("判断杠牌")
		inhandIdList := getInHandCardIdList(sideInfo.cardList)
		gangCardId := canGang(inhandIdList, nil)
		if gangCardId != 0 {
			sideInfo.procGang(gangCardId)
			return
		}
		//出牌
		log.Debug("不能自杠，出牌")
		discard := getRobotDiscard(sideInfo.cardList)
		log.Debug("discard[%v]", discard.oid)
		for i := 0; i < len(sideInfo.cardList); i++ {
			if sideInfo.cardList[i].oid == discard.oid {
				sideInfo.cardList[i].status = CardStatus_PRE_DISCARD
				sideInfo.process = ProcessStatus_TURN_OVER
				sendDiscard(sideInfo.playerInfo.roomId, discard)
				break
			}
		}
	}
}

func cardStatusToPbCardStatus(status CardStatus) pb.CardStatus {
	switch status {
	case CardStatus_INHAND:
		return pb.CardStatus_inHand
	case CardStatus_GANG:
		return pb.CardStatus_beGang
	case CardStatus_PENG:
		return pb.CardStatus_bePeng
	case CardStatus_DISCARD:
		return pb.CardStatus_discard
	}
	return pb.CardStatus_noDeal
}

//机器人处理杠操作
func (sideInfo *SideInfo) procGang(gangCardId int) {
	log.Debug("机器人[%v]处理杠牌[%v]", sideInfo.playerInfo.oid, gangCardId)
	var newCardList []*pb.CardInfo
	for _, curCard := range sideInfo.cardList {
		if curCard.id == int32(gangCardId) {
			curCard.status = CardStatus_GANG
		}
		card := &pb.CardInfo{}
		card.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
		card.CardOid = proto.Int32(curCard.oid)
		card.CardId = proto.Int32(curCard.id)
		card.Status = cardStatusToPbCardStatus(curCard.status).Enum()
		newCardList = append(newCardList, card)
	}
	sendUpdateCardInfoByPG(sideInfo.playerInfo.roomId, newCardList, pb.CardStatus_beGang.Enum())
}

func (sideInfo *SideInfo) unpdateDiscard(cardOid int32) {
	isFind := false
	var card *Card
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
		sendDiscard(sideInfo.playerInfo.roomId, card)
	} else {
		log.Debug("玩家出牌[%v]不在自己手牌中", cardOid)
	}
}

//将被碰或杠的牌添加到对应玩家手牌中
func (sideInfo *SideInfo) addDiscardAsPG(card *Card) {
	log.Debug("将牌%v(%v)加入到玩家[%v]的碰杠牌堆中", card.oid, card.id, sideInfo.playerInfo.oid)
	card.status = CardStatus_PENG
	card.fromOther = true
	sideInfo.cardList = append(sideInfo.cardList, card)
}

//从当前操作方手牌中去除被碰或杠的牌
func (sideInfo *SideInfo) deleteDiscard(card *Card) {
	log.Debug("将牌%v(%v)从玩家[%v]的牌堆中去除", card.oid, card.id, sideInfo.playerInfo.oid)
	for i, value := range sideInfo.cardList {
		if value.oid == card.oid {
			sideInfo.cardList = append(sideInfo.cardList[:i], sideInfo.cardList[i+1:]...)
			break
		}
	}
}

func (sideInfo *SideInfo) drawNewCard(newCard *Card) {
	log.Debug("切换操作方，摸牌[%v]", newCard.oid)
	sideInfo.cardList = append(sideInfo.cardList, newCard)
}
