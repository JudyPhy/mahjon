using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

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

    private BattleProcess _curProcess;
    public BattleProcess CurProcess
    {
        set { _curProcess = value; }
        get { return _curProcess; }
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
        _curPlaySide = getSideByPlayerOID(_dealerId);
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
                        curPaiList.Add(pai);
                    }
                }
                Debug.Log("self has " + curPaiList.Count + " cards.");
            }
            else
            {
                // 其他人直接更新所有牌
                _playerPaiInfoList[i].ClearPai();
                for (int j = 0; j < msg.cardList.Count; j++)
                {
                    _playerPaiInfoList[i].AddPai(msg.cardList[j]);
                }
                Debug.Log("other has " + _playerPaiInfoList[i].GetPaiList().Count + " cards.");
            }
        }
        EventDispatcher.TriggerEvent<pb.ExchangeType>(EventDefine.UpdateCardInfoAfterExchange, msg.type);
    }

    public pb.BattleSide GetDealerSide()
    {
        return getSideByPlayerOID(_dealerId);
    }

    private pb.BattleSide getSideByPlayerOID(int playerOid)
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
    public void TurnToNextPlayer(int playerOid)
    {
        _curPlaySide = getSideByPlayerOID(playerOid);
        EventDispatcher.TriggerEvent<pb.BattleSide>(EventDefine.TurnToPlayer, _curPlaySide);
    }

    private List<Pai> getAllUsefulCardsBySide(pb.BattleSide side) {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetUsefulPaiList();
            }
        }
        return null;
    }

    public bool IsHu()
    {
        Debug.Log("check hu pai...");
        pb.BattleSide side = GetSelfSide();
        List<Pai> inhandList = GetCardListBySideAndStatus(side, PaiStatus.InHand);
        List<Pai> pList = GetCardListBySideAndStatus(side, PaiStatus.Peng);
        List<Pai> gList = GetCardListBySideAndStatus(side, PaiStatus.Gang);
        int count = inhandList.Count + pList.Count + gList.Count;
        Debug.Log("check hu==> all card count is " + count);
        if (count >= 14 && count <= 18)
        {
            Debug.Log("peng card count=" + pList.Count + ", gang count=" + gList.Count);
            if (pList.Count % 3 != 0 || gList.Count % 4 != 0)
            {                
                return false;
            }
            bool isSevenPair = IsSevenPair(inhandList, pList, gList);
            if (isSevenPair)
            {
                return true;
            }
            else
            {
                inhandList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
                IsCommonHu(inhandList);
            }
        }
        return false;
    }

    private bool IsSevenPair(List<Pai> inhandList, List<Pai> pList, List<Pai> gList)
    {
        if ((pList.Count + gList.Count) > 0)
        {
            return false;
        }
        List<Pai> temp = new List<Pai>(inhandList);
        temp.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        int curId = 0;
        for (int i = 0; i < temp.Count; i++)
        {
            if (curId == 0)
            {
                curId = temp[i].Id;
                temp.RemoveAt(i);
                i--;
            }
            else
            {
                if (temp[i].Id == curId)
                {
                    curId = 0;
                    temp.RemoveAt(i);
                    i--;
                }
                else
                {
                    return false;
                }
            }
        }
        return curId == 0;
    }

    private void IsCommonHu(List<Pai> inhandList)
    {
        
    }

    #endregion
}
