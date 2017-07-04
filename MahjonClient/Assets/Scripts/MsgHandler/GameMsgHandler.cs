using UnityEngine;
using System.Collections;
using System.IO;

public class GameMsgHandler
{

    private static GameMsgHandler instance;
    public static GameMsgHandler Instance {
        get {
            if (instance == null) {
                instance = new GameMsgHandler();
            }
            return instance;
        }
    }

    #region C->GS
    public void SendMsgC2GSEnterGame(pb.GameMode mode)
    {
        Debug.Log("SendMsgC2GSEnterGame==>> [" + mode + "]");
        pb.C2GSEnterGame msg = new pb.C2GSEnterGame();
        msg.playerId = 1;
        msg.mode = mode;
        NetworkManager.Instance.SendToGS((int)MsgDef.C2GSEnterGame, msg);
    }

    #endregion


    #region GS->C

    public void RevMsgGS2CUpdateRoomInfo(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CUpdateRoomInfo");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CUpdateRoomInfo msg = ProtoBuf.Serializer.Deserialize<pb.GS2CUpdateRoomInfo>(stream);
        BattleManager.Instance.UpdatePlayerInfo(msg);
;    }

    #endregion
}
