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

    private BattleProcess _curProcess;
    public BattleProcess CurProcess
    {
        set { _curProcess = value; }
        get { return _curProcess; }
    }

    //playerOid : sideInfo
    private Dictionary<int, SideInfo> m_sideInfoDict = new Dictionary<int, SideInfo>();

    public void PrepareEnterRoom(pb.GS2CEnterGameRet msg)
    {
        MJLog.Log("PrepareEnterGame=> _gameType=" + msg.type.ToString() + ", _roomId=" + msg.roomId);
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
        MJLog.Log("GS2CUpdateRoomMember=> player count:" + msg.player.Count);
        msg.player.Sort((data1, data2) =>
        {
            if (data1.OID == Player.Instance.OID && data2.OID != Player.Instance.OID)
            {
                return 1;
            }
            else if (data1.OID != Player.Instance.OID && data2.OID == Player.Instance.OID)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        });
        MJLog.LogError("first player=" + msg.player[0].OID);
        for (int i = 0; i < msg.player.Count; i++)
        {
            if (m_sideInfoDict.ContainsKey(msg.player[i].OID))
            {
                m_sideInfoDict[msg.player[i].OID].UpdateInfo(msg.player[i]);
            }
            else
            {
                SideInfo info = new SideInfo();
                info.UpdateInfo(msg.player[i]);
                m_sideInfoDict.Add(msg.player[i].OID, info);
            }
        }        
        foreach (int id in m_sideInfoDict.Keys)
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
                m_sideInfoDict.Remove(id);
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.UpdateRoomMember);
        

        ////log
        //string str = "sort side from self=> ";
        //for (int i = 0; i < _sortSideFromSelf.Count; i++)
        //{
        //    str += _sortSideFromSelf[i].ToString() + ", ";
        //}
        //MJLog.LogError(str);
    }

    public pb.MahjonSide GetSelfSide()
    {
        if (m_sideInfoDict.ContainsKey(Player.Instance.OID))
        {
            return m_sideInfoDict[Player.Instance.OID].Side;
        }
        return pb.MahjonSide.DEFAULT;
    }

    public int GetSideIndexByPlayerOID(int playerOid)
    {
        if (m_sideInfoDict.ContainsKey(playerOid))
        {
            return m_sideInfoDict[playerOid].SideIndex;
        }
        return 0;
    }

    public List<SideInfo> GetRoomMembers()
    {
        return new List<SideInfo>(m_sideInfoDict.Values);
    }

    public SideInfo GetSideInfo(int sideIndex)
    {

        foreach (SideInfo info in m_sideInfoDict.Values)
        {
            if (info.SideIndex == sideIndex)
            {
                return info;
            }
        }
        return null;
    }

    public void PrepareGameStart(pb.GS2CBattleStart msg)
    {
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            pb.CardInfo card = msg.cardList[i];
            if (m_sideInfoDict.ContainsKey(card.playerOID))
            {
                m_sideInfoDict[card.playerOID].AddCard(card);
            }
        }
        _dealerId = msg.dealerId;
        MJLog.Log("_dealerId=" + _dealerId);
        EventDispatcher.TriggerEvent(EventDefine.PlayGamePrepareAni);

        //log        
        foreach (int player in m_sideInfoDict.Keys)
        {
            string str = "player" + player + " 's side:";
            str += m_sideInfoDict[player].Side.ToString() + ", cards: ";
            for (int i = 0; i < m_sideInfoDict[player].CardList.Count; i++)
            {
                str += m_sideInfoDict[player].CardList[i].Id + ", ";
            }
            MJLog.Log(str);
        }
    }

    public pb.MahjonSide GetSideByPlayerOID(int playerOid)
    {
        if (m_sideInfoDict.ContainsKey(playerOid))
        {
            return m_sideInfoDict[playerOid].Side;
        }
        return pb.MahjonSide.DEFAULT;
    }

    public int GetPlayerOIDBySide(pb.MahjonSide side)
    {
        foreach (int playerId in m_sideInfoDict.Keys)
        {
            if (m_sideInfoDict[playerId].Side == side)
            {
                return playerId;
            }
        }
        return 0;
    }

    public pb.CardType GetPlayerLack(int playerOid)
    {
        if (m_sideInfoDict.ContainsKey(playerOid))
        {
            return m_sideInfoDict[playerOid].Lack;
        }
        return pb.CardType.Default;
    }

    //收到定缺完毕信息
    public void LackRet(List<pb.LackCard> list)
    {
        MJLog.Log("LackRet");
        for (int i = 0; i < list.Count; i++)
        {
            int player = list[i].playerOID;
            if (m_sideInfoDict.ContainsKey(player))
            {
                m_sideInfoDict[player].Lack = list[i].type;
            }
            else
            {
                MJLog.LogError("has no player" + player + " 's sideInfo.");
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.ShowLackCard);
    }

    public List<Card> GetCardList(int playerOid, CardStatus status)
    {
        if (m_sideInfoDict.ContainsKey(playerOid))
        {
            return m_sideInfoDict[playerOid].GetCardList(status);
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

        foreach (int player in m_sideInfoDict.Keys)
        {
            if (!newCardDict.ContainsKey(player))
            {
                MJLog.LogError("player" + player + " has no new cardlist after exchange.");
                continue;
            }
            List<pb.CardInfo> newList = newCardDict[player];
            newList.Sort((card1, card2) => { return card1.ID.CompareTo(card2.ID); });

            //log
            string str = "player" + player + " 's cards: ";
            for (int i = 0; i < newList.Count; i++)
            {
                str += newList[i].ID + ", ";
            }
            MJLog.Log(str);

            if (player == Player.Instance.OID)
            {
                List<Card> oldList = m_sideInfoDict[player].GetCardList(CardStatus.InHand);
                m_sideInfoDict[player].CardList.Clear();
                for (int j = 0; j < newList.Count; j++)
                {
                    pb.CardInfo card = newList[j];
                    bool isFind = false;
                    for (int i = 0; i < oldList.Count; i++)
                    {
                        if (oldList[i].OID == card.OID)
                        {
                            m_sideInfoDict[player].AddCard(card);
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        Card newCard = new Card(card);
                        newCard.Status = CardStatus.Exchange;
                        m_sideInfoDict[player].CardList.Add(newCard);
                    }
                }
            }
            else
            {
                m_sideInfoDict[player].CardList.Clear();
                for (int i = 0; i < newList.Count; i++)
                {
                    m_sideInfoDict[player].AddCard(newList[i]);
                }
                for (int i = 0; i < 3; i++)
                {
                    m_sideInfoDict[player].CardList[i].Status = CardStatus.Exchange;
                }
            }
        }
        EventDispatcher.TriggerEvent<pb.ExchangeType>(EventDefine.UpdateAllCardsAfterExhchange, msg.type);
    }

    //收到出牌方跳转
    public void TurnToNextPlayer(int playerOid, pb.CardInfo drawnCard)
    {
        MJLog.Log("turn to next:" + playerOid);
        _curTurnPlayer = playerOid;
        int curPlayerSideIndex = 0;
        if (m_sideInfoDict.ContainsKey(_curTurnPlayer))
        {
            if (drawnCard != null)
            {
                m_sideInfoDict[_curTurnPlayer].AddCard(drawnCard);
            }
            curPlayerSideIndex = m_sideInfoDict[_curTurnPlayer].SideIndex;
        }
        else
        {
            MJLog.LogError("player " + playerOid + " not in room");
        }
        EventDispatcher.TriggerEvent<int>(EventDefine.TurnToPlayer, curPlayerSideIndex);
    }

    //收到当前可操作方式
    public void PlayerProc(pb.GS2CInterruptAction msg)
    {
        MJLog.Log("proc count=" + msg.procList.Count);
        for (int i = 0; i < msg.procList.Count; i++)
        {            
            pb.ProcType type = msg.procList[i];
            MJLog.Log("proc=" + type.ToString());
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
                _curProcess = BattleProcess.Discard;
                break;
            }
        }
    }

    //收到操作广播，以及新的手牌列表
    public void UpdateCardsInfo(pb.GS2CBroadcastProc msg)
    {
        if (!m_sideInfoDict.ContainsKey(msg.procPlayer))
        {
            MJLog.LogError("proc player not in _SideInfoDict.");
            return;
        }
        List<int> procPlayersList = new List<int>();
        procPlayersList.Add(_curTurnPlayer);
        if (Player.Instance.OID != msg.procPlayer)
        {
            //播放操作动画
            switch (msg.procType)
            {
                case pb.ProcType.Proc_Discard:
                    List<Card> oldDiscard = GetCardList(msg.procPlayer, CardStatus.Discard);
                    MJLog.LogError("procPlayer[" + msg.procPlayer + "] old card count=" + m_sideInfoDict[msg.procPlayer].CardList.Count);
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
                                MJLog.Log("Is robot discard");
                                List<Card> oldList = m_sideInfoDict[msg.procPlayer].CardList;
                                for (int j = 0; j < oldList.Count; j++)
                                {
                                    if (oldList[j].OID == msg.cardList[i].OID)
                                    {
                                        oldList[j].Status = CardStatus.Discard;
                                        break;
                                    }
                                }
                                MJLog.LogError(msg.procPlayer + "出牌" + msg.cardList[i].ID);
                                MJLog.Log("discard ani=> player[" + msg.procPlayer + "]'s card count=" + m_sideInfoDict[msg.procPlayer].CardList.Count);
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
        List<int> playerIds = new List<int>();
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            bool find = false;
            for (int j = 0; j < playerIds.Count; j++)
            {
                if (playerIds[j] == msg.cardList[i].playerOID)
                {
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                playerIds.Add(msg.cardList[i].playerOID);
            }
        }
        EventDispatcher.TriggerEvent<List<int>>(EventDefine.UpdateAllCardsList, playerIds);
    }

    private void updateCardsList(List<pb.CardInfo> newCardList)
    {
        MJLog.Log("updateCardsList");
        string recv = "";
        for (int i = 0; i < newCardList.Count; i++)
        {
            if (newCardList[i].playerOID == Player.Instance.OID)
            {
                recv += newCardList[i].ID + ", ";
            }
        }
        MJLog.LogError(recv);


        Dictionary<int, List<pb.CardInfo>> newCards = getPlayerCardDict(newCardList);
        foreach (int playerId in newCards.Keys)
        {
            if (m_sideInfoDict.ContainsKey(playerId))
            {
                List<pb.CardInfo> newList = newCards[playerId];
                List<Card> oldList = m_sideInfoDict[playerId].CardList;
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
                        MJLog.Log("Add new card[" + newList[i].ID + "] to player[" + playerId + "]'s card list.");
                        m_sideInfoDict[playerId].AddCard(newList[i]);
                    }
                }
                //delete
                for (int i = 0; i < oldList.Count; i++)
                {
                    bool isFind = false;
                    for (int j = 0; j < newList.Count; j++)
                    {
                        if (oldList[i].OID == newList[j].OID)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        MJLog.Log("Delete old card[" + oldList[i].Id + "]from player[" + playerId + "]'s card list.");
                        EventDispatcher.TriggerEvent<int, int>(EventDefine.RemoveDiscard, oldList[i].OID, oldList[i].PlayerID);
                        m_sideInfoDict[playerId].CardList.Remove(oldList[i]);
                        i--;
                    }
                }
                //MJLog.LogError("player[" + playerId + "]'s card count=" + _SideInfoDict[playerId].CardList.Count);
            }
        }

        //log
        string str = "InHand: ";
        List<Card> list = m_sideInfoDict[Player.Instance.OID].GetCardList(CardStatus.InHand);
        list.Sort((data1, data2) => { return data1.Id.CompareTo(data2.Id); });
        for (int i = 0; i < list.Count; i++)
        {
            str += list[i].Id + ", ";
        }
        MJLog.LogError(str);

        str = "deal: ";
        list = m_sideInfoDict[Player.Instance.OID].GetCardList(CardStatus.Deal);
        list.Sort((data1, data2) => { return data1.Id.CompareTo(data2.Id); });
        for (int i = 0; i < list.Count; i++)
        {
            str += list[i].Id + ", ";
        }
        MJLog.LogError(str);

        str = "peng: ";
        list = m_sideInfoDict[Player.Instance.OID].GetCardList(CardStatus.Peng);
        list.Sort((data1, data2) => { return data1.Id.CompareTo(data2.Id); });
        for (int i = 0; i < list.Count; i++)
        {
            str += list[i].Id + ", ";
        }
        MJLog.LogError(str);

        str = "gang: ";
        list = m_sideInfoDict[Player.Instance.OID].GetCardList(CardStatus.Gang);
        list.Sort((data1, data2) => { return data1.Id.CompareTo(data2.Id); });
        for (int i = 0; i < list.Count; i++)
        {
            str += list[i].Id + ", ";
        }
        MJLog.LogError(str);
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
        if (m_sideInfoDict.ContainsKey(player))
        {
            List<Card> list = m_sideInfoDict[player].CardList;            
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

    public void GameOver()
    {
        MJLog.LogError("!!!!!!!!!");
    }

}
