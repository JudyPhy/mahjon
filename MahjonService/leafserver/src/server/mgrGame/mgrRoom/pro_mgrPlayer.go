package mgrRoom

import (
	"server/mgrGame/mgrSide"
	"server/mgrPlayer"
	//	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) AddPlayer2Room(a gate.Agent) bool {
	if len(room.agents) >= 4 {
		return false
	}
	newplayer := mgrPlayer.GetPlayerByAgent(a)
	newPid := newplayer.GetPlayerId()
	isowner := (len(room.agents) == 0)
	room.agents = append(room.agents, a)
	newSide := mgrSide.RegSideByPlayer(isowner, room.getSide(), a)
	room.sideMap[newPid] = newSide
	log.Debug("roomMgr==>RoomAddPlayer==>玩家%v进入房间:", newPid)
	return true
}
