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
    C2GSCurTurnOver = 13,
    GS2CTurnToNext = 14,
    GS2CRobotProc = 15,
    C2GSRobotProcOver = 16,
    GS2CPlayerEnsureProc = 17,
    C2GSPlayerEnsureProcRet = 18,
    GS2CUpdateCardAfterPlayerProc = 19,
}
