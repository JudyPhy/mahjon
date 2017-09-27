package mgrRoom

import (
	"server/pb"

	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) nomalTurnToNext() {
	curSider, ok := room.sideMap[room.curPlayerId]
	if !ok {
		log.Debug("玩家%v不存在", room.curPlayerId)
	}
	curSider.SortHandCards()
	curSide := curSider.GetSide()
	nextsideId := pb.MahjonSide_value[curSide.String()] + 1
	if nextsideId == 6 {
		nextsideId = 2
	}
	nextSide := pb.MahjonSide_name[nextsideId]
	log.Debug("nomalTurnToNext==>curSide==> %v,nextSide==> %v", curSide, nextSide)

	for pid, sideinfo := range room.sideMap {
		if nextSide == sideinfo.GetSide().String() {
			room.curPlayerId = pid
		}
	}
}
