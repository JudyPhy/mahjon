using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class RoleInfo
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

    public RoleInfo(pb.RoleInfo role)
    {
        _oid = role.oid;
        _nickName = role.nickName;
        _headIcon = role.headIcon;
        _lev = role.lev;
    }

    public RoleInfo(pb.PlayerInfo player)
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
    private pb.BattleSide _side;

    public Pai(int id, pb.BattleSide side)
    {
        _id = id;
        _side = side;
    }

    public int Id
    {
        get { return _id; }
    }

    public pb.BattleSide Side
    {
        get { return _side; }
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
                    if (_playerPaiInfoDict.ContainsKey(msg.player[i].oid))
                    {
                        Debug.LogError("Dict has contained the player [" + msg.player[i].nickName + "], don't need add.");
                    }
                    else
                    {
                        SideInfo info = new SideInfo();
                        info.updateRoleInfo(msg.player[i]);
                        _playerPaiInfoDict.Add(msg.player[i].oid, info);
                        EventDispatcher.TriggerEvent<pb.RoleInfo>(EventDefine.AddRoleToRoom, msg.player[i]);
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.REMOVE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    if (_playerPaiInfoDict.ContainsKey(msg.player[i].oid))
                    {
                        _playerPaiInfoDict.Remove(msg.player[i].oid);
                    }
                    else
                    {
                        Debug.LogError("Dict doesn't contain the player [" + msg.player[i].nickName + "], can't remove.");
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.UPDATE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    if (_playerPaiInfoDict.ContainsKey(msg.player[i].oid))
                    {
                        _playerPaiInfoDict[msg.player[i].oid].updateRoleInfo(msg.player[i]);
                    }
                    else
                    {
                        Debug.LogError("Dict doesn't contain the player [" + msg.player[i].nickName + "], can't update.");
                    }
                }
                break;
            default:
                break;
        }
    }

    public int GetSideIndexFromSelf(pb.BattleSide side)
    {
        if (_playerPaiInfoDict.ContainsKey(Player.Instance.RoleInfo.OID))
        {
            pb.BattleSide curSide = _playerPaiInfoDict[Player.Instance.RoleInfo.OID].Side;
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

}
