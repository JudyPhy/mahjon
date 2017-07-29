package msgHandler

import (
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

func SendGS2CEnterGameRet(errorCode *pb.GS2CEnterGameRet_ErrorCode, mode *pb.GameMode, roomId string, a gate.Agent) {
	log.Debug("SendGS2CEnterGameRet-->>")
	data := &pb.GS2CEnterGameRet{}
	data.ErrorCode = errorCode
	data.Mode = mode
	data.RoomId = proto.String(roomId)
	a.WriteMsg(data)
}

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

func SendGS2CExchangeCardRet(errorCode *pb.GS2CExchangeCardRet_ErrorCode, a gate.Agent) {
	log.Debug("SendGS2CExchangeCardRet-->>")
	data := &pb.GS2CExchangeCardRet{}
	data.ErrorCode = errorCode
	a.WriteMsg(data)
}

func SendGS2CUpdateCardInfoAfterExchange(exchangeType *pb.ExchangeType, cardList []*pb.CardInfo, a gate.Agent) {
	log.Debug("SendGS2CUpdateCardInfoAfterExchange-->>")
	data := &pb.GS2CUpdateCardInfoAfterExchange{}
	data.Type = exchangeType
	data.CardList = cardList
	a.WriteMsg(data)
}

func SendGS2CSelectLackRet(list []*pb.LackCard, a gate.Agent) {
	log.Debug("SendGS2CSelectLackRet-->>")
	data := &pb.GS2CSelectLackRet{}
	data.LackCard = list
	a.WriteMsg(data)
}

func SendGS2CUpdateCardList(list []*pb.CardInfo, a gate.Agent) {
	log.Debug("SendGS2CUpdateCardList-->>")
	data := &pb.GS2CUpdateCardList{}
	data.CardList = list
	a.WriteMsg(data)
}

func SendGS2CProcAni(playerOid int32, status *pb.CardStatus, a gate.Agent) {
	log.Debug("SendGS2CProcAni-->>")
	data := &pb.GS2CProcAni{}
	data.PlayerId = proto.Int32(playerOid)
	data.Status = status
	a.WriteMsg(data)
}

func SendGS2CDealCard(cardOid int32, a gate.Agent) {
	log.Debug("SendGS2CDealCard-->>")
	data := &pb.GS2CDealCard{}
	data.CardOid = proto.Int32(cardOid)
	a.WriteMsg(data)
}
