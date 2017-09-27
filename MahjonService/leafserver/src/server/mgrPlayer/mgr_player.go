package mgrPlayer

import (
	"server/pb"

	"github.com/name5566/leaf/gate"
	"github.com/name5566/leaf/log"
)

var (
	a2pMap = make(map[gate.Agent]*PlayerInfo)
)

func Binda2pMap(a gate.Agent, player *pb.PlayerInfo) {
	log.Debug("playerMgr==>绑定玩家信息:玩家=%v", player.GetOID())
	_, ok := a2pMap[a]
	if ok {
		log.Debug("playerMgr==>Binda2pMap==>代理路径被占用")
		return
	}
	newPlayer := &PlayerInfo{
		oid:      player.GetOID(),
		nickName: player.GetNickName(),
		headIcon: player.GetHeadIcon(),
		gold:     player.GetGold(),
		fangka:   player.GetFangka(),
	}
	a2pMap[a] = newPlayer
}
