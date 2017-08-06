package roomMgr

import (
	"server/msgHandler"
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/log"
)

//接口必须在摸牌后执行
func (sideInfo *SideInfo) playerTurnSwitch() {
	log.Debug("turn switch to real player%v", sideInfo.playerInfo.oid)
	inhandList := getInHandCardIdList(sideInfo.cardList)
	gList := getGangCardIdList(sideInfo.cardList)
	pList := getPengCardIdList(sideInfo.cardList)
	if IsHu(inhandList, gList, pList) {
		log.Debug("Hu player self, game over!")
		sideInfo.procSelfHuPlayAndRobot()
	} else {
		log.Debug("can't self hu, check self gang.")
		inhandAndPList := append(inhandList[:], pList[:]...)
		gangCardId := canGang(inhandAndPList, nil)
		if gangCardId != 0 {
			sideInfo.procSelfGangPlayerAndRobot()
		} else {
			log.Debug("can't self gang, send discard proc to client.")
			sendCanDiscardProc(sideInfo.playerInfo.roomId)
		}
	}
}

func (sideInfo *SideInfo) playerUpdateDiscardInfo(cardOid int32) *Card {
	isFind := false
	var card *Card
	for _, value := range sideInfo.cardList {
		if value.oid == cardOid {
			value.status = CardStatus_PRE_DISCARD
			sideInfo.process = ProcessStatus_TURN_OVER
			isFind = true
			card = value
			break
		}
	}
	if isFind {
		log.Debug("玩家[%v]出牌[%v(%v)]成功", sideInfo.playerInfo.oid, card.oid, card.id)
	} else {
		log.Debug("玩家出牌[%v]不在自己手牌中", cardOid)
	}
	return card
}

func (roomInfo *RoomInfo) playerEnsureProc(procPlayerOid int32, procType pb.ProcType, procCardId int32) {
	log.Debug("player%v select proc=%v, procCardId=%v", procPlayerOid, procType, procCardId)
	if procType == pb.ProcType_Peng {
		//peng
		roomInfo.playerEnsurePeng(procPlayerOid)
	} else if procType == pb.ProcType_HuOther {
		//hu other
		roomInfo.playerEnsureHuOther(procPlayerOid)
	} else if procType == pb.ProcType_SelfHu {
		//self hu
		roomInfo.playerEnsureSelfHu(procPlayerOid)
	} else if procType == pb.ProcType_GangOther {
		//gang other
		roomInfo.playerEnsureGang(procPlayerOid)
	} else if procType == pb.ProcType_SelfGang {
		//self gang
		roomInfo.playerEnsureSelfGang(procPlayerOid, procCardId)
	}
}

func (roomInfo *RoomInfo) playerEnsurePeng(procPlayerOid int32) {
	log.Debug("playerEnsurePeng")
	preDiscard := roomInfo.getPreDiscard()
	if preDiscard != nil {
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if procPlayerOid == sideInfo.playerInfo.oid {
				sideInfo.addDiscardAsPeng(preDiscard)
				sideInfo.process = ProcessStatus_TURN_OVER_PENG
			} else if sideInfo.playerInfo.oid == curTurnPlayerOid {
				sideInfo.deleteDiscard(preDiscard)
			}
		}

		roomInfo.allCardLog()

		//send update cards to client
		var cardList []*pb.CardInfo
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if sideInfo.playerInfo.oid == curTurnPlayerOid || procPlayerOid == sideInfo.playerInfo.oid {
				for _, value := range sideInfo.cardList {
					card := &pb.CardInfo{}
					card.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
					card.CardOid = proto.Int32(value.oid)
					card.CardId = proto.Int32(value.id)
					card.Status = cardStatusToPbCardStatus(value.status).Enum()
					card.FromOther = proto.Bool(value.fromOther)
					cardList = append(cardList, card)
				}
			}
		}
		log.Debug("need update card count=%v", len(cardList))
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CUpdateCardAfterPlayerProc(cardList, sideInfo.agent)
			}
		}
		//real player peng over, check turn over
		roomInfo.checkTurnOver()
	} else {
		log.Error("current pre discard is nil.")
	}
}

