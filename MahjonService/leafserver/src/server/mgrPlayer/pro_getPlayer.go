package mgrPlayer

import (
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func GetPlayerByAgent(a gate.Agent) *PlayerInfo {
	player, ok := a2pMap[a]
	if !ok {
		log.Debug("playerMgr==>GetPlayerByAgent==>客户端路由不存在")
		return nil
	}
	return player
}
