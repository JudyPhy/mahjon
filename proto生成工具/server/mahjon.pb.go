// Code generated by protoc-gen-go.
// source: mahjon.proto
// DO NOT EDIT!

/*
Package pb is a generated protocol buffer package.

It is generated from these files:
	mahjon.proto

It has these top-level messages:
	LackCard
	CardInfo
	BattlePlayerInfo
	PlayerInfo
	C2GSLogin
	GS2CLoginRet
	C2GSEnterGame
	GS2CEnterGameRet
	GS2CUpdateRoomInfo
	GS2CBattleStart
	C2GSExchangeCard
	GS2CExchangeCardRet
	GS2CUpdateCardInfoAfterExchange
	C2GSSelectLack
	GS2CSelectLackRet
	C2GSDiscard
	GS2CDiscardRet
	GS2CTurnToNext
	GS2CRobotProc
	C2GSRobotProcOver
	GS2CPlayerEnsureProc
	C2GSPlayerEnsureProcRet
	GS2CUpdateCardAfterPlayerProc
	GS2CGameOver
*/
package pb

import proto "code.google.com/p/goprotobuf/proto"
import math "math"

// Reference imports to suppress errors if they are not otherwise used.
var _ = proto.Marshal
var _ = math.Inf

type GameMode int32

const (
	GameMode_JoinRoom   GameMode = 1
	GameMode_CreateRoom GameMode = 2
	GameMode_QuickEnter GameMode = 3
)

var GameMode_name = map[int32]string{
	1: "JoinRoom",
	2: "CreateRoom",
	3: "QuickEnter",
}
var GameMode_value = map[string]int32{
	"JoinRoom":   1,
	"CreateRoom": 2,
	"QuickEnter": 3,
}

func (x GameMode) Enum() *GameMode {
	p := new(GameMode)
	*p = x
	return p
}
func (x GameMode) String() string {
	return proto.EnumName(GameMode_name, int32(x))
}
func (x *GameMode) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(GameMode_value, data, "GameMode")
	if err != nil {
		return err
	}
	*x = GameMode(value)
	return nil
}

type BattleSide int32

const (
	BattleSide_east  BattleSide = 1
	BattleSide_south BattleSide = 2
	BattleSide_west  BattleSide = 3
	BattleSide_north BattleSide = 4
	BattleSide_none  BattleSide = 5
)

var BattleSide_name = map[int32]string{
	1: "east",
	2: "south",
	3: "west",
	4: "north",
	5: "none",
}
var BattleSide_value = map[string]int32{
	"east":  1,
	"south": 2,
	"west":  3,
	"north": 4,
	"none":  5,
}

func (x BattleSide) Enum() *BattleSide {
	p := new(BattleSide)
	*p = x
	return p
}
func (x BattleSide) String() string {
	return proto.EnumName(BattleSide_name, int32(x))
}
func (x *BattleSide) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(BattleSide_value, data, "BattleSide")
	if err != nil {
		return err
	}
	*x = BattleSide(value)
	return nil
}

type CardStatus int32

const (
	CardStatus_noDeal  CardStatus = 1
	CardStatus_inHand  CardStatus = 2
	CardStatus_bePeng  CardStatus = 3
	CardStatus_beGang  CardStatus = 4
	CardStatus_discard CardStatus = 5
	CardStatus_hu      CardStatus = 6
)

var CardStatus_name = map[int32]string{
	1: "noDeal",
	2: "inHand",
	3: "bePeng",
	4: "beGang",
	5: "discard",
	6: "hu",
}
var CardStatus_value = map[string]int32{
	"noDeal":  1,
	"inHand":  2,
	"bePeng":  3,
	"beGang":  4,
	"discard": 5,
	"hu":      6,
}

func (x CardStatus) Enum() *CardStatus {
	p := new(CardStatus)
	*p = x
	return p
}
func (x CardStatus) String() string {
	return proto.EnumName(CardStatus_name, int32(x))
}
func (x *CardStatus) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(CardStatus_value, data, "CardStatus")
	if err != nil {
		return err
	}
	*x = CardStatus(value)
	return nil
}

type CardType int32

const (
	CardType_Wan  CardType = 1
	CardType_Tiao CardType = 2
	CardType_Tong CardType = 3
	CardType_None CardType = 4
)

var CardType_name = map[int32]string{
	1: "Wan",
	2: "Tiao",
	3: "Tong",
	4: "None",
}
var CardType_value = map[string]int32{
	"Wan":  1,
	"Tiao": 2,
	"Tong": 3,
	"None": 4,
}

func (x CardType) Enum() *CardType {
	p := new(CardType)
	*p = x
	return p
}
func (x CardType) String() string {
	return proto.EnumName(CardType_name, int32(x))
}
func (x *CardType) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(CardType_value, data, "CardType")
	if err != nil {
		return err
	}
	*x = CardType(value)
	return nil
}

