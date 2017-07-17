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
    C2GSSelectLack = 6,
    GS2CSelectLackRet = 7,
    GS2CDiscardTimeOut = 8,
    C2GSDiscard = 9,
    GS2CDiscardRet = 10,
}
