package robot

import (
	"server/card"
	"server/pb"
)

type BattleProcess int32

const (
	ProcessStatus_DEFAULT         BattleProcess = 1
	ProcessStatus_EXCHANGE_OVER   BattleProcess = 2
	ProcessStatus_LACK_OVER       BattleProcess = 3
	ProcessStatus_TURN_START      BattleProcess = 4
	ProcessStatus_TURN_START_OVER BattleProcess = 5
	ProcessStatus_TURN_OVER       BattleProcess = 6
	ProcessStatus_TURN_OVER_PENG  BattleProcess = 7
	ProcessStatus_TURN_OVER_GANG  BattleProcess = 8
	ProcessStatus_TURN_OVER_HU    BattleProcess = 9
	ProcessStatus_WAITING_HU      BattleProcess = 10
	ProcessStatus_PROC_HU         BattleProcess = 11 //real player
	ProcessStatus_WAITING_GANG    BattleProcess = 12
	ProcessStatus_WAITING_PENG    BattleProcess = 13
	ProcessStatus_GAME_OVER       BattleProcess = 14
)

type Robot struct {
	oid      int32
	nickName string
	headIcon string
	gold     int32
	diamond  int32

	//battle
	roomId      string
	process     BattleProcess
	lackType    *pb.CardType
	cardList    []*card.Card
	discardList []*card.Card
}
