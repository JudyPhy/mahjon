package roomMgr

import (
	"math/rand"
	"server/msgHandler"
	"server/pb"
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
	ProcessStatus_DEFAULT         ProcessStatus = 1
	ProcessStatus_EXCHANGE_OVER   ProcessStatus = 2
	ProcessStatus_LACK_OVER       ProcessStatus = 3
	ProcessStatus_TURN_START      ProcessStatus = 4
	ProcessStatus_TURN_START_OVER ProcessStatus = 5
	ProcessStatus_TURN_OVER       ProcessStatus = 6
	ProcessStatus_TURN_OVER_PENG  ProcessStatus = 7
	ProcessStatus_TURN_OVER_GANG  ProcessStatus = 8
	ProcessStatus_TURN_OVER_HU    ProcessStatus = 9
	ProcessStatus_WAITING_HU      ProcessStatus = 10
	ProcessStatus_PROC_HU         ProcessStatus = 11 //real player
	ProcessStatus_WAITING_GANG    ProcessStatus = 12
	ProcessStatus_WAITING_PENG    ProcessStatus = 13
	ProcessStatus_GAME_OVER       ProcessStatus = 14
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

func robotSelfGangOver(roomId string) {
	log.Debug("robotSelfGangOver")
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		side := roomInfo.getSideByPlayerOid(curTurnPlayerOid)
		roomInfo.sendNormalTurnToNext(side)
	} else {
		log.Debug("room[%v] not exist.")
	}
}

func broadcastRobotDiscard(roomId string, discard *card.Card) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.broadcastDiscard(discard)
		//after discard, wait 1 seconds for client ani
		timer := time.NewTimer(time.Second * 1)
		<-timer.C
		roomInfo.checkTurnOver()
	} else {
		log.Error("broadcastDiscard, no room[%v]", roomId)
	}
}

func curTurnPlayerSelfGang(roomId string) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.procSelfGang()
	} else {
		log.Error("curTurnPlayerSelfGang, no room[%v]", roomId)
	}
}

func curTurnPlayerSelfHu(roomId string) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.procSelfHu()
	} else {
		log.Error("curTurnPlayerSelfGang, no room[%v]", roomId)
	}
}

func sendRobotSelfGangProc(roomId string) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendRobotProc(curTurnPlayerOid, 0, pb.ProcType_SelfGang)
	} else {
		log.Error("sendRobotSelfGang, no room[%v]", roomId)
	}
}

func sendRobotSelfHuProc(roomId string) {
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.sendRobotProc(curTurnPlayerOid, 0, pb.ProcType_SelfHu)
	} else {
		log.Error("sendRobotSelfHuProc, no room[%v]", roomId)
	}
}

func setOtherProcess(roomId string, exceptPlayerOid int32, process ProcessStatus) {
	log.Debug("setOtherProcess, roomId=%v, exceptPlayerOid=%v, process=%v", roomId, exceptPlayerOid, process)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.setOtherProcessBySelfProc(exceptPlayerOid, process)
	} else {
		log.Error("sendRobotSelfGang, no room[%v]", roomId)
	}
}

//------------------------------------------------------------------------------
//								   public func
//------------------------------------------------------------------------------
func Init() {
	log.Debug("init player map.")
	ChanPlayerStruct = &ChanPlayer{}
	ChanPlayerStruct.aPlayerMap = make(map[gate.Agent]*PlayerInfo)

	log.Debug("init room map.")
	RoomManager = &mgrRoom{}
	RoomManager.roomMap = make(map[string]*RolomInfo)
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

	//register out room event
	dispatcher := eventDispatch.GetSingletonDispatcher()
	var outRoomFunc eventDispatch.EventCallback = OutRoom
	dispatcher.AddEventListener("outRoom", &outRoomFunc)
}

