package mgrSide

import (
	"server/pb"
)

func (sideInfo *SideInfo) GetLackType() pb.CardType {
	return sideInfo.lackType
}
func (sideInfo *SideInfo) SetLackType(lacktype pb.CardType) {
	sideInfo.lackType = lacktype
	sideInfo.process = ProcessStatus_LACK_OVER
}
