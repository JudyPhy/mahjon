package mgrSide

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
)

func (sideInfo *SideInfo) ToPbPlayerInfo() *pb.PlayerInfo {
	result := sideInfo.player.ToPbPlayerInfo()
	result.Side = sideInfo.side.Enum()
	result.IsOwner = proto.Bool(sideInfo.isOwner)
	return result
}

func (sider *SideInfo) toInt32List() []int32 {
	cardList := make([]int32, 0)
	for _, card := range sider.handCards {
		cardList = append(cardList, card.Id)
	}
	return cardList
}
