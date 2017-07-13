package mahjon

import (
	"fmt"
)

func (mj *Mahjon) Discard(card CardInfo) (CardInfo, []CardInfo) {

	//a := false
	ss := mj.handCards[card.PlayerId]

	if inHand == card.status {

		for i, v := range ss {
			if card == v {
				ss = append(ss[:i], ss[i+1:]...)
				break
			}

		}

		card.status = dicard
		//a = true

	}

	fmt.Println("dis:	", card)

	return card, ss

}
