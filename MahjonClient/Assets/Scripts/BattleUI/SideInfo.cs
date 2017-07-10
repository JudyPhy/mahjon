using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private List<Pai> _paiList = new List<Pai>();

    public void UpdateBattlePlayerInfo(pb.BattlePlayerInfo role)
    {
        _side = role.side;
        _isOwner = role.isOwner;
        _playerInfo = new PlayerInfo(role.player);
    }

    public List<Pai> GetPaiList()
    {
        return _paiList;
    }

    public void ClearPai()
    {
        _paiList.Clear();
    }

    public void AddPai(pb.CardInfo card)
    {
        Pai pai = new Pai();
        pai.Id = card.CardId;
        pai.Status = card.Status;
        _paiList.Add(pai);
    }

}
