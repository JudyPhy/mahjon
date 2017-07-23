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
        Debug.Log("SendMsgC2GSLogin==>> account[" + account + "], password[" + password + "]");
        pb.C2GSLogin msg = new pb.C2GSLogin();
        msg.account = account;
        msg.password = password;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSLogin, msg);
    }

    public void SendMsgC2GSEnterGame(pb.GameMode mode, string roomId = "")
    {
        Debug.Log("SendMsgC2GSEnterGame==>> [" + mode.ToString() + "]");
        pb.C2GSEnterGame msg = new pb.C2GSEnterGame();
        msg.mode = mode;
        msg.roomId = roomId;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSEnterGame, msg);
    }

    public void SendMsgC2GSSelectLack(pb.CardType type)
    {
        Debug.Log("SendMsgC2GSSelectLack==>> [" + type.ToString() + "]");
        pb.C2GSSelectLack msg = new pb.C2GSSelectLack();
        msg.type = type;
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSSelectLack, msg);
    }

    public void SendMsgC2GSExchangeCard(List<Pai> exchangeList)
    {
        Debug.Log("SendMsgC2GSExchangeCard==>> [" + exchangeList.Count + "]");
        pb.C2GSExchangeCard msg = new pb.C2GSExchangeCard();
        for (int i = 0; i < exchangeList.Count; i++)
        {
            pb.CardInfo card = new pb.CardInfo();
            card.CardOid = exchangeList[i].OID;
            card.CardId = exchangeList[i].Id;
            card.playerId = exchangeList[i].PlayerID;
            card.Status = pb.CardStatus.inHand;
            msg.cardList.Add(card);
        }
        NetworkManager.Instance.SendToGS((UInt16)MsgDef.C2GSExchangeCard, msg);
    }

    #endregion


    #region GS->C

    public void RevMsgGS2CLoginRet(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CLoginRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CLoginRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CLoginRet>(stream);
        Debug.Log("errorCode=" + msg.errorCode.ToString());
        switch (msg.errorCode)
        {
            case pb.GS2CLoginRet.ErrorCode.SUCCESS:                
                Player.Instance.PlayerInfo = new PlayerInfo(msg.playerInfo);
                UIManager.Instance.ShowMainWindow<MainUI>(eWindowsID.MainUI);
                break;
            case pb.GS2CLoginRet.ErrorCode.FAIL:
                UIManager.Instance.ShowTips(TipsType.text, "登陆失败");
                break;
            case pb.GS2CLoginRet.ErrorCode.ACCOUNT_ERROR:
                UIManager.Instance.ShowTips(TipsType.text, "账号不存在");
                break;
            case pb.GS2CLoginRet.ErrorCode.PASSWORD_ERROR:
                UIManager.Instance.ShowTips(TipsType.text, "密码错误");
                break;
            default:
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
                UIManager.Instance.ShowTips(TipsType.text, "房间已满");
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


    public void RevMsgGS2CBattleStart(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CBattleStart");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CBattleStart msg = ProtoBuf.Serializer.Deserialize<pb.GS2CBattleStart>(stream);
        BattleManager.Instance.PrepareGameStart(msg);
    }

    public void RevMsgGS2CSelectLackRet(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CSelectLackRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CSelectLackRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CSelectLackRet>(stream);
        BattleManager.Instance.UpdateLackCardInfo(msg.lackCard);
    }

    public void RevMsgGS2CExchangeCardRet(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CExchangeCardRet");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CExchangeCardRet msg = ProtoBuf.Serializer.Deserialize<pb.GS2CExchangeCardRet>(stream);
        switch (msg.errorCode) {
            case pb.GS2CExchangeCardRet.ErrorCode.SUCCESS:
                break;
            case pb.GS2CExchangeCardRet.ErrorCode.FAIL_CARD_COUNT_ERROR:
            case pb.GS2CExchangeCardRet.ErrorCode.FAIL:
                UIManager.Instance.ShowTips(TipsType.text, "请重新选择要交换的牌");
                EventDispatcher.TriggerEvent(EventDefine.ReExchangeCard);
                break;
        }
    }

    public void RevMsgGS2CDiscardTimeOut(int pid, byte[] msgBuf, int msgSize)
    {
        Debug.Log("==>> RevMsgGS2CDiscardTimeOut");
        Stream stream = new MemoryStream(msgBuf);
        pb.GS2CDiscardTimeOut msg = ProtoBuf.Serializer.Deserialize<pb.GS2CDiscardTimeOut>(stream);
        BattleManager.Instance.DiscardTimeOut(msg.playerId);
    }
    #endregion
}
