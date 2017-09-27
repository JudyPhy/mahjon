package mgrRoom

import (
	"server/pb"

	"math/rand"
	"time"

	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) getSide() pb.MahjonSide {
	leftSideList := room.getLeftSideList()
	if len(leftSideList) == 0 {
		return pb.MahjonSide_DEFAULT
	}
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(leftSideList))
	log.Debug("玩家方位: rnd=%d ,%v", rnd, leftSideList[rnd])
	return leftSideList[rnd]
}

func (room *RoomInfo) getLeftSideList() []pb.MahjonSide {
	log.Debug("getLeftSideList")
	sideList := []pb.MahjonSide{pb.MahjonSide_EAST, pb.MahjonSide_SOUTH, pb.MahjonSide_WEST, pb.MahjonSide_NORTH}
	for _, sideInRoom := range room.sideMap {
		for index, value := range sideList {
			if sideInRoom.GetSide() == value {
				sideList = append(sideList[:index], sideList[index+1:]...)
				break
			}
		}
	}
	log.Debug("剩余方位:%v", sideList)
	return sideList
}
