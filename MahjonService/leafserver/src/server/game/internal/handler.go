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
		createRoomRet(a)
	case pb.GameMode_JoinRoom:
		log.Debug("join room, roomId=", m.GetRoomId())
		jointRoomRet(m.GetRoomId(), a)
	case pb.GameMode_QuickEnter:
		log.Debug("quick game")
	}
}

func createRoomRet(a gate.Agent) {
	// enter game ret
	ret := &pb.GS2CEnterGameRet{}
	ret.Mode = pb.GameMode_CreateRoom.Enum()
	roomId := roomMgr.ReqNewRoom(a)
	log.Debug("newRoomId=", roomId)
	ret.RoomId = proto.String(roomId)
	ret.ErrorCode = pb.GS2CEnterGameRet_SUCCESS.Enum()
	a.WriteMsg(ret)

	//test: send other 3 player info
	for i := 1; i < 4; i++ {
		jointRoomRet(roomId, nil)
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

func battleStart(roomId string) {
	log.Debug("battle start...")
	ret := &pb.GS2CBattleStart{}
	ret.DealerId = proto.Int32(roomMgr.GetDealerId())
}
