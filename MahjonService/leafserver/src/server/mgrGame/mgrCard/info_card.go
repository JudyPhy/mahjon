package mgrCard

type CardInfo struct {
	Oid       int32
	Id        int32
	PlayerId  int32
	Status    CardStatus
	FromOther bool
}

type CardStatus int32

const (
	CardStatus_Wall      = 1
	CardStatus_InHand    = 2
	CardStatus_Peng      = 3
	CardStatus_Gang      = 4
	CardStatus_DisCard   = 5
	CardStatus_Deal      = 6
	CardStatus_Hu        = 7
	CardStatus_Exchanged = 8
)
