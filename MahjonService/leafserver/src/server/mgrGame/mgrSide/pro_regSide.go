package mgrSide

import (
	"server/mgrGame/mgrCard"
	"server/mgrPlayer"
	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func RegSideByPlayer(isowner bool, side pb.MahjonSide, a gate.Agent) *SideInfo {
	log.Debug("sideMgr==>RegSideOfPlayer==>玩家创建房间: isowner=%v", isowner)
	sideinfo := &SideInfo{
		isRobot: false,
		isOwner: isowner,
		side:    side,
		agent:   a,
		player:  mgrPlayer.GetPlayerByAgent(a),

		handCards: make([]*mgrCard.CardInfo, 0),
	}
	return sideinfo
}
