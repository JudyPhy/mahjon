package internal

import (
	"reflect"
	"server/pb"
	"server/roomMgr"

	"github.com/golang/protobuf/proto"
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
	log.Debug("recvC2GSLogin")
	m := args[0].(*pb.C2GSLogin)
	log.Debug("Account=", m.GetAccount())
	a := args[1].(gate.Agent)

	// get data from db
	var playerOid int32 = 1
	player := &pb.PlayerInfo{
		Oid:      proto.Int32(playerOid),
		NickName: proto.String(m.GetAccount()),
		HeadIcon: proto.String(""),
		Gold:     proto.Int32(99),
		Diamond:  proto.Int32(100)}

	chanPlayer := roomMgr.NewPlayer(player)
	roomMgr.AddChanPlayerInfo(a, chanPlayer)

	//ret to client
	if _, ok := roomMgr.ChanPlayerDict[a]; !ok {
		ret := &pb.GS2CLoginRet{
			ErrorCode:  pb.GS2CLoginRet_SUCCESS.Enum(),
			PlayerInfo: player}
		a.WriteMsg(ret)
	} else {
		log.Error("the agent has logined.")
	}
}
