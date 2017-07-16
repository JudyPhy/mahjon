package roomMgr

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/log"
)

func AddRobotToRoom(roomId string) {
	log.Debug("AddRobotToRoom roomId=%d", roomId)
	//pbPlayerInfo
	pbPlayer := &pb.PlayerInfo{}
	pbPlayer.Oid = proto.Int(10000 + len(Rooms.roomMap[roomId].playerList))
	pbPlayer.NickName = proto.String("电脑")
	pbPlayer.HeadIcon = proto.String("")
	pbPlayer.Gold = proto.Int(0)
	pbPlayer.Diamond = proto.Int(0)
	//battlePlayerInfo
	battlePlayer := &pb.BattlePlayerInfo{}
	sideList := getLeftSideList(roomId)
	battlePlayer.Side = getRandomSideBySideList(sideList)
	battlePlayer.IsOwner = proto.Bool(false)
	battlePlayer.Player = pbPlayer

	//roomPlayer
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = true
	roomPlayer.agent = nil
	roomPlayer.player = battlePlayer

	//room
	log.Debug("prepare room info")
	if _, ok := Rooms.roomMap[roomId]; ok {
		Rooms.roomMap[roomId].playerList = append(Rooms.roomMap[roomId].playerList, roomPlayer)
	} else {
		room := &RoomInfo{}
		room.playerList = append(room.playerList, roomPlayer)
		Rooms.roomMap[roomId] = room
	}

	// send update room playr event
	log.Debug("send add room player info to client")
	data := &pb.GS2CUpdateRoomInfo{}
	data.Player = append(data.Player, battlePlayer)
	data.Status = pb.GS2CUpdateRoomInfo_ADD.Enum()
	log.Debug("current plater count in room:", len(Rooms.roomMap[roomId].playerList))
	for n, value := range Rooms.roomMap[roomId].playerList {
		log.Debug("n=", n)
		if !value.isRobot && value.agent != nil {
			value.agent.WriteMsg(data)
		}
	}
}
