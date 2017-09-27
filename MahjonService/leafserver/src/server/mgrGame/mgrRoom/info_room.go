package mgrRoom

import (
	"server/mgrGame/mgrCard"
	"server/mgrGame/mgrSide"

	"github.com/name5566/leaf/gate"
)

type RoomInfo struct {
	gameType string
	allCards []*mgrCard.CardInfo
	roomId   string

	agents  []gate.Agent
	sideMap map[int32](*mgrSide.SideInfo) //playerOID : SideInfo

	dealerId    int32
	curPlayerId int32
	cardIndex   int
}
