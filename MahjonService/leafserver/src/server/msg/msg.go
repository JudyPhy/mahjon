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
	Processor.Register(&pb.GS2CUpdateCardInfoAfterExchange{}) //7
	Processor.Register(&pb.C2GSSelectLack{})                  //8
	Processor.Register(&pb.GS2CSelectLackRet{})               //9
	Processor.Register(&pb.C2GSDiscard{})                     //10
	Processor.Register(&pb.GS2CDiscardRet{})                  //11
	Processor.Register(&pb.GS2CTurnToNext{})                  //12
	Processor.Register(&pb.GS2CRobotProc{})                   //13
	Processor.Register(&pb.C2GSRobotProcOver{})               //14
	Processor.Register(&pb.GS2CPlayerEnsureProc{})            //15
	Processor.Register(&pb.C2GSPlayerEnsureProcRet{})         //16
	Processor.Register(&pb.GS2CUpdateCardAfterPlayerProc{})   //17
	Processor.Register(&pb.GS2CGameOver{})                    //18
}
