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
    GS2CExchangeCardRet = 7,
    GS2CUpdateCardInfoAfterExchange = 8,
    C2GSSelectLack = 9,
    GS2CSelectLackRet = 10,
    C2GSDiscard = 11,
    GS2CDiscardRet = 12,
    GS2CUpdateCardInfoByPG = 13,
    C2GSCurTurnOver = 14,
    GS2CTurnToNext = 15,
    C2GSProcPG = 16,
    C2GSRobotProcOver = 17,
}
