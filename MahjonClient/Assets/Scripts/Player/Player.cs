using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int _gold;
    public int Gold
    {
        get { return _gold; }
    }

    private int _diamond;
    public int Diamond
    {
        get { return _diamond; }
    }

    public PlayerInfo(pb.PlayerInfo player)
    {
        _oid = player.oid;
        _nickName = player.nickName;
        _headIcon = player.headIcon;
        _gold = player.gold;
        _diamond = player.diamond;
    }
}

public class Player
{
    private static Player _instance;
    public static Player Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Player();
            return _instance;
        }
    }

    private PlayerInfo _info;
    public PlayerInfo PlayerInfo
    {
        set { _info = value; }
        get { return _info; }
    }
}
