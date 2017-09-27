package mgrRoom

import (
	"server/mgrGame/mgrCard"
	"server/pb"

	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) ActionRetByPlayer(pid int32, procType pb.ProcType, drawcard *pb.CardInfo) {
	log.Debug("ActionRetByPlayer=>pid=%v", pid)
	sider := room.sideMap[pid]
	card := &mgrCard.CardInfo{
		Oid:       drawcard.GetOID(),
		Id:        drawcard.GetID(),
		PlayerId:  drawcard.GetPlayerOID(),
		FromOther: drawcard.GetFromOther(),
	}
	switch procType {
	case pb.ProcType_Proc_Discard:
		log.Debug("ActionRetByPlayer=>ProcType_Proc_Discard 111")
		card.Status = mgrCard.CardStatus_DisCard
		aftercards := sider.UpdateCardAfterDiscard(card)
		room.sendBroadcastProc(pid, procType, aftercards)
	}
}
