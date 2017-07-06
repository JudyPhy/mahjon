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

    private bool _isOwner;
    public bool IsOwner
    {
        get { return _isOwner; }
    }

    private PlayerInfo _playerInfo;
    public PlayerInfo PlayerInfo
    {
        get { return _playerInfo; }
    }

    private Pai pai;

    public void UpdateBattlePlayerInfo(pb.BattlePlayerInfo role)
    {
        _side = role.side;
        _isOwner = role.isOwner;
        _playerInfo = new PlayerInfo(role.player);
    }
}
