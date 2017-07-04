using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class RoleInfo
{
    private int _oid;
    public int OID {
        get {
            return _oid;
        }
    }
    private string _nickName;
    public string NickName
    {
        get
        {
            return _nickName;
        }
    }
    private string _headIcon;
    public string HeadIcon
    {
        get
        {
            return _headIcon;
        }
    }
    private int _lev;
    public int Lev
    {
        get
        {
            return _lev;
        }
    }

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
    public pb.BattleSide Side
    {
        get
        {
            return _side;
        }
    }
    private RoleInfo _playerInfo;
    public RoleInfo PlayerInfo {
        get {
            return _playerInfo;
        }
    }
    private Pai pai;

    public void updateRoleInfo(pb.RoleInfo role)
    {
        _side = role.side;
        _playerInfo = new RoleInfo(role);
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

    private Dictionary<pb.BattleSide, SideInfo> _playerPaiInfoDict = new Dictionary<pb.BattleSide, SideInfo>();

    public void UpdatePlayerInfo(pb.GS2CUpdateRoomInfo msg)
    {
        for (int i = 0; i < msg.players.Count; i++)
        {
            pb.RoleInfo role = msg.players[i];
            if (_playerPaiInfoDict.ContainsKey(role.side))
            {
                _playerPaiInfoDict[role.side].updateRoleInfo(role);
            }
            else
            {
                SideInfo info = new SideInfo();
                info.updateRoleInfo(role);
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.UpdateRoleInfo);
    }

    private List<pb.BattleSide> getSideSort(pb.BattleSide firstSide)
    {
        List<pb.BattleSide> list = new List<pb.BattleSide>();
        pb.BattleSide curSide = firstSide;
        do
        {
            list.Add(curSide);
            curSide = curSide + 1;
            if (curSide > pb.BattleSide.north)
            {
                curSide = pb.BattleSide.east;
            }
        }
        while (curSide != firstSide);
        Debug.Log("sorted side list count=" + list.Count.ToString());
        return list;
    }

    public List<RoleInfo> GetRoleInfo()
    {
        Dictionary<pb.BattleSide, RoleInfo> infoDict = new Dictionary<pb.BattleSide, RoleInfo>();
        pb.BattleSide selfSide = pb.BattleSide.east;
        foreach (SideInfo value in _playerPaiInfoDict.Values)
        {
            infoDict.Add(value.Side, value.PlayerInfo);
            if (Player.Instance.OID == value.PlayerInfo.OID)
            {
                selfSide = value.Side;
            }
        }
        List<RoleInfo> list = new List<RoleInfo>();
        List<pb.BattleSide> sortedSideList = getSideSort(selfSide);
        //for (int i=0;i< sortedSideList.Count)
        {
          //  if (infoDict.ContainsKey())
            //list.Add()
        }
        return list;
    }
}
