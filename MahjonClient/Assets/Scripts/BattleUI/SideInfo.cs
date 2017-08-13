using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

//public enum PaiStatus
//{
//    Idle = 0,
//    InHand,
//    PrePeng,
//    Peng,
//    PreGang,
//    Gang,
//    Exchange,
//    PreDiscard,
//    Discard,
//    DrawnCard,
//    Hu,
//}

//public class Pai
//{
//    private int _oid;
//    public int OID
//    {
//        set { _oid = value; }
//        get { return _oid; }
//    }

//    private int _id;
//    public int Id
//    {
//        set { _id = value; }
//        get { return _id; }
//    }

//    private PaiStatus _status;
//    public PaiStatus Status
//    {
//        set { _status = value; }
//        get { return _status; }
//    }

//    private int _playerId;
//    public int PlayerID
//    {
//        set { _playerId = value; }
//        get { return _playerId; }
//    }

//    private bool _isFromOther;
//    public bool IsFromOther
//    {
//        set { _isFromOther = value; }
//        get { return _isFromOther; }
//    }

//}

public class SideInfo
{

    private pb.MahjonSide _side;
    public pb.MahjonSide Side
    {
        get { return _side; }
    }

    private bool _isOwner;
    public bool IsOwner
    {
        get { return _isOwner; }
    }

    private int _playerOid;
    public int PlayerOID
    {
        get { return _playerOid; }
    }

    //    private List<Pai> _paiList = new List<Pai>();

    //    private pb.CardType _lackPaiType;
    //    public pb.CardType LackPaiType
    //    {
    //        set { _lackPaiType = value; }
    //        get { return _lackPaiType; }
    //    }

    public void UpdateInfo(pb.PlayerInfo info)
    {
        _side = info.Side;
        _isOwner = info.IsOwner;
        _playerOid = info.OID;
    }

    //    public List<Pai> GetPaiList()
    //    {
    //        return _paiList;
    //    }

    //    public List<Pai> GetPaiListByStatus(PaiStatus status)
    //    {
    //        List<Pai> list = new List<Pai>();
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (_paiList[i].Status == status)
    //            {
    //                list.Add(_paiList[i]);
    //            }
    //        }
    //        return list; 
    //    }

    //    public List<int> GetPaiIdListByStatus(PaiStatus status)
    //    {
    //        List<int> list = new List<int>();
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (_paiList[i].Status == status)
    //            {
    //                list.Add(_paiList[i].Id);
    //            }
    //        }
    //        return list; 
    //    }

    //    public pb.CardType GetExchangeType()
    //    {
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (_paiList[i].Status == PaiStatus.Exchange)
    //            {
    //                return (pb.CardType)Mathf.CeilToInt(_paiList[i].Id / 10);
    //            }
    //        }
    //        return pb.CardType.None;
    //    }

    //    public int GetExchangeCardCount()
    //    {
    //        int count = 0;
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (_paiList[i].Status == PaiStatus.Exchange)
    //            {
    //                count++;
    //            }
    //        }
    //        return count;
    //    }


    //    public void ClearPai()
    //    {
    //        _paiList.Clear();
    //    }

    //    private PaiStatus getPaiStatus(pb.CardStatus status)
    //    {
    //        switch (status)
    //        {
    //            case pb.CardStatus.inHand:
    //                return PaiStatus.InHand;
    //            case pb.CardStatus.bePeng:
    //                return PaiStatus.Peng;
    //            case pb.CardStatus.beGang:
    //                return PaiStatus.Gang;
    //            case pb.CardStatus.discard:
    //                return PaiStatus.Discard;
    //            case pb.CardStatus.hu:
    //                return PaiStatus.Hu;
    //            default:
    //                return PaiStatus.Idle;
    //        }
    //    }

    //    public void AddPai(pb.CardInfo card)
    //    {
    //        Pai pai = new Pai();
    //        pai.OID = card.CardOid;
    //        pai.Id = card.CardId;
    //        pai.Status = getPaiStatus(card.Status);
    //        pai.PlayerID = card.playerId;
    //        pai.IsFromOther = card.fromOther;
    //        _paiList.Add(pai);
    //    }

    //    public void UpdatePai(Pai origInfo, pb.CardInfo newInfo)
    //    {
    //        origInfo.OID = newInfo.CardOid;
    //        origInfo.Id = newInfo.CardId;
    //        origInfo.Status = getPaiStatus(newInfo.Status);
    //        origInfo.PlayerID = newInfo.playerId;
    //        origInfo.IsFromOther = newInfo.fromOther;
    //    }

    //    public void RemoveExchangeCard()
    //    {
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (_paiList[i].Status == PaiStatus.Exchange)
    //            {
    //                _paiList.RemoveAt(i);
    //                i--;
    //            }
    //        }
    //    }

    //    public List<Pai> GetUsefulPaiList()
    //    {
    //        List<Pai> list = new List<Pai>();
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (_paiList[i].Status == PaiStatus.InHand || _paiList[i].Status == PaiStatus.Gang || _paiList[i].Status == PaiStatus.Peng)
    //            {
    //                list.Add(_paiList[i]);
    //            }
    //        }
    //        return list;
    //    }

    //    public Pai getDealCard(int cardOid) {
    //        for (int i = 0; i < _paiList.Count; i++)
    //        {
    //            if (cardOid == _paiList[i].OID)
    //            {
    //                return _paiList[i];
    //            }
    //        }
    //        return null;
    //    }

}
