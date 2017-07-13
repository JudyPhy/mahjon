package mahjon

import (
	"fmt"
	"math/rand"
	"time"
)

type CardInfo struct {
	CardId   int32
	PlayerId int32
	status   CardStatus
}

type CardStatus int32

const (
	noDeal CardStatus = iota
	inHand
	bePeng
	beGang
	dicard
)

func newRoom() string {

	rnd := rand.New(rand.NewSource(time.Now().UnixNano()))

	vcode := fmt.Sprintf("%06v", rnd.Int31n(1000000))

	return vcode
}

func newCards() [108]CardInfo {

	typeId := 0
	var cards [108]int32
	var mjcards [108]CardInfo

	for i := 0; i < 27; i++ {

		if 0 == typeId%10 {
			typeId++
		}

		for j := 0; j < 4; j++ {
			cardId := j + i*4
			cards[cardId] = int32(typeId)
		}
		typeId++
	}

	for i := 0; i < 108; i++ {

		mjcards[i] = CardInfo{

			cards[i],
			1,
			noDeal,
		}
	}

	return mjcards
}
