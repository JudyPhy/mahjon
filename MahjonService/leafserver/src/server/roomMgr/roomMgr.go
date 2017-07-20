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

type RoomPlayerInfo struct {
	isRobot    bool
	agent      gate.Agent
	side       *pb.BattleSide
	isOwner    bool
	playerInfo *PlayerInfo
}

type CardList struct {
	playerId int32
	list     []*Card
}

type RoomInfo struct {
	playerList     []*RoomPlayerInfo
	cardList_east  *CardList
	cardList_south *CardList
	cardList_west  *CardList
	cardList_north *CardList
}

// ---------------------
// | roomId | RoomInfo |
// ---------------------
type mgrRoom struct {
	lock    sync.Mutex
	roomMap map[string]*RoomInfo
}

var RoomManager *mgrRoom

func getLeftSideList(roomId string) []*pb.BattleSide {
	origList := []*pb.BattleSide{pb.BattleSide_east.Enum(), pb.BattleSide_south.Enum(), pb.BattleSide_west.Enum(), pb.BattleSide_north.Enum()}
	if _, ok := RoomManager.roomMap[roomId]; ok {
		result := []*pb.BattleSide{}
		for n, value := range origList {
			log.Debug("n=", n)
			curSide := value
			isFind := false
			for i, player := range RoomManager.roomMap[roomId].playerList {
				log.Debug("i=", i)
				if player.side == curSide {
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

func getRandomSideBySideList(sideList []*pb.BattleSide) *pb.BattleSide {
	log.Debug("getRandomSideBySideList, left side list count=", len(sideList))
	rand.Seed(time.Now().Unix())
	rnd := rand.Intn(len(sideList))
	return sideList[rnd]
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

func playerInfoToPbPlayerInfo(info *PlayerInfo) *pb.PlayerInfo {
	player := &pb.PlayerInfo{}
	player.Oid = proto.Int32(info.oid)
	player.NickName = proto.String(info.nickName)
	player.HeadIcon = proto.String(info.headIcon)
	player.Gold = proto.Int32(info.gold)
	player.Diamond = proto.Int32(info.diamond)
	return player
}

// RoomPlayerInfo -> pb.BattlePlayerInfo
func playerInfoToPbBattlePlayerInfo(info *RoomPlayerInfo) *pb.BattlePlayerInfo {
	player := &pb.BattlePlayerInfo{}
	player.Side = info.side
	player.IsOwner = proto.Bool(info.isOwner)
	player.Player = playerInfoToPbPlayerInfo(info.playerInfo)
	return player
}

func reqDealer(roomId string) int32 {
	dealerId := int32(0)
	RoomManager.lock.Lock()
	if _, ok := RoomManager.roomMap[roomId]; ok {
		count := len(RoomManager.roomMap[roomId].playerList)
		rand.Seed(time.Now().UnixNano())
		index := rand.Intn(count)
		dealerId = RoomManager.roomMap[roomId].playerList[index].playerInfo.oid
	}
	RoomManager.lock.Unlock()
	return dealerId
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

func ReqNewRoom(a gate.Agent) string {
	log.Debug("ReqNewRoom")
	newRoomId := getRandomRoomId(6)
	RoomManager.lock.Lock()
	for {
		_, ok := RoomManager.roomMap[newRoomId]
		if ok {
			newRoomId = getRandomRoomId(6)
		} else {
			RoomManager.roomMap[newRoomId] = &RoomInfo{}
			break
		}
	}
	RoomManager.lock.Unlock()
	return newRoomId
}

//添加真实玩家到房间中
func AddPlayerToRoom(roomId string, a gate.Agent, isOwner bool) bool {
	log.Debug("add player to room=", roomId)

	//roomPlayer
	basePlayer := getPlayerBtAgent(a)
	if basePlayer == nil {
		log.Error("player has not logined, can't add.")
		return false
	}
	sideList := getLeftSideList(roomId)
	side := getRandomSideBySideList(sideList)
	roomPlayer := &RoomPlayerInfo{}
	roomPlayer.isRobot = false
	roomPlayer.agent = a
	roomPlayer.side = side
	roomPlayer.isOwner = isOwner
	roomPlayer.playerInfo = basePlayer

	//room
	log.Debug("prepare room info")
	RoomManager.lock.Lock()
	if _, ok := RoomManager.roomMap[roomId]; ok {
		RoomManager.roomMap[roomId].playerList = append(RoomManager.roomMap[roomId].playerList, roomPlayer)
	} else {
		room := &RoomInfo{}
		room.playerList = append(room.playerList, roomPlayer)
		RoomManager.roomMap[roomId] = room
	}

	// send update room playr event
	log.Debug("send add room player info to client")
	battlePlayer := playerInfoToPbBattlePlayerInfo(roomPlayer)
	var players []*pb.BattlePlayerInfo
	players = append(players, battlePlayer)
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	for n, value := range RoomManager.roomMap[roomId].playerList {
		log.Debug("n=", n)
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
	RoomManager.lock.Unlock()
	return true
}

func GetRoomMemberCount(roomId string) int {
	return len(RoomManager.roomMap[roomId].playerList)
}

func SendAddedRobotMember(roomId string) {
	log.Debug("SendAddedRobotMember, roomId=%d", roomId)
	RoomManager.lock.Lock()
	var players []*pb.BattlePlayerInfo
	for n, value := range RoomManager.roomMap[roomId].playerList {
		if n == 0 {
		}
		if value.isRobot {
			battlePlayer := playerInfoToPbBattlePlayerInfo(value)
			players = append(players, battlePlayer)
		}
	}
	status := pb.GS2CUpdateRoomInfo_ADD.Enum()
	for n, value := range RoomManager.roomMap[roomId].playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CUpdateRoomInfo(players, status, value.agent)
		}
	}
	RoomManager.lock.Unlock()
}

func getCardListBySide(side *pb.BattleSide, roomInfo *RoomInfo) *CardList {
	if side == pb.BattleSide_east.Enum() {
		return roomInfo.cardList_east
	} else if side == pb.BattleSide_south.Enum() {
		return roomInfo.cardList_south
	} else if side == pb.BattleSide_west.Enum() {
		return roomInfo.cardList_west
	} else if side == pb.BattleSide_north.Enum() {
		return roomInfo.cardList_north
	}
	return nil
}

func StartBattle(roomId string) {
	log.Debug("StartBattle, roomId=%d", roomId)
	RoomManager.lock.Lock()

	dealerId := reqDealer(roomId)
	log.Debug("dealerId=%d", dealerId)

	// deal cards
	var allPlayerCards []*pb.CardInfo
	loadAllCards()
	roomInfo := RoomManager.roomMap[roomId]
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		cardList := getCardListBySide(value.side, roomInfo)
		cardList = &CardList{}
		cardList.playerId = value.playerInfo.oid
		cardList.list = getCardListByBattleStart()
		log.Debug("side=", value.side, ", deal card count=", len(cardList.list))
		for n := 0; n < len(cardList.list); n++ {
			card := &pb.CardInfo{}
			card.PlayerId = proto.Int32(value.playerInfo.oid)
			card.CardOid = proto.Int32(cardList.list[n].oid)
			card.CardId = proto.Int32(cardList.list[n].id)
			card.Status = pb.CardStatus_noDeal.Enum()
			allPlayerCards = append(allPlayerCards, card)
		}
	}

	//prepare send
	log.Debug("battle start, deal card count=", len(allPlayerCards))
	for n, value := range roomInfo.playerList {
		if n == 0 {
		}
		if !value.isRobot && value.agent != nil {
			msgHandler.SendGS2CBattleStart(dealerId, allPlayerCards, value.agent)
		}
	}
	RoomManager.lock.Unlock()
}

func OutRoom(roomId string, a gate.Agent) {
	log.Debug("out room=", roomId)
	RoomManager.lock.Lock()
	if _, ok := RoomManager.roomMap[roomId]; ok {
		chanlist := RoomManager.roomMap[roomId].playerList
		log.Debug("before=>player count in room", roomId, " :", len(RoomManager.roomMap[roomId].playerList))
		for n, value := range chanlist {
			if value.agent == a {
				chanlist = append(chanlist[:n], chanlist[n+1:]...)
				log.Debug("after offline=>player count in room", roomId, " :", len(RoomManager.roomMap[roomId].playerList))

				//send remove player event to client
				log.Debug("send remove room player info to client")
				battlePlayer := &pb.BattlePlayerInfo{}
				playerInfo := getPlayerBtAgent(a)
				battlePlayer.Player = &pb.PlayerInfo{}
				battlePlayer.Player.Oid = proto.Int32(playerInfo.oid)
				data := &pb.GS2CUpdateRoomInfo{}
				data.Player = append(data.Player, battlePlayer)
				data.Status = pb.GS2CUpdateRoomInfo_REMOVE.Enum()
				for n, value := range RoomManager.roomMap[roomId].playerList {
					log.Debug("n=", n)
					value.agent.WriteMsg(data)
				}
				break
			}
		}
	} else {
		log.Error("room ", roomId, " not exist.")
	}
	RoomManager.lock.Unlock()
}

func JoinRoom(roomId string, a gate.Agent) *pb.GS2CEnterGameRet_ErrorCode {
	memberCount := len(RoomManager.roomMap)
	if memberCount >= 4 {
		return pb.GS2CEnterGameRet_PLAYER_COUNT_LIMITE.Enum()
	}
	RoomManager.lock.Lock()
	result := AddPlayerToRoom(roomId, a, false)
	RoomManager.lock.Unlock()
	if result {
		return pb.GS2CEnterGameRet_SUCCESS.Enum()
	} else {
		return pb.GS2CEnterGameRet_FAIL.Enum()
	}
}
