package mgrRoom

import (
	"server/mgrGame/mgrCard"
	"server/mgrGame/mgrSide"

	"github.com/name5566/leaf/gate"
)

func RegNewRoom(gtype string, roomId string) *RoomInfo {
	newRoom := &RoomInfo{
		gameType:  gtype,
		allCards:  mgrCard.LoadAllCards(gtype),
		agents:    make([]gate.Agent, 0),
		sideMap:   make(map[int32](*mgrSide.SideInfo)),
		roomId:    roomId,
		cardIndex: 0,
	}
	return newRoom
}
