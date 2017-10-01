package mgrSide

import (
	"server/mgrGame/mgrCard"
	"server/mgrPlayer"
	"server/pb"

	"math/rand"
	"time"
)

func RegSideByRobot(side pb.MahjonSide, pid int32) *SideInfo {
	sideinfo := &SideInfo{
		isRobot: true,
		isOwner: false,
		side:    side,
		player:  mgrPlayer.RegRobotById(pid),

		handCards: make([]*mgrCard.CardInfo, 0),
	}
	return sideinfo
}

func (robot *SideInfo) SelectRobotExchangeCard() {
	mapCard := robot.getSeparateCardTypeMap()
	countList := []int{len(mapCard[0]), len(mapCard[1]), len(mapCard[2])}
	countMin := 14
	indexMin := 0
	for i, count := range countList {
		if count >= 3 && count < countMin {
			countMin = count
			indexMin = i
		}
	}
	typeCardList := mapCard[indexMin]
	exCardList := typeCardList[0:3]
	robot.SetExchangeCard(exCardList)
}

func (robot *SideInfo) SelectRobotLackType() {
	mapCard := robot.getSeparateCardTypeMap()
	countList := []int{len(mapCard[0]), len(mapCard[1]), len(mapCard[2])}
	countMin := 14
	indexMin := 0
	for i, count := range countList {
		if count < countMin {
			countMin = count
			indexMin = i
		}
	}
	var lacktype pb.CardType
	switch indexMin {
	case 0:
		lacktype = pb.CardType_Wan
	case 1:
		lacktype = pb.CardType_Tiao
	case 2:
		lacktype = pb.CardType_Tong
	}
	robot.SetLackType(lacktype)
}

//将列表中的牌按照花色分开，分装到一个map中
func (robot *SideInfo) getSeparateCardTypeMap() map[int][]int32 {
	resultMap := make(map[int][]int32)
	var listWan []int32
	var listTiao []int32
	var listTong []int32
	for _, card := range robot.handCards {
		if card.Id > 0 && card.Id < 10 {
			listWan = append(listWan, card.Oid)
		} else if card.Id > 10 && card.Id < 20 {
			listTiao = append(listTiao, card.Oid)
		} else if card.Id > 20 && card.Id < 30 {
			listTong = append(listTong, card.Oid)
		}
	}
	resultMap[0] = listWan
	resultMap[1] = listTiao
	resultMap[2] = listTong
	return resultMap
}

func (robot *SideInfo) RobotDiscardAI() *mgrCard.CardInfo {
	rand.Seed(time.Now().Unix())
	index := rand.Intn(len(robot.handCards))
	drawCard := robot.handCards[index]

	return drawCard
}
