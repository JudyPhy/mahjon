package roomMgr

import (
	"bytes"
	"math/rand"
	"server/pb"
	"strconv"
	"time"

	"github.com/name5566/leaf/log"
)

func (roomInfo *RoomInfo) selectRobotExchangeCard(cardList []*Card) []*Card {
	log.Debug("robot select exchange cards...")
	mapCard := getSeparateCardTypeMap(cardList)
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
	for i := 0; i < 3; i++ {
		log.Debug("get random exchange card, orig list count=%v", len(typeCardList))
		rand.Seed(time.Now().Unix())
		rnd := rand.Intn(len(typeCardList))
		for j, card := range cardList {
			if j == 0 {
			}
			if card.oid == typeCardList[rnd] {
				card.status = CardStatus_EXCHANGE
				break
			}
		}
		typeCardList = append(typeCardList[:rnd], typeCardList[rnd+1:]...)
	}

	//log
	logStr := "robot exchange card oid list =>"
	buf := bytes.NewBufferString(logStr)
	for i, j := range cardList {
		if i == 0 {
		}
		str := strconv.Itoa(int(j.oid))
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	return cardList
}

//将列表中的牌按照花色分开，分装到一个map中
func getSeparateCardTypeMap(list []*Card) map[int][]int32 {
	resultMap := make(map[int][]int32)
	var listWan []int32
	var listTiao []int32
	var listTong []int32
	for i, card := range list {
		if i == 0 {
		}
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

func getLackIndexByLackType(lackType *pb.CardType) int {
	if lackType == pb.CardType_Wan.Enum() {
		return 1
	} else if lackType == pb.CardType_Tiao.Enum() {
		return 2
	} else if lackType == pb.CardType_Tong.Enum() {
		return 3
	}
	return 0
}

func (sideInfo *SideInfo) getRobotDiscard() *Card {
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

//接口必须在摸牌后执行
func (sideInfo *SideInfo) robotTurnSwitch() {
	log.Debug("turn switch to robot%v", sideInfo.playerInfo.oid)
	timer := time.NewTimer(time.Second * 1)
	<-timer.C
	//1秒后执行
	inhandList := getInHandCardIdList(sideInfo.cardList)
	gList := getGangCardIdList(sideInfo.cardList)
	pList := getPengCardIdList(sideInfo.cardList)
	if IsHu(inhandList, gList, pList) {
		log.Debug("Hu robot self, game over!")
		sideInfo.procSelfHuPlayAndRobot()
	} else {
		log.Debug("can't self hu, check self gang.")
		inhandAndPList := append(inhandList[:], pList[:]...)
		gangCardId := canGang(inhandAndPList, nil)
		if gangCardId != 0 {
			sideInfo.procSelfGangPlayerAndRobot()
		} else {
			log.Debug("can't self gang, proc discard")
			discard := sideInfo.getRobotDiscard()
			log.Debug("robot 出牌[%v](%v)", discard.oid, discard.id)
			isFind := false
			for n, card := range sideInfo.cardList {
				if card.oid == discard.oid {
					card.status = CardStatus_PRE_DISCARD
					sideInfo.cardList = append(sideInfo.cardList[:n], sideInfo.cardList[n+1:]...)
					sideInfo.cardList = append(sideInfo.cardList, card)
					sideInfo.process = ProcessStatus_TURN_OVER
					broadcastRobotDiscard(sideInfo.playerInfo.roomId, discard)
					isFind = true
					break
				}
			}
			if !isFind {
				log.Error("robot discard is not in it's cardList.")
			}
		}
	}
}

func (sideInfo *SideInfo) robotSelfGang() {
	log.Debug("robotSelfGang")
	dict := make(map[int32]int)
	for _, card := range sideInfo.cardList {
		if card.status == CardStatus_INHAND || card.status == CardStatus_PENG {
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
		log.Error("player%v has no self gang card!", sideInfo.playerInfo.oid)
	} else {
		for _, card := range sideInfo.cardList {
			if card.status == CardStatus_INHAND && card.id == gCardId {
				card.status = CardStatus_GANG
			}
		}
		sendRobotSelfGangProc(sideInfo.playerInfo.roomId)
	}
}

func (sideInfo *SideInfo) robotTurnSwitchAfterPeng() {
	log.Debug("turn switch to robot%v after peng", sideInfo.playerInfo.oid)
	timer := time.NewTimer(time.Second * 1)
	<-timer.C
	//1秒后执行
	discard := sideInfo.getRobotDiscard()
	log.Debug("discard[%v](%v)", discard.oid, discard.id)
	for n, card := range sideInfo.cardList {
		if card.oid == discard.oid {
			card.status = CardStatus_PRE_DISCARD
			sideInfo.cardList = append(sideInfo.cardList[:n], sideInfo.cardList[n+1:]...)
			sideInfo.cardList = append(sideInfo.cardList, card)
			sideInfo.process = ProcessStatus_TURN_OVER
			broadcastRobotDiscard(sideInfo.playerInfo.roomId, discard)
			break
		}
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