type ExchangeType int32

const (
	ExchangeType_ClockWise ExchangeType = 1
	ExchangeType_AntiClock ExchangeType = 2
	ExchangeType_Opposite  ExchangeType = 3
)

var ExchangeType_name = map[int32]string{
	1: "ClockWise",
	2: "AntiClock",
	3: "Opposite",
}
var ExchangeType_value = map[string]int32{
	"ClockWise": 1,
	"AntiClock": 2,
	"Opposite":  3,
}

func (x ExchangeType) Enum() *ExchangeType {
	p := new(ExchangeType)
	*p = x
	return p
}
func (x ExchangeType) String() string {
	return proto.EnumName(ExchangeType_name, int32(x))
}
func (x *ExchangeType) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(ExchangeType_value, data, "ExchangeType")
	if err != nil {
		return err
	}
	*x = ExchangeType(value)
	return nil
}

type ProcType int32

const (
	ProcType_SelfGang  ProcType = 1
	ProcType_GangOther ProcType = 2
	ProcType_Peng      ProcType = 3
	ProcType_SelfHu    ProcType = 4
	ProcType_HuOther   ProcType = 5
	ProcType_Discard   ProcType = 6
)

var ProcType_name = map[int32]string{
	1: "SelfGang",
	2: "GangOther",
	3: "Peng",
	4: "SelfHu",
	5: "HuOther",
	6: "Discard",
}
var ProcType_value = map[string]int32{
	"SelfGang":  1,
	"GangOther": 2,
	"Peng":      3,
	"SelfHu":    4,
	"HuOther":   5,
	"Discard":   6,
}

func (x ProcType) Enum() *ProcType {
	p := new(ProcType)
	*p = x
	return p
}
func (x ProcType) String() string {
	return proto.EnumName(ProcType_name, int32(x))
}
func (x *ProcType) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(ProcType_value, data, "ProcType")
	if err != nil {
		return err
	}
	*x = ProcType(value)
	return nil
}

type TurnSwitchType int32

const (
	TurnSwitchType_Normal         TurnSwitchType = 1
	TurnSwitchType_JustCanDiscard TurnSwitchType = 2
	TurnSwitchType_NotDrawCard    TurnSwitchType = 3
)

var TurnSwitchType_name = map[int32]string{
	1: "Normal",
	2: "JustCanDiscard",
	3: "NotDrawCard",
}
var TurnSwitchType_value = map[string]int32{
	"Normal":         1,
	"JustCanDiscard": 2,
	"NotDrawCard":    3,
}

func (x TurnSwitchType) Enum() *TurnSwitchType {
	p := new(TurnSwitchType)
	*p = x
	return p
}
func (x TurnSwitchType) String() string {
	return proto.EnumName(TurnSwitchType_name, int32(x))
}
func (x *TurnSwitchType) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(TurnSwitchType_value, data, "TurnSwitchType")
	if err != nil {
		return err
	}
	*x = TurnSwitchType(value)
	return nil
}

type GS2CLoginRet_ErrorCode int32

const (
	GS2CLoginRet_SUCCESS        GS2CLoginRet_ErrorCode = 1
	GS2CLoginRet_ACCOUNT_ERROR  GS2CLoginRet_ErrorCode = 2
	GS2CLoginRet_PASSWORD_ERROR GS2CLoginRet_ErrorCode = 3
	GS2CLoginRet_FAIL           GS2CLoginRet_ErrorCode = 4
)

var GS2CLoginRet_ErrorCode_name = map[int32]string{
	1: "SUCCESS",
	2: "ACCOUNT_ERROR",
	3: "PASSWORD_ERROR",
	4: "FAIL",
}
var GS2CLoginRet_ErrorCode_value = map[string]int32{
	"SUCCESS":        1,
	"ACCOUNT_ERROR":  2,
	"PASSWORD_ERROR": 3,
	"FAIL":           4,
}

func (x GS2CLoginRet_ErrorCode) Enum() *GS2CLoginRet_ErrorCode {
	p := new(GS2CLoginRet_ErrorCode)
	*p = x
	return p
}
func (x GS2CLoginRet_ErrorCode) String() string {
	return proto.EnumName(GS2CLoginRet_ErrorCode_name, int32(x))
}
func (x *GS2CLoginRet_ErrorCode) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(GS2CLoginRet_ErrorCode_value, data, "GS2CLoginRet_ErrorCode")
	if err != nil {
		return err
	}
	*x = GS2CLoginRet_ErrorCode(value)
	return nil
}

type GS2CEnterGameRet_ErrorCode int32

const (
	GS2CEnterGameRet_SUCCESS             GS2CEnterGameRet_ErrorCode = 1
	GS2CEnterGameRet_FAIL                GS2CEnterGameRet_ErrorCode = 2
	GS2CEnterGameRet_PLAYER_COUNT_LIMITE GS2CEnterGameRet_ErrorCode = 3
)

