package roomMgr

import (
	"server/pb"
	"sync"

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

// ----------------------
// | chan | playerInfo |
// ----------------------
type ChanPlayer struct {
	lock       sync.Mutex
	aPlayerMap map[gate.Agent]*PlayerInfo
}

var ChanPlayerStruct *ChanPlayer

func NewPlayer(player *pb.PlayerInfo) *PlayerInfo {
	log.Debug("=============>NewPlayer:", player.GetOid())
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
	log.Debug("=============>AddChanPlayerInfo newPlayer oid:", player._oid)
	ChanPlayerStruct.lock.Lock()
	if _, ok := ChanPlayerStruct.aPlayerMap[a]; ok {
		log.Error("the agent has existed, don't need add to dict.")
	} else {
		log.Debug("add new player agent, player=", player._nickName)
		ChanPlayerStruct.aPlayerMap[a] = player
	}
	ChanPlayerStruct.lock.Unlock()
}

func DeleteChan(a gate.Agent) {
	log.Debug("DeleteChan")
	ChanPlayerStruct.lock.Lock()
	if _, ok := ChanPlayerStruct.aPlayerMap[a]; ok {
		if roomId := ChanPlayerStruct.aPlayerMap[a]._roomId; roomId != "" {
			OutRoom(roomId, a)
		}
		delete(ChanPlayerStruct.aPlayerMap, a)
	}
	ChanPlayerStruct.lock.Unlock()
}

func getPlayerBtAgent(a gate.Agent) *PlayerInfo {
	if _, ok := ChanPlayerStruct.aPlayerMap[a]; ok {
		return ChanPlayerStruct.aPlayerMap[a]
	} else {
		return nil
	}
}

func HasLogined(a gate.Agent) bool {
	ChanPlayerStruct.lock.Lock()
	if _, ok := ChanPlayerStruct.aPlayerMap[a]; ok {
		ChanPlayerStruct.lock.Unlock()
		return true
	} else {
		ChanPlayerStruct.lock.Unlock()
		return false
	}
}
