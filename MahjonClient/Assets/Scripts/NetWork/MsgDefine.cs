using UnityEngine;
using System.Collections;

public enum MsgDef
{
    None = -1,
    C2GSLogin = 0,
    GS2CLoginRet = 1,
    C2GSEnterGame = 2,
    GS2CEnterGameRet = 3,
    GS2CUpdateRoomMember = 4,
    GS2CBattleStart = 5,
    C2GSExchangeCard = 6,
    GS2CExchangeCardRet = 7,
    C2GSSelectLack = 8,
    GS2CSelectLackRet = 9,
    GS2CTurnToNext = 10,
    GS2CInterruptAction = 11,
    C2GSInterruptActionRet = 12,
    GS2CBroadcastProc = 13,
    GS2CGameOver = 14,
}
