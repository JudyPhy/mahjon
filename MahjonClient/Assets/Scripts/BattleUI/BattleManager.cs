using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public enum BattleProcess
{
    Default,

    PlayTableAniStart,
    PlayingTableAni,
    PlayingTableAniOver,

    PlayShaiZiAniStart,
    PlayingShaiZiAni,
    PlayShaiZiAniOver,

    PlayStartDrawAniStart,
    PlayingStartDrawAni,
    PlayStartDrawAniOver,

    SortPai,
    SortPaiOver,

    SelectingExchangeCard,
    WaitingExchangeCardOver,

    PlayingExchangeAni,
    PlayExchangeAniOver,

    SelectingLackCard,
    WaitingLackCardInfo,
    PlayingLackAni,

    BattleReady,

    DrawingCard,
    DrawCardOver,

    SortingCard,
    SortCardOver,

    CheckingHu,
    EnsureHuStart,
    EnsuringHu,
    WaitingHuRet,

    CheckingGang,
    EnsureGangStart,

    CheckingPeng,
    EnsurePengStart,

    SelectingDiscard,
    WaitingDiscardRet,

    CheckPengOver,
    EnsurePG,

    SelfTurnOver,

    SelfGanging,

    GameOver,
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

    private pb.GameMode _gameMode;
    private string _roomId;
    public string RoomID
    {
        get { return _roomId; }
    }
    public bool IsWaitingEnterRoomRet = false;
    private int _dealerId;
    public int DealerID
    {
        get { return _dealerId; }
    }

    private List<SideInfo> _playerPaiInfoList = new List<SideInfo>();

    //playing params
    private pb.BattleSide _curPlaySide;
    public pb.BattleSide CurPlaySide
    {
        get { return _curPlaySide; }
    }

    private BattleProcess _curProcess;
    public BattleProcess CurProcess
    {
        set { _curProcess = value; }
        get { return _curProcess; }
    }

    private int _curTurnDrawnCardOid;
    public int CurTurnDrawnCardOid
    {
        set { _curTurnDrawnCardOid = value; }
        get { return _curTurnDrawnCardOid; }
    }

    private int _curTurnDiscard;
    public int CurTurnDiscard
    {
        set { _curTurnDiscard = value; }
        get { return _curTurnDiscard; }
    }

    private int _curSelfGangCardId;
    public int CurSelfGangCardId
    {
        set { _curSelfGangCardId = value; }
        get { return _curSelfGangCardId; }
    }


    private SideInfo getSideInfoBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (side == _playerPaiInfoList[i].Side)
            {
                return _playerPaiInfoList[i];
            }
        }
        return null;
    }

    private pb.BattleSide GetSelfSide()
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == Player.Instance.PlayerInfo.OID)
            {
                return _playerPaiInfoList[i].Side;
            }
        }
        return pb.BattleSide.none;
    }

    private int getPlayerIndexInList(int playerId)
    {
        int index = -1;
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == playerId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void PrepareEnterGame(pb.GS2CEnterGameRet msg)
    {
        Debug.Log("PrepareEnterGame=> _gameMode=" + msg.mode.ToString() + ", _roomId=" + msg.roomId);
        _gameMode = msg.mode;
        _roomId = msg.roomId;
        switch (msg.mode)
        {
            case pb.GameMode.CreateRoom:
                BattleManager.Instance.IsWaitingEnterRoomRet = false;
                break;
            default:
                break;
        }
    }

    public void UpdatePlayerInfo(pb.GS2CUpdateRoomInfo msg)
    {
        Debug.Log("UpdatePlayerInfo=> status:" + msg.status.ToString() + ", player count:" + msg.player.Count);
        switch (msg.status)
        {
            case pb.GS2CUpdateRoomInfo.Status.ADD:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    if (getPlayerIndexInList(msg.player[i].player.oid) != -1)
                    {
                        Debug.LogError("List has contained the player [" + msg.player[i].player.nickName + "], don't need add.");
                    }
                    else
                    {
                        SideInfo info = new SideInfo();
                        info.UpdateBattlePlayerInfo(msg.player[i]);
                        _playerPaiInfoList.Add(info);
                        Debug.Log("add player to room, oid=" + info.PlayerInfo.OID + ", side=" + msg.player[i].side);
                        EventDispatcher.TriggerEvent(EventDefine.UpdateRoleInRoom);
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.REMOVE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    int index = getPlayerIndexInList(msg.player[i].player.oid);
                    if (index != -1)
                    {
                        _playerPaiInfoList.RemoveAt(index);
                    }
                    else
                    {
                        Debug.LogError("Dict doesn't contain the player [" + msg.player[i].player.nickName + "], can't remove.");
                    }
                }
                break;
            case pb.GS2CUpdateRoomInfo.Status.UPDATE:
                for (int i = 0; i < msg.player.Count; i++)
                {
                    int index = getPlayerIndexInList(msg.player[i].player.oid);
                    if (index != -1)
                    {
                        _playerPaiInfoList[index].UpdateBattlePlayerInfo(msg.player[i]);
                    }
                    else
                    {
                        Debug.LogError("Dict doesn't contain the player [" + msg.player[i].player.nickName + "], can't update.");
                    }
                }
                break;
            default:
                break;
        }
    }

    //方位列表：从自己方位开始按照东南西北排序
    public List<pb.BattleSide> GetSortSideListFromSelf()
    {
        pb.BattleSide selfSide = GetSelfSide();
        pb.BattleSide curSide = selfSide;
        List<pb.BattleSide> sideSortList = new List<pb.BattleSide>();
        do
        {
            sideSortList.Add(curSide);
            curSide++;
            if (curSide > pb.BattleSide.north)
            {
                curSide = pb.BattleSide.east;
            }
        } while (curSide != selfSide);
        return sideSortList;
    }

    public PlayerInfo GetPlayerInfoBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].PlayerInfo;
            }
        }
        return null;
    }

    public void PrepareGameStart(pb.GS2CBattleStart msg)
    {
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            pb.CardInfo card = msg.cardList[i];
            for (int j = 0; j < _playerPaiInfoList.Count; j++)
            {
                if (_playerPaiInfoList[j].PlayerInfo.OID == card.playerId)
                {
                    _playerPaiInfoList[j].AddPai(card);
                }
            }
        }
        _dealerId = msg.dealerId;
        _curPlaySide = GetSideByPlayerOID(_dealerId);
        Debug.Log("_dealerId=" + _dealerId + ", side=" + _curPlaySide.ToString());
        EventDispatcher.TriggerEvent(EventDefine.PlayGamePrepareAni);
    }

    public Pai GetPaiInfoByIndexAndSide(pb.BattleSide side, int index)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (side == _playerPaiInfoList[i].Side)
            {
                List<Pai> list = _playerPaiInfoList[i].GetPaiList();
                //Debug.LogError("side=" + side.ToString() + ", current pai list count=" + list.Count);
                if (index < list.Count)
                {
                    return list[index];
                }
            }
        }
        return null;
    }

    public pb.BattleSide GetPaiDrawnSideByShaiZi(pb.BattleSide dealerSide, int shaiziValue)
    {
        pb.BattleSide curSide = dealerSide;
        while (shaiziValue > 1)
        {
            curSide--;
            if (curSide < pb.BattleSide.east)
            {
                curSide = pb.BattleSide.north;
            }
            shaiziValue--;
        }
        return curSide;
    }

    public void DiscardTimeOut(int playerId)
    {

    }

    public bool HasRecvSelfPlayerInfo()
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == Player.Instance.PlayerInfo.OID)
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateLackCardInfo(List<pb.LackCard> list)
    {
        Debug.Log("lack card count=" + list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = 0; j < _playerPaiInfoList.Count; j++)
            {
                if (_playerPaiInfoList[j].PlayerInfo.OID == list[i].playerId)
                {
                    Debug.Log("player " + _playerPaiInfoList[j].PlayerInfo.NickName + " 's lack card is " + list[i].type);
                    _playerPaiInfoList[j].LackPaiType = list[i].type;
                    break;
                }
            }
        }
        EventDispatcher.TriggerEvent(EventDefine.ShowLackCard);
    }

    public pb.CardType GetLackCardTypeByPlayerId(int playerId)
    {
        for (int j = 0; j < _playerPaiInfoList.Count; j++)
        {
            if (_playerPaiInfoList[j].PlayerInfo.OID == playerId)
            {
                return _playerPaiInfoList[j].LackPaiType;
            }
        }
        return pb.CardType.None;
    }

    public pb.CardType GetLackCardTypeBySide(pb.BattleSide side)
    {
        SideInfo sideInfo = getSideInfoBySide(side);
        if (sideInfo != null)
        {
            return sideInfo.LackPaiType;
        }
        return pb.CardType.None;
    }

    public pb.CardType GetExchangeTypeBySide(pb.BattleSide side)
    {
        SideInfo sideInfo = getSideInfoBySide(side);
        if (sideInfo != null)
        {
            return sideInfo.GetExchangeType();
        }
        return pb.CardType.None;
    }

    public int GetExchangeCardCountBySide(pb.BattleSide side)
    {
        SideInfo sideInfo = getSideInfoBySide(side);
        if (sideInfo != null)
        {
            return sideInfo.GetExchangeCardCount();
        }
        return 0;
    }

    public List<Pai> GetCardListBySideAndStatus(pb.BattleSide side, PaiStatus status)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetPaiListByStatus(status);
            }
        }
        return null;
    }

    public List<int> GetCardIdListBySideAndStatus(pb.BattleSide side, PaiStatus status)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetPaiIdListByStatus(status);
            }
        }
        return null;
    }

    public void UpdateExchangeCardInfo(pb.GS2CUpdateCardInfoAfterExchange msg)
    {
        //只处理自己交换牌
        List<pb.CardInfo> selfNewCardList = new List<pb.CardInfo>();
        for (int i = 0; i < msg.cardList.Count; i++)
        {
            if (msg.cardList[i].playerId == Player.Instance.PlayerInfo.OID)
            {
                selfNewCardList.Add(msg.cardList[i]);
            }
        }
        Debug.Log("After exchange card, self card count is " + selfNewCardList.Count);

        List<int> _selfExchangeCardOid = new List<int>();
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            int playerId = _playerPaiInfoList[i].PlayerInfo.OID;
            if (playerId == Player.Instance.PlayerInfo.OID)
            {
                //自己的牌将交换牌区分出来，用以动画表现
                _playerPaiInfoList[i].RemoveExchangeCard();
                List<Pai> curPaiList = _playerPaiInfoList[i].GetPaiListByStatus(PaiStatus.InHand);
                Debug.Log("self has " + curPaiList.Count + " inhand cards.");
                for (int n = 0; n < selfNewCardList.Count; n++)
                {
                    pb.CardInfo curCard = selfNewCardList[n];
                    bool isFind = false;
                    for (int j = 0; j < curPaiList.Count; j++)
                    {
                        if (curPaiList[j].OID == curCard.CardOid)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        Pai pai = new Pai();
                        pai.OID = curCard.CardOid;
                        pai.Id = curCard.CardId;
                        pai.Status = PaiStatus.Exchange;
                        pai.PlayerID = curCard.playerId;
                        _playerPaiInfoList[i].GetPaiList().Add(pai);
                    }
                }
                Debug.Log("self has " + _playerPaiInfoList[i].GetPaiList().Count + " cards.");
            }
            else
            {
                // 其他人直接更新所有牌
                _playerPaiInfoList[i].ClearPai();
                for (int j = 0; j < msg.cardList.Count; j++)
                {
                    if (playerId == msg.cardList[j].playerId)
                    {
                        _playerPaiInfoList[i].AddPai(msg.cardList[j]);
                    }
                }
                Debug.Log("other has " + _playerPaiInfoList[i].GetPaiList().Count + " cards.");
            }
        }
        EventDispatcher.TriggerEvent<pb.ExchangeType>(EventDefine.UpdateCardInfoAfterExchange, msg.type);
    }

    public pb.BattleSide GetDealerSide()
    {
        return GetSideByPlayerOID(_dealerId);
    }

    public pb.BattleSide GetSideByPlayerOID(int playerOid)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == playerOid)
            {
                return _playerPaiInfoList[i].Side;
            }
        }
        return pb.BattleSide.none;
    }

    #region playing
    public void TurnToNextPlayer(int playerOid, pb.CardInfo drawnCard, pb.TurnSwitchType type)
    {
        Debug.Log("turn to next:" + playerOid + ", type=" + type.ToString());
        if (drawnCard != null)
        {
            Debug.Log("draw new card：" + drawnCard.CardOid);
            BattleManager.Instance.CurTurnDrawnCardOid = drawnCard.CardOid;
        }
        _curPlaySide = GetSideByPlayerOID(playerOid);
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == playerOid && drawnCard != null)
            {
                _playerPaiInfoList[i].AddPai(drawnCard);
            }
        }
        EventDispatcher.TriggerEvent<pb.BattleSide, pb.CardInfo, pb.TurnSwitchType>(EventDefine.TurnToPlayer, _curPlaySide, drawnCard, type);
    }

    private List<Pai> getAllUsefulCardsBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetUsefulPaiList();
            }
        }
        return null;
    }

    public bool CanHu(List<int> inhandList, List<int> pList, List<int> gList)
    {
        Debug.Log("check hu pai...");

        int count = inhandList.Count + pList.Count + gList.Count;
        Debug.Log("check hu==> all card count is " + count);
        if (count < 14 || count > 18)
        {
            Debug.Log("check hu==> all card count is error.");
            return false;
        }

        if (!checkPeng(pList))
        {
            return false;
        }

        if (!checkGang(gList))
        {
            return false;
        }

        if (checkSevenPair(inhandList))
        {
            Debug.Log("check hu==> is 7 pair.");
            return true;
        }

        return checkCommonHu(inhandList);
    }

    private bool checkPeng(List<int> list)
    {
        if (list.Count % 3 != 0)
        {
            Debug.Log("peng card count[" + list.Count + "] is error.");
            return false;
        }
        for (int i = 0; i < list.Count; i++)
        {
            List<int> ds = list.FindAll(delegate (int id) { return id == list[i]; });
            if (ds.Count != 3)
            {
                Debug.Log("peng card[" + ds[0] + "]'count[" + ds.Count + "] is error.");
                return false;
            }
        }
        return true;
    }

    private bool checkGang(List<int> list)
    {
        if (list.Count % 4 != 0)
        {
            Debug.Log("gang card count[" + list.Count + "] is error.");
            return false;
        }
        for (int i = 0; i < list.Count; i++)
        {
            List<int> ds = list.FindAll(delegate (int id) { return id == list[i]; });
            if (ds.Count != 4)
            {
                Debug.Log("gang card[" + ds[0] + "]'count[" + ds.Count + "] is error.");
                return false;
            }
        }
        return true;
    }

    private bool checkSevenPair(List<int> list)
    {
        if (list.Count != 14)
        {
            return false;
        }
        for (int i = 0; i < list.Count; i++)
        {
            List<int> ds = list.FindAll(delegate (int id) { return id == list[i]; });
            if (ds.Count % 2 != 0)
            {
                return false;
            }
        }
        return true;
    }

    private bool checkCommonHu(List<int> list)
    {
        list.Sort((x, y) => { return x.CompareTo(y); });

        string str = "checkCommonHu list: ";
        for (int i = 0; i < list.Count; i++)
        {
            str += list[i].ToString() + ", ";
        }
        Debug.LogError(str);

        for (int i = 0; i < list.Count; i++)
        {
            List<int> tempList = new List<int>(list);
            List<int> ds = tempList.FindAll(delegate (int id) { return id == list[i]; });
            if (ds.Count >= 2)
            {
                //Debug.LogError("将牌：" + ds[0]);
                //选择将牌
                tempList.Remove(list[i]);
                tempList.Remove(list[i]);
                i += ds.Count;
                //判断剩余牌的情况
                if (huPaiPanDing(tempList))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool huPaiPanDing(List<int> list)
    {
        //string str = "huPaiPanDing list: ";
        //for (int i = 0; i < list.Count; i++)
        //{
        //    str += list[i].ToString() + ", ";
        //}
        //Debug.LogError(str);

        if (list.Count == 0)
        {
            return true;
        }

        List<int> tempList = list.FindAll(delegate (int id) { return id == list[0]; });

        //检查刻子
        if (tempList.Count == 3)
        {
            //Debug.Log("去除刻子:" + list[0]);
            list.Remove(list[0]);
            list.Remove(list[0]);
            list.Remove(list[0]);
            return huPaiPanDing(list);
        }
        else
        {
            if (list.Contains(list[0] + 1) && list.Contains(list[0] + 2))
            {
                //Debug.Log("去除顺子:" + list[0] + ", " + (list[0] + 1) + ", " + (list[0] + 2));
                list.Remove(list[0] + 2);
                list.Remove(list[0] + 1);
                list.Remove(list[0]);
                return huPaiPanDing(list);
            }
            //Debug.Log("没顺子，没刻子");
            return false;
        }
    }

    public bool CanGang(List<int> list)
    {
        Debug.Log("check gang pai...");

        //string str = "";
        //for (int i = 0; i < list.Count; i++)
        //{
        //    str += list[i] + ", ";
        //}
        //Debug.Log(str);

        for (int i = 0; i < list.Count; i++)
        {
            List<int> tempList = list.FindAll(delegate (int id) { return id == list[i]; });
            if (tempList.Count == 4)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanPeng(List<int> list, int pCard)
    {
        Debug.Log("check peng pai...");

        //string str = "";
        //for (int i = 0; i < list.Count; i++)
        //{
        //    str += list[i] + ", ";
        //}
        //Debug.Log(str);

        List<int> tempList = list.FindAll(delegate (int id) { return id == pCard; });
        if (tempList.Count == 3)
        {
            return true;
        }
        return false;
    }
    #endregion

    public Pai GetCardInfoByCurTurnOid(int oid)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            List<Pai> list = _playerPaiInfoList[i].GetPaiList();
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].OID == oid)
                {
                    list[j].Status = PaiStatus.Discard;
                    return list[j];
                }
            }
        }
        return null;
    }

    public Pai GetCardInfoByCardOid(int cardOid)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            List<Pai> list = _playerPaiInfoList[i].GetPaiList();
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].OID == cardOid)
                {
                    return list[j];
                }
            }
        }
        return null;
    }
    
    public void UpdateCardInfoByDiscardRet(int discardOid)
    {
        _curTurnDiscard = discardOid;
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            bool isFind = false;
            List<Pai> cardList = _playerPaiInfoList[i].GetPaiList();
            for (int n = 0; n < cardList.Count; n++)
            {
                if (cardList[n].OID == discardOid)
                {
                    Pai temp = cardList[n];
                    cardList.RemoveAt(n);
                    cardList.Add(temp); //将最新出的牌排列在最后
                    isFind = true;
                    break;
                }
            }
            if (isFind)
            {
                break;
            }
        }
    }

    private Dictionary<int, List<pb.CardInfo>> getDictByCardList(List<pb.CardInfo> list) {
        Dictionary<int, List<pb.CardInfo>> dict = new Dictionary<int, List<pb.CardInfo>>(); //playerOid : cardList
        for (int i = 0; i < list.Count; i++)
        {
            if (dict.ContainsKey(list[i].playerId))
            {
                dict[list[i].playerId].Add(list[i]);
            }
            else
            {
                List<pb.CardInfo> cardList = new List<pb.CardInfo>();
                cardList.Add(list[i]);
                dict.Add(list[i].playerId, cardList);
            }
        }
        return dict;
    }

    private void updatePaiByCardList(List<pb.CardInfo> newCardInfoList)
    {
        Dictionary<int, List<pb.CardInfo>> dict = getDictByCardList(newCardInfoList);
        foreach (int playerOid in dict.Keys)
        {
            List<pb.CardInfo> newCardList = dict[playerOid];
            Debug.Log("current player[" + playerOid + "] has card count=" + newCardList.Count);
            for (int i = 0; i < _playerPaiInfoList.Count; i++)
            {
                if (_playerPaiInfoList[i].PlayerInfo.OID == playerOid)
                {
                    //原始牌堆没有new中的牌，则添加，有则更新信息
                    List<Pai> origCardList = _playerPaiInfoList[i].GetPaiList();
                    for (int n = 0; n < newCardList.Count; n++)
                    {
                        bool isFind = false;
                        for (int m = 0; m < origCardList.Count; m++)
                        {
                            if (origCardList[m].OID == newCardList[n].CardOid)
                            {
                                _playerPaiInfoList[i].UpdatePai(origCardList[m], newCardList[n]);
                                isFind = true;
                                break;
                            }
                        }
                        if (!isFind)
                        {
                            _playerPaiInfoList[i].AddPai(newCardList[n]);
                        }
                    }

                    _playerPaiInfoList[i].ClearPai();
                    for (int n = 0; n < dict[playerOid].Count; n++)
                    {
                        _playerPaiInfoList[i].AddPai(dict[playerOid][n]);
                    }
                    //原始牌堆中若有比new多的牌，则删除
                    for (int n = 0; n < origCardList.Count; n++)
                    {
                        bool isFind = false;
                        for (int m = 0; m < newCardList.Count; m++)
                        {
                            if (newCardList[m].CardOid == origCardList[n].OID)
                            {
                                isFind = true;
                                break;
                            }
                        }
                        if (!isFind)
                        {
                            origCardList.RemoveAt(n);
                            n--;
                        }
                    }
                    Debug.Log("After update player" + playerOid + "'s card, card count=" + origCardList.Count);
                    break;
                }
            }
        }
    }

    public void ProcessRobotProc(pb.GS2CRobotProc msg)
    {
        Debug.Log("ProcessRobotProc, procRobot=" + msg.procPlayer + ", beProcPlayer=" + msg.beProcPlayer + ", type=" + msg.procType.ToString());
        //更新牌信息
        updatePaiByCardList(msg.cardList);
        //处理操作动画
        EventDispatcher.TriggerEvent<int, int, pb.ProcType>(EventDefine.RobotProc, msg.procPlayer, msg.beProcPlayer, msg.procType);
    }

    public void ProcessPlayerProc(pb.GS2CPlayerEnsureProc msg)
    {
        Debug.Log("ProcessPlayerProc");
        if (msg.procPlayer == Player.Instance.PlayerInfo.OID)
        {
            EventDispatcher.TriggerEvent<int, pb.ProcType, int>(EventDefine.SelfEnsureProc, msg.beProcPlayer, msg.procType, msg.procCardId);
        }
    }

    public void UpdateCardInfoByPlayerProcOver(List<pb.CardInfo> list)
    {
        //更新牌信息
        updatePaiByCardList(list);
        List<int> updatePlayerOid = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            bool isFind = false;
            for (int j = 0; j < updatePlayerOid.Count; j++)
            {
                if (updatePlayerOid[j] == list[i].playerId)
                {
                    isFind = true;
                    break;
                }
            }
            if (!isFind)
            {
                updatePlayerOid.Add(list[i].playerId);
            }
        }
        //对更新牌的玩家重新摆放牌堆
        EventDispatcher.TriggerEvent<List<int>>(EventDefine.ReplacePlayerCards, updatePlayerOid);
    }

    public int GetSelfGangCardId()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        List<int> inhandList = GetCardIdListBySideAndStatus(GetSelfSide(), PaiStatus.InHand);
        List<int> pList = GetCardIdListBySideAndStatus(GetSelfSide(), PaiStatus.Peng);
        for (int j = 0; j < 2; j++)
        {
            List<int> list = j == 0 ? inhandList : pList;
            for (int i = 0; i < list.Count; i++)
            {
                if (dict.ContainsKey(list[i]))
                {
                    dict[list[i]]++;
                }
                else
                {
                    dict.Add(list[i], 1);
                }
            }
        }
        foreach (int id in dict.Keys)
        {
            if (dict[id] == 4)
            {
                return id;
            }
        }
        return 0;
    }

    public void GameOver()
    {
        _curProcess = BattleProcess.GameOver;
        EventDispatcher.TriggerEvent(EventDefine.GameOver);
    }

}
