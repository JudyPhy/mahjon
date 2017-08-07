package internal

import (
	"reflect"
	"server/pb"
	"server/player"
	"server/roomMgr"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type testAgent int

func (t *testAgent) SetUserData() {

}

func handleMsg(m interface{}, h interface{}) {
	skeleton.RegisterChanRPC(reflect.TypeOf(m), h)
}

func init() {
	handleMsg(&pb.C2GSLogin{}, recvC2GSLogin)
}

func recvC2GSLogin(args []interface{}) {
	m := args[0].(*pb.C2GSLogin)
	log.Debug("recvC2GSLogin <<-- account=%v", m.GetAccount())
	a := args[1].(gate.Agent)

	// get data from db
	player := &player.Player{}
	player.oid = 10000
	player.nickName = m.GetAccount()
	player.headIcon = "nil"
	player.gold = 0
	player.diamond = 0
	ret := realPlayer.AddChanPlayerInfo(a, player)

	//ret to client
	msg := &pb.GS2CLoginRet{}
	if ret {
		log.Error("the agent login success.")
		msg.ErrorCode = pb.GS2CLoginRet_SUCCESS.Enum()
		msg.PlayerInfo = player.ToPbPlayerInfo()
	} else {
		log.Error("the agent login fail.")
		msg.ErrorCode = pb.GS2CLoginRet_FAIL.Enum()
	}
	a.WriteMsg(msg)
}
