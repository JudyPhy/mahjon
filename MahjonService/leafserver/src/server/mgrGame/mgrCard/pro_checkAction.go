package mgrCard

import (
	"github.com/name5566/leaf/log"
)

func CheckAnGang(cardList []int32) (int32, bool) {
	dict := make(map[int32]int) //cardId : count
	for _, cardId := range cardList {
		count, ok := dict[cardId]
		if ok {
			count++
		} else {
			dict[cardId] = 1
		}
	}
	for cardid, count := range dict {
		if count == 4 {
			return cardid, true
		}
	}

	return -1, false
}

func CheckPeng(cardList []int32, card int32) bool {
	countNum := countNumOfDrawcard(cardList, card)
	if countNum >= 2 {
		return true
	}
	return false
}

func CheckGang(cardList []int32, card int32) bool {

	countNum := countNumOfDrawcard(cardList, card)
	log.Debug("checkGang=>list=%v,countNum=%v,card=%v", cardList, countNum, card)
	if countNum == 3 {
		return true
	}
	return false
}

func countNumOfDrawcard(cardList []int32, card int32) int {
	ct := 0
	for _, cards := range cardList {
		if cards == card {
			ct++
		}
	}
	return ct
}
