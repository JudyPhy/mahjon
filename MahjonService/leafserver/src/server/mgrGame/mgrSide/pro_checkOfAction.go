package mgrSide

import (
	"server/mgrGame/mgrCard"
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/log"
)

func (sider *SideInfo) CheckHPGbySelf() ([]pb.ProcType, *pb.CardInfo) {
	log.Debug("CheckHPGbySelf")
	procList := make([]pb.ProcType, 0)
	drawcard := &pb.CardInfo{}
	cardList := sider.toInt32List()

	if mgrCard.CheckHu(cardList) {
		procList = append(procList, pb.ProcType_Proc_Hu)
		for _, huCard := range sider.handCards {
			if huCard.Status == mgrCard.CardStatus_Deal {
				drawcard.ID = proto.Int32(huCard.Id)
				drawcard.PlayerOID = proto.Int32(sider.player.GetPlayerId())
			}
		}
	}
	log.Debug("CheckHPGbySelf=>CheckHu=>playerOid=%v", drawcard.GetPlayerOID())
	gangCard, ok := mgrCard.CheckAnGang(cardList)
	if ok {
		procList = append(procList, pb.ProcType_Proc_Gang)
		drawcard.ID = proto.Int32(gangCard)
		drawcard.PlayerOID = proto.Int32(sider.player.GetPlayerId())
	}
	procList = append(procList, pb.ProcType_Proc_Discard)

	log.Debug("CheckHPGbySelf=>playerOid=%v", drawcard.GetPlayerOID())
	if len(procList) == 1 {
		return procList, nil
	}
	return procList, drawcard
}

func (sider *SideInfo) CheckHPGWhenDiscard(drawcard *mgrCard.CardInfo) []pb.ProcType {
	procList := make([]pb.ProcType, 0)
	cardList := sider.toInt32List()
	cardList2 := append(cardList, drawcard.Id)
	log.Debug("CheckHPGWhenDiscard=>")
	log.Debug("cardList=%v", cardList)
	log.Debug("cardList2=%v", cardList2)

	if mgrCard.CheckHu(cardList2) {
		procList = append(procList, pb.ProcType_Proc_Hu)
	}
	if mgrCard.CheckGang(cardList, drawcard.Id) {
		procList = append(procList, pb.ProcType_Proc_Gang)
	}
	if mgrCard.CheckPeng(cardList, drawcard.Id) {
		procList = append(procList, pb.ProcType_Proc_Peng)
	}
	procList = append(procList, pb.ProcType_Proc_Pass)

	if len(procList) > 1 {
		sider.process = ProcessStatus_WAITING_PENG
	}
	return procList
}
