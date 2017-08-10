package roomMgr

import (
	"server/card"
	"server/pb"
	"time"

	"github.com/name5566/leaf/log"
)

func sendRobotProc(roomId string, procPlayer int32, procType pb.ProcType, beProcPlayer int32) {
	log.Debug("sendRobotProc, roomId=%v, procType=%v", roomId, procType)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendRobotProc(procPlayer, procType, beProcPlayer)
	} else {
		log.Error("sendRobotProc, no room[%v]", roomId)
	}
}

func sendRealPlayerProc(roomId string, procPlayer int32, procType pb.ProcType, beProcPlayer int32, procCardId int32) {
	log.Debug("sendRealPlayerProc, roomId=%v, procType=%v", roomId, procType)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendRealPlayerProc(procPlayer, procType, beProcPlayer, procCardId)
	} else {
		log.Error("sendRealPlayerProc, no room[%v]", roomId)
	}
}

func sendRealPlayerCardListAfterProc(roomId string, procPlayer int32, beProcPlayer int32) {
	log.Debug("sendRealPlayerCardListAfterProc, roomId=%v", roomId)
	pbCardList := make([]*pb.CardInfo, 0)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendRealPlayerCardListAfterProc(procPlayer, beProcPlayer)
	} else {
		log.Error("sendRealPlayerCardListAfterProc, no room[%v]", roomId)
	}
}

func broadcastRobotDiscard(roomId string, discard *card.Card) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.broadcastAndProcDiscard(discard)
		//after discard, wait 1 seconds for client ani
		timer := time.NewTimer(time.Second * 1)
		<-timer.C
		roomInfo.checkTurnOver()
	} else {
		log.Error("broadcastDiscard, no room[%v]", roomId)
	}
}

func turnToSelfAfterGang(roomId string, side pb.BattleSide) {
	log.Debug("turnToSelfAfterGang, roomId%v, side%v", roomId, side)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendNormalTurnToNext(side)
	} else {
		log.Error("turnToSelfAfterGang, no room[%v]", roomId)
	}
}

func turnToSelfAfterHu(roomId string, sideInfoList []*SideInfo) {
	log.Debug("turnToSelfAfterHu, roomId%v", roomId)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendHuTurnToNext(sideInfoList)
	} else {
		log.Error("turnToSelfAfterGang, no room[%v]", roomId)
	}
}

func realPlayerTurnToSelfAfterPeng(roomId string, playerOid int32) {
	log.Debug("realPlayerTurnToSelfAfterPeng, roomId%v, playerOid%v", roomId, playerOid)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendRealPlayerPengTurnToNext(playerOid)
	} else {
		log.Error("realPlayerTurnToSelfAfterPeng, no room[%v]", roomId)
	}
}

func checkHuOtherOver(roomId string, curHuOverPlayer int32) {
	log.Debug("checkHuOtherOver, roomId%v, curHuOverPlayer%v", roomId, curHuOverPlayer)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.checkHuOtherOver(curHuOverPlayer)
	} else {
		log.Error("checkHuOtherOver, no room[%v]", roomId)
	}
}

func turnToNextAfterHuOther(roomId string) {
	log.Debug("turnToNextAfterHuOther, roomId%v", roomId)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.checkHuOtherOver(curHuOverPlayer)
	} else {
		log.Error("checkHuOtherOver, no room[%v]", roomId)
	}
}
