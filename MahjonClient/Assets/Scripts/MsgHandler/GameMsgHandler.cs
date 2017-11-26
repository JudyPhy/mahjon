using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using EventTransmit;

public class GameMsgHandler
{

    private static GameMsgHandler instance;
    public static GameMsgHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameMsgHandler();
            }
            return instance;
        }
    }

    #region C->GS
    public void SendMsgC2GSLogin(string account, string password)
    {
        MJLog.Log("SendMsgC2GSLogin==>> account[" + account + "], password[" + password + "]");
        pb.C2GSLogin msg = new pb.C2GSLogin();
        msg.account = account;
        msg.password = password;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSLogin, msg);
    }

    public void SendMsgC2GSEnterGame(pb.GameType type,pb.EnterMode mode, string roomId = "")
    {
        MJLog.Log("SendMsgC2GSEnterGame==>> type[" + type.ToString() + "], mode[" + mode.ToString() + "], roomId[" + roomId + "]");
        pb.C2GSEnterGame msg = new pb.C2GSEnterGame();
        msg.type = type;
        msg.mode = mode;
        msg.roomId = roomId;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSEnterGame, msg);
    }

    public void SendMsgC2GSExchangeCard(List<Card> exchangeList)
    {
        MJLog.Log("SendMsgC2GSExchangeCard==>> [" + exchangeList.Count + "]");
        pb.C2GSExchangeCard msg = new pb.C2GSExchangeCard();
        for (int i = 0; i < exchangeList.Count; i++)
        {
            msg.cardOIDList.Add(exchangeList[i].OID);
        }
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSExchangeCard, msg);
    }

    public void SendMsgC2GSSelectLack(pb.CardType type)
    {
        MJLog.Log("SendMsgC2GSSelectLack==>> [" + type.ToString() + "]");
        pb.C2GSSelectLack msg = new pb.C2GSSelectLack();
        msg.type = type;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSSelectLack, msg);
    }

    public void SendMsgC2GSInterruptActionRet(pb.ProcType procType,pb.CardInfo info)
    {
        MJLog.Log("SendMsgC2GSInterruptActionRet==>> procType[" + procType + "], cardOid[" + info.OID + "]");
        MJLog.LogError("cardOid[" + info.OID + "], playerOid:"+ info.playerOID+", id:"+ info.ID+", status:"+ info.Status.ToString());
        pb.C2GSInterruptActionRet msg = new pb.C2GSInterruptActionRet();
        msg.procType = procType;
        msg.drawCard = info;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSInterruptActionRet, msg);
    }
    #endregion


    #region GS->C

    public void RevMsgGS2CLoginRet(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CLoginRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CLoginRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CLoginRet>(stream);
        MJLog.Log("errorCode=" + msg.errorCode.ToString());
        switch (msg.errorCode)
        {
            case pb.GS2CLoginRet.ErrorCode.SUCCESS:
                Player.Instance.UpdatePlayer(msg.playerInfo);
                UIManager.Instance.ShowMainWindow<MainUI>(eWindowsID.MainUI);
                break;
            case pb.GS2CLoginRet.ErrorCode.FAIL:
                UIManager.Instance.ShowTips(TipsType.text, "登陆失败");
                break;
            case pb.GS2CLoginRet.ErrorCode.ACCOUNT_ERROR:
            case pb.GS2CLoginRet.ErrorCode.PASSWORD_ERROR:
                UIManager.Instance.ShowTips(TipsType.text, "账号或密码错误");
                break;
            default:
                break;
        }
    }

    public void RevMsgGS2CEnterGameRet(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CEnterGameRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CEnterGameRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CEnterGameRet>(stream);
        switch (msg.errorCode)
        {
            case pb.GS2CEnterGameRet.ErrorCode.SUCCESS:
                BattleManager.Instance.PrepareEnterRoom(msg);
                break;
            case pb.GS2CEnterGameRet.ErrorCode.FAIL:
                UIManager.Instance.ShowTips(TipsType.text, "进入房间失败");
                break;
            case pb.GS2CEnterGameRet.ErrorCode.PLAYER_COUNT_LIMITE:
                UIManager.Instance.ShowTips(TipsType.text, "房间已满");
                break;
        }
    }

    public void RevMsgGS2CUpdateRoomMember(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CUpdateRoomMember");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CUpdateRoomMember msg = ProtoBuf.Serializer.Deserialize<pb.GS2CUpdateRoomMember>(stream);
        BattleManager.Instance.GS2CUpdateRoomMember(msg);
    }


    public void RevMsgGS2CBattleStart(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CBattleStart");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CBattleStart msg = ProtoBuf.Serializer.Deserialize<pb.GS2CBattleStart>(stream);
        BattleManager.Instance.PrepareGameStart(msg);
    }

    public void RevMsgGS2CExchangeCardRet(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CExchangeCardRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CExchangeCardRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CExchangeCardRet>(stream);
        BattleManager.Instance.UpdateAllCardsAfterExhchange(msg);
    }

    public void RevMsgGS2CSelectLackRet(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CSelectLackRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CSelectLackRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CSelectLackRet>(stream);
        BattleManager.Instance.LackRet(msg.lackCard);
    }

    public void RevMsgGS2CTurnToNext(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.LogError("==>> RevMsgGS2CTurnToNext");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CTurnToNext msg = ProtoBuf.Serializer.Deserialize<pb.GS2CTurnToNext>(stream);
        BattleManager.Instance.TurnToNextPlayer(msg.playerOID, msg.drawCard);
    }

    public void RevMsgGS2CInterruptAction(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CInterruptAction");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CInterruptAction msg = ProtoBuf.Serializer.Deserialize<pb.GS2CInterruptAction>(stream);
        BattleManager.Instance.PlayerProc(msg);
    }

    public void RevMsgGS2CBroadcastProc(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CBroadcastProc");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CBroadcastProc msg = ProtoBuf.Serializer.Deserialize<pb.GS2CBroadcastProc>(stream);
        BattleManager.Instance.UpdateCardsInfo(msg);
    }

    public void RevMsgGS2CGameOver(int pid, byte[] msgBuf, int msgSize)
    {
        MJLog.Log("==>> RevMsgGS2CGameOver");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CGameOver msg = ProtoBuf.Serializer.Deserialize<pb.GS2CGameOver>(stream);
        BattleManager.Instance.GameOver();
    }
    #endregion
}
