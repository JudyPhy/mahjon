package internal

import (
	"reflect"

	"server/mgrDB"
	"server/mgrMsg"
	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func handleMsg(m interface{}, h interface{}) {
	skeleton.RegisterChanRPC(reflect.TypeOf(m), h)
}

func init() {
	handleMsg(&pb.C2GSLogin{}, recvC2GSLogin)
}

func recvC2GSLogin(args []interface{}) {

	m := args[0].(*pb.C2GSLogin)
	a := args[1].(gate.Agent)
	log.Debug("login=>recvC2GSLogin=> Account=%v,Password=%v", m.GetAccount(), m.GetPassword())

	errCode, player := mgrDB.LoginHandler(m.GetAccount(), m.GetPassword(), a)
	mgrMsg.SendLoginRet(errCode, player, a)
}
