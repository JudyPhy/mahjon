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
	cMap map[gate.Agent]*Player
}

type Player struct {
	oid      int32
	nickName string
	headIcon string
	gold     int32
	diamond  int32

	roomId string
}

var AgentPlayer *ChanPlayer

func AddChanPlayerInfo(a gate.Agent, player *Player) bool {
	log.Debug("AddChanPlayerInfo")
	ret := false
	AgentPlayer.lock.Lock()
	if _, ok := AgentPlayer.cMap[a]; ok {
		log.Error("the agent has existed, don't need add to dict.")
	} else {
		log.Debug("add new player agent, player=%v", player.oid)
		AgentPlayer.cMap[a] = player
		ret = true
	}
	AgentPlayer.lock.Unlock()
	return ret
}

func DeleteChan(a gate.Agent) {
	log.Debug("DeleteChan, addr=%v", a.RemoteAddr())
	AgentPlayer.lock.Lock()
	if player, ok := AgentPlayer.cMap[a]; ok {
		if roomId := AgentPlayer.cMap[a].roomId; roomId != "" {
			dispatcher := eventDispatch.GetSingletonDispatcher()
			params := make(map[string]interface{})
			params["roomId"] = roomId
			params["playerOid"] = player.oid
			event := eventDispatch.CreateEvent("outRoom", params)
			dispatcher.TriggerEvent(event)
		}
		delete(AgentPlayer.cMap, a)
	}
	AgentPlayer.lock.Unlock()
}

func GetPlayerBtAgent(a gate.Agent) *Player {
	if _, ok := AgentPlayer.cMap[a]; ok {
		return AgentPlayer.cMap[a]
	} else {
		return nil
	}
}

func (player *Player) ToPbBattlePlayerInfo(side pb.BattleSide, isOwner bool) *pb.BattlePlayerInfo {
	pbPlayer := &pb.BattlePlayerInfo{}
	pbPlayer.Player = &pb.PlayerInfo{}
	pbPlayer.Player.Oid = proto.Int32(player.oid)
	pbPlayer.Player.NickName = proto.String(player.nickName)
	pbPlayer.Player.HeadIcon = proto.String(player.headIcon)
	pbPlayer.Player.Gold = proto.Int32(player.gold)
	pbPlayer.Player.Diamond = proto.Int32(player.diamond)
	pbPlayer.Side = side.Enum()
	pbPlayer.IsOwner = proto.Bool(isOwner)
	return pbPlayer
}
