package internal

import (
	"reflect"
	"server/pb"
	"server/roomMgr"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func init() {
	// 向当前模块（game 模块）注册 Hello 消息的消息处理函数 handleHello
	handler(&pb.C2GSEnterGame{}, recvC2GSEnterGame)
	handler(&pb.C2GSExchangeCard{}, recvC2GSExchangeCard)
}

func handler(m interface{}, h interface{}) {
	skeleton.RegisterChanRPC(reflect.TypeOf(m), h)
}

func recvC2GSEnterGame(args []interface{}) {
	m := args[0].(*pb.C2GSEnterGame)
	a := args[1].(gate.Agent)
	log.Debug("recvC2GSEnterGame => gameMode=", m.GetMode())
	switch m.GetMode() {
	case pb.GameMode_CreateRoom:
		log.Debug("create room")
		roomMgr.CreateRoomRet(a)
	case pb.GameMode_JoinRoom:
		log.Debug("join room, roomId=", m.GetRoomId())
		jointRoomRet(m.GetRoomId(), a)
	case pb.GameMode_QuickEnter:
		log.Debug("quick game")
	}
}

func jointRoomRet(roomId string, a gate.Agent) {
	result := roomMgr.JoinRoom(roomId, a)
	ret := &pb.GS2CEnterGameRet{}
	ret.Mode = pb.GameMode_JoinRoom.Enum()
	ret.RoomId = proto.String(roomId)
	ret.ErrorCode = result
	a.WriteMsg(ret)
}

func recvC2GSExchangeCard(args []interface{}) {
	m := args[0].(*pb.C2GSExchangeCard)
	a := args[1].(gate.Agent)
	roomMgr.UpdateExchangeCard(m, a)
}
