package roomMgr

import (
	"server/pb"
	"sync"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type PlayerInfo struct {
	oid      int32
	nickName string
	headIcon string
	gold     int32
	diamond  int32
	roomId   string
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
	newPlayer := &PlayerInfo{}
	newPlayer.oid = player.GetOid()
	newPlayer.nickName = player.GetNickName()
	newPlayer.headIcon = player.GetHeadIcon()
	newPlayer.gold = player.GetGold()
	newPlayer.diamond = player.GetDiamond()
	newPlayer.roomId = ""
	return newPlayer
}

func AddChanPlayerInfo(a gate.Agent, player *PlayerInfo) {
	log.Debug("AddChanPlayerInfo")
	ChanPlayerStruct.lock.Lock()
	if _, ok := ChanPlayerStruct.aPlayerMap[a]; ok {
		log.Error("the agent has existed, don't need add to dict.")
	} else {
		log.Debug("add new player agent, player=", player.nickName)
		ChanPlayerStruct.aPlayerMap[a] = player
	}
	ChanPlayerStruct.lock.Unlock()
}

func DeleteChan(a gate.Agent) {
	log.Debug("DeleteChan")
	ChanPlayerStruct.lock.Lock()
	if _, ok := ChanPlayerStruct.aPlayerMap[a]; ok {
		if roomId := ChanPlayerStruct.aPlayerMap[a].roomId; roomId != "" {
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
