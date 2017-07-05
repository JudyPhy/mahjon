using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideInfo
{

    private pb.BattleSide _side;
    public pb.BattleSide Side
    {
        get { return _side; }
    }

    private RoleInfo _playerInfo;
    public RoleInfo PlayerInfo
    {
        get { return _playerInfo; }
    }

    private Pai pai;

    public void updateRoleInfo(pb.RoleInfo role)
    {
        _side = role.side;
        _playerInfo = new RoleInfo(role);
    }
}
