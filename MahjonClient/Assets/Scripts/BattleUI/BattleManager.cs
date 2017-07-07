using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class PlayerInfo
{
    private int _oid;
    public int OID
    {
        get { return _oid; }
    }
    private string _nickName;
    public string NickName
    {
        get { return _nickName; }
    }
    private string _headIcon;
    public string HeadIcon
    {
        get { return _headIcon; }
    }
    private int _lev;
    public int Lev
    {
        get { return _lev; }
    }

    public PlayerInfo(pb.PlayerInfo player)
    {
        _oid = player.oid;
        _nickName = player.nickName;
        _headIcon = player.headIcon;
        _lev = player.lev;
    }
}

public class Pai
{
    private int _id;
    public int Id
    {
        set { _id = value; }
        get { return _id; }
    }

    private pb.CardStatus _status;
    public pb.CardStatus Status
    {
        set { _status = value; }
        get { return _status; }
    }
}

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
    public pb.GameMode GameMode
    {
        get { return _gameMode; }
    }

    private int _roomId;
    public int RoomID
    {
        get { return _roomId; }
    }
    private Dictionary<int, SideInfo> _playerPaiInfoDict = new Dictionary<int, SideInfo>();

    public void PrepareEnterGame(pb.GS2CEnterGameRet msg)
    {
        _gameMode = msg.mode;
        _roomId = msg.roomId;
        switch (msg.mode)
        {
            case pb.GameMode.CreateRoom:
                UIManager.Instance.ShowMainWindow<PanelBattle>(eWindowsID.BattleUI);
                break;
            default:
                break;
        }
    }

    public void UpdatePlayerInfo(pb.GS2CUpdateRoomInfo msg)
    {
        switch (msg.status)
        {
            case pb.GS2CUpdateRoomInfo.Status.ADD:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    if (_playerPaiInfoDict.ContainsKey(msg.player[i].player.oid))
                    {
                        Debug.LogError("Dict has contained the player [" + msg.player[i].player.nickName + "], don't need add.");
                    }
                    else
                    {
                        SideInfo info = new SideInfo();
                        info.UpdateBattlePlayerInfo(msg.player[i]);
                        _playerPaiInfoDict.Add(msg.player[i].player.oid, info);
                        EventDispatcher.TriggerEvent<pb.BattlePlayerInfo>(EventDefine.AddRoleToRoom, msg.player[i]);
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.REMOVE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    if (_playerPaiInfoDict.ContainsKey(msg.player[i].player.oid))
                    {
                        _playerPaiInfoDict.Remove(msg.player[i].player.oid);
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
                    if (_playerPaiInfoDict.ContainsKey(msg.player[i].player.oid))
                    {
                        _playerPaiInfoDict[msg.player[i].player.oid].UpdateBattlePlayerInfo(msg.player[i]);
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
    
    public int GetSideIndexFromSelf(pb.BattleSide side)
    {
        if (_playerPaiInfoDict.ContainsKey(Player.Instance.PlayerInfo.OID))
        {
            pb.BattleSide curSide = _playerPaiInfoDict[Player.Instance.PlayerInfo.OID].Side;
            int index = 0;
            while (curSide != side)
            {
                curSide++;
                index++;
                if (curSide > pb.BattleSide.north)
                {
                    curSide = pb.BattleSide.east;
                }
            }
            Debug.Log("index=" + index.ToString());
            return index;
        }
        return -1;
    }

    private void ClearTable()
    {
        foreach (int playerId in _playerPaiInfoDict.Keys)
        {
            _playerPaiInfoDict[playerId].ClearPai();
        }
    }

    public void PrepareGameStart(pb.GS2CBattleStart msg)
    {
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            pb.CardInfo card = msg.cardList[i];
            if (_playerPaiInfoDict.ContainsKey(card.playerId))
            {
                _playerPaiInfoDict[card.playerId].AddPai(card);
            }
        }

        foreach (int playerId in _playerPaiInfoDict.Keys)
        {
            List<Pai> list = _playerPaiInfoDict[playerId].GetPaiList();
            Debug.LogError("player[" + _playerPaiInfoDict[playerId].PlayerInfo.NickName + "] has " + list.Count.ToString() + " cards when game start.");
        }

        EventDispatcher.TriggerEvent<int>(EventDefine.PlayGameStartAni, msg.dealerId);
    }

    private void UpdatePaiInfo(List<pb.CardInfo> list)
    {

    }

}
