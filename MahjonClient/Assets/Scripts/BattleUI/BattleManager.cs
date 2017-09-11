using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public enum BattleProcess
{
    Default,
    ExchangCard,
    ExchangCardOver,

    Lack,
    LackOver,

    Discard,
    DiscardOver,

    SelfGangChoose,
    ProcEnsureOver,
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

    private pb.GameType _gameType;

    private string _roomId;
    public string RoomID
    {
        get { return _roomId; }
    }

    private int _dealerId;
    public int DealerID
    {
        get { return _dealerId; }
    }

    private Card _procCard;
    public Card ProcCard
    {
        set { _procCard = value; }
        get { return _procCard; }
    }

    private int _curTurnPlayer;
    public int CurTurnPlayer
    {
        get { return _curTurnPlayer; }
    }

    private BattleProcess _curProcess;
    public BattleProcess CurProcess
    {
        set { _curProcess = value; }
        get { return _curProcess; }
    }

    //playerOid : sideInfo
    private Dictionary<int, SideInfo> _SideInfoDict = new Dictionary<int, SideInfo>();

    private List<pb.MahjonSide> _sortSideFromSelf = new List<pb.MahjonSide>();
    public List<pb.MahjonSide> SortSideFromSelf
    {
        get { return _sortSideFromSelf; }
    }

    public void PrepareEnterRoom(pb.GS2CEnterGameRet msg)
    {
        Debug.Log("PrepareEnterGame=> _gameType=" + msg.type.ToString() + ", _roomId=" + msg.roomId);
        _gameType = msg.type;
        _roomId = msg.roomId;
        switch (_gameType)
        {
            case pb.GameType.XueZhan:
                UIManager.Instance.ShowMainWindow<Panel_battle_mj>(eWindowsID.BattleUI_MJ);
                break;
            default:
                break;
        }
    }

    public void GS2CUpdateRoomMember(pb.GS2CUpdateRoomMember msg)
    {
        Debug.Log("GS2CUpdateRoomMember=> player count:" + msg.player.Count);
        for (int i = 0; i < msg.player.Count; i++)
        {
            if (_SideInfoDict.ContainsKey(msg.player[i].OID))
            {
                _SideInfoDict[msg.player[i].OID].UpdateInfo(msg.player[i]);
            }
            else
            {
                SideInfo info = new SideInfo();
                info.UpdateInfo(msg.player[i]);
                _SideInfoDict.Add(msg.player[i].OID, info);
            }
        }
        foreach (int id in _SideInfoDict.Keys)
        {
            bool isFind = false;
            for (int i = 0; i < msg.player.Count; i++)
            {
                if (msg.player[i].OID == id)
                {
                    isFind = true;
                    break;
                }
            }
            if (!isFind)
            {
                _SideInfoDict.Remove(id);
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.UpdateRoomMember);
        _sortSideFromSelf = GetSortedSideFromSelf();
    }

    public List<pb.MahjonSide> GetSortedSideFromSelf()
    {
        List<pb.MahjonSide> result = new List<pb.MahjonSide>();
        int curSide = (int)_SideInfoDict[Player.Instance.OID].Side;
        for (int i = 0; i < 4; i++)
        {
            result.Add((pb.MahjonSide)curSide);
            curSide++;
            if (curSide > (int)pb.MahjonSide.NORTH)
            {
                curSide = (int)pb.MahjonSide.EAST;
            }
        }
        return result;
    }

    public List<SideInfo> GetRoomMembers()
    {
        return new List<SideInfo>(_SideInfoDict.Values);
    }

    public int GetSideIndexFromSelf(pb.MahjonSide side)
    {
        for (int i = 0; i < _sortSideFromSelf.Count; i++)
        {
            if (side == _sortSideFromSelf[i])
            {
                return i;
            }
        }
        return 0;
    }

    public Card GetDrawCardInfo(pb.MahjonSide side, int index)
    {
        foreach (SideInfo sideInfo in _SideInfoDict.Values)
        {
            if (sideInfo.Side == side && index < sideInfo.CardList.Count)
            {
                return sideInfo.CardList[index];
            }
        }
        return null;
    }

    public void PrepareGameStart(pb.GS2CBattleStart msg)
    {
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            pb.CardInfo card = msg.cardList[i];
            if (_SideInfoDict.ContainsKey(card.playerOID))
            {
                _SideInfoDict[card.playerOID].AddCard(card);
            }
        }
        _dealerId = msg.dealerId;
        Debug.Log("_dealerId=" + _dealerId);
        EventDispatcher.TriggerEvent(EventDefine.PlayGamePrepareAni);

        //log        
        foreach (int player in _SideInfoDict.Keys)
        {
            string str = "player" + player + " 's cards: ";
            for (int i = 0; i < _SideInfoDict[player].CardList.Count; i++)
            {
                str += _SideInfoDict[player].CardList[i].Id + ", ";
            }
            Debug.Log(str);
        }
    }

    public pb.MahjonSide GetSideByPlayerOID(int playerOid)
    {
        if (_SideInfoDict.ContainsKey(playerOid))
        {
            return _SideInfoDict[playerOid].Side;
        }
        return pb.MahjonSide.DEFAULT;
    }

    public int GetPlayerOIDBySide(pb.MahjonSide side)
    {
        foreach (int playerId in _SideInfoDict.Keys)
        {
            if (_SideInfoDict[playerId].Side == side)
            {
                return playerId;
            }
        }
        return 0;
    }

    //收到定缺完毕信息
    public void LackRet(List<pb.LackCard> list)
    {
        Debug.Log("LackRet");
        for (int i = 0; i < list.Count; i++)
        {
            int player = list[i].playerOID;
            if (_SideInfoDict.ContainsKey(player))
            {
                _SideInfoDict[player].Lack = list[i].type;
            }
            else
            {
                Debug.LogError("has no player" + player + " 's sideInfo.");
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.ShowLackCard);
    }

    public List<Card> GetCardList(int playerOid, CardStatus status)
    {
        if (_SideInfoDict.ContainsKey(playerOid))
        {
            return _SideInfoDict[playerOid].GetCardList(status);
        }
        return null;
    }

    //收到交换牌
    public void UpdateAllCardsAfterExhchange(pb.GS2CExchangeCardRet msg)
    {
        Dictionary<int, List<pb.CardInfo>> newCardDict = new Dictionary<int, List<pb.CardInfo>>();
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            if (!newCardDict.ContainsKey(msg.cardList[i].playerOID))
            {
                newCardDict.Add(msg.cardList[i].playerOID, new List<pb.CardInfo>());
            }
            newCardDict[msg.cardList[i].playerOID].Add(msg.cardList[i]);
        }
        foreach (int player in _SideInfoDict.Keys)
        {
            if (!newCardDict.ContainsKey(player))
            {
                Debug.LogError("player" + player + " has no new cardlist after exchange.");
                continue;
            }
            List<pb.CardInfo> list = newCardDict[player];
            list.Sort((card1, card2) => { return card1.ID.CompareTo(card2.ID); });

            //log
            string str = "player" + player + " 's cards: ";
            for (int i = 0; i < list.Count; i++)
            {
                str += list[i].ID + ", ";
            }
            Debug.Log(str);

            if (player == Player.Instance.OID)
            {
                List<Card> inhand = _SideInfoDict[player].GetCardList(CardStatus.InHand);
                _SideInfoDict[player].CardList.Clear();
                for (int j = 0; j < list.Count; j++)
                {
                    pb.CardInfo card = list[j];
                    bool isFind = false;
                    for (int i = 0; i < inhand.Count; i++)
                    {
                        if (inhand[i].OID == card.OID)
                        {
                            _SideInfoDict[player].AddCard(card);
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        Card newCard = new Card(card);
                        newCard.Status = CardStatus.Exchange;
                        _SideInfoDict[player].CardList.Add(newCard);
                    }
                }
            }
            else
            {
                _SideInfoDict[player].CardList.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    _SideInfoDict[player].AddCard(list[i]);
                }
            }
        }
        EventDispatcher.TriggerEvent<pb.ExchangeType>(EventDefine.UpdateAllCardsAfterExhchange, msg.type);
    }

    public List<int> GetOtherPlayers()
    {
        List<int> result = new List<int>();
        foreach (int id in _SideInfoDict.Keys)
        {
            if (id != Player.Instance.OID)
            {
                result.Add(id);
            }
        }
        return result;
    }

    //收到出牌方跳转
    public void TurnToNextPlayer(int playerOid, pb.CardInfo drawnCard)
    {
        Debug.Log("turn to next:" + playerOid);
        _curTurnPlayer = playerOid;
        int curPlayerSideIndex = 0;
        if (_SideInfoDict.ContainsKey(_curTurnPlayer))
        {
            _SideInfoDict[_curTurnPlayer].AddCard(drawnCard);
            curPlayerSideIndex = GetSideIndexFromSelf(_SideInfoDict[_curTurnPlayer].Side);
        }
        else
        {
            Debug.LogError("player " + playerOid + " not in room");
        }
        EventDispatcher.TriggerEvent<int>(EventDefine.TurnToPlayer, curPlayerSideIndex);
    }

    //收到当前可操作方式
    public void PlayerProc(pb.GS2CInterruptAction msg)
    {
        for (int i = 0; i < msg.procList.Count; i++)
        {
            pb.ProcType type = msg.procList[i];
            if (type == pb.ProcType.Proc_Gang || type == pb.ProcType.Proc_Hu || type == pb.ProcType.Proc_Peng)
            {
                //碰杠胡
                _procCard = new Card(msg.drawCard);
                if (_curTurnPlayer == Player.Instance.OID && type == pb.ProcType.Proc_Gang)
                {
                    _curProcess = BattleProcess.SelfGangChoose;
                }
                EventDispatcher.TriggerEvent<List<pb.ProcType>>(EventDefine.ProcHPG, msg.procList);
                break;
            }
            else if (type == pb.ProcType.Proc_Discard)
            {
                //出牌
                EventDispatcher.TriggerEvent(EventDefine.ChooseDiscard);
                break;
            }
        }
    }

    //收到操作广播，以及新的手牌列表
    public void UpdateCardsInfo(pb.GS2CBroadcastProc msg)
    {
        if (!_SideInfoDict.ContainsKey(msg.procPlayer))
        {
            Debug.LogError("proc player not in _SideInfoDict.");
            return;
        }
        if (Player.Instance.OID != msg.procPlayer)
        {
            //播放操作动画
            switch (msg.procType)
            {
                case pb.ProcType.Proc_Discard:
                    List<Card> oldDiscard = GetCardList(msg.procPlayer, CardStatus.Discard);
                    for (int i = 0; i < msg.cardList.Count; i++)
                    {
                        if (msg.cardList[i].Status == pb.CardStatus.Dis)
                        {
                            bool isFind = false;
                            for (int j = 0; j < oldDiscard.Count; j++)
                            {
                                if (oldDiscard[j].OID == msg.cardList[i].OID)
                                {
                                    isFind = true;
                                    break;
                                }
                            }
                            if (!isFind)
                            {
                                Debug.Log(msg.procPlayer + "出牌" + msg.cardList[i].ID);
                                _SideInfoDict[msg.procPlayer].AddCard(msg.cardList[i]);
                                EventDispatcher.TriggerEvent<pb.CardInfo>(EventDefine.BroadcastDiscard, msg.cardList[i]);
                                break;
                            }
                        }
                    }
                    break;
                case pb.ProcType.Proc_Hu:
                case pb.ProcType.Proc_Peng:
                case pb.ProcType.Proc_Gang:
                    EventDispatcher.TriggerEvent<pb.ProcType>(EventDefine.BroadcastProc, msg.procType);
                    break;
                default:
                    break;
            }
        }

        updateCardsList(msg.cardList);
        EventDispatcher.TriggerEvent(EventDefine.UpdateAllCardsList);
    }

    private void updateCardsList(List<pb.CardInfo> newCardList)
    {
        Dictionary<int, List<pb.CardInfo>> newCards = getPlayerCardDict(newCardList);
        foreach (int playerId in newCards.Keys)
        {
            if (_SideInfoDict.ContainsKey(playerId))
            {
                List<pb.CardInfo> newList = newCards[playerId];
                List<Card> oldList = _SideInfoDict[playerId].CardList;
                //add
                for (int i = 0; i < newList.Count; i++)
                {
                    bool isFind = false;
                    for (int j = 0; j < oldList.Count; j++)
                    {
                        if (oldList[j].OID == newList[i].OID)
                        {
                            oldList[j] = new Card(newList[i]);
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        Debug.Log("Add new card[" + newList[i].ID + "] to player[" + playerId + "]'s card list.");
                        _SideInfoDict[playerId].AddCard(newList[i]);
                    }
                }
                //delete
                for (int i = 0; i < oldList.Count; i++)
                {
                    bool isFind = false;
                    for (int j = 0; j < newList.Count; j++)
                    {
                        if (oldList[j].OID == newList[i].OID)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        Debug.Log("Delete old card[" + oldList[i].Id + "]from player[" + playerId + "]'s card list.");
                        _SideInfoDict[playerId].CardList.Remove(oldList[i]);
                        i--;
                    }
                }
            }
        }
    }

    private Dictionary<int, List<pb.CardInfo>> getPlayerCardDict(List<pb.CardInfo> list)
    {
        Dictionary<int, List<pb.CardInfo>> result = new Dictionary<int, List<pb.CardInfo>>();
        for (int i = 0; i < list.Count; i++)
        {
            if (!result.ContainsKey(list[i].playerOID))
            {
                result.Add(list[i].playerOID, new List<pb.CardInfo>());
            }
            result[list[i].playerOID].Add(list[i]);
        }
        return result;
    }

    public int GetCardCount(int player, int cardId)
    {
        int count = 0;
        if (_SideInfoDict.ContainsKey(player))
        {
            List<Card> list = _SideInfoDict[player].CardList;            
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == cardId)
                {
                    count++;
                }
            }
        }
        return count;
    }

}
