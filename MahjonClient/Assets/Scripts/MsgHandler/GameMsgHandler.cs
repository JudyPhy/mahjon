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
    public void SendMsgC2GSLogin(string nickName, string password)
    {
        Debug.Log("SendMsgC2GSLogin==>> nickName[" + nickName + "], password[" + password + "]");
        pb.C2GSLogin msg = new pb.C2GSLogin();
        msg.nickName = nickName;
        msg.password = password;
        NetworkManager.Instance.SendToGS((int)MsgDef.C2GSLogin, msg);
    }

    public void SendMsgC2GSEnterGame(pb.GameMode mode, int roomId = 0)
    {
        Debug.Log("SendMsgC2GSEnterGame==>> [" + mode + "]");
        pb.C2GSEnterGame msg = new pb.C2GSEnterGame();
        msg.mode = mode;
        msg.roomId = roomId;
        NetworkManager.Instance.SendToGS((int)MsgDef.C2GSEnterGame, msg);
    }

    #endregion


    #region GS->C

    public void RevMsgGS2CLoginRet(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CLoginRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CLoginRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CLoginRet>(stream);
        switch (msg.errorCode)
        {
            case pb.GS2CLoginRet.ErrorCode.SUCCESS:
                Player.Instance.PlayerInfo = new PlayerInfo(msg.playerInfo);
                UIManager.Instance.ShowMainWindow<MainUI>(eWindowsID.MainUI);
                break;
            case pb.GS2CLoginRet.ErrorCode.FAIL:
                break;
            case pb.GS2CLoginRet.ErrorCode.NICKNAME_ERROR:
                break;
            case pb.GS2CLoginRet.ErrorCode.PASSWORD_ERROR:
                break;
        }
    }

    public void RevMsgGS2CEnterGameRet(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CEnterGameRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CEnterGameRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CEnterGameRet>(stream);
        switch (msg.errorCode)
        {
            case pb.GS2CEnterGameRet.ErrorCode.SUCCESS:
                BattleManager.Instance.PrepareEnterGame(msg);
                break;
            case pb.GS2CEnterGameRet.ErrorCode.FAIL:
                break;
            case pb.GS2CEnterGameRet.ErrorCode.PLAYER_COUNT_LIMITE:
                break;
        }
    }

    public void RevMsgGS2CUpdateRoomInfo(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CUpdateRoomInfo");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CUpdateRoomInfo msg = ProtoBuf.Serializer.Deserialize<pb.GS2CUpdateRoomInfo>(stream);
        BattleManager.Instance.UpdatePlayerInfo(msg);
    }

    #endregion
}
