package mgrPlayer

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
)

func (player *PlayerInfo) ToPbPlayerInfo() *pb.PlayerInfo {
	result := &pb.PlayerInfo{}
	result.OID = proto.Int32(player.oid)
	result.NickName = proto.String(player.nickName)
	result.HeadIcon = proto.String(player.headIcon)
	result.Gold = proto.Int32(player.gold)
	result.Fangka = proto.Int32(player.fangka)
	return result
}

func (playerInfo *PlayerInfo) GetPlayerId() int32 {
	return playerInfo.oid
}
