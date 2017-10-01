package mgrRoom

import (
	"server/mgrGame/mgrSide"
	"server/mgrPlayer"
	"server/pb"

	//	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) SubmitLackType(lackType pb.CardType, pid int32) {
	sider := room.sideMap[pid]
	sider.SetLackType(lackType)
	if room.checkLackOver() {
		for _, sider := range room.sideMap {
			if sider.GetIsRobot() {
				sider.SelectRobotLackType()
			}
		}
		room.sendLackTypeRet()
		room.gameStart()
	}
}

func (room *RoomInfo) checkLackOver() bool {
	for _, a := range room.agents {
		pid := mgrPlayer.GetPlayerByAgent(a).GetPlayerId()
		if room.sideMap[pid].GetProcess() != mgrSide.ProcessStatus_LACK_OVER {
			return false
		}
	}
	return true
}