var GS2CEnterGameRet_ErrorCode_name = map[int32]string{
	1: "SUCCESS",
	2: "FAIL",
	3: "PLAYER_COUNT_LIMITE",
}
var GS2CEnterGameRet_ErrorCode_value = map[string]int32{
	"SUCCESS":             1,
	"FAIL":                2,
	"PLAYER_COUNT_LIMITE": 3,
}

func (x GS2CEnterGameRet_ErrorCode) Enum() *GS2CEnterGameRet_ErrorCode {
	p := new(GS2CEnterGameRet_ErrorCode)
	*p = x
	return p
}
func (x GS2CEnterGameRet_ErrorCode) String() string {
	return proto.EnumName(GS2CEnterGameRet_ErrorCode_name, int32(x))
}
func (x *GS2CEnterGameRet_ErrorCode) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(GS2CEnterGameRet_ErrorCode_value, data, "GS2CEnterGameRet_ErrorCode")
	if err != nil {
		return err
	}
	*x = GS2CEnterGameRet_ErrorCode(value)
	return nil
}

type GS2CUpdateRoomInfo_Status int32

const (
	GS2CUpdateRoomInfo_ADD    GS2CUpdateRoomInfo_Status = 1
	GS2CUpdateRoomInfo_REMOVE GS2CUpdateRoomInfo_Status = 2
	GS2CUpdateRoomInfo_UPDATE GS2CUpdateRoomInfo_Status = 3
)

var GS2CUpdateRoomInfo_Status_name = map[int32]string{
	1: "ADD",
	2: "REMOVE",
	3: "UPDATE",
}
var GS2CUpdateRoomInfo_Status_value = map[string]int32{
	"ADD":    1,
	"REMOVE": 2,
	"UPDATE": 3,
}

func (x GS2CUpdateRoomInfo_Status) Enum() *GS2CUpdateRoomInfo_Status {
	p := new(GS2CUpdateRoomInfo_Status)
	*p = x
	return p
}
func (x GS2CUpdateRoomInfo_Status) String() string {
	return proto.EnumName(GS2CUpdateRoomInfo_Status_name, int32(x))
}
func (x *GS2CUpdateRoomInfo_Status) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(GS2CUpdateRoomInfo_Status_value, data, "GS2CUpdateRoomInfo_Status")
	if err != nil {
		return err
	}
	*x = GS2CUpdateRoomInfo_Status(value)
	return nil
}

type GS2CExchangeCardRet_ErrorCode int32

const (
	GS2CExchangeCardRet_SUCCESS               GS2CExchangeCardRet_ErrorCode = 1
	GS2CExchangeCardRet_FAIL                  GS2CExchangeCardRet_ErrorCode = 2
	GS2CExchangeCardRet_FAIL_CARD_COUNT_ERROR GS2CExchangeCardRet_ErrorCode = 3
)

var GS2CExchangeCardRet_ErrorCode_name = map[int32]string{
	1: "SUCCESS",
	2: "FAIL",
	3: "FAIL_CARD_COUNT_ERROR",
}
var GS2CExchangeCardRet_ErrorCode_value = map[string]int32{
	"SUCCESS":               1,
	"FAIL":                  2,
	"FAIL_CARD_COUNT_ERROR": 3,
}

func (x GS2CExchangeCardRet_ErrorCode) Enum() *GS2CExchangeCardRet_ErrorCode {
	p := new(GS2CExchangeCardRet_ErrorCode)
	*p = x
	return p
}
func (x GS2CExchangeCardRet_ErrorCode) String() string {
	return proto.EnumName(GS2CExchangeCardRet_ErrorCode_name, int32(x))
}
func (x *GS2CExchangeCardRet_ErrorCode) UnmarshalJSON(data []byte) error {
	value, err := proto.UnmarshalJSONEnum(GS2CExchangeCardRet_ErrorCode_value, data, "GS2CExchangeCardRet_ErrorCode")
	if err != nil {
		return err
	}
	*x = GS2CExchangeCardRet_ErrorCode(value)
	return nil
}

type LackCard struct {
	PlayerId         *int32    `protobuf:"varint,1,req,name=playerId" json:"playerId,omitempty"`
	Type             *CardType `protobuf:"varint,2,req,name=type,enum=pb.CardType" json:"type,omitempty"`
	XXX_unrecognized []byte    `json:"-"`
}

func (m *LackCard) Reset()         { *m = LackCard{} }
func (m *LackCard) String() string { return proto.CompactTextString(m) }
func (*LackCard) ProtoMessage()    {}

func (m *LackCard) GetPlayerId() int32 {
	if m != nil && m.PlayerId != nil {
		return *m.PlayerId
	}
	return 0
}

