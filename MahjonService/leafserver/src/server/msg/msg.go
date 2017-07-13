package msg

import (
	"server/pb"

	"github.com/name5566/leaf/network/protobuf"
)

var Processor = protobuf.NewProcessor()

func init() {
	Processor.Register(&pb.C2GSLogin{})
	Processor.Register(&pb.GS2CLoginRet{})
	Processor.Register(&pb.C2GSEnterGame{})
	Processor.Register(&pb.GS2CEnterGameRet{})
	Processor.Register(&pb.GS2CUpdateRoomInfo{})
	Processor.Register(&pb.GS2CBattleStart{})
	Processor.Register(&pb.GS2CDiscardTimeOut{})
}
