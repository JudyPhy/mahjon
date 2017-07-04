using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Dictionary<int, List<Pai>> PlayerPaiInfoDict = new Dictionary<int, List<Pai>>();

    public void UpdatePlayerInfo(pb.GS2CUpdateRoomInfo msg)
    {
        for (int i = 0; i < msg.players.Count; i++)
        {

        }
    }
}
