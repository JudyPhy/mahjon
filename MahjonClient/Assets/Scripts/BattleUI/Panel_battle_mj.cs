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
    private Dictionary<pb.MahjonSide, List<Item_card>> putedExchangeCards = new Dictionary<pb.MahjonSide, List<Item_card>>(); //side : cardItemList

    private GameObject _lackObj;
    private List<GameObject> _lackBtns = new List<GameObject>();

    private GameObject _sideTipsObj;
    private List<UISprite> _sideTips = new List<UISprite>();
    private System.DateTime _sideTipsTime;
    private int _sideTipsIndex;
    private bool _playingTipsAni;

    private List<GameObject> _sideCardsRoot = new List<GameObject>();
    private Dictionary<pb.MahjonSide, List<Item_card>> _sideCardsDict = new Dictionary<pb.MahjonSide, List<Item_card>>();
    private Dictionary<pb.MahjonSide, List<Item_card>> _sideDiscardsDict = new Dictionary<pb.MahjonSide, List<Item_card>>();

    private GameObject _procObj;
    private UISprite _procCard;
    private UIGrid _procGrid;
    private List<Item_proc> _procItems = new List<Item_proc>();

    private bool _wait_updateMember = false;
    private bool _wait_battleStart = false;
    private RoomProcess _roomProcess;

    private Dictionary<int, ItemGroup_side0> m_sideItems = new Dictionary<int, ItemGroup_side0>();
    private Dictionary<int, int> m_sideDrawCardIndex = new Dictionary<int, int>();
    private int m_curSideIndex = 0;
    private int m_drawCardTurn = 0;

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

        for (int i = 0; i < 4; i++)
        {
            GameObject root = transform.FindChild("CardRoot/Anchor" + i.ToString()).gameObject;
            _sideCardsRoot.Add(root);
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

        EventDispatcher.AddEventListener<pb.ExchangeType>(EventDefine.UpdateAllCardsAfterExhchange, PutOtherExchangeCardToCenter);
        EventDispatcher.AddEventListener(EventDefine.ShowLackCard, ShowLackCard);

        EventDispatcher.AddEventListener<int>(EventDefine.TurnToPlayer, TurnToNextPlayer);
        EventDispatcher.AddEventListener<Card>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
        EventDispatcher.AddEventListener<Card>(EventDefine.EnsureDiscard, EnsureDiscard);

        EventDispatcher.AddEventListener<List<pb.ProcType>>(EventDefine.ProcHPG, ShowProcHPGBtns);
        EventDispatcher.AddEventListener<pb.ProcType>(EventDefine.EnsureProcHPG, EnsureProcHPG);
        EventDispatcher.AddEventListener(EventDefine.UpdateSelfGangCard, UpdateSelfGangCard);

        EventDispatcher.AddEventListener<pb.ProcType>(EventDefine.BroadcastProc, PlayPGHProcAni);

        EventDispatcher.AddEventListener<pb.CardInfo>(EventDefine.BroadcastDiscard, BroadcastDiscard);
        EventDispatcher.AddEventListener<List<int>>(EventDefine.UpdateAllCardsList, UpdateAllCardsList);
        EventDispatcher.AddEventListener<int, int>(EventDefine.RemoveDiscard, HideDiscard);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoomMember, RefreshRoleItems);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);

        EventDispatcher.RemoveEventListener<pb.ExchangeType>(EventDefine.UpdateAllCardsAfterExhchange, PutOtherExchangeCardToCenter);
        EventDispatcher.RemoveEventListener(EventDefine.ShowLackCard, ShowLackCard);

        EventDispatcher.RemoveEventListener<int>(EventDefine.TurnToPlayer, TurnToNextPlayer);
        EventDispatcher.RemoveEventListener<Card>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
        EventDispatcher.RemoveEventListener<Card>(EventDefine.EnsureDiscard, EnsureDiscard);

        EventDispatcher.RemoveEventListener<List<pb.ProcType>>(EventDefine.ProcHPG, ShowProcHPGBtns);
        EventDispatcher.RemoveEventListener<pb.ProcType>(EventDefine.EnsureProcHPG, EnsureProcHPG);
        EventDispatcher.RemoveEventListener(EventDefine.UpdateSelfGangCard, UpdateSelfGangCard);

        EventDispatcher.RemoveEventListener<pb.ProcType>(EventDefine.BroadcastProc, PlayPGHProcAni);

        EventDispatcher.RemoveEventListener<pb.CardInfo>(EventDefine.BroadcastDiscard, BroadcastDiscard);
        EventDispatcher.RemoveEventListener<List<int>>(EventDefine.UpdateAllCardsList, UpdateAllCardsList);
        EventDispatcher.RemoveEventListener<int, int>(EventDefine.RemoveDiscard, HideDiscard);
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

    private Item_role getItemRole(int index, int sideIndex)
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
            int sideIndex = list[i].SideIndex;
            //Debug.Log("sideIndex:" + sideIndex + ", side:" + list[i].Side.ToString());
            Item_role role = getItemRole(n, sideIndex);
            n++;
            role.gameObject.SetActive(true);
            role.UpdateUI(list[i]);
            role.transform.localPosition = pos[sideIndex];
        }
    }

    #region draw cards animation at game start
    private int rightItemDepth = 20;
    private int frontItemDiscardDepth = 5;

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

        Invoke("DrawCardStart", 0.5f);
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

    //Invoke
    private void DrawCardStart()
    {        
        for (int i = 0; i < 4; i++)
        {
            ItemGroup_side0 script = UIManager.AddChild<ItemGroup_side0>(_sideCardsRoot[i]);
            script.Init(BattleManager.Instance.GetSideInfo(i));
            m_sideItems.Add(i, script);
            m_sideDrawCardIndex.Add(i, 0);
        }
        m_curSideIndex = BattleManager.Instance.GetSideIndexByPlayerOID(BattleManager.Instance.DealerID);
        m_drawCardTurn = 0;
        PlayDrawCardsAni();
    }

    private void PlayDrawCardsAni()
    {
        Debug.Log("PlayDrawCardsAni, m_drawCardTurn:" + m_drawCardTurn + ", m_curSideIndex:" + m_curSideIndex);
        if (m_drawCardTurn > 16)
        {
            Debug.Log("draw animation over, start exchange cards...");
            m_sideItems[0].SortInhandCards();           
            _exchangeObj.SetActive(true);
            ShowSideExchangeTips();
            BattleManager.Instance.CurProcess = BattleProcess.ExchangCard;
            return;
        }
        int curDrawCount = m_drawCardTurn > 11 ? 1 : 4;
        m_sideItems[m_curSideIndex].DrawInhandCard(curDrawCount);
        m_drawCardTurn++;
        m_curSideIndex++;
        if (m_curSideIndex >= 4)
        {
            m_curSideIndex = 0;
        }
        Invoke("PlayDrawCardsAni", 0.5f);
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
            string name = "text_select";
            if (BattleManager.Instance.CurProcess == BattleProcess.Lack)
            {
                name = "text_lack";
            }
            _sideTips[i].spriteName = name + _sideTipsIndex.ToString();
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
            UIManager.Instance.ShowTips(TipsType.text, "请选择3张同花色的牌");
            return;
        }
        _exchangeObj.SetActive(false);
        m_sideItems[0].PutExchangeCardsToCenter();
        Invoke("PutExchangeOver", 0.5f);
    }

    //Invoke
    private void PutExchangeOver()
    {
        List<Card> list = m_sideItems[0].SideInfo.GetCardList(CardStatus.Exchange);
        GameMsgHandler.Instance.SendMsgC2GSExchangeCard(list);
        BattleManager.Instance.CurProcess = BattleProcess.ExchangCardOver;
    }

    private void PutOtherExchangeCardToCenter(pb.ExchangeType type)
    {
        _playingTipsAni = false;
        _sideTipsObj.SetActive(false);
        
        for (int i = 1; i < 4; i++)
        {
            m_sideItems[i].PutExchangeCardsToCenter();
        }
        for (int i = 0; i < 4; i++)
        {
            m_sideItems[i].ExchangePlayerCards(type);
        }
        Invoke("ShowExchangeCards", 1.1f);
    }

    //Invoke
    private void ShowExchangeCards()
    {
        Debug.Log("ShowExchangeCards");
        for (int i = 0; i < 4; i++)
        {
            m_sideItems[i].ShowExchangeCards();
        }
        Invoke("ExchangeOver", 1f);
    }

    //Invoke
    private void ExchangeOver()
    {
        Debug.Log("sort and place self cards after exchange.");
        for (int i = 0; i < 4; i++)
        {
            m_sideItems[0].SortForLack();
        }        
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
        m_sideItems[sideIndex].SortForDiscard();
    }    

    private void UnSelectOtherDiscard(Card preDiscard)
    {
        m_sideItems[0].UnChooseDiscard(preDiscard.OID);
    }

    private void EnsureDiscard(Card discard)
    {
        Debug.Log("EnsureDiscard oid:" + discard.OID + ", id:" + discard.Id);
        m_sideItems[0].SortInhandCards();
        m_sideItems[0].PlayDiscardAni(discard);

        pb.CardInfo info = discard.ToPbInfo();
        info.Status = pb.CardStatus.Dis;
        GameMsgHandler.Instance.SendMsgC2GSInterruptActionRet(pb.ProcType.Proc_Discard, info);

        BattleManager.Instance.CurProcess = BattleProcess.DiscardOver;
    }

    private void BroadcastDiscard(pb.CardInfo discard)
    {
        Debug.Log("BroadcastDiscard, discardId=" + discard.ID);
        Card card = new Card(discard);
        int sideIndex = BattleManager.Instance.GetSideIndexByPlayerOID(card.PlayerID);
        m_sideItems[sideIndex].PlayDiscardAni(card);
    }

    private void hideAllProcBtns()
    {
        for (int i = 0; i < _procItems.Count; i++)
        {
            _procItems[i].gameObject.SetActive(false);
        }
    }

    private Item_proc getProcBtnItem(int index)
    {
        if (index < _procItems.Count)
        {
            return _procItems[index];
        }
        Item_proc item = UIManager.AddChild<Item_proc>(_procGrid.gameObject);
        _procItems.Add(item);
        return item;
    }

    private void ShowProcHPGBtns(List<pb.ProcType> procTypes)
    {
        iTween.MoveTo(_procObj, iTween.Hash("x", 275 - 100 * procTypes.Count, "islocal", true, "time", 0.5f));
        _procCard.spriteName = BattleManager.Instance.ProcCard.Id.ToString();
        _procCard.MakePixelPerfect();

        //g、p、h btn
        hideAllProcBtns();
        for (int i = 0; i < procTypes.Count; i++)
        {
            Debug.Log("proc type=" + procTypes[i].ToString());
            Item_proc procBtn = getProcBtnItem(i);
            procBtn.gameObject.name = i.ToString();
            procBtn.gameObject.SetActive(true);
            procBtn.UpdateUI(procTypes[i]);
        }
        _procGrid.Reposition();
    }

    private void EnsureProcHPG(pb.ProcType type)
    {
        iTween.MoveTo(_procObj, iTween.Hash("x", 450, "islocal", true, "time", 0.5f));
        pb.CardInfo card = BattleManager.Instance.ProcCard.ToPbInfo();
        GameMsgHandler.Instance.SendMsgC2GSInterruptActionRet(type, card);
        BattleManager.Instance.CurProcess = BattleProcess.ProcEnsureOver;
    }

    private void UpdateSelfGangCard()
    {
        _procCard.spriteName = BattleManager.Instance.ProcCard.Id.ToString();
        _procCard.MakePixelPerfect();
    }

    private void PlayPGHProcAni(pb.ProcType type)
    {
        Debug.LogError("PlayPGHProcAni, type:" + type.ToString());
    }

    private void HideDiscard(int cardOid, int playerOid)
    {
        Debug.Log("HideDiscard, cardOid:" + cardOid + ", playerOid:" + playerOid);
        pb.MahjonSide side = BattleManager.Instance.GetSideByPlayerOID(playerOid);
        if (_sideDiscardsDict.ContainsKey(side))
        {
            for (int i = 0; i < _sideDiscardsDict[side].Count; i++)
            {
                if (_sideDiscardsDict[side][i].Info.OID == cardOid)
                {
                    _sideDiscardsDict[side][i].gameObject.SetActive(false);
                    break;
                }
            }
        }
            
    }

    private void UpdateAllCardsList(List<int> needUpdatePlayers)
    {
        Debug.Log("UpdateAllCardsList: needUpdatePlayers=" + needUpdatePlayers.Count);
        for (int i = 0; i < needUpdatePlayers.Count; i++)
        {
            int sideIndex = BattleManager.Instance.GetSideIndexByPlayerOID(needUpdatePlayers[i]);
            m_sideItems[sideIndex].SortAllCards();
        }
    }

    #endregion

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
