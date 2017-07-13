package gate

import (
	"server/game"
	"server/login"
	"server/msg"
	"server/pb"
)

func init() {
	// 这里指定消息 Hello 路由到 game 模块
	msg.Processor.SetRouter(&pb.C2GSLogin{}, login.ChanRPC)
	msg.Processor.SetRouter(&pb.C2GSEnterGame{}, game.ChanRPC)
}
