using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PaiStatus
{
    Idle = 0,
    InHand,
    PrePeng,
    Peng,
    PreGang,
    Gang,
}

public class Pai
{
    private int _id;
    public int Id
    {
        set { _id = value; }
        get { return _id; }
    }

    private PaiStatus _status;
    public PaiStatus Status
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

    private pb.CardType _lackPaiType;
    public pb.CardType LackPaiType
    {
        set { _lackPaiType = value; }
        get { return _lackPaiType; }
    }

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

    public List<Pai> GetPaiListByStatus(PaiStatus status)
    {
        List<Pai> list = new List<Pai>();
        for (int i = 0; i < _paiList.Count; i++)
        {
            if (_paiList[i].Status == status)
            {
                list.Add(_paiList[i]);
            }
        }
        return list;
    }

    public void ClearPai()
    {
        _paiList.Clear();
    }

    private PaiStatus getPaiStatus(pb.CardStatus status)
    {
        switch (status)
        {
            case pb.CardStatus.inHand:
                return PaiStatus.InHand;
            case pb.CardStatus.bePeng:
                return PaiStatus.Peng;
            case pb.CardStatus.beGang:
                return PaiStatus.Gang;
            default:
                return PaiStatus.Idle;
        }
    }

    public void AddPai(pb.CardInfo card)
    {
        Pai pai = new Pai();
        pai.Id = card.CardId;
        pai.Status = getPaiStatus(card.Status);
        _paiList.Add(pai);
    }

}
