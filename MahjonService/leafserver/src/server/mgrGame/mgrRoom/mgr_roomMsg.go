package mgrRoom

import (
	"server/mgrGame/mgrCard"
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/log"
)

func (room *RoomInfo) broadcastRoom(msg interface{}) {
	for _, a := range room.agents {
		a.WriteMsg(msg)
	}
}

func (room *RoomInfo) SendUpdateRoomMemberRet() {
	players := make([]*pb.PlayerInfo, 0)
	for _, v := range room.sideMap {
		players = append(players, v.ToPbPlayerInfo())
	}
	data := &pb.GS2CUpdateRoomMember{
		Player: players,
	}
	room.broadcastRoom(data)
}

func (room *RoomInfo) sendUpdateStartBattleRet(cardListSum []*mgrCard.CardInfo) {
	log.Debug("sendUpdateStartBattleRet==>")
	cardInfoRet := make([]*pb.CardInfo, 0)
	for _, v := range cardListSum {
		cardInfoRet = append(cardInfoRet, v.ToPBCardInfo())
	}
	data := &pb.GS2CBattleStart{
		DealerId: proto.Int32(room.dealerId),
		CardList: cardInfoRet,
	}
	log.Debug("开局发牌cardInfoRet=%v", cardInfoRet)
	room.broadcastRoom(data)
}

func (room *RoomInfo) sendExchangeCardRet(exchangeType pb.ExchangeType, newCardList []*mgrCard.CardInfo) {
	cardInfoRet := make([]*pb.CardInfo, 0)
	for _, v := range newCardList {
		cardInfoRet = append(cardInfoRet, v.ToPBCardInfo())
	}
	log.Debug("换牌后cardInfoRet==> %v", cardInfoRet)
	data := &pb.GS2CExchangeCardRet{
		Type:     exchangeType.Enum(),
		CardList: cardInfoRet,
	}
	room.broadcastRoom(data)
}

func (room *RoomInfo) sendLackTypeRet() {
	typeList := make([]*pb.LackCard, 0)
	for pid, sider := range room.sideMap {
		tempData := &pb.LackCard{
			PlayerOID: proto.Int32(pid),
			Type:      sider.GetLackType().Enum(),
		}
		typeList = append(typeList, tempData)
	}
	data := &pb.GS2CSelectLackRet{
		LackCard: typeList,
	}
	room.broadcastRoom(data)
}

func (room *RoomInfo) sendTurnToNext(pid int32, drawcard *mgrCard.CardInfo) {
	log.Debug("sendTurnToNext=>pid=%v", pid)
	data := &pb.GS2CTurnToNext{
		PlayerOID: proto.Int32(pid),
	}
	if drawcard != nil {
		data.DrawCard = drawcard.ToPBCardInfo()
	}
	room.broadcastRoom(data)
}

func (room *RoomInfo) sendBroadcastProc(procPlayer int32, proc pb.ProcType, drawcards []*mgrCard.CardInfo) {
	log.Debug("sendBroadcastProc=>pid=%v", procPlayer)
	cardList := make([]*pb.CardInfo, 0)
	for _, card := range drawcards {
		cardList = append(cardList, card.ToPBCardInfo())
	}
	data := &pb.GS2CBroadcastProc{
		ProcPlayer: proto.Int32(procPlayer),
		ProcType:   proc.Enum(),
		CardList:   cardList,
	}
	room.broadcastRoom(data)
	if proc == pb.ProcType_Proc_Discard {
		room.checkHPGWhenDiscard(drawcards[0])
	}

}
