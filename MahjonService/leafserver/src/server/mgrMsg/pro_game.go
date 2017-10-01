package mgrMsg

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func SendLoginRet(errCode *pb.GS2CLoginRet_ErrorCode, playerInfo *pb.PlayerInfo, a gate.Agent) {
	log.Debug("msgHandler ==> SendLoginRet ==>开始[%v]", errCode)
	data := &pb.GS2CLoginRet{
		ErrorCode:  errCode,
		PlayerInfo: playerInfo,
	}
	a.WriteMsg(data)
}

func SendEnterGameRet(errCode *pb.GS2CEnterGameRet_ErrorCode, gameType *pb.GameType, roomId string, a gate.Agent) {
	log.Debug("msgHandler ==> SendEnterGameRet ==>开始")
	data := &pb.GS2CEnterGameRet{
		ErrorCode: errCode.Enum(),
		Type:      gameType.Enum(),
		RoomId:    proto.String(roomId),
	}
	a.WriteMsg(data)
}

func SendUpdateRoomMemberRet(players []*pb.PlayerInfo, roomId string) {
	log.Debug("msgHandler ==> SendUpdateRoomMemberRet ==>")

}
