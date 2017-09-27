package mgrRoom

import (
	"server/mgrGame/mgrSide"
)

func (room *RoomInfo) addRobot2Room(num int) bool {
	for i := 0; i < num; i++ {
		if len(room.sideMap) == 4 {
			return false
		}
		sideCode := room.getSide()
		playerId := int32(20001 + i)
		room.sideMap[playerId] = mgrSide.RegSideByRobot(sideCode, playerId)
	}
	room.SendUpdateRoomMemberRet()
	return true
}
