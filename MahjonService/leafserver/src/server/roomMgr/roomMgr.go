package roomMgr

import (
	"math/rand"
	"server/pb"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

type RoomPlayerInfo struct {
	agent  gate.Agent
	player *pb.BattlePlayerInfo
}

type RoomInfo struct {
	playerList []*RoomPlayerInfo
}

// ---------------------
// | roomId | RoomInfo |
// ---------------------
var RoomChanDict map[string]*RoomInfo
var lock_dict sync.Mutex

func Init() {
	log.Debug("init roomMgr...")
	ChanPlayerDict = make(map[gate.Agent]*PlayerInfo)
	RoomChanDict = make(map[string]*RoomInfo)
}

func ReqNewRoom(a gate.Agent) string {
	log.Debug("ReqNewRoom")
	newRoomId := getRandomRoomId(6)
	lock_dict.Lock()
	defer lock_dict.Unlock()
	for {
		_, ok := RoomChanDict[newRoomId]
		if ok {
			//exist
			newRoomId = getRandomRoomId(6)
		} else {
			addPlayerToRoom(newRoomId, a, true)
			break
		}
	}
	return newRoomId
}

func getRandomRoomId(length int) string {
	log.Debug("getRandomRoomId")
	rand.Seed(time.Now().UnixNano())
	rs := make([]string, length)
	for start := 0; start < length; start++ {
		t := rand.Intn(3)
		if t == 0 {
			//0~9数字
			rs = append(rs, strconv.Itoa(rand.Intn(10)))
		} else if t == 1 {
			//26个小写字母
			rs = append(rs, string(rand.Intn(26)+65))
		} else if t == 2 {
			//26个大写字母
			rs = append(rs, string(rand.Intn(26)+97))
		}
	}
	return strings.Join(rs, "") //使用""拼接rs切片
}

func addPlayerToRoom(roomId string, a gate.Agent, isOwner bool) bool {
	log.Debug("add player to room=", roomId)
	//battlePlayerInfo
	battlePlayer := &pb.BattlePlayerInfo{}
	chanPlayer := getPlayerBtAgent(a)
	if chanPlayer == nil {
		log.Error("player has not logined, can't add to room.")
		return false
	}
	sideList := getLeftSideList(roomId)
	battlePlayer.Side = getRandomSideBySideList(sideList)
	battlePlayer.IsOwner = &isOwner
	battlePlayer.Player = &pb.PlayerInfo{}
	battlePlayer.Player.Oid = &chanPlayer._oid
	battlePlayer.Player.NickName = &chanPlayer._nickName
	battlePlayer.Player.HeadIcon = &chanPlayer._headIcon
	battlePlayer.Player.Gold = &chanPlayer._gold
	battlePlayer.Player.Diamond = &chanPlayer._diamond

	//roomPlayer
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.agent = a
	roomPlayer.player = battlePlayer

	//room
	lock_dict.Lock()
	defer lock_dict.Unlock()
	if _, ok := RoomChanDict[roomId]; ok {
		RoomChanDict[roomId].playerList = append(RoomChanDict[roomId].playerList, roomPlayer)
	} else {
		room := &RoomInfo{}
		room.playerList = append(room.playerList, roomPlayer)
		RoomChanDict[roomId] = room
	}

	// send update room playr event
	data := &pb.GS2CUpdateRoomInfo{}
	data.Player = append(data.Player, battlePlayer)
	data.Status = pb.GS2CUpdateRoomInfo_ADD.Enum()
	for n, value := range RoomChanDict[roomId].playerList {
		value.agent.WriteMsg(data)
	}

	return true
}

func OutRoom(roomId string, a gate.Agent) {
	log.Debug("out room=", roomId)
	/*if _, ok := RoomChanDict[roomId]; ok {
		chanlist := RoomChanDict[roomId]
		for n, value := range chanlist {
			log.Debug("n=", n)
			if value == a {
				break
			}
		}
	} else {
		log.Error("room ", roomId, " not exist.")
	}*/
}

func getLeftSideList(roomId string) []pb.BattleSide {
	origList := []pb.BattleSide{pb.BattleSide_east, pb.BattleSide_south, pb.BattleSide_west, pb.BattleSide_north}
	lock_dict.Lock()
	defer lock_dict.Unlock()
	if _, ok := RoomChanDict[roomId]; ok {
		result := []pb.BattleSide{}
		for n, value := range origList {
			log.Debug("n=", n)
			curSide := value
			isFind := false
			for i, player := range RoomChanDict[roomId].playerList {
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
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(sideList))
	return &sideList[rnd]
}

func JoinRoom(roomId string, a gate.Agent) *pb.GS2CEnterGameRet_ErrorCode {
	return pb.GS2CEnterGameRet_SUCCESS.Enum()
}
