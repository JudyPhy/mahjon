using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public enum RoomProcess
{
    PlayingEnterRoomAni,
    PlayEnterRoomAniOver,
}

public class Panel_battle_mj : WindowsBasePanel
{
    private GameObject _roomIdObj;
    private UILabel _roomId;
    private UISprite _gameType;
    private GameObject _btnRule;
    private GameObject _btnSetting;
    private GameObject _btnChat;

    private GameObject _timerObj;
    private UILabel _timer;
    private List<GameObject> _sideObjList = new List<GameObject>();
    private UILabel _restCard;
    private UILabel _restRound;

    private GameObject _playerObj;
    private GameObject _playerRootLeft;
    private GameObject _playerRootRight;
    private List<Item_role> _playerItems = new List<Item_role>();

    private GameObject _exchangeObj;
    private UISprite _exchangeBtnEnsure;
    private pb.ExchangeType _exchagneType;
    private Dictionary<pb.MahjonSide, List<Item_card>> putedExchangeCards = new Dictionary<pb.MahjonSide, List<Item_card>>(); //side : cardItemList

    private GameObject _lackObj;
    private List<GameObject> _lackBtns = new List<GameObject>();    

    private GameObject _sideTipsObj;
    private List<UISprite> _sideTips = new List<UISprite>();
    private System.DateTime _sideTipsTime;
    private int _sideTipsIndex;
    private bool _playingTipsAni;

    private Dictionary<pb.MahjonSide, GameObject> _sideCardsRoot = new Dictionary<pb.MahjonSide, GameObject>();
    private Dictionary<pb.MahjonSide, List<Item_card>> _sideCardsDict = new Dictionary<pb.MahjonSide, List<Item_card>>();
    private Dictionary<pb.MahjonSide, List<Item_card>> _sideDiscardsDict = new Dictionary<pb.MahjonSide, List<Item_card>>();

    private GameObject _procObj;
    private UISprite _procCard;
    private UIGrid _procGrid;
    private List<Item_proc> _procItems = new List<Item_proc>();

    private bool _wait_updateMember = false;
    private bool _wait_battleStart = false;
    private RoomProcess _roomProcess;


    public override void OnAwake()
    {
        base.OnAwake();

        _roomIdObj = transform.FindChild("Table/coner1/RoomId").gameObject;
        _roomId = _roomIdObj.transform.FindChild("id").GetComponent<UILabel>();
        _gameType = transform.FindChild("Table/name").GetComponent<UISprite>();
        _btnRule = transform.FindChild("Table/coner1/Rule").gameObject;
        _btnSetting = transform.FindChild("Table/corner3/Setting").gameObject;
        _btnChat = transform.FindChild("Table/corner3/Chat").gameObject;
        UIEventListener.Get(_btnRule).onClick = OnClickRule;
        UIEventListener.Get(_btnSetting).onClick = OnClickSetting;
        UIEventListener.Get(_btnChat).onClick = OnClickChat;

        _timerObj = transform.FindChild("Timer").gameObject;
        _timer = _timerObj.transform.FindChild("timer/Label").GetComponent<UILabel>();
        for (int i = 0; i < 4; i++)
        {
            GameObject side = _timerObj.transform.FindChild("Side" + i.ToString()).gameObject;
            _sideObjList.Add(side);
        }
        _restCard = _timerObj.transform.FindChild("restCard/value").GetComponent<UILabel>();
        _restRound = _timerObj.transform.FindChild("restRound/value").GetComponent<UILabel>();

        _playerObj = transform.FindChild("Players").gameObject;
        _playerRootLeft = transform.FindChild("Players/LeftAnchor").gameObject;
        _playerRootRight = transform.FindChild("Players/RightAnchor").gameObject;

        _exchangeObj = transform.FindChild("Exchange").gameObject;
        _exchangeBtnEnsure = _exchangeObj.transform.FindChild("Button/ensure").GetComponent<UISprite>();
        UIEventListener.Get(_exchangeBtnEnsure.transform.parent.gameObject).onClick = OnClickEnsureExchange;

        _lackObj = transform.FindChild("Lack").gameObject;
        for (int i = 1; i <= 3; i++)
        {
            GameObject btn = _lackObj.transform.FindChild("Button" + i.ToString()).gameObject;
            _lackBtns.Add(btn);
            UIEventListener.Get(btn).onClick = OnClickLack;
        }

        _sideTipsObj = transform.FindChild("SideTips").gameObject;
        for (int i = 1; i < 4; i++)
        {
            UISprite sp = _sideTipsObj.transform.FindChild("Anchor" + i.ToString() + "/selecting").GetComponent<UISprite>();
            _sideTips.Add(sp);
        }

        for (pb.MahjonSide i = pb.MahjonSide.EAST; i <= pb.MahjonSide.NORTH; i++)
        {
            int curIndex = (int)(i - pb.MahjonSide.EAST);
            GameObject root = transform.FindChild("CardRoot/Anchor" + curIndex.ToString()).gameObject;
            _sideCardsRoot.Add(i, root);
        }

        _procObj = transform.FindChild("Proc/bar").gameObject;
        _procCard = _procObj.transform.FindChild("procCard/value").GetComponent<UISprite>();
        _procGrid = _procObj.transform.FindChild("Grid").GetComponent<UIGrid>();
    }

