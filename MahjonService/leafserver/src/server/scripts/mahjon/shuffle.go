package mahjon

import (
	"fmt"
	"math/rand"
	"time"
)

func (mj *Mahjon) Shuffle() {

	k := 108
	r_seed := rand.New(rand.NewSource(time.Now().UnixNano()))

	for i := 0; i < 108; i++ {
		x := r_seed.Intn(k)

		t := mj.MjCards[x].CardId
		mj.MjCards[x].CardId = mj.MjCards[k-1].CardId
		mj.MjCards[k-1].CardId = t
		k--
	}

	fmt.Println("shuffled")

}
