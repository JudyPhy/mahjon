package roomMgr

import (
	"bytes"
	"math/rand"
	"server/card"
	"server/pb"
	"strconv"
	"time"

	"github.com/name5566/leaf/log"
)

func (sideInfo *SideInfo) robotSelectExchangeCard() {
	log.Debug("robot% select exchange cards", sideInfo.playerOid)
	//策略：选取数量最少的花色随机挑选3张牌交换
	mapCard := getSeparateCardTypeMap(sideInfo.cardList)
	countList := []int{len(mapCard[0]), len(mapCard[1]), len(mapCard[2])}
	countMin := 14
	indexMin := 0
	for i, count := range countList {
		if count >= 3 && count < countMin {
			countMin = count
			indexMin = i
		}
	}
	log.Debug("robot req exchange card type is %v(0,1,2)", indexMin)
	typeCardList := mapCard[indexMin]
	exchangeList := make([]int32, 0)
	for i := 0; i < 3; i++ {
		rand.Seed(time.Now().Unix())
		rnd := rand.Intn(len(typeCardList))
		exchangeList = append(exchangeList, typeCardList[rnd])
		typeCardList = append(typeCardList[:rnd], typeCardList[rnd+1:]...)
	}
	for _, oid := range exchangeList {
		for _, card := range sideInfo.cardList {
			if card.oid == oid {
				card.status = CardStatus_EXCHANGE
				break
			}
		}
	}
	sideInfo.process = ProcessStatus_EXCHANGE_OVER

	//log
	logStr := "robot"
	buf := bytes.NewBufferString(logStr)
	buf.Write([]byte(strconv.Itoa(sideInfo.playerOid)))
	buf.Write([]byte(" exchange card oid list =>"))
	for _, card := range sideInfo.cardList {
		if card.status == CardStatus_EXCHANGE {
			str := strconv.Itoa(int(card.oid))
			buf.Write([]byte(str))
			buf.Write([]byte(", "))
		}
	}
	log.Debug(buf.String())
}

func (sideInfo *SideInfo) robotSelectLackCard() {
	log.Debug("robot%v select lack", sideInfo.playerOid)
	//策略：选取数量最少的花色作为缺的一门
	mapCard := getSeparateCardTypeMap(sideInfo.cardList)
	countList := []int{len(mapCard[0]), len(mapCard[1]), len(mapCard[2])}
	countMin := 14
	indexMin := 0
	for i, count := range countList {
		if count < countMin {
			countMin = count
			indexMin = i
		}
	}
	sideInfo.lackType = getLackTypeByIndex(indexMin)
	log.Debug("robot%v lack type is %v", sideInfo.playerOid, sideInfo.lackType)
	sideInfo.process = ProcessStatus_LACK_OVER
}

func getLackTypeByIndex(index int) pb.CardType {
	if index == 0 {
		return pb.CardType_Wan
	} else if index == 1 {
		return pb.CardType_Tiao
	} else if index == 2 {
		return pb.CardType_Tong
	}
	return pb.CardType_None
}

//将列表中的牌按照花色分开，分装到一个map中
func getSeparateCardTypeMap(list []*card.Card) map[int][]int32 {
	resultMap := make(map[int][]int32) // type(0/1/2)  : cardIdList
	var listWan []int32
	var listTiao []int32
	var listTong []int32
	for _, card := range list {
		if card.id > 0 && card.id < 10 {
			listWan = append(listWan, card.oid)
		} else if card.id > 10 && card.id < 20 {
			listTiao = append(listTiao, card.oid)
		} else if card.id > 20 && card.id < 30 {
			listTong = append(listTong, card.oid)
		}
	}
	resultMap[0] = listWan
	resultMap[1] = listTiao
	resultMap[2] = listTong
	return resultMap
}

//接口必须在摸牌后执行
func (sideInfo *SideInfo) robotTurnSwitch() {
	log.Debug("turn switch to robot%v, wait for 1 second.", sideInfo.playerInfo.oid)
	timer := time.NewTimer(time.Second * 1)
	<-timer.C

	inhandList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_INHAND)
	dealList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_DEAL)
	gList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_GANG)
	pList := card.GetCardIdListByStatus(sideInfo.cardList, card.CardStatus_PENG)
	if card.IsHu(dealList, inhandList, gList, pList) {
		log.Debug("robot%v self Hu, game over!", sideInfo.playerOid)
		sendRobotProc(sideInfo.roomId, sideInfo.playerOid, pb.ProcType_SelfHu, 0)
	} else {
		log.Debug("can't self hu, check self gang.")
		inhandPDList := append(inhandList[:], dealList[:]...)
		inhandPDList = append(inhandPDList[:], pList[:]...)
		if card.CanSelfGang(inhandPDList) {
			sideInfo.robotProcSelfGang()
		} else {
			log.Debug("can't self gang, proc discard")
			sideInfo.robotDiscard()
		}
	}
}

