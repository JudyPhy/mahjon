package mgrRoom

import (
	"server/mgrGame/mgrCard"
	"server/mgrGame/mgrSide"
	"server/pb"

	"time"

	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) gameStart() {
	log.Debug("gameStart")
	room.sendTurnToNext(room.curPlayerId, nil)
	log.Debug("gameStart=>sendTurnToNext")
	room.actionAfterDeal()
}

func (room *RoomInfo) actionAfterDeal() {
	sider := room.sideMap[room.curPlayerId]
	procList, ddcard := sider.CheckHPGbySelf()
	if sider.GetIsRobot() {
		timer := time.NewTimer(time.Second * 1)
		<-timer.C
		drawcard := sider.RobotDiscardAI()
		aftercards := sider.UpdateCardAfterDiscard(drawcard)
		room.sendBroadcastProc(room.curPlayerId, pb.ProcType_Proc_Discard, aftercards)
	} else {
		log.Debug("gameStart==>procList=%v", procList)
		sider.SendInterruptActionListBySelf(procList, ddcard)
	}
}

func (room *RoomInfo) checkHPGWhenDiscard(drawcard *mgrCard.CardInfo) {
	log.Debug("checkHPGWhenDiscard=v%", drawcard)
	for pid, sider := range room.sideMap {
		if pid != room.curPlayerId {
			procList := sider.CheckHPGWhenDiscard(drawcard)
			if len(procList) == 1 {
				sider.TackPassWhenDiscard()
			} else if sider.GetIsRobot() {
				sider.TackPassWhenDiscard()
			} else {
				sider.SendInterruptActionListWhenDiscard(procList, drawcard)
			}
		}
	}
	if room.checkTurnOver() {
		room.turnToNextByNomal()
	}
}

func (room *RoomInfo) checkTurnOver() bool {
	for _, sider := range room.sideMap {
		if sider.GetProcess() != mgrSide.ProcessStatus_TURN_OVER {
			return false
		}
	}
	return true
}

func (room *RoomInfo) turnToNextByNomal() {
	room.nomalTurnToNext()
	card := room.dealcards(room.curPlayerId, 1)
	room.sendTurnToNext(room.curPlayerId, card[0])
	room.actionAfterDeal()
}
