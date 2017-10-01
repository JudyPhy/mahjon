package mgrCard

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/log"
)

func (card *CardInfo) ToPBCardInfo() *pb.CardInfo {
	pbCardInfo := &pb.CardInfo{
		PlayerOID: proto.Int32(card.PlayerId),
		OID:       proto.Int32(card.Oid),
		ID:        proto.Int32(card.Id),
		Status:    card.transCardStatus(),
		FromOther: proto.Bool(false),
	}
	return pbCardInfo
}

func (card *CardInfo) transCardStatus() *pb.CardStatus {
	switch card.Status {
	case CardStatus_Wall:
		return pb.CardStatus_Wall.Enum()
	case CardStatus_InHand:
		return pb.CardStatus_InHand.Enum()
	case CardStatus_Deal:
		return pb.CardStatus_Deal.Enum()
	case CardStatus_Exchanged:
		log.Debug("卡牌状态仍为[Exchanged]:%v", card.Status)
		return pb.CardStatus_InHand.Enum()
	case CardStatus_DisCard:
		return pb.CardStatus_Dis.Enum()
	default:
		log.Debug("卡牌状态异常:%v", card.Status)
	}
	return nil
}

func GetCardIdList(cardList []*CardInfo) []int32 {
	var cardIdList []int32
	for _, card := range cardList {
		cardIdList = append(cardIdList, card.Id)
	}
	return cardIdList
}
