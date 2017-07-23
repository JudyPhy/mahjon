package roomMgr

import (
	"bytes"
	"math/rand"
	"strconv"
	"time"

	"github.com/name5566/leaf/log"
)

func addRobotToRoom(roomInfo *RoomInfo, oid int) {
	log.Debug("addRobotToRoom roomId=%v", roomInfo.roomId)
	//PlayerInfo
	basePlayer := &PlayerInfo{}
	basePlayer.oid = int32(oid)
	basePlayer.nickName = "游客"
	basePlayer.headIcon = "nil"
	basePlayer.gold = 0
	basePlayer.diamond = 0
	basePlayer.roomId = roomInfo.roomId

	//roomPlayer
	sideList := getLeftSideList(roomInfo)
	side := getRandomSideBySideList(sideList)
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = true
	roomPlayer.agent = nil
	roomPlayer.side = side
	roomPlayer.isOwner = false
	roomPlayer.playerInfo = basePlayer
	roomInfo.playerList = append(roomInfo.playerList, roomPlayer)
}

func selectRobotExchangeCard(roomInfo *RoomInfo, playerId int32) {
	var mapCard map[int][]int32
	for i, cardList := range roomInfo.cardList {
		if i == 0 {
		}
		if cardList.playerId == playerId {
			mapCard = getSeparateCardTypeMap(cardList.list)
			break
		}
	}
	selectedCardOidList := getExchangeCardOID(mapCard)
	log.Debug("robot select exchange card count is %v", len(selectedCardOidList))

	//log
	logStr := "robot exchange card oid list =>"
	buf := bytes.NewBufferString(logStr)
	for i, j := range selectedCardOidList {
		if i == 0 {
		}
		str := strconv.Itoa(int(j))
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	for i, cardList := range roomInfo.cardList {
		if i == 0 {
		}
		if cardList.playerId == playerId {
			sumCount := 0
			for n, oid := range selectedCardOidList {
				if n == 0 {
				}
				for j, card := range cardList.list {
					if j == 0 {
					}
					if card.oid == oid {
						card.status = CardStatus_EXCHANGE
						sumCount++
						break
					}
				}
			}
			if sumCount == 3 {
				log.Debug("player %v exchange over.", playerId)
				cardList.process = ProcessStatus_EXCHANGE_OVER
			}
		}
	}
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

//获取交换牌的oid列表
func getExchangeCardOID(mapCard map[int][]int32) []int32 {
	log.Debug("every type card count is : %v, %v, %v", len(mapCard[0]), len(mapCard[1]), len(mapCard[2]))
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
	resultList := getRandomExchangeCardOIDList(mapCard[indexMin], 3)
	return resultList
}

func getRandomExchangeCardOIDList(cardList []int32, count int) []int32 {
	log.Debug("req cards, list count=%v, req count=%d", len(cardList), count)
	var result []int32
	for i := 0; i < count; i++ {
		log.Debug("get random exchange card, orig list count=%v", len(cardList))
		rand.Seed(time.Now().Unix())
		rnd := rand.Intn(len(cardList))
		result = append(result, cardList[rnd])
		cardList = append(cardList[:rnd], cardList[rnd+1:]...)
	}
	return result
}
