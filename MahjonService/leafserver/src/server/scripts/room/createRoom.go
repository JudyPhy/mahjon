package room

import (
	"fmt"
	"math/rand"
	"time"
)

type Room struct {
	RoomId int32
}

func createRoom() *Room {

	return &Room{}
}
