package gate

import (
	"server/game"
	"server/login"
	"server/msg"
	"server/pb"
)

func init() {

	msg.Processor.SetRouter(&pb.C2GSLogin{}, login.ChanRPC)

	msg.Processor.SetRouter(&pb.C2GSEnterGame{}, game.ChanRPC)
	msg.Processor.SetRouter(&pb.C2GSExchangeCard{}, game.ChanRPC)
	msg.Processor.SetRouter(&pb.C2GSSelectLack{}, game.ChanRPC)
	msg.Processor.SetRouter(&pb.C2GSInterruptActionRet{}, game.ChanRPC)

}
