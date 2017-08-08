package internal

import (
	"reflect"
	"server/pb"
	"server/roomMgr"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func init() {
	// 向当前模块（game 模块）注册 Hello 消息的消息处理函数 handleHello
	handler(&pb.C2GSEnterGame{}, recvC2GSEnterGame)
	handler(&pb.C2GSExchangeCard{}, recvC2GSExchangeCard)
	handler(&pb.C2GSSelectLack{}, recvC2GSSelectLack)
	handler(&pb.C2GSDiscard{}, recvC2GSDiscard)
	handler(&pb.C2GSRobotProcOver{}, recvC2GSRobotProcOver)
	handler(&pb.C2GSPlayerEnsureProcRet{}, recvC2GSPlayerEnsureProcRet)
}

func handler(m interface{}, h interface{}) {
	skeleton.RegisterChanRPC(reflect.TypeOf(m), h)
}

func recvC2GSEnterGame(args []interface{}) {
	m := args[0].(*pb.C2GSEnterGame)
	a := args[1].(gate.Agent)
	log.Debug("recvC2GSEnterGame <<-- gameMode=%v", m.GetMode())
	switch m.GetMode() {
	case pb.GameMode_CreateRoom:
		log.Debug("create room")
		roomMgr.CreateRoomRet(a)
	case pb.GameMode_JoinRoom:
		log.Debug("join room, roomId=%v", m.GetRoomId())
		roomMgr.JoinRoomRet(m.GetRoomId(), a)
	case pb.GameMode_QuickEnter:
		log.Debug("quick game")
		roomMgr.QuickEnterRoomRet(a)
	}
}

func recvC2GSExchangeCard(args []interface{}) {
	log.Debug("recvC2GSExchangeCard <<--")
	m := args[0].(*pb.C2GSExchangeCard)
	a := args[1].(gate.Agent)
	roomMgr.UpdateExchangeCard(m.CardOidList, a)
}

func recvC2GSSelectLack(args []interface{}) {
	log.Debug("recvC2GSSelectLack <<--")
	m := args[0].(*pb.C2GSSelectLack)
	a := args[1].(gate.Agent)
	roomMgr.UpdateLackCard(m.GetType(), a)
}

func recvC2GSDiscard(args []interface{}) {
	log.Debug("recvC2GSDiscard <<--")
	m := args[0].(*pb.C2GSDiscard)
	a := args[1].(gate.Agent)
	roomMgr.UpdateDiscard(m.GetCardOid(), a)
}

func recvC2GSRobotProcOver(args []interface{}) {
	log.Debug("recvC2GSRobotProcOver <<--")
	m := args[0].(*pb.C2GSRobotProcOver)
	a := args[1].(gate.Agent)
	roomMgr.RobotProcOver(m.GetRobotOid(), m.GetProcType(), a)
}

func recvC2GSPlayerEnsureProcRet(args []interface{}) {
	log.Debug("recvC2GSPlayerEnsureProcRet <<--")
	m := args[0].(*pb.C2GSPlayerEnsureProcRet)
	a := args[1].(gate.Agent)
	roomMgr.PlayerEnsureProc(m.GetProcType(), m.GetProcCardId(), a)
}
