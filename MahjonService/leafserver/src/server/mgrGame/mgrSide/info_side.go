package mgrSide

import (
	"server/mgrGame/mgrCard"
	"server/mgrPlayer"
	"server/pb"

	"github.com/name5566/leaf/gate"
)

type SideInfo struct {
	isRobot  bool
	isOwner  bool
	side     pb.MahjonSide
	lackType pb.CardType
	agent    gate.Agent
	player   *mgrPlayer.PlayerInfo

	handCards []*mgrCard.CardInfo
	disCards  []*mgrCard.CardInfo
	process   ProcessStatus
}

type ProcessStatus int32

const (
	ProcessStatus_DEFAULT         ProcessStatus = 1
	ProcessStatus_EXCHANGE_OVER   ProcessStatus = 2
	ProcessStatus_LACK_OVER       ProcessStatus = 3
	ProcessStatus_TURN_START      ProcessStatus = 4
	ProcessStatus_TURN_START_OVER ProcessStatus = 5
	ProcessStatus_TURN_OVER       ProcessStatus = 6
	ProcessStatus_TURN_OVER_PENG  ProcessStatus = 7
	ProcessStatus_TURN_OVER_GANG  ProcessStatus = 8
	ProcessStatus_TURN_OVER_HU    ProcessStatus = 9
	ProcessStatus_WAITING_HU      ProcessStatus = 10
	ProcessStatus_PROC_HU         ProcessStatus = 11 //real player
	ProcessStatus_WAITING_GANG    ProcessStatus = 12
	ProcessStatus_WAITING_PENG    ProcessStatus = 13
	ProcessStatus_GAME_OVER       ProcessStatus = 14
)
