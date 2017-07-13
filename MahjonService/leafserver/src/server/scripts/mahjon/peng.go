package mahjon

import (
	"fmt"
)

func (mj *Mahjon) Peng(card CardInfo, user int32) []CardInfo {

	count := 0

	if card.PlayerId == user {
		fmt.Println("Cannot Peng self")
	} else {
		for _, v := range mj.handCards[user] {
			if v.CardId == card.CardId && inHand == v.status {

				count++

				v.status == bePeng
			}
		}
	}
	return mj.handCards[user]

}

func (mj *Mahjon) Gang(card CardInfo, user int32) (int32, int32, []CardInfo) {

	count := 0

	if card.PlayerId == user {

		for _, v := range mj.handCards[user] {

			if v == card {
				count++
			}
		}

	}
}
