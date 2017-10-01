package mgrGame

import (
	"server/mgrGame/mgrRoom"
	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"

	"fmt"
	"math/rand"
	"time"
)

var (
	RoomMap   = make(map[string](*mgrRoom.RoomInfo))
	A2roomMap = make(map[gate.Agent]string)
)

func CreateRoomHandler(gtype string, a gate.Agent) (*pb.GS2CEnterGameRet_ErrorCode, string) {
	if len(RoomMap) >= 10000 {
		return pb.GS2CEnterGameRet_NO_EMPTY_ROOM.Enum(), ""
	}
	_, ok := A2roomMap[a]
	if ok {
		return pb.GS2CEnterGameRet_FAIL.Enum(), ""
	}
	rid := rndRoomId()
	room := mgrRoom.RegNewRoom(gtype, rid)
	log.Debug("enterGameHandler==>CreateRoom ==>roomid= %v", rid)
	RoomMap[rid] = room
	joinRet := JoinRoomHandler(rid, a)
	return joinRet, rid
}

func JoinRoomHandler(roomid string, a gate.Agent) *pb.GS2CEnterGameRet_ErrorCode {
	log.Debug("enterGameHandler==>JoinRoom==>roomid= %v", roomid)
	_, ok := A2roomMap[a]
	if ok {
		log.Debug("enterGameHandler==>JoinRoom==>玩家已在游戏中")
		return pb.GS2CEnterGameRet_FAIL.Enum()
	}
	room, hasRoom := RoomMap[roomid]
	if !hasRoom {
		log.Debug("enterGameHandler==>JoinRoom==>目标房间 v% 不存在", roomid)
		return pb.GS2CEnterGameRet_ROOM_NOT_EXIST.Enum()
	}
	ok = room.AddPlayer2Room(a)
	if !ok {
		return pb.GS2CEnterGameRet_PLAYER_COUNT_LIMITE.Enum()
	}

	A2roomMap[a] = roomid
	return pb.GS2CEnterGameRet_SUCCESS.Enum()
}

func rndRoomId() string {
	rnd := rand.New(rand.NewSource(time.Now().UnixNano()))
	roomId := fmt.Sprintf("%06v", rnd.Int31n(1000000))

	_, ok := RoomMap[roomId]
	if ok {
		log.Debug("房间%v已存在", roomId)
		return rndRoomId()
	}
	return roomId
}
