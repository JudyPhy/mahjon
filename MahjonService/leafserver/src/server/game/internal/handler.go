package internal

import (
	"reflect"
	"server/mgrGame"
	"server/mgrMsg"
	"server/mgrPlayer"
	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func init() {
	handler(&pb.C2GSEnterGame{}, recvC2GSEnterGame)
	handler(&pb.C2GSExchangeCard{}, recvC2GSExchangeCard)
	handler(&pb.C2GSSelectLack{}, recvC2GSSelectLack)
	handler(&pb.C2GSInterruptActionRet{}, recvC2GSInterruptActionRet)
}

func handler(m interface{}, h interface{}) {
	skeleton.RegisterChanRPC(reflect.TypeOf(m), h)
}

func recvC2GSEnterGame(args []interface{}) {
	m := args[0].(*pb.C2GSEnterGame)
	a := args[1].(gate.Agent)
	log.Debug("recvC2GSEnterGame ==> gameMode= %v", m.GetMode())

	var errCode *pb.GS2CEnterGameRet_ErrorCode
	roomId := m.GetRoomId()
	switch m.GetMode() {
	case pb.EnterMode_CreateRoom:
		errCode, roomId = mgrGame.CreateRoomHandler(m.GetType().String(), a)
	case pb.EnterMode_JoinRoom:
		log.Debug("join room, roomId=%v", m.GetRoomId())
		errCode = mgrGame.JoinRoomHandler(m.GetRoomId(), a)
	case pb.EnterMode_QuickEnter:
		log.Debug("quick game")
	}
	mgrMsg.SendEnterGameRet(errCode, m.Type, roomId, a)
	if errCode.String() == "SUCCESS" {
		mgrGame.RoomMap[roomId].SendUpdateRoomMemberRet()
		mgrGame.RoomMap[roomId].WaitingRoomOk()
	}
}

func recvC2GSExchangeCard(args []interface{}) {
	m := args[0].(*pb.C2GSExchangeCard)
	a := args[1].(gate.Agent)
	log.Debug("recvC2GSExchangeCard=%v", m.GetCardOIDList())
	roomid := mgrGame.A2roomMap[a]
	player := mgrPlayer.GetPlayerByAgent(a)
	mgrGame.RoomMap[roomid].UpdateExchangeCard(m.GetCardOIDList(), player.GetPlayerId())
}

func recvC2GSSelectLack(args []interface{}) {
	m := args[0].(*pb.C2GSSelectLack)
	a := args[1].(gate.Agent)

	roomid := mgrGame.A2roomMap[a]
	player := mgrPlayer.GetPlayerByAgent(a)
	mgrGame.RoomMap[roomid].SubmitLackType(m.GetType(), player.GetPlayerId())
}

func recvC2GSInterruptActionRet(args []interface{}) {
	log.Debug("recvC2GSInterruptActionRet ==>")
	m := args[0].(*pb.C2GSInterruptActionRet)
	a := args[1].(gate.Agent)

	roomid := mgrGame.A2roomMap[a]
	player := mgrPlayer.GetPlayerByAgent(a)

	mgrGame.RoomMap[roomid].ActionRetByPlayer(player.GetPlayerId(), m.GetProcType(), m.GetDrawCard())

}
