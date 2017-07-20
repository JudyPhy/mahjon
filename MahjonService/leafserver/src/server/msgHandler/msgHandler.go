package msgHandler

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func SendGS2CUpdateRoomInfo(playerList []*pb.BattlePlayerInfo, status *pb.GS2CUpdateRoomInfo_Status, a gate.Agent) {
	log.Debug("sendGS2CUpdateRoomInfo-->>")
	data := &pb.GS2CUpdateRoomInfo{}
	data.Player = playerList
	data.Status = status
	a.WriteMsg(data)
}

func SendGS2CBattleStart(dealerId int32, cardList []*pb.CardInfo, a gate.Agent) {
	log.Debug("SendGS2CBattleStart-->>")
	data := &pb.GS2CBattleStart{}
	data.DealerId = proto.Int32(dealerId)
	data.CardList = cardList
	a.WriteMsg(data)
}
