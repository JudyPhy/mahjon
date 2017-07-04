using System.Collections;
using System.Collections.Generic;
using UnityEngine;
<<<<<<< HEAD
=======
using EventTransmit;

public class RoleInfo
{
    private int _oid;
    private string _nickName;
    private string _headIcon;
    private int _lev;

    public RoleInfo(pb.RoleInfo role)
    {
        _oid = role.oid;
        _nickName = role.nickName;
        _headIcon = role.headIcon;
        _lev = role.lev;
    }
}

public class SideInfo
{
    private pb.BattleSide _side;
    private RoleInfo _playerInfo;
    private Pai pai;

    public void updateRoleInfo(pb.RoleInfo role)
    {
        _side = role.side;
        _playerInfo = new RoleInfo(role);
    }
}
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50

public class Pai
{
    private int _id;
    private pb.BattleSide _side;

    public Pai(int id, pb.BattleSide side)
    {
        _id = id;
        _side = side;
    }

    public int Id {
        get {
            return _id;
        }
    }

    public pb.BattleSide Side
    {
        get
        {
            return _side;
        }
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

<<<<<<< HEAD
    Dictionary<int, List<Pai>> PlayerPaiInfoDict = new Dictionary<int, List<Pai>>();
=======
    private Dictionary<pb.BattleSide, SideInfo> _playerPaiInfoDict = new Dictionary<pb.BattleSide, SideInfo>();
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50

    public void UpdatePlayerInfo(pb.GS2CUpdateRoomInfo msg)
    {
        for (int i = 0; i < msg.players.Count; i++)
        {
<<<<<<< HEAD

        }
=======
            pb.RoleInfo role = msg.players[i];
            if (_playerPaiInfoDict.ContainsKey(role.side))
            {
                _playerPaiInfoDict[role.side].updateRoleInfo(role);
            }
            else {
                SideInfo info = new SideInfo();
                info.updateRoleInfo(role);
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.UpdateRoleInfo);
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    }
}
