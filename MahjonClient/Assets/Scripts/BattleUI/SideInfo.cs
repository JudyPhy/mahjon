using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public enum CardStatus
{
    Idle = 0,
    InHand,
    Exchange,
    PrePeng,
    Peng,
    PreGang,
    Gang,    
    PreDiscard,
    Discard,
    Deal,
    Hu,
}

public class Card
{
    private int _playerId;
    public int PlayerID
    {
        set { _playerId = value; }
        get { return _playerId; }
    }

    private int _oid;
    public int OID
    {
        set { _oid = value; }
        get { return _oid; }
    }

    private int _id;
    public int Id
    {
        set { _id = value; }
        get { return _id; }
    }

    private CardStatus _status;
    public CardStatus Status
    {
        set { _status = value; }
        get { return _status; }
    }

    private bool _isFromOther;
    public bool IsFromOther
    {
        set { _isFromOther = value; }
        get { return _isFromOther; }
    }
}

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

    private int _score;
    public int Score
    {
        get { return _score; }
    }

    private List<Card> _cardList = new List<Card>();
    public List<Card> CardList
    {
        get { return _cardList; }
    }

    private pb.CardType _lack;
    public pb.CardType Lack
    {
        set { _lack = value; }
        get { return _lack; }
    }

    public void UpdateInfo(pb.PlayerInfo info)
    {
        Debug.Log("OID:" + info.OID + ", isowner:" + info.IsOwner);
        _side = info.Side;
        _isOwner = info.IsOwner;
        _oid = info.OID;
        _nickName = info.NickName;
        _headIcon = info.HeadIcon;
    }

    //    public List<Pai> GetPaiList()
    //    {
    //        return _paiList;
    //    }

    public List<Card> GetCardList(CardStatus status)
    {
        List<Card> list = new List<Card>();
        for (int i = 0; i < _cardList.Count; i++)
        {
            if (_cardList[i].Status == status)
            {
                list.Add(_cardList[i]);
            }
        }
        return list;
    }

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

    private CardStatus getCardStatus(pb.CardStatus status)
    {
        switch (status)
        {
            case pb.CardStatus.InHand:
                return CardStatus.InHand;
            case pb.CardStatus.P:
                return CardStatus.Peng;
            case pb.CardStatus.G:
                return CardStatus.Gang;
            case pb.CardStatus.Dis:
                return CardStatus.Discard;
            case pb.CardStatus.Deal:
                return CardStatus.Deal;
            case pb.CardStatus.Hu:
                return CardStatus.Hu;
            default:
                return CardStatus.Idle;
        }
    }

    public void AddCard(pb.CardInfo card)
    {
        Card newCard = new Card();
        newCard.PlayerID = card.playerOID;
        newCard.OID = card.OID;
        newCard.Id = card.ID;
        newCard.Status = getCardStatus(card.Status);        
        newCard.IsFromOther = card.fromOther;
        _cardList.Add(newCard);
    }

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
