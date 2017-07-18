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
    public pb.BattleSide CurPlaySide
    {
        set { _curPlaySide = value; }
        get { return _curPlaySide; }
    }

    private BattleProcess _curProcess;
    public BattleProcess CurProcess
    {
        set { _curProcess = value; }
        get { return _curProcess; }
    }

    private BattleProcess _playingProcess = BattleProcess.Default;
    public BattleProcess PlayingProcess
    {
        set { _playingProcess = value; }
        get { return _playingProcess; }
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
        _curPlaySide = GetDealerSide();
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

    public pb.BattleSide GetDealerSide()
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == _dealerId)
            {
                return _playerPaiInfoList[i].Side;
            }
        }
        return pb.BattleSide.none;
    }
    
    public List<Pai> GetAllInHandPaiListBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetPaiListByStatus(PaiStatus.InHand);
            }
        }
        return new List<Pai>();
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

    public List<Pai> GetExchangeCardListBySide(pb.BattleSide side)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].Side == side)
            {
                return _playerPaiInfoList[i].GetPaiListByStatus(PaiStatus.Exchange);
            }
        }
        return null;
    }

    public pb.BattleSide GetSideByPlayerId(int playerId)
    {
        for (int i = 0; i < _playerPaiInfoList.Count; i++)
        {
            if (_playerPaiInfoList[i].PlayerInfo.OID == playerId)
            {
                return _playerPaiInfoList[i].Side;
            }
        }
        return pb.BattleSide.none;
    }

}
