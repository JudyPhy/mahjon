package internal

import (
	"reflect"
	"server/pb"
	"server/player"

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
	onlinePlayer := &player.Player{}
	onlinePlayer.OID = 10000
	onlinePlayer.NickName = m.GetAccount()
	onlinePlayer.HeadIcon = "nil"
	onlinePlayer.Gold = 0
	onlinePlayer.Diamond = 0
	ret := player.AddChanPlayerInfo(a, onlinePlayer)

	//ret to client
	msg := &pb.GS2CLoginRet{}
	if ret {
		log.Error("the agent login success.")
		msg.ErrorCode = pb.GS2CLoginRet_SUCCESS.Enum()
		msg.PlayerInfo = onlinePlayer.ToPbPlayerInfo()
	} else {
		log.Error("the agent login fail.")
		msg.ErrorCode = pb.GS2CLoginRet_FAIL.Enum()
	}
	a.WriteMsg(msg)
}
