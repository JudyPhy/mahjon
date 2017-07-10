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
    GS2CDiscardTimeOut = 6,
    C2GSDiscard = 7,
    GS2CDiscardRet = 8,
}
