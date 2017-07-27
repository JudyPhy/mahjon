package roomMgr

import (
	"bytes"
	"server/pb"
	"strconv"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type SideInfo struct {
	//player
	isRobot    bool
	agent      gate.Agent
	side       pb.BattleSide
	isOwner    bool
	playerInfo *PlayerInfo
	//card
	lackType *pb.CardType
	cardList []*Card
	process  ProcessStatus
}

func (sideInfo *SideInfo) selectLack() {
	typeCount := []int{0, 0, 0}
	for i, value := range sideInfo.cardList {
		if i == 0 {
		}
		if value.id > 0 && value.id < 10 {
			typeCount[0]++
		} else if value.id > 10 && value.id < 20 {
			typeCount[1]++
		} else if value.id > 20 && value.id < 30 {
			typeCount[2]++
		}
	}

	logStr := "type count: "
	buf := bytes.NewBufferString(logStr)
	for i := 0; i < len(typeCount); i++ {
		str := strconv.Itoa(typeCount[i])
		buf.Write([]byte(str))
		buf.Write([]byte(", "))
	}
	log.Debug(buf.String())

	countMin := 14
	typeIndex := 0
	for i := 0; i < len(typeCount); i++ {
		if typeCount[i] < countMin {
			typeIndex = i
			countMin = typeCount[i]
		}
	}

	if typeIndex == 0 {
		sideInfo.lackType = pb.CardType_Wan.Enum()
	} else if typeIndex == 1 {
		sideInfo.lackType = pb.CardType_Tiao.Enum()
	} else {
		sideInfo.lackType = pb.CardType_Tong.Enum()
	}
	log.Debug("playeroid[%v], lack type=%v", sideInfo.playerInfo.oid, sideInfo.lackType)
	sideInfo.process = ProcessStatus_LACK_OVER
}
