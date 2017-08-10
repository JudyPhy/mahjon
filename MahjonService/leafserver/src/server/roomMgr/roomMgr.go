package roomMgr

import (
	"math/rand"
	"server/msgHandler"
	"server/pb"
	"server/player"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type ProcessStatus int32

const (
	ProcessStatus_DEFAULT        ProcessStatus = 1
	ProcessStatus_EXCHANGE_OVER  ProcessStatus = 2
	ProcessStatus_LACK_OVER      ProcessStatus = 3
	ProcessStatus_TURN_START     ProcessStatus = 4
	ProcessStatus_TURN_OVER      ProcessStatus = 5
	ProcessStatus_TURN_OVER_PENG ProcessStatus = 6
	ProcessStatus_TURN_OVER_GANG ProcessStatus = 7
	ProcessStatus_TURN_OVER_HU   ProcessStatus = 8
	ProcessStatus_WAITING_HU     ProcessStatus = 9
	ProcessStatus_PROC_HU        ProcessStatus = 10 //real player
	ProcessStatus_WAITING_GANG   ProcessStatus = 11
	ProcessStatus_WAITING_PENG   ProcessStatus = 12
	ProcessStatus_GAME_OVER      ProcessStatus = 13
)

func (x ProcessStatus) Enum() *ProcessStatus {
	p := new(ProcessStatus)
	*p = x
	return p
}

type TurnOverType int32

const (
	TurnOverType_DEFAULT TurnOverType = 1
	TurnOverType_NORMAL  TurnOverType = 2
	TurnOverType_PENG    TurnOverType = 3
	TurnOverType_GANG    TurnOverType = 4
	TurnOverType_HU      TurnOverType = 5
)

func (x TurnOverType) Enum() *TurnOverType {
	p := new(TurnOverType)
	*p = x
	return p
}

// ---------------------
// | roomId | RoomInfo |
// ---------------------
type mgrRoom struct {
	lock    sync.Mutex
	roomMap map[string]*RoomInfo
}

var RoomManager *mgrRoom

func getRandomRoomId(length int) string {
	log.Debug("getRandomRoomId")
	rand.Seed(time.Now().UnixNano())
	rs := make([]string, length)
	for start := 0; start < length; start++ {
		rs = append(rs, strconv.Itoa(rand.Intn(10)))
	}
	return strings.Join(rs, "") //使用""拼接rs切片
}

func reqNewRoom(a gate.Agent) *RoomInfo {
	log.Debug("ReqNewRoom")
	newRoomId := getRandomRoomId(6)
	for {
		RoomManager.lock.Lock()
		_, ok := RoomManager.roomMap[newRoomId]
		if ok {
			newRoomId = getRandomRoomId(6)
		} else {
			RoomManager.roomMap[newRoomId] = &RoomInfo{}
			break
		}
	}
	roomInfo := RoomManager.roomMap[newRoomId]
	roomInfo.Init(newRoomId)
	RoomManager.lock.Unlock()
	return roomInfo
}

//------------------------------------------------------------------------------
//								   public func
//------------------------------------------------------------------------------
func Init() {
	log.Debug("init player map.")
	player.AgentPlayer = &player.ChanPlayer{}
	player.AgentPlayer.CMap = make(map[gate.Agent]*player.Player)

	log.Debug("init room map.")
	RoomManager = &mgrRoom{}
	RoomManager.roomMap = make(map[string]*RoomInfo)
}

func CreateRoomRet(a gate.Agent) {
	log.Debug("CreateRoomRet")
	mode := pb.GameMode_CreateRoom.Enum()
	roomInfo := reqNewRoom(a)
	result := roomInfo.addPlayerToRoom(a, true)
	errorCode := pb.GS2CEnterGameRet_SUCCESS.Enum()
	if !result {
		log.Error("add player to room fail.")
		errorCode = pb.GS2CEnterGameRet_FAIL.Enum()
	}
	msgHandler.SendGS2CEnterGameRet(errorCode, mode, roomInfo.roomId, a)

	//test: add other 3 robot to room
	roomInfo.waitingRoomOk()
}

func JoinRoomRet(roomId string, a gate.Agent) {
	log.Debug("JoinRoomRet: room=%v", roomId)
	ret := &pb.GS2CEnterGameRet{}
	ret.Mode = pb.GameMode_JoinRoom.Enum()
	ret.RoomId = proto.String(roomId)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		if len(roomInfo.sideInfoMap.cMap) >= 4 {
			ret.ErrorCode = pb.GS2CEnterGameRet_PLAYER_COUNT_LIMITE.Enum()
		} else {
			roomInfo.addPlayerToRoom(a, false)
			ret.ErrorCode = pb.GS2CEnterGameRet_SUCCESS.Enum()
		}
	} else {
		ret.ErrorCode = pb.GS2CEnterGameRet_ROOM_NOT_EXIST.Enum()
	}
	a.WriteMsg(ret)

	roomInfo.startBattle()
}

