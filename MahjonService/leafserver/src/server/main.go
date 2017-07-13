package main

import (
	"server/conf"
	"server/game"
	"server/gate"
	"server/login"

	"github.com/name5566/leaf"
	lconf "github.com/name5566/leaf/conf"

	"server/scripts/mahjon"

	"fmt"
)

func main() {
	lconf.LogLevel = conf.Server.LogLevel
	lconf.LogPath = conf.Server.LogPath
	lconf.LogFlag = conf.LogFlag
	lconf.ConsolePort = conf.Server.ConsolePort
	lconf.ProfilePath = conf.Server.ProfilePath

	mj := mahjon.NewMahjon()

	//fmt.Println(mj.MjCards[10].PlayerId)
	mj.Shuffle()
	fmt.Println(mj.MjCards)
	fmt.Println(mj.MjCards[107])

	cd := mj.Deal(10, false, 3)
	fmt.Println(cd)
	//fmt.Println(mj.HandCards[])

	a, ds := mj.Discard(mj.MjCards[3])
	fmt.Println(a, ds)

	leaf.Run(
		game.Module,
		gate.Module,
		login.Module,
	)

}
