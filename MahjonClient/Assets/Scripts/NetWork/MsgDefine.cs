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
    C2GSDiscard = 10,
    GS2CDiscardRet = 11,
    GS2CTurnToNext = 12,
    GS2CInterruptAction = 13,
    C2GSInterruptActionRet = 14,
    GS2CBroadcastProc = 15,
    GS2CGameOver = 16,
}
