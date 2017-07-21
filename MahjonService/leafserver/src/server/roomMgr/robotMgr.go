package roomMgr

import (
	"github.com/name5566/leaf/log"
)

func addRobotToRoom(roomInfo *RoomInfo, oid int) {
	log.Debug("addRobotToRoom roomId=%d", roomInfo.roomId)
	//PlayerInfo
	basePlayer := &PlayerInfo{}
	basePlayer.oid = int32(oid)
	basePlayer.nickName = "游客"
	basePlayer.headIcon = "nil"
	basePlayer.gold = 0
	basePlayer.diamond = 0
	basePlayer.roomId = roomInfo.roomId

	//roomPlayer
	sideList := getLeftSideList(roomInfo)
	side := getRandomSideBySideList(sideList)
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = true
	roomPlayer.agent = nil
	roomPlayer.side = side
	roomPlayer.isOwner = false
	roomPlayer.playerInfo = basePlayer
	roomInfo.playerList = append(roomInfo.playerList, roomPlayer)
}
