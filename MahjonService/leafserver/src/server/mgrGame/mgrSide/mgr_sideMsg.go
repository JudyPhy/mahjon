package mgrSide

import (
	"server/mgrGame/mgrCard"
	"server/pb"

	"github.com/name5566/leaf/log"
)

func (sider *SideInfo) SendInterruptActionListBySelf(procList []pb.ProcType, drawcard *pb.CardInfo) {
	log.Debug("SendInterruptActionListBySelf=%v,sider=%v", procList, sider.player.GetPlayerId())
	data := &pb.GS2CInterruptAction{
		ProcList: procList,
		DrawCard: drawcard,
	}
	sider.agent.WriteMsg(data)
}

func (sider *SideInfo) SendInterruptActionListWhenDiscard(procList []pb.ProcType, drawcard *mgrCard.CardInfo) {
	data := &pb.GS2CInterruptAction{
		ProcList: procList,
		DrawCard: drawcard.ToPBCardInfo(),
	}
	sider.agent.WriteMsg(data)
}
