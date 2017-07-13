package mahjon

import (
	"fmt"
)

type Mahjon struct {
	RoomId  string
	UserNum int

	MjCards   [108]CardInfo
	handCards [4][]CardInfo
}

func NewMahjon() *Mahjon {

	var handCards [4][]CardInfo

	roomId := newRoom()
	userNum := 1

	mjcards := newCards()

	for i := 0; i < 4; i++ {
		handCards[i] = make([]CardInfo, 18)
	}

	fmt.Println("mahjon initialized")
	return &Mahjon{roomId, userNum, mjcards, handCards}

}
