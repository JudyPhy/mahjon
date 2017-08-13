using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int _fangka;
    public int Fangka
    {
        get { return _fangka; }
    }

    public void UpdatePlayer(pb.PlayerInfo player)
    {
        _oid = player.OID;
        _nickName = player.NickName;
        _headIcon = player.HeadIcon;
        _gold = player.Gold;
        _fangka = player.Fangka;
    }

    public string GetGold()
    {
        if (_gold > 9999)
        {
            float value = _gold / 10000f;
            return value.ToString("0.0") + "万";
        }
        return _gold.ToString();
    }
}
