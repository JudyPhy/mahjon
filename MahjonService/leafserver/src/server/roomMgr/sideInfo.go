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
func (sideInfo *SideInfo) robotTurnIn() {
	log.Debug("playerOid[%v] 's turn...", sideInfo.playerInfo.oid)
	if IsHu(sideInfo.cardList) {
		//胡牌，游戏结束
	} else {
		//未胡牌
		//判断杠牌
		inhandIdList := getInHandCardIdList(sideInfo.cardList)
		gangCardId := canGang(inhandIdList)
		if gangCardId != 0 {
			sendProcAni(sideInfo.playerInfo.roomId, sideInfo.playerInfo.oid, pb.CardStatus_beGang.Enum())
			sideInfo.procGang(gangCardId)
			return
		}
		//出牌
		discardOid := getRobotDiscardOid(sideInfo.cardList)
		for i := 0; i < len(sideInfo.cardList); i++ {
			if sideInfo.cardList[i].oid == discardOid {
				sideInfo.cardList[i].status = CardStatus_DEAL
				sendDiscard(sideInfo.playerInfo.roomId, discardOid)
				break
			}
		}
	}
}

func (sideInfo *SideInfo) procGang(gangCardId int) {
	log.Debug("playerOid[%v] procGang...", sideInfo.playerInfo.oid)
	var gangCards []*pb.CardInfo
	for i := 0; i < len(sideInfo.cardList); i++ {
		curCard := sideInfo.cardList[i]
		if curCard.id == int32(gangCardId) {
			curCard.status = CardStatus_GANG

			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
			card.CardOid = proto.Int32(curCard.oid)
			card.CardId = proto.Int32(curCard.id)
			card.Status = pb.CardStatus_beGang.Enum()
			card.FromOther = proto.Bool(curCard.fromOther)
			gangCards = append(gangCards, card)
		}
	}
	sendUpdateCardInfo(sideInfo.playerInfo.roomId, gangCards)
}
