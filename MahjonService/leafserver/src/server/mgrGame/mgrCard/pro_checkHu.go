package mgrCard

import (
	"github.com/name5566/leaf/log"
)

func CheckHu(cardList []int32) bool {
	sortList := sortCardList(cardList)
	log.Debug("CheckHu=>sortList=%v", sortList)
	for i := 0; i < len(sortList); i++ {
		tempList := sortList[0:]
		count := countNumOfDrawcard(sortList, sortList[i])
		if count >= 2 {
			tempList = removeCards(tempList, sortList[i], 2)
			i = i + count - 1
			return huPaiPanDing(tempList)
		}
	}
	return false
}

func huPaiPanDing(list []int32) bool {
	if len(list) == 0 {
		return true
	}
	count := countNumOfDrawcard(list, list[0])

	//组成刻子
	if count == 3 {
		return huPaiPanDing(list[3:])
	} else {
		//组成顺子
		if hasCardById(list, list[0]+1) && hasCardById(list, list[0]+2) {
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
			return huPaiPanDing(list)
		}
		return false
	}
}

func hasCardById(list []int32, id int32) bool {
	isFind := false
	for i := 0; i < len(list); i++ {
		if list[i] == id {
			isFind = true
			break
		}
	}
	return isFind
}

func sortCardList(list []int32) []int32 {

	for i := 0; i < len(list)-1; i++ {
		for j := 0; j < len(list)-1-i; j++ {
			if list[j] > list[j+1] {
				t := list[j]
				list[j] = list[j+1]
				list[j+1] = t
			}
		}
	}
	return list
}

func removeCards(list []int32, id int32, Num int) []int32 {
	count := 0
	for i := 0; i < len(list); i++ {
		if list[i] == id {
			list = append(list[:i], list[i+1:]...)
			count++
			if count == Num {
				break
			}
			i--
		}
	}
	return list
}
