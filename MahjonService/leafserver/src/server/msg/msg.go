package msg

import (
	"server/pb"

	"github.com/name5566/leaf/network/protobuf"
)

var Processor = protobuf.NewProcessor()

func init() {
	Processor.Register(&pb.C2GSLogin{})                       //0
	Processor.Register(&pb.GS2CLoginRet{})                    //1
	Processor.Register(&pb.C2GSEnterGame{})                   //2
	Processor.Register(&pb.GS2CEnterGameRet{})                //3
	Processor.Register(&pb.GS2CUpdateRoomInfo{})              //4
	Processor.Register(&pb.GS2CBattleStart{})                 //5
	Processor.Register(&pb.C2GSExchangeCard{})                //6
	Processor.Register(&pb.GS2CExchangeCardRet{})             //7
	Processor.Register(&pb.GS2CUpdateCardInfoAfterExchange{}) //8
}