func (sideInfo *SideInfo) robotProcSelfGang() {
	log.Debug("robotProcSelfGang")
	dict := make(map[int32]int)
	for _, card := range sideInfo.cardList {
		if card.status == card.CardStatus_INHAND || card.status == card.CardStatus_PENG || card.status == card.CardStatus_DEAL {
			_, ok := dict[card.id]
			if ok {
				dict[card.id]++
			} else {
				dict[card.id] = 1
			}
		}
	}
	gCardId := int32(0)
	for id, count := range dict {
		if count == 4 {
			gCardId = id
			break
		}
	}
	if gCardId == 0 {
		log.Error("player%v has no self gang card!", sideInfo.playerOid)
	} else {
		for _, card := range sideInfo.cardList {
			if card.id == gCardId {
				card.status = CardStatus_GANG
			}
		}
		sendRobotProc(sideInfo.roomId, sideInfo.playerOid, pb.ProcType_SelfGang, 0)
	}
}

func (sideInfo *SideInfo) getRobotDiscard() *card.Card {
	var ableDiscardList []*Card
	for _, curCard := range sideInfo.cardList {
		if curCard.status == CardStatus_INHAND {
			ableDiscardList = append(ableDiscardList, curCard)
		}
	}
	log.Debug("robot inhand card count=%v", len(ableDiscardList))
	//first find lack card
	lackCardList := make([]*Card, 0)
	for _, curCard := range ableDiscardList {
		if curCard.id > 0 && curCard.id < 10 && sideInfo.lackType == pb.CardType_Wan.Enum() {
			lackCardList = append(lackCardList, curCard)
		} else if curCard.id > 10 && curCard.id < 20 && sideInfo.lackType == pb.CardType_Tiao.Enum() {
			lackCardList = append(lackCardList, curCard)
		} else if curCard.id > 20 && curCard.id < 30 && sideInfo.lackType == pb.CardType_Tong.Enum() {
			lackCardList = append(lackCardList, curCard)
		}
	}
	if len(lackCardList) > 0 {
		return lackCardList[0]
	}

	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(ableDiscardList))
	return ableDiscardList[rnd]
}

func (sideInfo *SideInfo) robotDiscard() {
	discard := sideInfo.getRobotDiscard()
	log.Debug("robot%v 出牌%v(%v)", discard.oid, discard.id)
	isFind := false
	for n, card := range sideInfo.cardList {
		if card.oid == discard.oid {
			card.status = CardStatus_PRE_DISCARD
			sideInfo.cardList = append(sideInfo.cardList[:n], sideInfo.cardList[n+1:]...)
			sideInfo.cardList = append(sideInfo.cardList, card)
			sideInfo.process = ProcessStatus_TURN_OVER
			broadcastRobotDiscard(sideInfo.roomId, discard)
			isFind = true
			break
		}
	}
	if !isFind {
		log.Error("robot%v discard%v is not in it's cardList.", sideInfo.playerOid, discard.oid)
	}
}

func (sideInfo *SideInfo) robotProcOver(procType pb.ProcType) {
	if procType == pb.ProcType_Peng {
		log.Debug("robot peng over, turn switch.")
		//peng
		sideInfo.process = ProcessStatus_TURN_OVER_PENG
	} else if procType == pb.ProcType_HuOther {
		log.Debug("robot hu over, wait check turn over.")
		//hu other
		sideInfo.process = ProcessStatus_TURN_OVER_HU
	} else if procType == pb.ProcType_SelfHu {
		//self hu
		sideInfo.process = ProcessStatus_TURN_OVER_HU
		setOtherProcess(sideInfo.playerInfo.roomId, sideInfo.playerInfo.oid, ProcessStatus_TURN_OVER)
	} else if procType == pb.ProcType_GangOther {
		log.Debug("robot gang over, turn switch.")
		//gang other
		sideInfo.process = ProcessStatus_TURN_OVER_GANG
	} else if procType == pb.ProcType_SelfGang {
		//self gang
		sideInfo.process = ProcessStatus_TURN_OVER_GANG
		setOtherProcess(sideInfo.playerInfo.roomId, sideInfo.playerInfo.oid, ProcessStatus_TURN_OVER)
	}
}
