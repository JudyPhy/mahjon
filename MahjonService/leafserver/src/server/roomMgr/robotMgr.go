package roomMgr

import (
	"bytes"
	"math/rand"
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
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(list))
	return list[rnd]
}

//出牌后，机器人根据出牌信息判断自方情况
func (sideInfo *SideInfo) robotProcAfterDiscard(card *Card) {
	log.Debug("robot: proc after discard, playerOid[%v]", sideInfo.playerInfo.oid)
	if curTurnPlayerOid == sideInfo.playerInfo.oid {
		return
	}
	handCard := getInHandCardIdList(sideInfo.cardList)
	handCard = append(handCard, int(card.id))
	pList := getPengCardIdList(sideInfo.cardList)
	gList := getGangCardIdList(sideInfo.cardList)

	if IsHu(handCard, pList, gList) {
		log.Debug("胡牌")
		sideInfo.process = ProcessStatus_WAITING_HU
	} else {
		if canGang(handCard, card) != 0 {
			log.Debug("可以杠")
			sideInfo.process = ProcessStatus_WAITING_GANG
		} else {
			if canPeng(handCard, card) {
				log.Debug("可以碰")
				sideInfo.process = ProcessStatus_WAITING_PENG
			} else {
				log.Debug("机器人本轮不能胡、杠、碰，结束")
				sideInfo.process = ProcessStatus_TURN_OVER
			}
		}
	}
}