    public override void OnRegisterEvent()
    {
        base.OnRegisterEvent();
        EventDispatcher.AddEventListener(EventDefine.UpdateRoomMember, RefreshRoleItems);
        EventDispatcher.AddEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);

        EventDispatcher.AddEventListener<pb.ExchangeType>(EventDefine.UpdateAllCardsAfterExhchange, PutOtherExchangeCard);
        EventDispatcher.AddEventListener(EventDefine.ShowLackCard, ShowLackCard);

        EventDispatcher.AddEventListener<int>(EventDefine.TurnToPlayer, TurnToNextPlayer);
        EventDispatcher.AddEventListener(EventDefine.ChooseDiscard, ChooseDiscard);
        EventDispatcher.AddEventListener<Card>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
        EventDispatcher.AddEventListener<Card>(EventDefine.EnsureDiscard, EnsureDiscard);

        EventDispatcher.AddEventListener<pb.CardInfo>(EventDefine.BroadcastDiscard, BroadcastDiscard);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoomMember, RefreshRoleItems);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);

        EventDispatcher.RemoveEventListener<pb.ExchangeType>(EventDefine.UpdateAllCardsAfterExhchange, PutOtherExchangeCard);
        EventDispatcher.RemoveEventListener(EventDefine.ShowLackCard, ShowLackCard);

        EventDispatcher.RemoveEventListener<int>(EventDefine.TurnToPlayer, TurnToNextPlayer);
        EventDispatcher.RemoveEventListener(EventDefine.ChooseDiscard, ChooseDiscard);
        EventDispatcher.RemoveEventListener<Card>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
        EventDispatcher.RemoveEventListener<Card>(EventDefine.EnsureDiscard, EnsureDiscard);

        EventDispatcher.RemoveEventListener<pb.CardInfo>(EventDefine.BroadcastDiscard, BroadcastDiscard);
    }

    private void ResetGame()
    {
        _wait_updateMember = false;
        _wait_battleStart = false;
        BattleManager.Instance.CurProcess = BattleProcess.Default;
        _exchangeObj.SetActive(false);
        _lackObj.SetActive(false);
        _sideTipsObj.SetActive(false);
        _playingTipsAni = false;
        _procObj.transform.localPosition = new Vector3(450, 238, 0);
    }

    public override void OnEnableWindow()
    {
        base.OnEnableWindow();

        ResetGame();
        PlayEnterRoomAni();
    }

    private void PlayEnterRoomAni()
    {
        Debug.Log("PlayEnterRoomAni...");
        _roomProcess = RoomProcess.PlayingEnterRoomAni;

        //roomId
        _roomId.text = BattleManager.Instance.RoomID;
        _roomIdObj.transform.localPosition = new Vector3(-93.5f, -40.5f, 0);
        iTween.MoveTo(_roomIdObj, iTween.Hash("x", 93.5f, "islocal", true, "time", 0.5f));

        //setting
        _btnSetting.transform.localPosition = new Vector3(-60f, 51f, 0);
        iTween.MoveTo(_btnSetting, iTween.Hash("y", -51f, "islocal", true, "time", 0.5f, "delay", 0.3f));

        //rule
        _btnRule.transform.localPosition = new Vector3(-30f, -160f, 0);
        iTween.MoveTo(_btnRule, iTween.Hash("x", 38f, "islocal", true, "time", 0.5f, "delay", 0.5f));        

        //chat
        _btnChat.SetActive(false);

        //players
        hideAllPlayerItems();

        //timer
        _timerObj.SetActive(false);

        Invoke("PlayEnterRoomAniOver", 1f);
    }

    private void PlayEnterRoomAniOver()
    {
        Debug.Log("PlayEnterRoomAniOver");
        _roomProcess = RoomProcess.PlayEnterRoomAniOver;
        if (_wait_updateMember)
        {
            RefreshRoleItems();
        }
        if (_wait_battleStart)
        {
            PlayGamePrepareAni();
        }
    }

    private void hideAllPlayerItems()
    {
        for (int i = 0; i < _playerItems.Count; i++)
        {
            _playerItems[i].gameObject.SetActive(false);
        }
    }

    private void OnClickRule(GameObject go)
    {

    }
    private void OnClickSetting(GameObject go)
    {
        UIManager.Instance.ShowTips(TipsType.text, "功能暂未开放");
    }

    private void OnClickChat(GameObject go)
    {
        UIManager.Instance.ShowTips(TipsType.text, "功能暂未开放");
    }

    private Item_role getItemRole(int index,int sideIndex)
    {
        if (index < _playerItems.Count)
        {
            return _playerItems[index];
        }
        GameObject root = sideIndex == 0 || sideIndex == 3 ? _playerRootLeft : _playerRootRight;
        Item_role item = UIManager.AddChild<Item_role>(root);
        _playerItems.Add(item);
        return item;
    }

    private void RefreshRoleItems()
    {
        if (_roomProcess == RoomProcess.PlayingEnterRoomAni)
        {
            Debug.Log("Is playing enter room ani, can't show player item.");
            _wait_updateMember = true;
            return;
        }
        _wait_updateMember = false;
        List<SideInfo> list = BattleManager.Instance.GetRoomMembers();
        Debug.Log("current member count:" + list.Count);
        Vector3[] pos = { new Vector3(65, -95, 0), new Vector3(-65, 10, 0), new Vector3(-65, 180, 0), new Vector3(65, 50, 0) };
        hideAllPlayerItems();
        int n = 0;
        for (int i = 0; i < list.Count; i++)
        {
            int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(list[i].Side);
            //Debug.Log("sideIndex:" + sideIndex + ", side:" + list[i].Side.ToString());
            Item_role role = getItemRole(n, sideIndex);
            n++;
            role.gameObject.SetActive(true);
            role.UpdateUI(list[i]);
            role.transform.localPosition = pos[sideIndex];
        }
    }

    #region draw cards animation at game start
    private int _turns = 0;
    private Dictionary<pb.MahjonSide, int> _drawItemIndex = new Dictionary<pb.MahjonSide, int>();
    private pb.MahjonSide _curSide;
    private int rightItemDepth = 20;
    private void PlayGamePrepareAni()
    {
        if (_roomProcess == RoomProcess.PlayingEnterRoomAni)
        {
            Debug.Log("Is playing enter room ani, can't show battle start ani.");
            _wait_battleStart = true;
            return;
        }
        _wait_battleStart = false;
        Debug.Log("PlayGamePrepareAni...");
        _timerObj.SetActive(true);
        _restCard.text = "108";
        _restRound.text = "8";
        ShowDealer();

        hideAllSideCardItem();
        _curSide = BattleManager.Instance.GetSideByPlayerOID(BattleManager.Instance.DealerID);
        for (pb.MahjonSide i = pb.MahjonSide.EAST; i <= pb.MahjonSide.NORTH; i++)
        {
            _drawItemIndex.Add(i, 0);
        }
        _turns = 0;

        Invoke("PlayDrawCardsAni", 0.5f);
    }

    private void ShowDealer()
    {
        for (int i = 0; i < _playerItems.Count; i++)
        {
            if (_playerItems[i].Info.OID == BattleManager.Instance.DealerID)
            {
                _playerItems[i].ShowDealer();
            }
        }
    }

    private void PlayDrawCardsAni()
    {
        Debug.Log("PlayDrawCardsAni, _turns:" + _turns + ", curSide:" + _curSide);
        if (_turns > 16)
        {
            PlaceInHandCardList(Player.Instance.OID);

            Debug.Log("draw animation over, start exchange cards...");        
            _exchangeObj.SetActive(true);
            ShowSideExchangeTips();            
            BattleManager.Instance.CurProcess = BattleProcess.ExchangCard;
            return;
        }
        int curDrawCount = _turns > 11 ? 1 : 4;
        //Debug.Log("current draw card " + curDrawCount);
        Vector3[] itemAttr = getCardsItemAttr(_curSide);
        int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(_curSide);
        for (int i = 0; i < curDrawCount; i++)
        {
            Card card = BattleManager.Instance.GetDrawCardInfo(_curSide, _drawItemIndex[_curSide]);
            Item_card item = getCardsItem(_curSide, _drawItemIndex[_curSide]);
            item.gameObject.SetActive(true);
            item.UpdateUI(_curSide, card);
            item.transform.localPosition = itemAttr[0] + itemAttr[1] * _drawItemIndex[_curSide];
            if (sideIndex == 1)
            {
                //右侧item要修改depth
                int curDepth = rightItemDepth - _drawItemIndex[_curSide];
                item.SetDepth(curDepth);
            }
            _drawItemIndex[_curSide]++;           
        }
        _turns++;
        _curSide = _curSide == pb.MahjonSide.NORTH ? pb.MahjonSide.EAST : _curSide + 1;
        Invoke("PlayDrawCardsAni", 0.5f);
    }

    private void PlaceInHandCardList(int playerOid)
    {
        List<Card> inhand = BattleManager.Instance.GetCardList(playerOid, CardStatus.InHand);
        inhand.Sort((card1, card2) => { return card1.Id.CompareTo(card2.Id); });
        pb.MahjonSide curSide = BattleManager.Instance.GetSideByPlayerOID(playerOid);
        int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(curSide);
        Vector3[] itemAttr = getCardsItemAttr(curSide);
        hideOneSideCardItem(curSide);
        for (int i = 0; i < inhand.Count; i++)
        {
            Item_card item = getCardsItem(curSide, i);
            item.gameObject.SetActive(true);
            item.UpdateUI(curSide, inhand[i]);
            item.transform.localPosition = itemAttr[0] + itemAttr[1] * i;
            if (sideIndex == 1)
            {
                //右侧item要修改depth
                int curDepth = rightItemDepth - i;
                item.SetDepth(curDepth);
            }
        }        
    }
    #endregion

    #region exchange
    private void ShowSideExchangeTips()
    {
        _sideTipsObj.SetActive(true);
        _playingTipsAni = true;
        _sideTipsIndex = 1;        
        UpdateExchangeSelecting();
    }

    private void UpdateExchangeSelecting()
    {
        _sideTipsTime = System.DateTime.Now;
        for (int i = 0; i < _sideTips.Count; i++)
        {
            _sideTips[i].spriteName = "text_select" + _sideTipsIndex.ToString();
            _sideTips[i].MakePixelPerfect();
        }
        _sideTipsIndex++;
        if (_sideTipsIndex > 4)
        {
            _sideTipsIndex = 1;
        }        
    }

    private void OnClickEnsureExchange(GameObject go)
    {
        List<Card> list = BattleManager.Instance.GetCardList(Player.Instance.OID, CardStatus.Exchange);
        if (list.Count != 3)
        {
            UIManager.Instance.ShowTips(TipsType.text, "请选择3张牌");
            return;
        }
        _exchangeObj.SetActive(false);
        PutSelfExchangeCards(Player.Instance.OID);
        Invoke("PutExchangeOver", 0.5f);
    }

    private void PutSelfExchangeCards(int playerOid)
    {
        Debug.Log("PutExchangeCards");
        putedExchangeCards.Clear();
        PlaceInHandCardList(Player.Instance.OID);

        List<Card> inhand = BattleManager.Instance.GetCardList(Player.Instance.OID, CardStatus.InHand);
        pb.MahjonSide curSide = BattleManager.Instance.GetSideByPlayerOID(playerOid);
        //2:exchange_startPos、3:exchange_startInlineOffset、4:exchange_endPos、5:exchange_endInlineOffset       
        Vector3[] itemAttr = getCardsItemAttr(curSide);
        List<Item_card> exchangeCard = new List<Item_card>();
        for (int i = 0; i < 3; i++)
        {
            Item_card item = getCardsItem(curSide, inhand.Count + i);
            item.gameObject.SetActive(true);
            item.ShowBack(BattleManager.Instance.GetSideIndexFromSelf(curSide));
            item.gameObject.SetActive(true);
            item.transform.localPosition = itemAttr[2] + itemAttr[3] * i;
            item.transform.localScale = Vector3.one * 1.5f;
            iTween.MoveTo(item.gameObject, iTween.Hash("position", itemAttr[4] + itemAttr[5] * i, "islocal", true, "time", 0.5f));
            iTween.ScaleTo(item.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f));
            exchangeCard.Add(item);
        }
        putedExchangeCards.Add(curSide, exchangeCard);
    }

    private void PutExchangeOver()
    {
        List<Card> list = BattleManager.Instance.GetCardList(Player.Instance.OID, CardStatus.Exchange);
        GameMsgHandler.Instance.SendMsgC2GSExchangeCard(list);
        BattleManager.Instance.CurProcess = BattleProcess.ExchangCardOver;        
    }

    private void PutOtherExchangeCard(pb.ExchangeType type)
    {
        _exchagneType = type;
        _playingTipsAni = false;
        _sideTipsObj.SetActive(false);

        List<int> players = BattleManager.Instance.GetOtherPlayers();
        for (int i = 0; i < players.Count; i++)
        {
            List<Card> inhand = BattleManager.Instance.GetCardList(players[i], CardStatus.InHand);
            for (int j = 1; j <= 3; j++)
            {
                inhand[inhand.Count - j].Status = CardStatus.Exchange;
            }
            PlaceInHandCardList(players[i]);
            pb.MahjonSide curSide = BattleManager.Instance.GetSideByPlayerOID(players[i]);
            //2:exchange_startPos、3:exchange_startInlineOffset、4:exchange_endPos、5:exchange_endInlineOffset       
            Vector3[] itemAttr = getCardsItemAttr(curSide);
            List<Item_card> exchangeCard = new List<Item_card>();
            for (int j = 0; j < 3; j++)
            {
                Item_card item = getCardsItem(curSide, inhand.Count - 3 + j);
                item.gameObject.SetActive(true);
                item.ShowBack(BattleManager.Instance.GetSideIndexFromSelf(curSide));
                item.gameObject.SetActive(true);
                item.transform.localPosition = itemAttr[2] + itemAttr[3] * j;
                item.transform.localScale = Vector3.one * 1.5f;
                iTween.MoveTo(item.gameObject, iTween.Hash("position", itemAttr[4] + itemAttr[5] * j, "islocal", true, "time", 0.5f));
                iTween.ScaleTo(item.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f));
                exchangeCard.Add(item);
            }
            putedExchangeCards.Add(curSide, exchangeCard);
        }
        Invoke("UpdateAllCardsAfterExhchange", 0.5f);
    }

    //Invoke
    private void UpdateAllCardsAfterExhchange()
    {
        Debug.Log("exchange type:" + _exchagneType.ToString());
        switch (_exchagneType)
        {
            case pb.ExchangeType.ClockWise:
                PlayClockWiseAni();
                break;
            case pb.ExchangeType.AntiClock:
                PlayAntiClockWiseAni();
                break;
            case pb.ExchangeType.Opposite:
                PlayOppositeAni();
                break;
            default:
                break;
        }
        Invoke("ShowExchangeCards", 0.6f);
    }

    private void PlayClockWiseAni()
    {
        Vector3[] toPos = { new Vector3(-677, -170, 0), new Vector3(415, -332, 0), new Vector3(677, 215, 0), new Vector3(-415, 388, 0) };
        Vector3[] toOffset = { new Vector3(37, 0, 0), new Vector3(0, -28, 0), new Vector3(-37, 0, 0), new Vector3(0, -28, 0) };
        foreach (pb.MahjonSide side in putedExchangeCards.Keys)
        {
            int fromIndex = BattleManager.Instance.GetSideIndexFromSelf(side);
            int toIndex = fromIndex - 1;
            if (toIndex < 0)
            {
                toIndex = 3;
            }
            for (int i = 0; i < putedExchangeCards[side].Count; i++)
            {
                Item_card item = putedExchangeCards[side][i];
                item.ShowBack(toIndex);
                iTween.MoveTo(item.gameObject, iTween.Hash("position", toPos[toIndex] + toOffset[toIndex] * i, "islocal", true, "time", 0.4f));
            }
        }
    }

    private void PlayAntiClockWiseAni()
    {
        Vector3[] toPos = { new Vector3(603, -170, 0), new Vector3(415, 388, 0), new Vector3(-603, 215, 0), new Vector3(-415, -332, 0) };
        Vector3[] toOffset = { new Vector3(37, 0, 0), new Vector3(0, -28, 0), new Vector3(-37, 0, 0), new Vector3(0, -28, 0) };
        foreach (pb.MahjonSide side in putedExchangeCards.Keys)
        {
            int fromIndex = BattleManager.Instance.GetSideIndexFromSelf(side);
            int toIndex = fromIndex + 1;
            if (toIndex > 3)
            {
                toIndex = 0;
            }
            for (int i = 0; i < putedExchangeCards[side].Count; i++)
            {
                Item_card item = putedExchangeCards[side][i];
                item.ShowBack(toIndex);
                iTween.MoveTo(item.gameObject, iTween.Hash("position", toPos[toIndex] + toOffset[toIndex] * i, "islocal", true, "time", 0.4f));
            }
        }
    }

    private void PlayOppositeAni()
    {
        Vector3[] toPos = { new Vector3(-37, -530, 0), new Vector3(1050, 28, 0), new Vector3(37, 570, 0), new Vector3(-1050, -28, 0) };
        Vector3[] toOffset = { new Vector3(37, 0, 0), new Vector3(0, -28, 0), new Vector3(-37, 0, 0), new Vector3(0, 28, 0) };
        foreach (pb.MahjonSide side in putedExchangeCards.Keys)
        {
            int fromIndex = BattleManager.Instance.GetSideIndexFromSelf(side);
            int toIndex = fromIndex + 2;
            if (toIndex > 3)
            {
                toIndex -= 4;
            }
            for (int i = 0; i < putedExchangeCards[side].Count; i++)
            {
                Item_card item = putedExchangeCards[side][i];
                item.ShowBack(toIndex);
                iTween.MoveTo(item.gameObject, iTween.Hash("position", toPos[toIndex] + toOffset[toIndex] * i, "islocal", true, "time", 0.4f));
            }
        }
    }

    //Invoke
    private void ShowExchangeCards()
    {
        Debug.Log("ShowExchangeCards");
        hideAllSideCardItem();
        List<int> players = BattleManager.Instance.GetOtherPlayers();
        players.Add(Player.Instance.OID);
        for (int i = 0; i < players.Count; i++)
        {            
            //inhand list
            List<Card> inhand = BattleManager.Instance.GetCardList(players[i], CardStatus.InHand);
            inhand.Sort((card1, card2) => { return card1.Id.CompareTo(card2.Id); });
            pb.MahjonSide curSide = BattleManager.Instance.GetSideByPlayerOID(players[i]);
            int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(curSide);
            Vector3[] itemAttr = getCardsItemAttr(curSide);
            for (int n = 0; n < inhand.Count; n++)
            {
                Item_card item = getCardsItem(curSide, n);
                item.gameObject.SetActive(true);
                item.UpdateUI(curSide, inhand[n]);
                item.transform.localPosition = itemAttr[0] + itemAttr[1] * n;
                if (sideIndex == 1)
                {
                    //右侧item要修改depth
                    int curDepth = rightItemDepth - n;
                    item.SetDepth(curDepth);
                }
            }
            //exchange list
            List<Card> exchange = BattleManager.Instance.GetCardList(players[i], CardStatus.Exchange);
            exchange.Sort((card1, card2) => { return card1.Id.CompareTo(card2.Id); });
            for (int n = 0; n < exchange.Count; n++)
            {
                Item_card item = getCardsItem(curSide, inhand.Count + n);
                item.gameObject.SetActive(true);
                exchange[n].Status = CardStatus.InHand;
                item.UpdateUI(curSide, exchange[n]);
                item.transform.localPosition = itemAttr[0] + itemAttr[1] * (inhand.Count + n) + itemAttr[6];
                if (sideIndex == 1)
                {
                    //右侧item要修改depth
                    int curDepth = rightItemDepth - (inhand.Count + n);
                    item.SetDepth(curDepth);
                }
                Vector3 endPos = item.transform.localPosition - itemAttr[6];
                iTween.MoveTo(item.gameObject, iTween.Hash("position", endPos, "islocal", true, "time", 0.5f, "delay", 0.5f));
            }
        }
        Invoke("ExchangeOver", 1f);
    }

    //Invoke
    private void ExchangeOver()
    {
        Debug.Log("sort and place self cards after exchange.");
        PlaceInHandCardList(Player.Instance.OID);
        LackStart();
    }
    #endregion

    #region lack
    private void LackStart()
    {
        Debug.Log("start select lack...");
        _lackObj.SetActive(true);
        _sideTipsObj.SetActive(true);
        _sideTipsIndex = 1;
        UpdateLackSelecting();
        _playingTipsAni = true;
        BattleManager.Instance.CurProcess = BattleProcess.Lack;
    }

    private void UpdateLackSelecting()
    {
        _sideTipsTime = System.DateTime.Now;
        for (int i = 0; i < _sideTips.Count; i++)
        {
            _sideTips[i].spriteName = "text_lack" + _sideTipsIndex.ToString();
            _sideTips[i].MakePixelPerfect();
        }
        _sideTipsIndex++;
        if (_sideTipsIndex > 4)
        {
            _sideTipsIndex = 1;
        }
    }

    private void OnClickLack(GameObject go)
    {
        for (int i = 0; i < _lackBtns.Count; i++)
        {
            if (go == _lackBtns[i])
            {
                pb.CardType type = (pb.CardType)(i + 2);
                GameMsgHandler.Instance.SendMsgC2GSSelectLack(type);
                break;
            }
        }
        _lackObj.SetActive(false);
        BattleManager.Instance.CurProcess = BattleProcess.LackOver;
    }

    private void ShowLackCard()
    {
        _sideTipsObj.SetActive(false);
        for (int i = 0; i < _playerItems.Count; i++)
        {
            pb.CardType lack = _playerItems[i].Info.Lack;
            Debug.Log("player" + _playerItems[i].Info.OID + ", lack:" + lack.ToString());
            _playerItems[i].ShowLack();
        }
    }
    #endregion

    #region playing game
    private void TurnToNextPlayer(int sideIndex)
    {
        for (int i = 0; i < _sideObjList.Count; i++)
        {
            _sideObjList[i].SetActive(i == sideIndex);
        }
        pb.MahjonSide side = BattleManager.Instance.GetSideByPlayerOID(BattleManager.Instance.CurTurnPlayer);
        SortOneCards(side);
    }

    private void SortOneCards(pb.MahjonSide side)
    {
        int playerId = BattleManager.Instance.GetPlayerOIDBySide(side);
        hideOneSideCardItem(side);
        Vector3[] vecs = getCardsItemAttr(side);

        //inhand
        //0:inhand_startPos、1:inhand_inlineOffset
        List<Card> inhand = BattleManager.Instance.GetCardList(playerId, CardStatus.InHand);
        inhand.Sort((card1, card2) => { return card1.Id.CompareTo(card2.Id); });
        for (int i = 0; i < inhand.Count; i++)
        {
            Item_card script = getCardsItem(side, i);
            script.gameObject.SetActive(true);
            script.transform.localPosition = vecs[0] + i * vecs[1];
            script.UpdateUI(side, inhand[i]);
        }
        Vector3 inhandLastPos = vecs[0] + inhand.Count * vecs[1];

        //deal
        //7:inhand_deal_space
        inhandLastPos += vecs[7];
        List<Card> deal = BattleManager.Instance.GetCardList(playerId, CardStatus.Deal);
        for (int i = 0; i < deal.Count; i++)
        {
            Item_card script = getCardsItem(side, inhand.Count + i);
            script.gameObject.SetActive(true);
            script.transform.localPosition = inhandLastPos + i * vecs[1];
            script.UpdateUI(side, deal[i]);
        }
        Vector3 dealLastPos = inhandLastPos + (deal.Count == 0 ? 1 : deal.Count) * vecs[1];

        //gang
        //8:deal_gang_space、9:gang_gang_space
        dealLastPos += vecs[8];
        List<Card> gang = BattleManager.Instance.GetCardList(playerId, CardStatus.Gang);
        gang.Sort((card1, card2) =>
        {
            int result = card1.IsFromOther.CompareTo(card2.IsFromOther);
            if (result == 0)
            {
                result = card1.Id.CompareTo(card2.Id);
            }
            return result;
        });
        bool gangOther = false;
        int gIndex = 0;
        for (int i = 0; i < gang.Count; i++)
        {
            if (i % 4 == 0)
            {
                gangOther = gang[i].IsFromOther;
                gIndex++;
            }
            Item_card script = getCardsItem(side, inhand.Count + deal.Count + i);
            script.gameObject.SetActive(true);
            script.transform.localPosition = dealLastPos + i * vecs[1] + (gIndex - 1) * vecs[9];
            if (gangOther)
            {
                script.UpdateUI(side, gang[i]);
            }
            else
            {
                if (i % 4 == 0)
                {
                    script.UpdateUI(side, gang[i]);
                }
                else
                {
                    script.ShowBack(BattleManager.Instance.GetSideIndexFromSelf(side));
                }
            }
        }
        Vector3 gangLastPos = dealLastPos + gang.Count * vecs[1] + (gang.Count / 4 - 1) * vecs[9];

        //peng
        //10:gang_peng_space、11:peng_peng_space
        gangLastPos += vecs[10];
        List<Card> peng = BattleManager.Instance.GetCardList(playerId, CardStatus.Peng);
        peng.Sort((card1, card2) => { return card1.Id.CompareTo(card2.Id); });
        int pIndex = 0;
        for (int i = 0; i < peng.Count; i++)
        {
            pIndex = i % 3 == 0 ? pIndex + 1 : pIndex;
            Item_card script = getCardsItem(side, inhand.Count + deal.Count + gang.Count + i);
            script.gameObject.SetActive(true);
            script.transform.localPosition = gangLastPos + i * vecs[1] + (pIndex - 1) * vecs[11];
            script.UpdateUI(side, gang[i]);
        }

        Debug.Log("Sort cards over.");
    }

    private void ChooseDiscard()
    {
        Debug.Log("start choose discard ==>>");
        BattleManager.Instance.CurProcess = BattleProcess.Discard;
    }

    private void UnSelectOtherDiscard(Card preDiscard)
    {
        pb.MahjonSide side = BattleManager.Instance.GetSideByPlayerOID(BattleManager.Instance.CurTurnPlayer);
        if (!_sideCardsDict.ContainsKey(side))
        {
            Debug.LogError("_sideCardsDict not contains side " + side + ", playerId " + BattleManager.Instance.CurTurnPlayer);
            return;
        }
        List<Item_card> list = _sideCardsDict[side];
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Info.OID != preDiscard.OID)
            {
                list[i].UnChoose();
            }
        }
    }

    private void EnsureDiscard(Card discard)
    {
        Debug.Log("EnsureDiscard oid:" + discard.OID + ", id:" + discard.Id);
        PlayDiscardAni(discard);

        //send discard msg
        pb.CardInfo info = new pb.CardInfo();
        info.OID = discard.OID;
        info.ID = discard.Id;
        info.playerOID = discard.PlayerID;
        info.Status = pb.CardStatus.Dis;
        GameMsgHandler.Instance.SendMsgC2GSInterruptActionRet(pb.ProcType.Proc_Discard, info);
    }

    private void BroadcastDiscard(pb.CardInfo discard)
    {
        Debug.Log("BroadcastDiscard, discardId=" + discard.ID);
        Card card = new Card();
        card.PlayerID = discard.playerOID;
        card.OID = discard.OID;
        card.Id = discard.ID;
        card.Status = CardStatus.Discard;
        card.IsFromOther = discard.fromOther;
        PlayDiscardAni(card);
    }

    private void PlayDiscardAni(Card discard)
    {
        Debug.Log("PlayDiscardAni discardOid:" + discard.OID + ", discardId:" + discard.Id);
        pb.MahjonSide side = BattleManager.Instance.GetSideByPlayerOID(discard.PlayerID);
        SortOneCards(side);
        //animation
        //12:discard_startPos、13:discard_inlineOffset、14:discard_betweenLineOffset、15:discard_ani_startPos
        List<Card> dList = BattleManager.Instance.GetCardList(discard.PlayerID, CardStatus.Discard);
        Item_card item = getDiscardsItem(side, dList.Count - 1);
        item.gameObject.SetActive(true);
        item.UpdateUI(side, discard);
        Vector3[] vecs = getCardsItemAttr(side);
        item.transform.localPosition = vecs[15];
        item.transform.localScale = Vector3.one * 1.2f;
        Vector3 to = vecs[12] + dList.Count / 10 * vecs[14] + dList.Count % 10 * vecs[13];
        iTween.MoveTo(item.gameObject, iTween.Hash("position", to, "islocal", true, "time", 0.5f));
        iTween.ScaleTo(item.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f));
    }

    private void SortOneDiscard(pb.MahjonSide side)
    {
        hideOneSideDiscardItem(side);
        Vector3[] vecs = getCardsItemAttr(side);

        //discard
        //12:discard_startPos、13:discard_inlineOffset、14:discard_betweenLineOffset
        int playerId = BattleManager.Instance.GetPlayerOIDBySide(side);
        List<Card> discard = BattleManager.Instance.GetCardList(playerId, CardStatus.InHand);
        for (int i = 0; i < discard.Count; i++)
        {
            Item_card script = getDiscardsItem(side, i);
            script.gameObject.SetActive(true);
            script.transform.localPosition = vecs[12] + i%10 * vecs[13] + i / 10 * vecs[14];
            script.UpdateUI(side, discard[i]);
        }
    }
    #endregion

    private void hideAllSideCardItem()
    {
        for (int i = 1; i < 5; i++)
        {
            hideOneSideCardItem((pb.MahjonSide)i);
        }
    }

    private void hideOneSideCardItem(pb.MahjonSide side)
    {
        if (_sideCardsDict.ContainsKey(side))
        {
            for (int i = 0; i < _sideCardsDict[side].Count; i++)
            {
                _sideCardsDict[side][i].gameObject.SetActive(false);
            }
        }
    }

    private Item_card getCardsItem(pb.MahjonSide side, int index)
    {
        if (!_sideCardsDict.ContainsKey(side))
        {
            _sideCardsDict.Add(side, new List<Item_card>());
        }
        if (index < _sideCardsDict[side].Count)
        {
            Item_card item = _sideCardsDict[side][index];
            item.transform.localScale = Vector3.one;
            return item;
        }
        else {
            Item_card item = UIManager.AddChild<Item_card>(_sideCardsRoot[side]);
            _sideCardsDict[side].Add(item);
            return item;
        }
    }

    private void hideOneSideDiscardItem(pb.MahjonSide side)
    {
        if (_sideDiscardsDict.ContainsKey(side))
        {
            for (int i = 0; i < _sideDiscardsDict[side].Count; i++)
            {
                _sideDiscardsDict[side][i].gameObject.SetActive(false);
            }
        }
    }

    private Item_card getDiscardsItem(pb.MahjonSide side, int index)
    {
        if (!_sideDiscardsDict.ContainsKey(side))
        {
            _sideDiscardsDict.Add(side, new List<Item_card>());
        }
        if (index < _sideDiscardsDict[side].Count)
        {
            Item_card item = _sideDiscardsDict[side][index];
            item.transform.localScale = Vector3.one;
            return item;
        }
        else
        {
            Item_card item = UIManager.AddChild<Item_card>(_sideCardsRoot[side]);
            _sideDiscardsDict[side].Add(item);
            return item;
        }
    }

    private Vector3[] getCardsItemAttr(pb.MahjonSide side)
    {
        //0:inhand_startPos、1:inhand_inlineOffset、
        //2:exchange_startPos、3:exchange_startInlineOffset、4:exchange_endPos、5:exchange_endInlineOffset、6:exchange_upOffset
        //7:inhand_deal_space
        //8:deal_gang_space、9:gang_gang_space
        //10:gang_peng_space、11:peng_peng_space
        //12:discard_startPos、13:discard_inlineOffset、14:discard_betweenLineOffset、15:discard_ani_startPos
        int sideIndexFromSelf = BattleManager.Instance.GetSideIndexFromSelf(side);
        Vector3[] result = { Vector3.zero, Vector3.zero,
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero  };
        switch (sideIndexFromSelf)
        {
            case 0:
                result[0] = new Vector3(-453, 80, 0);
                result[1] = new Vector3(75, 0, 0);

                result[2] = new Vector3(-65, 160, 0);
                result[3] = new Vector3(65, 0, 0);
                result[4] = new Vector3(-37, 214, 0);
                result[5] = new Vector3(37, 0, 0);
                result[6] = new Vector3(0, 25, 0);

                result[7] = new Vector3(20, 0, 0);

                result[8] = new Vector3(20, 0, 0);
                result[9] = new Vector3(20, 0, 0);

                result[10] = new Vector3(20, 0, 0);
                result[11] = new Vector3(20, 0, 0);

                result[12] = new Vector3(-453, 180, 0);
                result[13] = new Vector3(75, 0, 0);
                result[14] = new Vector3(0, -80, 0);
                result[15] = new Vector3(0, -80, 0);
                break;
            case 1:
                result[0] = new Vector3(-160, -155, 0);
                result[1] = new Vector3(0, 28, 0);

                result[2] = new Vector3(-215, -42, 0);
                result[3] = new Vector3(0, 42, 0);
                result[4] = new Vector3(-285, -28, 0);
                result[5] = new Vector3(0, 28, 0);
                result[6] = new Vector3(0, 10, 0);

                result[7] = new Vector3(0, 10, 0);

                result[8] = new Vector3(0, 10, 0);
                result[9] = new Vector3(0, 10, 0);

                result[10] = new Vector3(0, 10, 0);
                result[11] = new Vector3(0, 10, 0);

                result[12] = new Vector3(-160, -155, 0);
                result[13] = new Vector3(0, -28, 0);
                result[14] = new Vector3(28, 0, 0);
                result[15] = new Vector3(-80, 0, 0);
                break;
            case 2:
                result[0] = new Vector3(228, -65, 0);
                result[1] = new Vector3(-38, 0, 0);

                result[2] = new Vector3(54, -135, 0);
                result[3] = new Vector3(-54, 0, 0);
                result[4] = new Vector3(36, -200, 0);
                result[5] = new Vector3(-36, 0, 0);
                result[6] = new Vector3(0, 10, 0);

                result[7] = new Vector3(-20, 0, 0);

                result[8] = new Vector3(-20, 0, 0);
                result[9] = new Vector3(-20, 0, 0);

                result[10] = new Vector3(-20, 0, 0);
                result[11] = new Vector3(-20, 0, 0);

                result[12] = new Vector3(228, -65, 0);
                result[13] = new Vector3(-38, 0, 0);
                result[14] = new Vector3(0, 30, 0);
                result[15] = new Vector3(0, -80, 0);
                break;
            case 3:
                result[0] = new Vector3(160, 210, 0);
                result[1] = new Vector3(0, -28, 0);

                result[2] = new Vector3(215, 42, 0);
                result[3] = new Vector3(0, -42, 0);
                result[4] = new Vector3(285, 28, 0);
                result[5] = new Vector3(0, -28, 0);
                result[6] = new Vector3(0, 10, 0);

                result[7] = new Vector3(0, -10, 0);

                result[8] = new Vector3(0, -10, 0);
                result[9] = new Vector3(0, -10, 0);

                result[10] = new Vector3(0, -10, 0);
                result[11] = new Vector3(0, -10, 0);

                result[12] = new Vector3(160, 210, 0);
                result[13] = new Vector3(0, -30, 0);
                result[14] = new Vector3(-28, 0, 0);
                result[15] = new Vector3(80, 0, 0);
                break;
            default:
                break;
        }
        return result;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (_playingTipsAni)
        {
            if (System.DateTime.Now.Subtract(_sideTipsTime).TotalMilliseconds >= 1000)
            {
                UpdateExchangeSelecting();
            }
        }
    }

}