func (m *LackCard) GetType() CardType {
	if m != nil && m.Type != nil {
		return *m.Type
	}
	return CardType_Wan
}

type CardInfo struct {
	PlayerId         *int32      `protobuf:"varint,1,req,name=playerId" json:"playerId,omitempty"`
	CardOid          *int32      `protobuf:"varint,2,req" json:"CardOid,omitempty"`
	CardId           *int32      `protobuf:"varint,3,req" json:"CardId,omitempty"`
	Status           *CardStatus `protobuf:"varint,4,req,enum=pb.CardStatus" json:"Status,omitempty"`
	FromOther        *bool       `protobuf:"varint,5,opt,name=fromOther" json:"fromOther,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *CardInfo) Reset()         { *m = CardInfo{} }
func (m *CardInfo) String() string { return proto.CompactTextString(m) }
func (*CardInfo) ProtoMessage()    {}

func (m *CardInfo) GetPlayerId() int32 {
	if m != nil && m.PlayerId != nil {
		return *m.PlayerId
	}
	return 0
}

func (m *CardInfo) GetCardOid() int32 {
	if m != nil && m.CardOid != nil {
		return *m.CardOid
	}
	return 0
}

func (m *CardInfo) GetCardId() int32 {
	if m != nil && m.CardId != nil {
		return *m.CardId
	}
	return 0
}

func (m *CardInfo) GetStatus() CardStatus {
	if m != nil && m.Status != nil {
		return *m.Status
	}
	return CardStatus_noDeal
}

func (m *CardInfo) GetFromOther() bool {
	if m != nil && m.FromOther != nil {
		return *m.FromOther
	}
	return false
}

type BattlePlayerInfo struct {
	Side             *BattleSide `protobuf:"varint,1,req,name=side,enum=pb.BattleSide" json:"side,omitempty"`
	IsOwner          *bool       `protobuf:"varint,2,req,name=isOwner" json:"isOwner,omitempty"`
	Player           *PlayerInfo `protobuf:"bytes,3,req,name=player" json:"player,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *BattlePlayerInfo) Reset()         { *m = BattlePlayerInfo{} }
func (m *BattlePlayerInfo) String() string { return proto.CompactTextString(m) }
func (*BattlePlayerInfo) ProtoMessage()    {}

func (m *BattlePlayerInfo) GetSide() BattleSide {
	if m != nil && m.Side != nil {
		return *m.Side
	}
	return BattleSide_east
}

func (m *BattlePlayerInfo) GetIsOwner() bool {
	if m != nil && m.IsOwner != nil {
		return *m.IsOwner
	}
	return false
}

func (m *BattlePlayerInfo) GetPlayer() *PlayerInfo {
	if m != nil {
		return m.Player
	}
	return nil
}

type PlayerInfo struct {
	Oid              *int32  `protobuf:"varint,1,req,name=oid" json:"oid,omitempty"`
	NickName         *string `protobuf:"bytes,2,req,name=nickName" json:"nickName,omitempty"`
	HeadIcon         *string `protobuf:"bytes,3,req,name=headIcon" json:"headIcon,omitempty"`
	Gold             *int32  `protobuf:"varint,4,req,name=gold" json:"gold,omitempty"`
	Diamond          *int32  `protobuf:"varint,5,req,name=diamond" json:"diamond,omitempty"`
	XXX_unrecognized []byte  `json:"-"`
}

func (m *PlayerInfo) Reset()         { *m = PlayerInfo{} }
func (m *PlayerInfo) String() string { return proto.CompactTextString(m) }
func (*PlayerInfo) ProtoMessage()    {}

func (m *PlayerInfo) GetOid() int32 {
	if m != nil && m.Oid != nil {
		return *m.Oid
	}
	return 0
}

func (m *PlayerInfo) GetNickName() string {
	if m != nil && m.NickName != nil {
		return *m.NickName
	}
	return ""
}

func (m *PlayerInfo) GetHeadIcon() string {
	if m != nil && m.HeadIcon != nil {
		return *m.HeadIcon
	}
	return ""
}

func (m *PlayerInfo) GetGold() int32 {
	if m != nil && m.Gold != nil {
		return *m.Gold
	}
	return 0
}

func (m *PlayerInfo) GetDiamond() int32 {
	if m != nil && m.Diamond != nil {
		return *m.Diamond
	}
	return 0
}

// ///////////////////////////////////////////////////////////////////
type C2GSLogin struct {
	Account          *string `protobuf:"bytes,1,req,name=account" json:"account,omitempty"`
	Password         *string `protobuf:"bytes,2,req,name=password" json:"password,omitempty"`
	XXX_unrecognized []byte  `json:"-"`
}

func (m *C2GSLogin) Reset()         { *m = C2GSLogin{} }
func (m *C2GSLogin) String() string { return proto.CompactTextString(m) }
func (*C2GSLogin) ProtoMessage()    {}

func (m *C2GSLogin) GetAccount() string {
	if m != nil && m.Account != nil {
		return *m.Account
	}
	return ""
}

func (m *C2GSLogin) GetPassword() string {
	if m != nil && m.Password != nil {
		return *m.Password
	}
	return ""
}

type GS2CLoginRet struct {
	ErrorCode        *GS2CLoginRet_ErrorCode `protobuf:"varint,1,req,name=errorCode,enum=pb.GS2CLoginRet_ErrorCode" json:"errorCode,omitempty"`
	PlayerInfo       *PlayerInfo             `protobuf:"bytes,2,opt,name=playerInfo" json:"playerInfo,omitempty"`
	XXX_unrecognized []byte                  `json:"-"`
}

func (m *GS2CLoginRet) Reset()         { *m = GS2CLoginRet{} }
func (m *GS2CLoginRet) String() string { return proto.CompactTextString(m) }
func (*GS2CLoginRet) ProtoMessage()    {}

func (m *GS2CLoginRet) GetErrorCode() GS2CLoginRet_ErrorCode {
	if m != nil && m.ErrorCode != nil {
		return *m.ErrorCode
	}
	return GS2CLoginRet_SUCCESS
}

func (m *GS2CLoginRet) GetPlayerInfo() *PlayerInfo {
	if m != nil {
		return m.PlayerInfo
	}
	return nil
}

type C2GSEnterGame struct {
	Mode             *GameMode `protobuf:"varint,1,req,name=mode,enum=pb.GameMode" json:"mode,omitempty"`
	RoomId           *string   `protobuf:"bytes,2,opt,name=roomId" json:"roomId,omitempty"`
	XXX_unrecognized []byte    `json:"-"`
}

func (m *C2GSEnterGame) Reset()         { *m = C2GSEnterGame{} }
func (m *C2GSEnterGame) String() string { return proto.CompactTextString(m) }
func (*C2GSEnterGame) ProtoMessage()    {}

func (m *C2GSEnterGame) GetMode() GameMode {
	if m != nil && m.Mode != nil {
		return *m.Mode
	}
	return GameMode_JoinRoom
}

func (m *C2GSEnterGame) GetRoomId() string {
	if m != nil && m.RoomId != nil {
		return *m.RoomId
	}
	return ""
}

type GS2CEnterGameRet struct {
	ErrorCode        *GS2CEnterGameRet_ErrorCode `protobuf:"varint,1,req,name=errorCode,enum=pb.GS2CEnterGameRet_ErrorCode" json:"errorCode,omitempty"`
	Mode             *GameMode                   `protobuf:"varint,2,req,name=mode,enum=pb.GameMode" json:"mode,omitempty"`
	RoomId           *string                     `protobuf:"bytes,3,req,name=roomId" json:"roomId,omitempty"`
	XXX_unrecognized []byte                      `json:"-"`
}

func (m *GS2CEnterGameRet) Reset()         { *m = GS2CEnterGameRet{} }
func (m *GS2CEnterGameRet) String() string { return proto.CompactTextString(m) }
func (*GS2CEnterGameRet) ProtoMessage()    {}

func (m *GS2CEnterGameRet) GetErrorCode() GS2CEnterGameRet_ErrorCode {
	if m != nil && m.ErrorCode != nil {
		return *m.ErrorCode
	}
	return GS2CEnterGameRet_SUCCESS
}

func (m *GS2CEnterGameRet) GetMode() GameMode {
	if m != nil && m.Mode != nil {
		return *m.Mode
	}
	return GameMode_JoinRoom
}

func (m *GS2CEnterGameRet) GetRoomId() string {
	if m != nil && m.RoomId != nil {
		return *m.RoomId
	}
	return ""
}

type GS2CUpdateRoomInfo struct {
	Player           []*BattlePlayerInfo        `protobuf:"bytes,1,rep,name=player" json:"player,omitempty"`
	Status           *GS2CUpdateRoomInfo_Status `protobuf:"varint,2,req,name=status,enum=pb.GS2CUpdateRoomInfo_Status" json:"status,omitempty"`
	XXX_unrecognized []byte                     `json:"-"`
}

func (m *GS2CUpdateRoomInfo) Reset()         { *m = GS2CUpdateRoomInfo{} }
func (m *GS2CUpdateRoomInfo) String() string { return proto.CompactTextString(m) }
func (*GS2CUpdateRoomInfo) ProtoMessage()    {}

func (m *GS2CUpdateRoomInfo) GetPlayer() []*BattlePlayerInfo {
	if m != nil {
		return m.Player
	}
	return nil
}

func (m *GS2CUpdateRoomInfo) GetStatus() GS2CUpdateRoomInfo_Status {
	if m != nil && m.Status != nil {
		return *m.Status
	}
	return GS2CUpdateRoomInfo_ADD
}

type GS2CBattleStart struct {
	DealerId         *int32      `protobuf:"varint,1,req,name=dealerId" json:"dealerId,omitempty"`
	CardList         []*CardInfo `protobuf:"bytes,2,rep,name=cardList" json:"cardList,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *GS2CBattleStart) Reset()         { *m = GS2CBattleStart{} }
func (m *GS2CBattleStart) String() string { return proto.CompactTextString(m) }
func (*GS2CBattleStart) ProtoMessage()    {}

func (m *GS2CBattleStart) GetDealerId() int32 {
	if m != nil && m.DealerId != nil {
		return *m.DealerId
	}
	return 0
}

func (m *GS2CBattleStart) GetCardList() []*CardInfo {
	if m != nil {
		return m.CardList
	}
	return nil
}

type C2GSExchangeCard struct {
	CardList         []*CardInfo `protobuf:"bytes,1,rep,name=cardList" json:"cardList,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *C2GSExchangeCard) Reset()         { *m = C2GSExchangeCard{} }
func (m *C2GSExchangeCard) String() string { return proto.CompactTextString(m) }
func (*C2GSExchangeCard) ProtoMessage()    {}

func (m *C2GSExchangeCard) GetCardList() []*CardInfo {
	if m != nil {
		return m.CardList
	}
	return nil
}

type GS2CExchangeCardRet struct {
	ErrorCode        *GS2CExchangeCardRet_ErrorCode `protobuf:"varint,1,req,name=errorCode,enum=pb.GS2CExchangeCardRet_ErrorCode" json:"errorCode,omitempty"`
	XXX_unrecognized []byte                         `json:"-"`
}

func (m *GS2CExchangeCardRet) Reset()         { *m = GS2CExchangeCardRet{} }
func (m *GS2CExchangeCardRet) String() string { return proto.CompactTextString(m) }
func (*GS2CExchangeCardRet) ProtoMessage()    {}

func (m *GS2CExchangeCardRet) GetErrorCode() GS2CExchangeCardRet_ErrorCode {
	if m != nil && m.ErrorCode != nil {
		return *m.ErrorCode
	}
	return GS2CExchangeCardRet_SUCCESS
}

type GS2CUpdateCardInfoAfterExchange struct {
	Type             *ExchangeType `protobuf:"varint,1,req,name=type,enum=pb.ExchangeType" json:"type,omitempty"`
	CardList         []*CardInfo   `protobuf:"bytes,2,rep,name=cardList" json:"cardList,omitempty"`
	XXX_unrecognized []byte        `json:"-"`
}

func (m *GS2CUpdateCardInfoAfterExchange) Reset()         { *m = GS2CUpdateCardInfoAfterExchange{} }
func (m *GS2CUpdateCardInfoAfterExchange) String() string { return proto.CompactTextString(m) }
func (*GS2CUpdateCardInfoAfterExchange) ProtoMessage()    {}

func (m *GS2CUpdateCardInfoAfterExchange) GetType() ExchangeType {
	if m != nil && m.Type != nil {
		return *m.Type
	}
	return ExchangeType_ClockWise
}

func (m *GS2CUpdateCardInfoAfterExchange) GetCardList() []*CardInfo {
	if m != nil {
		return m.CardList
	}
	return nil
}

type C2GSSelectLack struct {
	Type             *CardType `protobuf:"varint,1,req,name=type,enum=pb.CardType" json:"type,omitempty"`
	XXX_unrecognized []byte    `json:"-"`
}

func (m *C2GSSelectLack) Reset()         { *m = C2GSSelectLack{} }
func (m *C2GSSelectLack) String() string { return proto.CompactTextString(m) }
func (*C2GSSelectLack) ProtoMessage()    {}

func (m *C2GSSelectLack) GetType() CardType {
	if m != nil && m.Type != nil {
		return *m.Type
	}
	return CardType_Wan
}

type GS2CSelectLackRet struct {
	LackCard         []*LackCard `protobuf:"bytes,1,rep,name=lackCard" json:"lackCard,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *GS2CSelectLackRet) Reset()         { *m = GS2CSelectLackRet{} }
func (m *GS2CSelectLackRet) String() string { return proto.CompactTextString(m) }
func (*GS2CSelectLackRet) ProtoMessage()    {}

func (m *GS2CSelectLackRet) GetLackCard() []*LackCard {
	if m != nil {
		return m.LackCard
	}
	return nil
}

type C2GSDiscard struct {
	CardOid          *int32 `protobuf:"varint,1,req,name=cardOid" json:"cardOid,omitempty"`
	XXX_unrecognized []byte `json:"-"`
}

func (m *C2GSDiscard) Reset()         { *m = C2GSDiscard{} }
func (m *C2GSDiscard) String() string { return proto.CompactTextString(m) }
func (*C2GSDiscard) ProtoMessage()    {}

func (m *C2GSDiscard) GetCardOid() int32 {
	if m != nil && m.CardOid != nil {
		return *m.CardOid
	}
	return 0
}

type GS2CDiscardRet struct {
	CardOid          *int32 `protobuf:"varint,1,req,name=cardOid" json:"cardOid,omitempty"`
	XXX_unrecognized []byte `json:"-"`
}

func (m *GS2CDiscardRet) Reset()         { *m = GS2CDiscardRet{} }
func (m *GS2CDiscardRet) String() string { return proto.CompactTextString(m) }
func (*GS2CDiscardRet) ProtoMessage()    {}

func (m *GS2CDiscardRet) GetCardOid() int32 {
	if m != nil && m.CardOid != nil {
		return *m.CardOid
	}
	return 0
}

type GS2CTurnToNext struct {
	PlayerOid        *int32          `protobuf:"varint,1,req,name=playerOid" json:"playerOid,omitempty"`
	Card             *CardInfo       `protobuf:"bytes,2,opt,name=card" json:"card,omitempty"`
	Type             *TurnSwitchType `protobuf:"varint,3,req,name=type,enum=pb.TurnSwitchType" json:"type,omitempty"`
	XXX_unrecognized []byte          `json:"-"`
}

func (m *GS2CTurnToNext) Reset()         { *m = GS2CTurnToNext{} }
func (m *GS2CTurnToNext) String() string { return proto.CompactTextString(m) }
func (*GS2CTurnToNext) ProtoMessage()    {}

func (m *GS2CTurnToNext) GetPlayerOid() int32 {
	if m != nil && m.PlayerOid != nil {
		return *m.PlayerOid
	}
	return 0
}

func (m *GS2CTurnToNext) GetCard() *CardInfo {
	if m != nil {
		return m.Card
	}
	return nil
}

func (m *GS2CTurnToNext) GetType() TurnSwitchType {
	if m != nil && m.Type != nil {
		return *m.Type
	}
	return TurnSwitchType_Normal
}

type GS2CRobotProc struct {
	ProcPlayer       *int32      `protobuf:"varint,1,req,name=procPlayer" json:"procPlayer,omitempty"`
	ProcType         *ProcType   `protobuf:"varint,2,req,name=procType,enum=pb.ProcType" json:"procType,omitempty"`
	BeProcPlayer     *int32      `protobuf:"varint,3,opt,name=beProcPlayer" json:"beProcPlayer,omitempty"`
	CardList         []*CardInfo `protobuf:"bytes,4,rep,name=cardList" json:"cardList,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *GS2CRobotProc) Reset()         { *m = GS2CRobotProc{} }
func (m *GS2CRobotProc) String() string { return proto.CompactTextString(m) }
func (*GS2CRobotProc) ProtoMessage()    {}

func (m *GS2CRobotProc) GetProcPlayer() int32 {
	if m != nil && m.ProcPlayer != nil {
		return *m.ProcPlayer
	}
	return 0
}

func (m *GS2CRobotProc) GetProcType() ProcType {
	if m != nil && m.ProcType != nil {
		return *m.ProcType
	}
	return ProcType_SelfGang
}

func (m *GS2CRobotProc) GetBeProcPlayer() int32 {
	if m != nil && m.BeProcPlayer != nil {
		return *m.BeProcPlayer
	}
	return 0
}

func (m *GS2CRobotProc) GetCardList() []*CardInfo {
	if m != nil {
		return m.CardList
	}
	return nil
}

type C2GSRobotProcOver struct {
	RobotOid         *int32    `protobuf:"varint,1,req,name=robotOid" json:"robotOid,omitempty"`
	ProcType         *ProcType `protobuf:"varint,2,req,name=procType,enum=pb.ProcType" json:"procType,omitempty"`
	XXX_unrecognized []byte    `json:"-"`
}

func (m *C2GSRobotProcOver) Reset()         { *m = C2GSRobotProcOver{} }
func (m *C2GSRobotProcOver) String() string { return proto.CompactTextString(m) }
func (*C2GSRobotProcOver) ProtoMessage()    {}

func (m *C2GSRobotProcOver) GetRobotOid() int32 {
	if m != nil && m.RobotOid != nil {
		return *m.RobotOid
	}
	return 0
}

func (m *C2GSRobotProcOver) GetProcType() ProcType {
	if m != nil && m.ProcType != nil {
		return *m.ProcType
	}
	return ProcType_SelfGang
}

type GS2CPlayerEnsureProc struct {
	ProcPlayer       *int32    `protobuf:"varint,1,req,name=procPlayer" json:"procPlayer,omitempty"`
	ProcType         *ProcType `protobuf:"varint,2,req,name=procType,enum=pb.ProcType" json:"procType,omitempty"`
	BeProcPlayer     *int32    `protobuf:"varint,3,opt,name=beProcPlayer" json:"beProcPlayer,omitempty"`
	ProcCardId       *int32    `protobuf:"varint,4,opt,name=procCardId" json:"procCardId,omitempty"`
	XXX_unrecognized []byte    `json:"-"`
}

func (m *GS2CPlayerEnsureProc) Reset()         { *m = GS2CPlayerEnsureProc{} }
func (m *GS2CPlayerEnsureProc) String() string { return proto.CompactTextString(m) }
func (*GS2CPlayerEnsureProc) ProtoMessage()    {}

func (m *GS2CPlayerEnsureProc) GetProcPlayer() int32 {
	if m != nil && m.ProcPlayer != nil {
		return *m.ProcPlayer
	}
	return 0
}

func (m *GS2CPlayerEnsureProc) GetProcType() ProcType {
	if m != nil && m.ProcType != nil {
		return *m.ProcType
	}
	return ProcType_SelfGang
}

func (m *GS2CPlayerEnsureProc) GetBeProcPlayer() int32 {
	if m != nil && m.BeProcPlayer != nil {
		return *m.BeProcPlayer
	}
	return 0
}

func (m *GS2CPlayerEnsureProc) GetProcCardId() int32 {
	if m != nil && m.ProcCardId != nil {
		return *m.ProcCardId
	}
	return 0
}

type C2GSPlayerEnsureProcRet struct {
	ProcType         *ProcType `protobuf:"varint,1,req,name=procType,enum=pb.ProcType" json:"procType,omitempty"`
	ProcCardId       *int32    `protobuf:"varint,2,opt,name=procCardId" json:"procCardId,omitempty"`
	XXX_unrecognized []byte    `json:"-"`
}

func (m *C2GSPlayerEnsureProcRet) Reset()         { *m = C2GSPlayerEnsureProcRet{} }
func (m *C2GSPlayerEnsureProcRet) String() string { return proto.CompactTextString(m) }
func (*C2GSPlayerEnsureProcRet) ProtoMessage()    {}

func (m *C2GSPlayerEnsureProcRet) GetProcType() ProcType {
	if m != nil && m.ProcType != nil {
		return *m.ProcType
	}
	return ProcType_SelfGang
}

func (m *C2GSPlayerEnsureProcRet) GetProcCardId() int32 {
	if m != nil && m.ProcCardId != nil {
		return *m.ProcCardId
	}
	return 0
}

type GS2CUpdateCardAfterPlayerProc struct {
	CardList         []*CardInfo `protobuf:"bytes,1,rep,name=cardList" json:"cardList,omitempty"`
	XXX_unrecognized []byte      `json:"-"`
}

func (m *GS2CUpdateCardAfterPlayerProc) Reset()         { *m = GS2CUpdateCardAfterPlayerProc{} }
func (m *GS2CUpdateCardAfterPlayerProc) String() string { return proto.CompactTextString(m) }
func (*GS2CUpdateCardAfterPlayerProc) ProtoMessage()    {}

func (m *GS2CUpdateCardAfterPlayerProc) GetCardList() []*CardInfo {
	if m != nil {
		return m.CardList
	}
	return nil
}

type GS2CGameOver struct {
	XXX_unrecognized []byte `json:"-"`
}

func (m *GS2CGameOver) Reset()         { *m = GS2CGameOver{} }
func (m *GS2CGameOver) String() string { return proto.CompactTextString(m) }
func (*GS2CGameOver) ProtoMessage()    {}

func init() {
	proto.RegisterEnum("pb.GameMode", GameMode_name, GameMode_value)
	proto.RegisterEnum("pb.BattleSide", BattleSide_name, BattleSide_value)
	proto.RegisterEnum("pb.CardStatus", CardStatus_name, CardStatus_value)
	proto.RegisterEnum("pb.CardType", CardType_name, CardType_value)
	proto.RegisterEnum("pb.ExchangeType", ExchangeType_name, ExchangeType_value)
	proto.RegisterEnum("pb.ProcType", ProcType_name, ProcType_value)
	proto.RegisterEnum("pb.TurnSwitchType", TurnSwitchType_name, TurnSwitchType_value)
	proto.RegisterEnum("pb.GS2CLoginRet_ErrorCode", GS2CLoginRet_ErrorCode_name, GS2CLoginRet_ErrorCode_value)
	proto.RegisterEnum("pb.GS2CEnterGameRet_ErrorCode", GS2CEnterGameRet_ErrorCode_name, GS2CEnterGameRet_ErrorCode_value)
	proto.RegisterEnum("pb.GS2CUpdateRoomInfo_Status", GS2CUpdateRoomInfo_Status_name, GS2CUpdateRoomInfo_Status_value)
	proto.RegisterEnum("pb.GS2CExchangeCardRet_ErrorCode", GS2CExchangeCardRet_ErrorCode_name, GS2CExchangeCardRet_ErrorCode_value)
}
