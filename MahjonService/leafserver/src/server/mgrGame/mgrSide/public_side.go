package mgrSide

import (
	"server/mgrGame/mgrCard"
	"server/pb"
)

func (sideInfo *SideInfo) GetSide() pb.MahjonSide {
	return sideInfo.side
}

func (sideInfo *SideInfo) GetIsRobot() bool {
	return sideInfo.isRobot
}

func (sideInfo *SideInfo) GetProcess() ProcessStatus {
	return sideInfo.process
}

func (sider *SideInfo) GetDiscards() []*mgrCard.CardInfo {
	return sider.disCards
}
