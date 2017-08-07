package card

import (
	"bytes"
	"math/rand"
	"server/pb"
	"sort"
	"strconv"
	"time"

	"github.com/name5566/leaf/log"
)

type MJType int32

const (
	MJType_XUEZHAN MJType = 1
)

func (x MJType) Enum() *MJType {
	p := new(MJType)
	*p = x
	return p
}

type MJCardCount int32

const (
	MJCardCount_XUEZHAN MJCardCount = 108
)

func (x MJCardCount) Enum() *MJCardCount {
	p := new(MJCardCount)
	*p = x
	return p
}

type CardStatus int32

const (
	CardStatus_NODEAL      CardStatus = 1
	CardStatus_INHAND      CardStatus = 2
	CardStatus_EXCHANGE    CardStatus = 3
	CardStatus_GANG        CardStatus = 4
	CardStatus_PENG        CardStatus = 5
	CardStatus_DEAL        CardStatus = 6
	CardStatus_PRE_DISCARD CardStatus = 7
	CardStatus_DISCARD     CardStatus = 8
	CardStatus_HU          CardStatus = 9
)

func (x CardStatus) Enum() *CardStatus {
	p := new(CardStatus)
	*p = x
	return p
}

type Card struct {
	oid       int32
	id        int32
	status    CardStatus
	fromOther bool
}

var mjType MJType

func cardStatusToPbCardStatus(status CardStatus) pb.CardStatus {
	switch status {
	case CardStatus_INHAND:
		return pb.CardStatus_inHand
	case CardStatus_GANG:
		return pb.CardStatus_beGang
	case CardStatus_PENG:
		return pb.CardStatus_bePeng
	case CardStatus_DISCARD:
		return pb.CardStatus_discard
	case CardStatus_HU:
		return pb.CardStatus_hu
	}
	return pb.CardStatus_noDeal
}

func loadAllCards() []*Card {
	log.Debug("loadAllCards")
	mjType := MJType_XUEZHAN
	mjCardCount := MJCardCount_XUEZHAN
	if mjType == MJType_XUEZHAN {
		mjCardCount = MJCardCount_XUEZHAN
	}
	maxCount := int(mjCardCount)
	log.Debug("max card count=%v", maxCount)

	var origCardWall []*Card
	id := int(0)
	for i := int(0); i < maxCount; i++ {
		card := &Card{}
		card.oid = int32(i)
		if i%4 == 0 {
			id++
			if id%10 == 0 {
				id++
			}
		}
		card.id = int32(id)
		card.status = CardStatus_NODEAL
		card.fromOther = false
		origCardWall = append(origCardWall, card)
		//log.Debug("card oid=%v, id=%v", card.oid, card.id)
	}

	cardWall := make([]*Card, 108)
	for i := 0; i < maxCount; i++ {
		rand.Seed(time.Now().Unix())
		rndCard := rand.Intn(len(origCardWall))
		card := origCardWall[rndCard]
		rand.Seed(time.Now().Unix())
		rndPos := rand.Intn(len(origCardWall))
		cardWall[rndPos] = card
		origCardWall = append(origCardWall[:i], origCardWall[i+1:]...)
	}
	log.Debug("load all card over, count=%v", len(cardWall))
	return cardWall
}

func IsHu(inhand []int, gang []int, peng []int) bool {
	log.Debug("check hu")
	sumCount := len(inhand) + len(peng) + len(gang)
	if sumCount < 14 || sumCount > 18 {
		log.Debug("sumCount[%v] is error.", sumCount)
		return false
	}

	//log.Debug("peng count=[%v]", len(peng))
	if !checkPeng(peng) {
		return false
	}
	//log.Debug("after peng count=[%v]", len(peng))

	//log.Debug("gang count=[%v]", len(gang))
	if !checkGang(gang) {
		return false
	}
	//log.Debug("after gang count=[%v]", len(gang))

	//log.Debug("inhand count=[%v]", len(inhand))
	if !checkInHand(inhand) {
		return false
	}
	//log.Debug("after inhand count=[%v]", len(inhand))

	return true
}

//检查碰牌
//list是cardId数组
func checkPeng(list []int) bool {
	if len(list)%3 != 0 {
		log.Error("peng card count[%v] is error", len(list))
		return false
	}
	dict := make(map[int]int) //cardId : count
	for i := 0; i < len(list); i++ {
		count, ok := dict[list[i]]
		if ok {
			count++
			dict[list[i]] = count
		} else {
			dict[list[i]] = 1
		}
	}
	for id, count := range dict {
		log.Debug("check peng => id[%v], count[%v]", id, count)
		if count != 3 {
			return false
		}
	}
	return true
}

//检查杠牌
//list是cardId数组
func checkGang(list []int) bool {
	if len(list)%4 != 0 {
		log.Error("gang card count[%v] is error", len(list))
		return false
	}
	dict := make(map[int]int) //cardId : count
	for i := 0; i < len(list); i++ {
		count, ok := dict[list[i]]
		if ok {
			count++
		} else {
			dict[list[i]] = 1
		}
	}
	for id, count := range dict {
		log.Debug("[HU] check gang => id[%v], count[%v]", id, count)
		if count != 4 {
			return false
		}
	}
	return true
}

