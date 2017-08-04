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

//获取要出的牌
func getRobotDiscard(list []*Card) *Card {
	var ableDiscardList []*Card
	for _, curCard := range list {
		if curCard.status == CardStatus_INHAND {
			ableDiscardList = append(ableDiscardList, curCard)
		}
	}
	log.Debug("robot inhand card count=%v", len(ableDiscardList))
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
		log.Debug("Game over!")
	} else {
		log.Debug("Is can self gang")
		inhandIdList := getInHandCardIdList(sideInfo.cardList)
		gangCardId := canGang(inhandIdList, nil)
		if gangCardId != 0 {
			sideInfo.procSelfGang(gangCardId)
			return
		}
		log.Debug("can't self gang, proc discard")
		discard := getRobotDiscard(sideInfo.cardList)
		log.Debug("robot 出牌[%v](%v)", discard.oid, discard.id)
		isFind := false
		for _, card := range sideInfo.cardList {
			if card.oid == discard.oid {
				card.status = CardStatus_PRE_DISCARD
				sideInfo.process = ProcessStatus_TURN_OVER
				broadcastDiscard(sideInfo.playerInfo.roomId, discard)
				isFind = true
				break
			}
		}
		if !isFind {
			log.Error("robot discard is not in it's cardList.")
		}
	}
}

func (sideInfo *SideInfo) robotTurnSwitchAfterPeng() {
	log.Debug("turn switch to robot%v after peng", sideInfo.playerInfo.oid)
	timer := time.NewTimer(time.Second * 1)
	<-timer.C
	//1秒后执行
	discard := getRobotDiscard(sideInfo.cardList)
	log.Debug("discard[%v](%v)", discard.oid, discard.id)
	for _, card := range sideInfo.cardList {
		if card.oid == discard.oid {
			card.status = CardStatus_PRE_DISCARD
			sideInfo.process = ProcessStatus_TURN_OVER
			broadcastDiscard(sideInfo.playerInfo.roomId, discard)
			break
		}
	}
}

func (sideInfo *SideInfo) robotProcOver(procType pb.ProcType) {
	if procType == pb.ProcType_Peng {
		log.Debug("robot peng over, turn switch.")
		sideInfo.process = ProcessStatus_TURN_OVER_PENG
	} else if procType == pb.ProcType_HuOther {
		log.Debug("robot hu over, wait check turn over.")
		sideInfo.process = ProcessStatus_TURN_OVER_HU
	} else if procType == pb.ProcType_GangOther {
		log.Debug("robot gang over, turn switch.")
		sideInfo.process = ProcessStatus_TURN_OVER_GANG
	}
}
