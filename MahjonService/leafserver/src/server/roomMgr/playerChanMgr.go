package roomMgr

import (
	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type PlayerInfo struct {
	_oid      int32
	_nickName string
	_headIcon string
	_gold     int32
	_diamond  int32
	_roomId   string
}

// ------------------
// | chan | playerInfo |
// ------------------
var ChanPlayerDict map[gate.Agent]*PlayerInfo

func NewPlayer(player *pb.PlayerInfo) *PlayerInfo {
	newPlayer := &PlayerInfo{}
	newPlayer._oid = player.GetGold()
	newPlayer._nickName = player.GetNickName()
	newPlayer._headIcon = player.GetHeadIcon()
	newPlayer._gold = player.GetGold()
	newPlayer._diamond = player.GetDiamond()
	newPlayer._roomId = ""
	return newPlayer
}

func AddChanPlayerInfo(a gate.Agent, player *PlayerInfo) {
	log.Debug("AddChanPlayerInfo")
	if _, ok := ChanPlayerDict[a]; ok {
		log.Error("the agent has existed, don't need add to dict.")
	} else {
		log.Debug("add new player agent, player=", player._nickName)
		ChanPlayerDict[a] = player
	}
}

func DeleteChan(a gate.Agent) {
	log.Debug("DeleteChan")
	if _, ok := ChanPlayerDict[a]; ok {
		if roomId := ChanPlayerDict[a]._roomId; roomId != "" {
			OutRoom(roomId, a)
		}
		delete(ChanPlayerDict, a)
	}
}

func getPlayerBtAgent(a gate.Agent) *PlayerInfo {
	if _, ok := ChanPlayerDict[a]; ok {
		return ChanPlayerDict[a]
	} else {
		return nil
	}
}
