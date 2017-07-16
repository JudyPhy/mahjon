package roomMgr

import (
	"math/rand"
	"server/pb"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type RoomPlayerInfo struct {
	agent   gate.Agent
	isRobot bool
	player  *pb.BattlePlayerInfo
}

type RoomInfo struct {
	roomId     string
	playerList []*RoomPlayerInfo
}

// ---------------------
// | roomId | RoomInfo |
// ---------------------
type RoomDict struct {
	lock    sync.Mutex
	roomMap map[string]*RoomInfo
}

var Rooms *RoomDict

func Init() {
	log.Debug("init player map.")
	ChanPlayerStruct = &ChanPlayer{}
	ChanPlayerStruct.aPlayerMap = make(map[gate.Agent]*PlayerInfo)

	log.Debug("init room map.")
	Rooms = &RoomDict{}
	Rooms.roomMap = make(map[string]*RoomInfo)
}

func ReqNewRoom(a gate.Agent) string {
	log.Debug("ReqNewRoom")
	newRoomId := getRandomRoomId(6)
	Rooms.lock.Lock()
	for {
		_, ok := Rooms.roomMap[newRoomId]
		if ok {
			newRoomId = getRandomRoomId(6)
		} else {
			result := addPlayerToRoom(newRoomId, a, true)
			if !result {
				log.Error("add player to room fail.")
			}
			break
		}
	}
	Rooms.lock.Unlock()
	return newRoomId
}

func getRandomRoomId(length int) string {
	log.Debug("getRandomRoomId")
	rand.Seed(time.Now().UnixNano())
	rs := make([]string, length)
	for start := 0; start < length; start++ {
		rs = append(rs, strconv.Itoa(rand.Intn(10)))
	}
	return strings.Join(rs, "") //使用""拼接rs切片
}

func getPbPlayerInfo(playerchan *PlayerInfo) *pb.PlayerInfo {
	player := &pb.PlayerInfo{}
	player.Oid = proto.Int32(playerchan.oid)
	player.NickName = proto.String(playerchan.nickName)
	player.HeadIcon = proto.String(playerchan.headIcon)
	player.Gold = proto.Int32(playerchan.gold)
	player.Diamond = proto.Int32(playerchan.diamond)
	return player
}

//添加真实玩家到房间中
func addPlayerToRoom(roomId string, a gate.Agent, isOwner bool) bool {
	log.Debug("add player to room=", roomId)

	//battlePlayerInfo
	chanPlayer := getPlayerBtAgent(a)
	if chanPlayer == nil {
		log.Error("player has not logined, can't add.")
		return false
	}
	battlePlayer := &pb.BattlePlayerInfo{}
	sideList := getLeftSideList(roomId)
	battlePlayer.Side = getRandomSideBySideList(sideList)
	battlePlayer.IsOwner = proto.Bool(isOwner)
	battlePlayer.Player = getPbPlayerInfo(chanPlayer)

	//roomPlayer
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = false
	roomPlayer.agent = a
	roomPlayer.player = battlePlayer

	//room
	log.Debug("prepare room info")
	if _, ok := Rooms.roomMap[roomId]; ok {
		Rooms.roomMap[roomId].playerList = append(Rooms.roomMap[roomId].playerList, roomPlayer)
	} else {
		room := &RoomInfo{}
		room.playerList = append(room.playerList, roomPlayer)
		Rooms.roomMap[roomId] = room
	}

	// send update room playr event
	log.Debug("send add room player info to client")
	data := &pb.GS2CUpdateRoomInfo{}
	data.Player = append(data.Player, battlePlayer)
	data.Status = pb.GS2CUpdateRoomInfo_ADD.Enum()
	log.Debug("current plater count in room:", len(Rooms.roomMap[roomId].playerList))
	for n, value := range Rooms.roomMap[roomId].playerList {
		log.Debug("n=", n)
		if !value.isRobot && value.agent != nil {
			value.agent.WriteMsg(data)
		}
	}
	return true
}

func getLeftSideList(roomId string) []pb.BattleSide {
	origList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	if _, ok := Rooms.roomMap[roomId]; ok {
		result := []pb.BattleSide{}
		for n, value := range origList {
			log.Debug("n=", n)
			curSide := value
			isFind := false
			for i, player := range Rooms.roomMap[roomId].playerList {
				log.Debug("i=", i)
				if player.player.GetSide() == curSide {
					isFind = true
					break
				}
			}
			if !isFind {
				result = append(result, curSide)
			}
		}
		log.Debug("current side list count is ", len(result))
		return result
	} else {
		return origList
	}
}

func getRandomSideBySideList(sideList []pb.BattleSide) *pb.BattleSide {
	log.Debug("getRandomSideBySideList, left side list count=", len(sideList))
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(sideList))
	return &sideList[rnd]
}

func TestGetSide(roomId string) *pb.BattleSide {
	leftList := getLeftSideList(roomId)
	return getRandomSideBySideList(leftList)
}

func OutRoom(roomId string, a gate.Agent) {
	log.Debug("out room=", roomId)
	Rooms.lock.Lock()
	if _, ok := Rooms.roomMap[roomId]; ok {
		chanlist := Rooms.roomMap[roomId].playerList
		log.Debug("before=>player count in room", roomId, " :", len(Rooms.roomMap[roomId].playerList))
		for n, value := range chanlist {
			if value.agent == a {
				chanlist = append(chanlist[:n], chanlist[n+1:]...)
				log.Debug("after offline=>player count in room", roomId, " :", len(Rooms.roomMap[roomId].playerList))

				//send remove player event to client
				log.Debug("send remove room player info to client")
				battlePlayer := &pb.BattlePlayerInfo{}
				playerInfo := getPlayerBtAgent(a)
				battlePlayer.Player = &pb.PlayerInfo{}
				battlePlayer.Player.Oid = proto.Int32(playerInfo.oid)
				data := &pb.GS2CUpdateRoomInfo{}
				data.Player = append(data.Player, battlePlayer)
				data.Status = pb.GS2CUpdateRoomInfo_REMOVE.Enum()
				for n, value := range Rooms.roomMap[roomId].playerList {
					log.Debug("n=", n)
					value.agent.WriteMsg(data)
				}
				break
			}
		}
	} else {
		log.Error("room ", roomId, " not exist.")
	}
	Rooms.lock.Unlock()
}

func JoinRoom(roomId string, a gate.Agent) *pb.GS2CEnterGameRet_ErrorCode {
	memberCount := len(Rooms.roomMap)
	if memberCount >= 4 {
		return pb.GS2CEnterGameRet_PLAYER_COUNT_LIMITE.Enum()
	}
	Rooms.lock.Lock()
	result := addPlayerToRoom(roomId, a, false)
	Rooms.lock.Unlock()
	if result {
		return pb.GS2CEnterGameRet_SUCCESS.Enum()
	} else {
		return pb.GS2CEnterGameRet_FAIL.Enum()
	}
}

func GetDealerId() int32 {
	return 0
}
