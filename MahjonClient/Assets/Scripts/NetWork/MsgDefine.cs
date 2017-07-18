using UnityEngine;
using System.Collections;

public enum MsgDef
{
    None = -1,
    C2GSLogin = 0,
    GS2CLoginRet = 1,
    C2GSEnterGame = 2,
    GS2CEnterGameRet = 3,
    GS2CUpdateRoomInfo = 4,
    GS2CBattleStart = 5,
    C2GSExchangeCard = 6,
    GS2CUpdateExchangeOverPlayer = 7,
    C2GSSelectLack = 8,
    GS2CSelectLackRet = 9,
    GS2CDiscardTimeOut = 10,
    C2GSDiscard = 11,
    GS2CDiscardRet = 12,
}
