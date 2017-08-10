package player

import (
	"server/eventDispatch"
	"server/pb"
	"sync"

	"github.com/golang/protobuf/proto"
	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

// ----------------------
// | agent | Player |
// ----------------------
type ChanPlayer struct {
	lock sync.Mutex
	CMap map[gate.Agent]*Player
}

type Player struct {
	OID      int32
	NickName string
	HeadIcon string
	Gold     int32
	Diamond  int32

	RoomId string
}

var AgentPlayer *ChanPlayer

func AddChanPlayerInfo(a gate.Agent, player *Player) bool {
	log.Debug("AddChanPlayerInfo")
	ret := false
	AgentPlayer.lock.Lock()
	if _, ok := AgentPlayer.CMap[a]; ok {
		log.Error("the agent has existed, don't need add to dict.")
	} else {
		log.Debug("add new player agent, player=%v", player.OID)
		AgentPlayer.CMap[a] = player
		ret = true
	}
	AgentPlayer.lock.Unlock()
	return ret
}

func DeleteChan(a gate.Agent) {
	log.Debug("DeleteChan, addr=%v", a.RemoteAddr())
	AgentPlayer.lock.Lock()
	if player, ok := AgentPlayer.CMap[a]; ok {
		if roomId := AgentPlayer.CMap[a].RoomId; roomId != "" {
			dispatcher := eventDispatch.GetSingletonDispatcher()
			params := make(map[string]interface{})
			params["roomId"] = roomId
			params["playerOid"] = player.OID
			event := eventDispatch.CreateEvent("outRoom", params)
			dispatcher.TriggerEvent(event)
		}
		delete(AgentPlayer.CMap, a)
	}
	AgentPlayer.lock.Unlock()
}

func GetPlayerBtAgent(a gate.Agent) *Player {
	if _, ok := AgentPlayer.CMap[a]; ok {
		return AgentPlayer.CMap[a]
	} else {
		return nil
	}
}

func (player *Player) ToPbPlayerInfo() *pb.PlayerInfo {
	pbPlayer := &pb.PlayerInfo{}
	pbPlayer.Oid = proto.Int32(player.OID)
	pbPlayer.NickName = proto.String(player.NickName)
	pbPlayer.HeadIcon = proto.String(player.HeadIcon)
	pbPlayer.Gold = proto.Int32(player.Gold)
	pbPlayer.Diamond = proto.Int32(player.Diamond)
	return pbPlayer
}

func (player *Player) ToPbBattlePlayerInfo(side pb.BattleSide, isOwner bool) *pb.BattlePlayerInfo {
	pbPlayer := &pb.BattlePlayerInfo{}
	pbPlayer.Player = &pb.PlayerInfo{}
	pbPlayer.Player.Oid = proto.Int32(player.OID)
	pbPlayer.Player.NickName = proto.String(player.NickName)
	pbPlayer.Player.HeadIcon = proto.String(player.HeadIcon)
	pbPlayer.Player.Gold = proto.Int32(player.Gold)
	pbPlayer.Player.Diamond = proto.Int32(player.Diamond)
	pbPlayer.Side = side.Enum()
	pbPlayer.IsOwner = proto.Bool(isOwner)
	return pbPlayer
}

func (player *Player) OffLine(a gate.Agent) {
	log.Debug("OffLine, player%v", player.OID)
	AgentPlayer.lock.Lock()
	_, ok := AgentPlayer.CMap[a]
	AgentPlayer.lock.Unlock()
	if ok {
		AgentPlayer.lock.Lock()
		delete(AgentPlayer.CMap, a)
		AgentPlayer.lock.Unlock()
	} else {
		log.Error("player%v not login.", player.OID)
	}
}