func QuickEnterRoomRet(a gate.Agent) {
	log.Debug("JoinRoomRet")
	ret := &pb.GS2CEnterGameRet{}
	ret.Mode = pb.GameMode_QuickEnter.Enum()
	findRoom := false
	var roomInfo *RoomInfo
	for _, room := range RoomManager.roomMap {
		if len(room.sideInfoMap.cMap) < 4 {
			ret.RoomId = proto.String(room.roomId)
			room.addPlayerToRoom(a, false)
			findRoom = true
			roomInfo = room
			break
		}
	}
	if findRoom {
		ret.ErrorCode = pb.GS2CEnterGameRet_SUCCESS.Enum()
	} else {
		ret.ErrorCode = pb.GS2CEnterGameRet_NO_EMPTY_ROOM.Enum()
	}
	a.WriteMsg(ret)

	if roomInfo != nil {
		roomInfo.startBattle()
	}
}

func PlayerOffline(a gate.Agent) {
	log.Debug("PlayerOffline")
	offPlayer := player.GetPlayerBtAgent(a)
	if offPlayer.RoomId != "" {
		log.Debug("player%v in room, out room first.", offPlayer.OID)
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[offPlayer.RoomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.outRoom(offPlayer)
			if roomInfo.isEmptyRoom() {
				RoomManager.lock.Lock()
				delete(RoomManager.roomMap, offPlayer.RoomId)
				RoomManager.lock.Unlock()
				offPlayer.RoomId = ""
			}
		} else {
			log.Error("room %v not exist.", offPlayer.RoomId)
		}
	} else {
		log.Debug("player%v not in room, exit directly.", offPlayer.OID)
	}
	offPlayer.OffLine(a)
}

func UpdateExchangeCard(cardOidList []int32, a gate.Agent) {
	log.Debug("UpdateExchangeCard")
	exchangeCount := len(cardOidList)
	if exchangeCount != 3 {
		log.Error("exchange card count[%v] is error", exchangeCount)
		return
	}
	curPlayer := player.GetPlayerBtAgent(a)
	if curPlayer != nil {
		log.Debug("player%v exchange card", curPlayer.OID)
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[curPlayer.RoomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.updateExchangeCards(cardOidList, curPlayer.OID)
		} else {
			log.Error("no room[%v]", curPlayer.RoomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func UpdateLackCard(lackType pb.CardType, a gate.Agent) {
	log.Debug("UpdateLackCard")
	curPlayer := player.GetPlayerBtAgent(a)
	if curPlayer != nil {
		log.Debug("player=%v select lack card type=%v", curPlayer.OID, lackType.String())
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[curPlayer.RoomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.updateLack(curPlayer.OID, lackType)
		} else {
			log.Error("no room[%v]", curPlayer.RoomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func UpdateDiscard(cardOid int32, a gate.Agent) {
	log.Debug("UpdateDiscard")
	curPlayer := player.GetPlayerBtAgent(a)
	if curPlayer != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[curPlayer.RoomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.recvRealPlayerDiscard(curPlayer.OID, cardOid)
		} else {
			log.Error("no room[%v]", curPlayer.RoomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func RobotProcOver(robotOid int32, procType pb.ProcType, a gate.Agent) {
	log.Debug("RobotProcOver, robotOid=%v, procType=%v", robotOid, procType)
	curPlayer := player.GetPlayerBtAgent(a)
	if curPlayer != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[curPlayer.RoomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.robotProcOver(robotOid, procType)
		} else {
			log.Error("no room[%v]", curPlayer.RoomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func PlayerEnsureProc(procType pb.ProcType, procCardId int32, a gate.Agent) {
	log.Debug("PlayerEnsureProc, procType=%v", procType)
	curPlayer := player.GetPlayerBtAgent(a)
	if curPlayer != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[curPlayer.RoomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.playerEnsureProc(curPlayer.OID, procType, procCardId)
		} else {
			log.Error("no room[%v]", curPlayer.RoomId)
		}
	} else {
		log.Error("player not login.")
	}
}
