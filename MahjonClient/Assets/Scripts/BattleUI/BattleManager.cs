using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class BattleManager
{
    private static BattleManager _instance;
    public static BattleManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BattleManager();
            return _instance;
        }
    }

    private pb.GameMode _gameMode;
    private string _roomId;
    public bool IsWaitingEnterRoomRet = false;
    private int _dealerId;

    private List<SideInfo> _playerPaiInfoList = new List<SideInfo>();

    public void PrepareEnterGame(pb.GS2CEnterGameRet msg)
    {
        Debug.LogError("PrepareEnterGame=> _gameMode=" + msg.mode.ToString() + ", _roomId=" + msg.roomId);
        _gameMode = msg.mode;
        _roomId = msg.roomId;
        switch (msg.mode)
        {
            case pb.GameMode.CreateRoom:
                BattleManager.Instance.IsWaitingEnterRoomRet = false;
                break;
            default:
                break;
        }
    }

    private int getPlayerIndexInList(int playerId)
    {
        int index = -1;
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == playerId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void UpdatePlayerInfo(pb.GS2CUpdateRoomInfo msg)
    {
        switch (msg.status)
        {
            case pb.GS2CUpdateRoomInfo.Status.ADD:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    if (getPlayerIndexInList(msg.player[i].player.oid) != -1)
                    {
                        Debug.LogError("List has contained the player [" + msg.player[i].player.nickName + "], don't need add.");
                    }
                    else
                    {
                        SideInfo info = new SideInfo();
                        info.UpdateBattlePlayerInfo(msg.player[i]);
                        _playerPaiInfoList.Add(info);
                        EventDispatcher.TriggerEvent(EventDefine.UpdateRoleInRoom);
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.REMOVE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    int index = getPlayerIndexInList(msg.player[i].player.oid);
                    if (index != -1)
                    {
                        _playerPaiInfoList.RemoveAt(index);
                    }
                    else
                    {
                        Debug.LogError("Dict doesn't contain the player [" + msg.player[i].player.nickName + "], can't remove.");
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.UPDATE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    int index = getPlayerIndexInList(msg.player[i].player.oid);
                    if (index != -1)
                    {
                        _playerPaiInfoList[index].UpdateBattlePlayerInfo(msg.player[i]);
                    }
                    else
                    {
                        Debug.LogError("Dict doesn't contain the player [" + msg.player[i].player.nickName + "], can't update.");
                    }
                }
                break;
            default:
                break;
        }
    }

    public List<pb.BattleSide> GetSortSideListFromSelf()
    {
        pb.BattleSide selfSide = GetSelfSide();
        pb.BattleSide curSide = selfSide;
        List<pb.BattleSide> sideSortList = new List<pb.BattleSide>();
        do
        {
            sideSortList.Add(curSide);
            curSide++;
            if (curSide >= pb.BattleSide.north)
            {
                curSide = pb.BattleSide.east;
            }
        } while (curSide != selfSide);
        return sideSortList;
    }

    public PlayerInfo GetPlayerInfoBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].PlayerInfo;
            }
        }
        return null;
    }

    private pb.BattleSide GetSelfSide()
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == Player.Instance.PlayerInfo.OID)
            {
                return _playerPaiInfoList[i].Side;
            }
        }
        return pb.BattleSide.none;
    }

    public void PrepareGameStart(pb.GS2CBattleStart msg)
    {
        _dealerId = msg.dealerId;
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            pb.CardInfo card = msg.cardList[i];
            for (int j = 0; j < _playerPaiInfoList.Count; j++)
            {
                if (_playerPaiInfoList[j].PlayerInfo.OID == card.playerId)
                {
                    _playerPaiInfoList[j].AddPai(card);
                }
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.PlayGameStartAni);
    }

    public pb.BattleSide GetDealerSide()
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == _dealerId)
            {
                return _playerPaiInfoList[i].Side;
            }
        }
        return pb.BattleSide.none;
    }
    
    public List<Pai> GetPaiListBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetPaiList();
            }
        }
        return new List<Pai>();
    }

    public void DiscardTimeOut(int playerId)
    {

    }

}
