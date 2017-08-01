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
	ProcessStatus_WAITING_HU      ProcessStatus = 7
	ProcessStatus_WAITING_GANG    ProcessStatus = 8
	ProcessStatus_WAITING_PENG    ProcessStatus = 9
	ProcessStatus_GAME_OVER       ProcessStatus = 10
)

func (x ProcessStatus) Enum() *ProcessStatus {
	p := new(ProcessStatus)
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

func playerInfoToPbPlayerInfo(info *PlayerInfo) *pb.PlayerInfo {
	player := &pb.PlayerInfo{}
	player.Oid = proto.Int32(info.oid)
	player.NickName = proto.String(info.nickName)
	player.HeadIcon = proto.String(info.headIcon)
	player.Gold = proto.Int32(info.gold)
	player.Diamond = proto.Int32(info.diamond)
	return player
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
	ChanPlayerStruct = &ChanPlayer{}
	ChanPlayerStruct.aPlayerMap = make(map[gate.Agent]*PlayerInfo)

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

func OutRoom(roomId string, a gate.Agent) {
	log.Debug("out room=%v", roomId)
	player := getPlayerBtAgent(a)
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		roomInfo.outRoom(player.oid)
	} else {
		log.Error("room %v not exist.", roomId)
	}
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
			roomInfo.updateDiscard(player.oid, cardOid)
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func PlayerTurnOver(a gate.Agent) {
	log.Debug("PlayerTurnOver")
	player := getPlayerBtAgent(a)
	if player != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.sideInfoTurnOver(player.oid)
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}

func robotSelfGangOver(roomId string) {
	log.Debug("robotSelfGangOver")
	RoomManager.lock.Lock()
	roomInfo, ok := RoomManager.roomMap[roomId]
	RoomManager.lock.Unlock()
	if ok {
		side := roomInfo.getSideByPlayerOid(curTurnPlayerOid)
		roomInfo.sendTurnToNext(side)
	} else {
		log.Debug("room[%v] not exist.")
	}
}

func ProcPlayerPG(procType pb.ProcType, a gate.Agent) {
	log.Debug("ProcPlayerPG")
	player := getPlayerBtAgent(a)
	if player != nil {
		RoomManager.lock.Lock()
		roomInfo, ok := RoomManager.roomMap[player.roomId]
		RoomManager.lock.Unlock()
		if ok {
			roomInfo.procPlayerPG(player.oid, procType)
		} else {
			log.Error("no room[%v]", player.roomId)
		}
	} else {
		log.Error("player not login.")
	}
}
