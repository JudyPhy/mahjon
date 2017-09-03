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
    
    private pb.MahjonSide _curPlaySide;
    public pb.MahjonSide CurPlaySide
    {
        get { return _curPlaySide; }
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

    //    private int _curTurnDrawnCardOid;
    //    public int CurTurnDrawnCardOid
    //    {
    //        set { _curTurnDrawnCardOid = value; }
    //        get { return _curTurnDrawnCardOid; }
    //    }

    //    private int _curTurnDiscard;
    //    public int CurTurnDiscard
    //    {
    //        set { _curTurnDiscard = value; }
    //        get { return _curTurnDiscard; }
    //    }

    //    private int _curSelfGangCardId;
    //    public int CurSelfGangCardId
    //    {
    //        set { _curSelfGangCardId = value; }
    //        get { return _curSelfGangCardId; }
    //    }

    //playerOid : sideInfo
    private Dictionary<int, SideInfo> _SideInfoDict = new Dictionary<int, SideInfo>();

    private List<pb.MahjonSide> _sortSideFromSelf = new List<pb.MahjonSide>();
    public List<pb.MahjonSide> SortSideFromSelf
    {
        get { return _sortSideFromSelf; }
    }

    //    private SideInfo getSideInfoBySide(pb.BattleSide side)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (side == _playerPaiInfoList[i].Side)
    //            {
    //                return _playerPaiInfoList[i];
    //            }
    //        }
    //        return null;
    //    }

    //    private pb.BattleSide GetSelfSide()
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (_playerPaiInfoList[i].PlayerInfo.OID == Player.Instance.PlayerInfo.OID)
    //            {
    //                return _playerPaiInfoList[i].Side;
    //            }
    //        }
    //        return pb.BattleSide.none;
    //    }

    //    private int getPlayerIndexInList(int playerId)
    //    {
    //        int index = -1;
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (_playerPaiInfoList[i].PlayerInfo.OID == playerId)
    //            {
    //                index = i;
    //                break;
    //            }
    //        }
    //        return index;
    //    }

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

    //    //方位列表：从自己方位开始按照东南西北排序
    //    public List<pb.BattleSide> GetSortSideListFromSelf()
    //    {
    //        pb.BattleSide selfSide = GetSelfSide();
    //        pb.BattleSide curSide = selfSide;
    //        List<pb.BattleSide> sideSortList = new List<pb.BattleSide>();
    //        do
    //        {
    //            sideSortList.Add(curSide);
    //            curSide++;
    //            if (curSide > pb.BattleSide.north)
    //            {
    //                curSide = pb.BattleSide.east;
    //            }
    //        } while (curSide != selfSide);
    //        return sideSortList;
    //    }

    //    public PlayerInfo GetPlayerInfoBySide(pb.BattleSide side)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (_playerPaiInfoList[i].Side == side)
    //            {
    //                return _playerPaiInfoList[i].PlayerInfo;
    //            }
    //        }
    //        return null;
    //    }

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

    //    public Pai GetPaiInfoByIndexAndSide(pb.BattleSide side, int index)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (side == _playerPaiInfoList[i].Side)
    //            {
    //                List<Pai> list = _playerPaiInfoList[i].GetPaiList();
    //                //Debug.LogError("side=" + side.ToString() + ", current pai list count=" + list.Count);
    //                if (index < list.Count)
    //                {
    //                    return list[index];
    //                }
    //            }
    //        }
    //        return null;
    //    }

    //    public pb.BattleSide GetPaiDrawnSideByShaiZi(pb.BattleSide dealerSide, int shaiziValue)
    //    {
    //        pb.BattleSide curSide = dealerSide;
    //        while (shaiziValue > 1)
    //        {
    //            curSide--;
    //            if (curSide < pb.BattleSide.east)
    //            {
    //                curSide = pb.BattleSide.north;
    //            }
    //            shaiziValue--;
    //        }
    //        return curSide;
    //    }

    //    public void DiscardTimeOut(int playerId)
    //    {

    //    }

    //    public bool HasRecvSelfPlayerInfo()
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (_playerPaiInfoList[i].PlayerInfo.OID == Player.Instance.PlayerInfo.OID)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

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

    //    public pb.CardType GetLackCardTypeByPlayerId(int playerId)
    //    {
    //        for (int j = 0; j < _playerPaiInfoList.Count; j++)
    //        {
    //            if (_playerPaiInfoList[j].PlayerInfo.OID == playerId)
    //            {
    //                return _playerPaiInfoList[j].LackPaiType;
    //            }
    //        }
    //        return pb.CardType.None;
    //    }

    //    public pb.CardType GetLackCardTypeBySide(pb.BattleSide side)
    //    {
    //        SideInfo sideInfo = getSideInfoBySide(side);
    //        if (sideInfo != null)
    //        {
    //            return sideInfo.LackPaiType;
    //        }
    //        return pb.CardType.None;
    //    }

    //public pb.CardType GetExchangeType(int playerOid)
    //{
    //    if (_SideInfoDict.ContainsKey(playerOid))
    //    {

    //        return sideInfo.GetExchangeType();
    //    }
    //    return pb.CardType.Default;
    //}

    //    public int GetExchangeCardCountBySide(pb.BattleSide side)
    //    {
    //        SideInfo sideInfo = getSideInfoBySide(side);
    //        if (sideInfo != null)
    //        {
    //            return sideInfo.GetExchangeCardCount();
    //        }
    //        return 0;
    //    }

    public List<Card> GetCardList(int playerOid, CardStatus status)
    {
        if (_SideInfoDict.ContainsKey(playerOid))
        {
            return _SideInfoDict[playerOid].GetCardList(status);
        }
        return null;
    }

    //    public List<int> GetCardIdListBySideAndStatus(pb.BattleSide side, PaiStatus status)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (_playerPaiInfoList[i].Side == side)
    //            {
    //                return _playerPaiInfoList[i].GetPaiIdListByStatus(status);
    //            }
    //        }
    //        return null;
    //    }

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
                        Card newCard = new Card();
                        newCard.PlayerID = card.playerOID;
                        newCard.OID = card.OID;
                        newCard.Id = card.ID;
                        newCard.Status = CardStatus.Exchange;
                        newCard.IsFromOther = card.fromOther;
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

    //    public pb.BattleSide GetDealerSide()
    //    {
    //        return GetSideByPlayerOID(_dealerId);
    //    }

    //出牌方跳转
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
                EventDispatcher.TriggerEvent<int>(EventDefine.TurnToPlayer, curPlayerSideIndex);
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

    //    private List<Pai> getAllUsefulCardsBySide(pb.BattleSide side)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            if (_playerPaiInfoList[i].Side == side)
    //            {
    //                return _playerPaiInfoList[i].GetUsefulPaiList();
    //            }
    //        }
    //        return null;
    //    }

    //    public bool CanHu(List<int> inhandList, List<int> pList, List<int> gList)
    //    {
    //        Debug.Log("check hu pai...");

    //        int count = inhandList.Count + pList.Count + gList.Count;
    //        Debug.Log("check hu==> all card count is " + count);
    //        if (count < 14 || count > 18)
    //        {
    //            Debug.Log("check hu==> all card count is error.");
    //            return false;
    //        }

    //        if (!checkPeng(pList))
    //        {
    //            return false;
    //        }

    //        if (!checkGang(gList))
    //        {
    //            return false;
    //        }

    //        if (checkSevenPair(inhandList))
    //        {
    //            Debug.Log("check hu==> is 7 pair.");
    //            return true;
    //        }

    //        return checkCommonHu(inhandList);
    //    }

    //    private bool checkPeng(List<int> list)
    //    {
    //        if (list.Count % 3 != 0)
    //        {
    //            Debug.Log("peng card count[" + list.Count + "] is error.");
    //            return false;
    //        }
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            List<int> ds = list.FindAll(delegate (int id) { return id == list[i]; });
    //            if (ds.Count != 3)
    //            {
    //                Debug.Log("peng card[" + ds[0] + "]'count[" + ds.Count + "] is error.");
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    private bool checkGang(List<int> list)
    //    {
    //        if (list.Count % 4 != 0)
    //        {
    //            Debug.Log("gang card count[" + list.Count + "] is error.");
    //            return false;
    //        }
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            List<int> ds = list.FindAll(delegate (int id) { return id == list[i]; });
    //            if (ds.Count != 4)
    //            {
    //                Debug.Log("gang card[" + ds[0] + "]'count[" + ds.Count + "] is error.");
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    private bool checkSevenPair(List<int> list)
    //    {
    //        if (list.Count != 14)
    //        {
    //            return false;
    //        }
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            List<int> ds = list.FindAll(delegate (int id) { return id == list[i]; });
    //            if (ds.Count % 2 != 0)
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    private bool checkCommonHu(List<int> list)
    //    {
    //        list.Sort((x, y) => { return x.CompareTo(y); });

    //        string str = "checkCommonHu list: ";
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            str += list[i].ToString() + ", ";
    //        }
    //        Debug.LogError(str);

    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            List<int> tempList = new List<int>(list);
    //            List<int> ds = tempList.FindAll(delegate (int id) { return id == list[i]; });
    //            if (ds.Count >= 2)
    //            {
    //                //Debug.LogError("将牌：" + ds[0]);
    //                //选择将牌
    //                tempList.Remove(list[i]);
    //                tempList.Remove(list[i]);
    //                i += ds.Count;
    //                //判断剩余牌的情况
    //                if (huPaiPanDing(tempList))
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    private bool huPaiPanDing(List<int> list)
    //    {
    //        //string str = "huPaiPanDing list: ";
    //        //for (int i = 0; i < list.Count; i++)
    //        //{
    //        //    str += list[i].ToString() + ", ";
    //        //}
    //        //Debug.LogError(str);

    //        if (list.Count == 0)
    //        {
    //            return true;
    //        }

    //        List<int> tempList = list.FindAll(delegate (int id) { return id == list[0]; });

    //        //检查刻子
    //        if (tempList.Count == 3)
    //        {
    //            //Debug.Log("去除刻子:" + list[0]);
    //            list.Remove(list[0]);
    //            list.Remove(list[0]);
    //            list.Remove(list[0]);
    //            return huPaiPanDing(list);
    //        }
    //        else
    //        {
    //            if (list.Contains(list[0] + 1) && list.Contains(list[0] + 2))
    //            {
    //                //Debug.Log("去除顺子:" + list[0] + ", " + (list[0] + 1) + ", " + (list[0] + 2));
    //                list.Remove(list[0] + 2);
    //                list.Remove(list[0] + 1);
    //                list.Remove(list[0]);
    //                return huPaiPanDing(list);
    //            }
    //            //Debug.Log("没顺子，没刻子");
    //            return false;
    //        }
    //    }

    //    public bool CanGang(List<int> list)
    //    {
    //        Debug.Log("check gang pai...");

    //        //string str = "";
    //        //for (int i = 0; i < list.Count; i++)
    //        //{
    //        //    str += list[i] + ", ";
    //        //}
    //        //Debug.Log(str);

    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            List<int> tempList = list.FindAll(delegate (int id) { return id == list[i]; });
    //            if (tempList.Count == 4)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    public bool CanPeng(List<int> list, int pCard)
    //    {
    //        Debug.Log("check peng pai...");

    //        //string str = "";
    //        //for (int i = 0; i < list.Count; i++)
    //        //{
    //        //    str += list[i] + ", ";
    //        //}
    //        //Debug.Log(str);

    //        List<int> tempList = list.FindAll(delegate (int id) { return id == pCard; });
    //        if (tempList.Count == 3)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //    #endregion

    //    public Pai GetCardInfoByCurTurnOid(int oid)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            List<Pai> list = _playerPaiInfoList[i].GetPaiList();
    //            for (int j = 0; j < list.Count; j++)
    //            {
    //                if (list[j].OID == oid)
    //                {
    //                    list[j].Status = PaiStatus.Discard;
    //                    return list[j];
    //                }
    //            }
    //        }
    //        return null;
    //    }

    //    public Pai GetCardInfoByCardOid(int cardOid)
    //    {
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            List<Pai> list = _playerPaiInfoList[i].GetPaiList();
    //            for (int j = 0; j < list.Count; j++)
    //            {
    //                if (list[j].OID == cardOid)
    //                {
    //                    return list[j];
    //                }
    //            }
    //        }
    //        return null;
    //    }

    //    public void UpdateCardInfoByDiscardRet(int discardOid)
    //    {
    //        _curTurnDiscard = discardOid;
    //        for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //        {
    //            bool isFind = false;
    //            List<Pai> cardList = _playerPaiInfoList[i].GetPaiList();
    //            for (int n = 0; n < cardList.Count; n++)
    //            {
    //                if (cardList[n].OID == discardOid)
    //                {
    //                    Pai temp = cardList[n];
    //                    cardList.RemoveAt(n);
    //                    cardList.Add(temp); //将最新出的牌排列在最后
    //                    isFind = true;
    //                    break;
    //                }
    //            }
    //            if (isFind)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    private Dictionary<int, List<pb.CardInfo>> getDictByCardList(List<pb.CardInfo> list) {
    //        Dictionary<int, List<pb.CardInfo>> dict = new Dictionary<int, List<pb.CardInfo>>(); //playerOid : cardList
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            if (dict.ContainsKey(list[i].playerId))
    //            {
    //                dict[list[i].playerId].Add(list[i]);
    //            }
    //            else
    //            {
    //                List<pb.CardInfo> cardList = new List<pb.CardInfo>();
    //                cardList.Add(list[i]);
    //                dict.Add(list[i].playerId, cardList);
    //            }
    //        }
    //        return dict;
    //    }

    //    private void updatePaiByCardList(List<pb.CardInfo> newCardInfoList)
    //    {
    //        Dictionary<int, List<pb.CardInfo>> dict = getDictByCardList(newCardInfoList);
    //        foreach (int playerOid in dict.Keys)
    //        {
    //            List<pb.CardInfo> newCardList = dict[playerOid];
    //            Debug.Log("current player[" + playerOid + "] has card count=" + newCardList.Count);
    //            for (int i = 0; i < _playerPaiInfoList.Count; i++)
    //            {
    //                if (_playerPaiInfoList[i].PlayerInfo.OID == playerOid)
    //                {
    //                    //原始牌堆没有new中的牌，则添加，有则更新信息
    //                    List<Pai> origCardList = _playerPaiInfoList[i].GetPaiList();
    //                    for (int n = 0; n < newCardList.Count; n++)
    //                    {
    //                        bool isFind = false;
    //                        for (int m = 0; m < origCardList.Count; m++)
    //                        {
    //                            if (origCardList[m].OID == newCardList[n].CardOid)
    //                            {
    //                                _playerPaiInfoList[i].UpdatePai(origCardList[m], newCardList[n]);
    //                                isFind = true;
    //                                break;
    //                            }
    //                        }
    //                        if (!isFind)
    //                        {
    //                            _playerPaiInfoList[i].AddPai(newCardList[n]);
    //                        }
    //                    }

    //                    _playerPaiInfoList[i].ClearPai();
    //                    for (int n = 0; n < dict[playerOid].Count; n++)
    //                    {
    //                        _playerPaiInfoList[i].AddPai(dict[playerOid][n]);
    //                    }
    //                    //原始牌堆中若有比new多的牌，则删除
    //                    for (int n = 0; n < origCardList.Count; n++)
    //                    {
    //                        bool isFind = false;
    //                        for (int m = 0; m < newCardList.Count; m++)
    //                        {
    //                            if (newCardList[m].CardOid == origCardList[n].OID)
    //                            {
    //                                isFind = true;
    //                                break;
    //                            }
    //                        }
    //                        if (!isFind)
    //                        {
    //                            origCardList.RemoveAt(n);
    //                            n--;
    //                        }
    //                    }
    //                    Debug.Log("After update player" + playerOid + "'s card, card count=" + origCardList.Count);
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    public void ProcessRobotProc(pb.GS2CRobotProc msg)
    //    {
    //        Debug.Log("ProcessRobotProc, procRobot=" + msg.procPlayer + ", beProcPlayer=" + msg.beProcPlayer + ", type=" + msg.procType.ToString());
    //        //更新牌信息
    //        updatePaiByCardList(msg.cardList);
    //        //处理操作动画
    //        EventDispatcher.TriggerEvent<int, int, pb.ProcType>(EventDefine.RobotProc, msg.procPlayer, msg.beProcPlayer, msg.procType);
    //    }

    //    public void ProcessPlayerProc(pb.GS2CPlayerEnsureProc msg)
    //    {
    //        Debug.Log("ProcessPlayerProc");
    //        if (msg.procPlayer == Player.Instance.PlayerInfo.OID)
    //        {
    //            EventDispatcher.TriggerEvent<int, pb.ProcType, int>(EventDefine.SelfEnsureProc, msg.beProcPlayer, msg.procType, msg.procCardId);
    //        }
    //    }

    //    public void UpdateCardInfoByPlayerProcOver(List<pb.CardInfo> list)
    //    {
    //        //更新牌信息
    //        updatePaiByCardList(list);
    //        List<int> updatePlayerOid = new List<int>();
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            bool isFind = false;
    //            for (int j = 0; j < updatePlayerOid.Count; j++)
    //            {
    //                if (updatePlayerOid[j] == list[i].playerId)
    //                {
    //                    isFind = true;
    //                    break;
    //                }
    //            }
    //            if (!isFind)
    //            {
    //                updatePlayerOid.Add(list[i].playerId);
    //            }
    //        }
    //        //对更新牌的玩家重新摆放牌堆
    //        EventDispatcher.TriggerEvent<List<int>>(EventDefine.ReplacePlayerCards, updatePlayerOid);
    //    }

    //    public int GetSelfGangCardId()
    //    {
    //        Dictionary<int, int> dict = new Dictionary<int, int>();
    //        List<int> inhandList = GetCardIdListBySideAndStatus(GetSelfSide(), PaiStatus.InHand);
    //        List<int> pList = GetCardIdListBySideAndStatus(GetSelfSide(), PaiStatus.Peng);
    //        for (int j = 0; j < 2; j++)
    //        {
    //            List<int> list = j == 0 ? inhandList : pList;
    //            for (int i = 0; i < list.Count; i++)
    //            {
    //                if (dict.ContainsKey(list[i]))
    //                {
    //                    dict[list[i]]++;
    //                }
    //                else
    //                {
    //                    dict.Add(list[i], 1);
    //                }
    //            }
    //        }
    //        foreach (int id in dict.Keys)
    //        {
    //            if (dict[id] == 4)
    //            {
    //                return id;
    //            }
    //        }
    //        return 0;
    //    }

    //    public void GameOver()
    //    {
    //        _curProcess = BattleProcess.GameOver;
    //        EventDispatcher.TriggerEvent(EventDefine.GameOver);
    //    }

}
