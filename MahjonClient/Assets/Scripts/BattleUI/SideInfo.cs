﻿using System.Collections;
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

    public Card(pb.CardInfo info)
    {
        _playerId = info.playerOID;
        _oid = info.OID;
        _id = info.ID;
        _status = getCardStatus(info.Status);
        _isFromOther = info.fromOther;
    }

    public pb.CardInfo ToPbInfo()
    {
        pb.CardInfo card = new pb.CardInfo();
        card.playerOID = _playerId;
        card.OID = _oid;
        card.ID = _id;
        card.Status = pb.CardStatus.InHand;
        card.fromOther = _isFromOther;
        return card;
    }

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
}

public class SideInfo
{

    public pb.MahjonSide Side
    {
        get { return _side; }
    }
    private pb.MahjonSide _side;

    public int SideIndex
    {
        get { return _sideIndex; }
    }
    private int _sideIndex;

    public bool IsOwner
    {
        get { return _isOwner; }
    }
    private bool _isOwner;

    public int OID
    {
        get { return _oid; }
    }
    private int _oid;

    public string NickName
    {
        get { return _nickName; }
    }
    private string _nickName;

    public string HeadIcon
    {
        get { return _headIcon; }
    }
    private string _headIcon;

    public int Score
    {
        get { return _score; }
    }
    private int _score;

    public List<Card> CardList
    {
        get { return _cardList; }
    }
    private List<Card> _cardList = new List<Card>();

    public pb.CardType Lack
    {
        set { _lack = value; }
        get { return _lack; }
    }
    private pb.CardType _lack;

    public void UpdateInfo(pb.PlayerInfo info)
    {
        Debug.LogError("OID:" + info.OID + ", isowner:" + info.IsOwner);
        _side = info.Side;
        _isOwner = info.IsOwner;
        _oid = info.OID;
        _nickName = info.NickName;
        _headIcon = info.HeadIcon;

        pb.MahjonSide selfSide = BattleManager.Instance.GetSelfSide();
        //Debug.LogError("selfSide=" + selfSide.ToString() + ", _side=" + _side.ToString());
        if (selfSide != pb.MahjonSide.DEFAULT)
        {
            int count = 0;
            int curSide = (int)selfSide;
            while (curSide != (int)_side)
            {
                curSide++;
                count++;
                if (curSide > 5)
                {
                    curSide = 2;
                }
            }
            _sideIndex = count;
        }
        Debug.Log("_sideIndex=" + _sideIndex);
    }

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

    public void AddCard(pb.CardInfo card)
    {
        Card newCard = new Card(card);
        _cardList.Add(newCard);
    }
}
