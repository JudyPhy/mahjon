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

func SendGS2CDiscardRet(cardOid int32, a gate.Agent) {
	log.Debug("SendGS2CDiscardRet-->> cardOid=%v", cardOid)
	data := &pb.GS2CDiscardRet{}
	data.CardOid = proto.Int32(cardOid)
	a.WriteMsg(data)
}

func SendGS2CTurnToNext(playerOid int32, newCard *pb.CardInfo, turnType *pb.TurnSwitchType, a gate.Agent) {
	log.Debug("SendGS2CTurnToNext-->> playerOid=%v, type=%v", playerOid, turnType)
	if newCard != nil {
		log.Debug("newCard=%v(%v)", newCard.GetCardOid(), newCard.GetCardId())
	}
	data := &pb.GS2CTurnToNext{}
	data.PlayerOid = proto.Int32(playerOid)
	data.Card = newCard
	data.Type = turnType
	a.WriteMsg(data)
}

func SendGS2CRobotProc(procPlayerOid int32, procType *pb.ProcType, beProcPlayerOid int32, list []*pb.CardInfo, a gate.Agent) {
	log.Debug("SendGS2CRobotProc-->>")
	data := &pb.GS2CRobotProc{}
	data.ProcPlayer = proto.Int32(procPlayerOid)
	data.ProcType = procType
	data.BeProcPlayer = proto.Int32(beProcPlayerOid)
	data.CardList = list
	a.WriteMsg(data)
}

func SendGS2CPlayerEnsureProc(procPlayerOid int32, procType *pb.ProcType, beProcPlayerOid int32, procCardId int32, a gate.Agent) {
	log.Debug("SendGS2CPlayerEnsureProc-->>")
	data := &pb.GS2CPlayerEnsureProc{}
	data.ProcPlayer = proto.Int32(procPlayerOid)
	data.ProcType = procType
	data.BeProcPlayer = proto.Int32(beProcPlayerOid)
	data.ProcCardId = proto.Int32(procCardId)
	a.WriteMsg(data)
}

func SendGS2CUpdateCardAfterPlayerProc(cardList []*pb.CardInfo, a gate.Agent) {
	log.Debug("SendGS2CUpdateCardAfterPlayerProc-->>")
	data := &pb.GS2CUpdateCardAfterPlayerProc{}
	data.CardList = cardList
	a.WriteMsg(data)
}

func SendGS2CGameOver(a gate.Agent) {
	log.Debug("SendGS2CGameOver-->>")
	data := &pb.GS2CGameOver{}
	a.WriteMsg(data)
}