func (roomInfo *RoomInfo) playerEnsureHuOther(procPlayerOid int32) {
	log.Debug("playerEnsureHu")
	preDiscard := roomInfo.getPreDiscard()
	if preDiscard == nil {
		log.Debug("has robot hu, pre discard has been proc, get it from robot card list.")
		preDiscard = roomInfo.getHuStatusCard()
	}
	if preDiscard != nil {
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if procPlayerOid == sideInfo.playerInfo.oid {
				sideInfo.addDiscardAsHu(preDiscard)
				sideInfo.process = ProcessStatus_TURN_OVER_HU
			} else if sideInfo.playerInfo.oid == curTurnPlayerOid {
				sideInfo.deleteDiscard(preDiscard)
			}
		}

		roomInfo.allCardLog()

		//send update cards to client
		var cardList []*pb.CardInfo
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if sideInfo.playerInfo.oid == curTurnPlayerOid || procPlayerOid == sideInfo.playerInfo.oid {
				for _, value := range sideInfo.cardList {
					card := &pb.CardInfo{}
					card.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
					card.CardOid = proto.Int32(value.oid)
					card.CardId = proto.Int32(value.id)
					card.Status = cardStatusToPbCardStatus(value.status).Enum()
					card.FromOther = proto.Bool(value.fromOther)
					cardList = append(cardList, card)
				}
			}
		}
		log.Debug("need update card count=%v", len(cardList))
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CUpdateCardAfterPlayerProc(cardList, sideInfo.agent)
			}
		}
		//real player hu over, if has other player is processing, wait, otherwise check turn over.
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if sideInfo.process == ProcessStatus_PROC_HU {
				log.Debug("has player is processing hu, can't check turn over.")
				return
			}
		}
		roomInfo.checkTurnOver()
	} else {
		log.Error("when hu, pre discard is nil.")
	}
}

func (roomInfo *RoomInfo) playerEnsureSelfHu(procPlayerOid int32) {
	log.Debug("playerEnsureSelfHu")
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if procPlayerOid == sideInfo.playerInfo.oid {
			sideInfo.process = ProcessStatus_TURN_OVER_HU
		} else {
			sideInfo.process = ProcessStatus_TURN_OVER
		}
	}
	roomInfo.checkTurnOver()
}

func (roomInfo *RoomInfo) playerEnsureGang(procPlayerOid int32) {
	log.Debug("playerEnsureGang")
	preDiscard := roomInfo.getPreDiscard()
	if preDiscard != nil {
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if procPlayerOid == sideInfo.playerInfo.oid {
				sideInfo.addDiscardAsGang(preDiscard)
				sideInfo.process = ProcessStatus_TURN_OVER_HU
			} else if sideInfo.playerInfo.oid == curTurnPlayerOid {
				sideInfo.deleteDiscard(preDiscard)
			}
		}

		roomInfo.allCardLog()

		//send update cards to client
		var cardList []*pb.CardInfo
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if sideInfo.playerInfo.oid == curTurnPlayerOid || procPlayerOid == sideInfo.playerInfo.oid {
				for _, value := range sideInfo.cardList {
					card := &pb.CardInfo{}
					card.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
					card.CardOid = proto.Int32(value.oid)
					card.CardId = proto.Int32(value.id)
					card.Status = cardStatusToPbCardStatus(value.status).Enum()
					card.FromOther = proto.Bool(value.fromOther)
					cardList = append(cardList, card)
				}
			}
		}
		log.Debug("need update card count=%v", len(cardList))
		for _, sideInfo := range roomInfo.cardMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CUpdateCardAfterPlayerProc(cardList, sideInfo.agent)
			}
		}
		//real player gang over, turn switch
		roomInfo.checkTurnOver()
	} else {
		log.Error("player%v gang other%v, but can't find pre discard.", procPlayerOid, curTurnPlayerOid)
	}
}

func (roomInfo *RoomInfo) playerEnsureSelfGang(procPlayerOid int32, procCardId int32) {
	log.Debug("playerEnsureSelfGang")
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if procPlayerOid == sideInfo.playerInfo.oid {
			sideInfo.updateCardInfoBySelfGang(procCardId)
			sideInfo.process = ProcessStatus_TURN_OVER_GANG
		} else {
			sideInfo.process = ProcessStatus_TURN_OVER
		}
	}
	//send update cards to client
	var cardList []*pb.CardInfo
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if sideInfo.playerInfo.oid == procPlayerOid {
			for _, value := range sideInfo.cardList {
				card := &pb.CardInfo{}
				card.PlayerId = proto.Int32(sideInfo.playerInfo.oid)
				card.CardOid = proto.Int32(value.oid)
				card.CardId = proto.Int32(value.id)
				card.Status = cardStatusToPbCardStatus(value.status).Enum()
				card.FromOther = proto.Bool(value.fromOther)
				cardList = append(cardList, card)
			}
			break
		}
	}
	for _, sideInfo := range roomInfo.cardMap.cMap {
		if !sideInfo.isRobot && sideInfo.agent != nil {
			msgHandler.SendGS2CUpdateCardAfterPlayerProc(cardList, sideInfo.agent)
		}
	}
	roomInfo.checkTurnOver()
}
