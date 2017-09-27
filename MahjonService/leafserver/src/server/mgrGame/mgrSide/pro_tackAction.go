package mgrSide

func (sider *SideInfo) TackPassWhenDiscard() {
	sider.process = ProcessStatus_TURN_OVER
}
