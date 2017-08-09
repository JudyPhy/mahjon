package roomMgr

import (
	"server/card"
	"server/msgHandler"
	"server/pb"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/log"
)

func (roomInfo *RoomInfo) playerEnsurePeng(procPlayerOid int32) {
	log.Debug("playerEnsurePeng")
	preDiscard := roomInfo.getPreDiscard()
	if preDiscard != nil {
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
			if !sideInfo.isRobot && sideInfo.agent != nil {
				msgHandler.SendGS2CUpdateCardAfterPlayerProc(cardList, sideInfo.agent)
			}
		}
		//real player hu over, if has other player is processing, wait, otherwise check turn over.
		for _, sideInfo := range roomInfo.sideMap.cMap {
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

func (roomInfo *RoomInfo) playerEnsureGang(procPlayerOid int32) {
	log.Debug("playerEnsureGang")
	preDiscard := roomInfo.getPreDiscard()
	if preDiscard != nil {
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
		for _, sideInfo := range roomInfo.sideMap.cMap {
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