func OutRoom(event *eventDispatch.Event) {
	roomId := event.params["roomId"]
	playerOid := event.params["playerOid"]
	log.Debug("OutRoom: player%v out room=%v", playerOid, roomId)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.outRoom(playerOid)
		if roomInfo.isEmptyRoom() {
			RoomManager.lock.Lock()
			delete(RoomManager.roomMap, roomId)
			RoomManager.lock.Unlock()
			//remove outRoom event
			dispatcher := eventDispatch.GetSingletonDispatcher()
			dispatcher.RemoveEventListener("outRoom", &outRoomFunc)
		}
	} else {
		log.Error("room %v not exist.", roomId)
	}
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
			ret.ErrorCode = GS2CEnterGameRet_PLAYER_COUNT_LIMITE.Enum()
		} else {
			roomInfo.addPlayerToRoom(a, false)
			ret.ErrorCode = GS2CEnterGameRet_SUCCESS.Enum()
		}
	} else {
		ret.ErrorCode = GS2CEnterGameRet_ROOM_NOT_EXIST.Enum()
	}
	a.WriteMsg(ret)

	roomInfo.startBattle()
}

func QuickEnterRoomRet(a gate.Agent) {
	log.Debug("JoinRoomRet: room=%v", roomId)
	ret := &pb.GS2CEnterGameRet{}
	ret.Mode = pb.GameMode_QuickEnter.Enum()
	findRoom := false
	for _, room := range RoomManager.roomMap {
		if len(roomInfo.sideInfoMap.cMap) < 4 {
			ret.RoomId = proto.String(room.roomId)
			room.addPlayerToRoom(a, false)
			findRoom = true
			break
		}
	}
	if findRoom {
		ret.ErrorCode = GS2CEnterGameRet_SUCCESS.Enum()
	} else {
		ret.ErrorCode = GS2CEnterGameRet_NO_EMPTY_ROOM.Enum()
	}
	a.WriteMsg(ret)

	roomInfo.startBattle()
}

func UpdateExchangeCard(m *pb.C2GSExchangeCard, a gate.Agent) {
	log.Debug("UpdateExchangeCard")
	exchangeCount := len(m.CardList)
	if exchangeCount != 3 {
		log.Error("exchange card count[%v] is error", exchangeCount)
		msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_FAIL_CARD_COUNT_ERROR.Enum(), a)
		return
	}
	player := getPlayerBtAgent(a)
	if player != nil {
		log.Debug("exchange player nickName=%v, roomId=%v", player.nickName, player.roomId)
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			result := roomInfo.updateExchangeCards(m.CardList, player.oid)
			if !result {
				msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_FAIL.Enum(), a)
			} else {
				log.Debug("The exchanging card has update in list.")
				msgHandler.SendGS2CExchangeCardRet(pb.GS2CExchangeCardRet_SUCCESS.Enum(), a)
				if roomInfo.checkExchangeCardOver() {
					roomInfo.processExchangeCard()
				}
			}
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func UpdateLackCard(lackType *pb.CardType, a gate.Agent) {
	log.Debug("UpdateLackCard")
	player := getPlayerBtAgent(a)
	if player != nil {
		log.Debug("player=%v select lack card type=%v", player.oid, lackType.String())
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.updateLack(player.oid, lackType)
			if roomInfo.selectLackOver() {
				roomInfo.sendLackCard()
				roomInfo.dealerStart()
			}
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func UpdateDiscard(cardOid int32, a gate.Agent) {
	log.Debug("UpdateDiscard")
	player := getPlayerBtAgent(a)
	if player != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.recvDiscard(player.oid, cardOid)
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func RobotProcOver(robotOid int32, procType pb.ProcType, a gate.Agent) {
	log.Debug("RobotProcOver, robotOid=%v, procType=%v", robotOid, procType)
	player := getPlayerBtAgent(a)
	if player != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.robotProcOver(robotOid, procType)
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func PlayerEnsureProc(procType pb.ProcType, procCardId int32, a gate.Agent) {
	log.Debug("PlayerEnsureProc, procType=%v", procType)
	player := getPlayerBtAgent(a)
	if player != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.playerEnsureProc(player.oid, procType, procCardId)
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}
