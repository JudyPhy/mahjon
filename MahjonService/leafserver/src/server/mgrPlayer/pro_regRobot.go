package mgrPlayer

import (
	"strconv"

	"github.com/name5566/leaf/log"
)

func RegRobotById(playerId int32) *PlayerInfo {
	log.Debug("playerMgr==>GetPlayerById==>创建机器人:机器人=%v", playerId)
	newRobot := &PlayerInfo{
		oid:      playerId,
		nickName: "yk" + strconv.Itoa(int(playerId)),
		headIcon: "",
		gold:     8888,
		fangka:  20,
	}
	return newRobot
}
