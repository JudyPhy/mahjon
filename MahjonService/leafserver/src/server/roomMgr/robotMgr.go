package roomMgr

import (
	"github.com/name5566/leaf/log"
)

func addRobotToRoom(roomId string, oid int) {
	log.Debug("addRobotToRoom roomId=%d", roomId)
	//PlayerInfo
	basePlayer := &PlayerInfo{}
	basePlayer.oid = int32(oid)
	basePlayer.nickName = "游客"
	basePlayer.headIcon = "nil"
	basePlayer.gold = 0
	basePlayer.diamond = 0
	basePlayer.roomId = roomId

	//roomPlayer
	sideList := getLeftSideList(roomId)
	side := getRandomSideBySideList(sideList)
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = true
	roomPlayer.agent = nil
	roomPlayer.side = side
	roomPlayer.isOwner = false
	roomPlayer.playerInfo = basePlayer

	//room
	log.Debug("prepare room info")
	if _, ok := RoomManager.roomMap[roomId]; ok {
		RoomManager.roomMap[roomId].playerList = append(RoomManager.roomMap[roomId].playerList, roomPlayer)
	} else {
		room := &RoomInfo{}
		room.playerList = append(room.playerList, roomPlayer)
		RoomManager.roomMap[roomId] = room
	}
}
