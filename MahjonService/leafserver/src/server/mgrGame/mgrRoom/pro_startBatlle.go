package mgrRoom

import (
	"time"

	"math/rand"

	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) WaitingRoomOk() {
	timer := time.NewTimer(time.Second * 1)
	over := false
	go func() {
		<-timer.C
		if over {
			return
		}
		if len(room.sideMap) < 4 {
			log.Debug("need add robot")
			room.addRobot2Room(4 - len(room.sideMap))
		}
		if len(room.sideMap) == 4 {
			room.startBattle()
			over = true
		}
	}()
}

func (room *RoomInfo) startBattle() {
	room.shuffle()
	room.setDealerId()

	cardListSum := room.dealStartBattle()
	log.Debug("roomMgr==>StartBattle==>牌数量=%v", len(cardListSum))
	room.sendUpdateStartBattleRet(cardListSum)
}

func (room *RoomInfo) setDealerId() {
	playerOidList := make([]int32, 0)
	for pid, _ := range room.sideMap {
		playerOidList = append(playerOidList, pid)
	}
	count := len(playerOidList)
	rand.Seed(time.Now().UnixNano())
	index := rand.Intn(count)
	room.dealerId = playerOidList[index]
	room.curPlayerId = room.dealerId
}