//检查手牌
//list是cardId数组
func checkInHand(list []int) bool {
	var sortList []int
	for i := 0; i < len(list); i++ {
		sortList = append(sortList, int(list[i]))
	}
	sort.Ints(sortList)

	logStr := "判定胡牌的原始牌队列, sortList: "
	buf := bytes.NewBufferString(logStr)
	for i := 0; i < len(sortList); i++ {
		str := strconv.Itoa(sortList[i])
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	//检查七小对
	if isSevenPair(sortList) {
		log.Debug("isSevenPair")
		return true
	}

	if len(sortList) == 2 {
		return sortList[0] == sortList[1]
	}

	for i := 0; i < len(sortList); i++ {
		var tempList []int
		for i := 0; i < len(sortList); i++ {
			tempList = append(tempList, sortList[i])
		}

		count := getCountInListById(sortList[i], sortList)
		//log.Debug("card[%v], count[%v]", sortList[i], count)
		//判断是否能做将牌
		if count >= 2 {
			//log.Debug("将牌：[%v]", sortList[i])
			//移除两张将牌
			tempList = removeJiang(sortList[i], tempList)
			//避免重复运算 将光标移到其他牌上
			i = i + count - 1
			//检查剩余牌顺子、刻子情况
			if huPaiPanDin(tempList) {
				log.Debug("is normal hu")
				return true
			}
		}
	}
	return false
}

func getCountInListById(id int, list []int) int {
	count := 0
	for i := 0; i < len(list); i++ {
		if list[i] == id {
			count++
		}
	}
	return count
}

func hasCardById(id int, list []int) bool {
	isFind := false
	for i := 0; i < len(list); i++ {
		if list[i] == id {
			isFind = true
			break
		}
	}
	return isFind
}

func removeJiang(id int, list []int) []int {
	count := 0
	for i := 0; i < len(list); i++ {
		if list[i] == id {
			list = append(list[:i], list[i+1:]...)
			count++
			if count == 2 {
				break
			}
			i--
		}
	}
	return list
}

func huPaiPanDin(list []int) bool {
	logStr := "huPaiPanDin, list: "
	buf := bytes.NewBufferString(logStr)
	for i := 0; i < len(list); i++ {
		str := strconv.Itoa(list[i])
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	if len(list) == 0 {
		return true
	}

	count := getCountInListById(list[0], list)

	//组成刻子
	if count == 3 {
		return huPaiPanDin(list[3:])
	} else {
		//组成顺子
		if hasCardById(list[0]+1, list) && hasCardById(list[0]+2, list) {
			firstId := list[0]
			list = list[1:]
			for i := 0; i < len(list); i++ {
				if list[i] == firstId+1 {
					list = append(list[:i], list[i+1:]...)
					break
				}
			}
			for i := 0; i < len(list); i++ {
				if list[i] == firstId+2 {
					list = append(list[:i], list[i+1:]...)
					break
				}
			}
			return huPaiPanDin(list)
		}
		return false
	}
}

func isSevenPair(list []int) bool {
	if len(list) != 14 {
		return false
	}
	dict := make(map[int]int) //id : count
	for i := 0; i < len(list); i++ {
		count, ok := dict[list[i]]
		if ok {
			count++
		} else {
			dict[list[i]] = 1
		}
	}
	for _, count := range dict {
		//log.Debug("seven pair=> id[%v], count[%v]", id, count)
		if count%2 != 0 {
			return false
		}
	}
	return true
}

func getPengCardIdList(list []*Card) []int {
	var result []int
	for i := 0; i < len(list); i++ {
		if list[i].status == CardStatus_PENG {
			result = append(result, int(list[i].id))
		}
	}
	return result
}

func getGangCardIdList(list []*Card) []int {
	var result []int
	for i := 0; i < len(list); i++ {
		if list[i].status == CardStatus_GANG {
			result = append(result, int(list[i].id))
		}
	}
	return result
}

func getInHandCardIdList(list []*Card) []int {
	var result []int
	for i := 0; i < len(list); i++ {
		if list[i].status == CardStatus_INHAND {
			result = append(result, int(list[i].id))
		}
	}
	return result
}

//返回杠牌ID
func canGang(list []int, card *Card) int {
	dict := make(map[int]int) //id : count
	for i := 0; i < len(list); i++ {
		count, ok := dict[list[i]]
		if ok {
			count++
			dict[list[i]] = count
		} else {
			dict[list[i]] = 1
		}
	}
	if card == nil {
		for id, count := range dict {
			log.Debug("can gang => id[%v], count[%v]", id, count)
			if count == 4 {
				return id
			}
		}
	} else {
		count, ok := dict[int(card.id)]
		if ok && count == 4 {
			return int(card.id)
		}
	}
	return 0
}

func canPeng(list []int, discard *Card) bool {
	count := 0
	for i := 0; i < len(list); i++ {
		if int(discard.id) == list[i] {
			count++
		}
	}
	//log.Debug("canPeng: count=%v, cardId=%v", count, discard.id)
	return count == 3
}
