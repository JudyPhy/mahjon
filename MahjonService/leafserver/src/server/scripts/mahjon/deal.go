package mahjon

import (
	"fmt"
)

func (mj *Mahjon) Deal(num int, order bool, user int32) []CardInfo {

	startId, endId := mj.getStartCard(num, order)
	fmt.Println("Dealing 111")

	_cardSlices := mj.MjCards[startId:endId]

	for i, _ := range _cardSlices {
		_cardSlices[i].PlayerId = user
		_cardSlices[i].status = inHand
	}

	mj.handCards[user] = append(_cardSlices)

	fmt.Println("test:	", mj.handCards[user])

	return _cardSlices
}

func (mj *Mahjon) getStartCard(num int, order bool) (int, int) {

	var startId int
	var endId int
	if false == order {

		for i := 0; i < 108; i++ {
			if 0 == mj.MjCards[i].status {

				startId = i
				break
			} else {
				fmt.Println("Dealed over")
			}

		}
	} else {
		for i := 0; i < 108; i++ {
			if 0 == mj.MjCards[107-i].status {

				startId = 108 - i - num

				fmt.Println("yoyoyo", startId)
				break
			} else {
				fmt.Println("Dealed over")
			}

		}
	}
	endId = startId + num

	return startId, endId
}
