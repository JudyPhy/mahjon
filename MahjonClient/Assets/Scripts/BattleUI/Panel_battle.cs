using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventTransmit;

public class Panel_battle : WindowsBasePanel
{
    private BattleProcess _battleProcess = BattleProcess.Default;

    private UILabel _roomId;

    // table
    private Animation _tableAni;
    private GameObject _sideObj;
    private List<SidePai> _sidePaiWallList = new List<SidePai>(); //从自己方位(0)开始，逆时针旋转
    private GameObject _curSideFlag;

    // players
    private GameObject _playerRoot;
    private List<Item_role> _roleItemList = new List<Item_role>();
    private List<pb.BattleSide> _sortedSideListFromSelf = new List<pb.BattleSide>(); //从自己方位(0)开始，逆时针旋转    

    // side pai   
    private GameObject _self2DCardItemRoot;
    private List<Item_pai> _self2DCardItemList = new List<Item_pai>();
    private GameObject _self3DCardItemRoot;
    private List<Item_pai_3d> _self3DCardItemList = new List<Item_pai_3d>();
    private List<GameObject> _otherCardObjRoot = new List<GameObject>();
    private Dictionary<pb.BattleSide, List<Item_pai_3d>> _sideCardObjDict = new Dictionary<pb.BattleSide, List<Item_pai_3d>>();

    // prepare ani  
    private bool _hasPlayStartAni = false;
    private int[] _shaiziValue = new int[2];
    private int _drawCardSumCount_prepareAni = 0;  //抓牌总张数
    private int _drawRound_prepareAni = 0;  //抓牌次数
    private pb.BattleSide _drawerSide_prepareAni;  //抓牌人方位
    private int[] _placedPaiCount_prepareAni = { 0, 0, 0, 0 }; //各个方位已经放置的牌张数
    private int _drawnSideIndex_prepareAni;
    private int _drawnCardOffsetIndex_prepareAni;

    // slelect exchange card
    private GameObject _exchangeCardContainer;
    private GameObject _exchangingContainer;
    private UIButton _btnEnsureSelectExchange;
    private List<UILabel> _exchangeTips = new List<UILabel>();
    private List<System.DateTime> _exchangeTipsAniTime = new List<System.DateTime>();

    // after exchange card
    private GameObject _afterExchangeContainer;
    private List<Item_exchangeArrow> _exchangeArrows = new List<Item_exchangeArrow>();
    private pb.ExchangeType _exchangeType;

    // select lack
    private GameObject _selectLackContainer;
    private List<Item_lack> _itemLackList = new List<Item_lack>();

    // sort all status cards
    private int _curSelf2DItemIndex_playing;
    private int _curOther3DItemIndex_playing;
    private Dictionary<pb.BattleSide, List<Item_pai_3d>> _other3DDiscard = new Dictionary<pb.BattleSide, List<Item_pai_3d>>();

    public override void OnAwake()
    {
        base.OnAwake();
        _roomId = transform.FindChild("RoomID").GetComponent<UILabel>();

        // table
        GameObject _tableRoot = GameObject.Find("TableRoot");
        _tableAni = _tableRoot.transform.FindChild("table").GetComponent<Animation>();
        _tableAni.gameObject.SetActive(true);
        _tableAni.Stop();
        _sideObj = _tableAni.transform.FindChild("Dummy001/Bone009").gameObject;
        _curSideFlag = transform.FindChild("CurSide").gameObject;
        _curSideFlag.SetActive(false);

        // wall
        for (int i = 0; i < 4; i++)
        {
            SidePai pai = UIManager.AddChild<SidePai>(_tableRoot);
            pai.UpdatePai(i);
            _sidePaiWallList.Add(pai);
        }

        // players info
        _playerRoot = transform.FindChild("RootPlayer").gameObject;

        // pai
        _self2DCardItemRoot = transform.FindChild("Root_pai/Side0").gameObject;
        _self3DCardItemRoot = _tableRoot.transform.FindChild("Side0").gameObject;
        for (int i = 1; i < 4; i++)
        {
            GameObject root = _tableRoot.transform.FindChild("Side" + i.ToString()).gameObject;
            _otherCardObjRoot.Add(root);
        }

        // slelect exchange card
        _exchangeCardContainer = transform.FindChild("ExchangeContainer").gameObject;
        _exchangeCardContainer.SetActive(false);

        _exchangingContainer = _exchangeCardContainer.transform.FindChild("ExchangingContainer").gameObject;
        _btnEnsureSelectExchange = _exchangingContainer.transform.FindChild("btnEnsure").GetComponent<UIButton>();
        _btnEnsureSelectExchange.isEnabled = false;
        for (int i = 0; i < 3; i++)
        {
            UILabel label = transform.FindChild("TipsContainer/Tips" + (i + 1).ToString()).GetComponent<UILabel>();
            label.text = "";
            _exchangeTips.Add(label);
        }
        UIEventListener.Get(_btnEnsureSelectExchange.gameObject).onClick = OnClickEnsureExchange;

        // after exchange
        _afterExchangeContainer = _exchangeCardContainer.transform.FindChild("AfterExchangeContainer").gameObject;

        //lack
        _selectLackContainer = transform.FindChild("ItemLackContainer").gameObject;
        _selectLackContainer.SetActive(false);
    }

    public override void OnStart()
    {
        base.OnStart();
        _roomId.text = "房间号：" + BattleManager.Instance.RoomID;
        UpdateRoleInRoom();
        if (!_hasPlayStartAni && BattleManager.Instance.DealerID != 0)
        {
            _battleProcess = BattleProcess.PlayTableAniStart;
            _hasPlayStartAni = true;
        }
    }

    public override void OnRegisterEvent()
    {
        base.OnRegisterEvent();
        EventDispatcher.AddEventListener(EventDefine.UpdateRoleInRoom, UpdateRoleInRoom);
        EventDispatcher.AddEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);
        EventDispatcher.AddEventListener<bool>(EventDefine.UpdateBtnExchangeCard, UpdateBtnExchangeCard);
        EventDispatcher.AddEventListener(EventDefine.ReExchangeCard, SelectExchangeCard);
        EventDispatcher.AddEventListener<pb.ExchangeType>(EventDefine.UpdateCardInfoAfterExchange, UpdateCardInfoAfterExchange);
        EventDispatcher.AddEventListener<pb.CardType>(EventDefine.SelectLack, SelectLack);
        EventDispatcher.AddEventListener<pb.CardType>(EventDefine.EnsureLack, EnsureLack);
        EventDispatcher.AddEventListener(EventDefine.ShowLackCard, ShowLackCard);
        EventDispatcher.AddEventListener<pb.BattleSide>(EventDefine.TurnToPlayer, TurnToPlayer);
        EventDispatcher.AddEventListener<Pai>(EventDefine.EnsureDiscard, EnsureDiscard);
        EventDispatcher.AddEventListener<Pai>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoleInRoom, UpdateRoleInRoom);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);
        EventDispatcher.RemoveEventListener<bool>(EventDefine.UpdateBtnExchangeCard, UpdateBtnExchangeCard);
        EventDispatcher.RemoveEventListener(EventDefine.ReExchangeCard, SelectExchangeCard);
        EventDispatcher.RemoveEventListener<pb.ExchangeType>(EventDefine.UpdateCardInfoAfterExchange, UpdateCardInfoAfterExchange);
        EventDispatcher.RemoveEventListener<pb.CardType>(EventDefine.SelectLack, SelectLack);
        EventDispatcher.RemoveEventListener<pb.CardType>(EventDefine.EnsureLack, EnsureLack);
        EventDispatcher.RemoveEventListener(EventDefine.ShowLackCard, ShowLackCard);
        EventDispatcher.RemoveEventListener<pb.BattleSide>(EventDefine.TurnToPlayer, TurnToPlayer);
        EventDispatcher.RemoveEventListener<Pai>(EventDefine.EnsureDiscard, EnsureDiscard);
        EventDispatcher.RemoveEventListener<Pai>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
    }

    private void hideAllRoleItem()
    {
        for (int i = 0; i < _roleItemList.Count; i++)
        {
            _roleItemList[i].gameObject.SetActive(false);
        }
    }

    private Item_role getRoleItem(int index)
    {
        if (index < _roleItemList.Count)
        {
            return _roleItemList[index];
        }
        else
        {
            Item_role script = UIManager.AddChild<Item_role>(_playerRoot);
            _roleItemList.Add(script);
            return script;
        }
    }

    private void rotateTable()
    {
        pb.BattleSide selfSide = _sortedSideListFromSelf[0];
        Vector3 origAngle = _sideObj.transform.localEulerAngles;
        _sideObj.transform.localEulerAngles = new Vector3(origAngle.x, (selfSide - pb.BattleSide.west) * 90, origAngle.z);
        if (selfSide == pb.BattleSide.east)
        {
            _sideObj.transform.localPosition = new Vector3(0.003f, 0.125f, 0.007f);
        }
        else if (selfSide == pb.BattleSide.south)
        {
            _sideObj.transform.localPosition = new Vector3(0.005f, 0.125f, 0.001f);
        }
        else if (selfSide == pb.BattleSide.west)
        {
            _sideObj.transform.localPosition = new Vector3(0f, 0.125f, 0f);
        }
        else if (selfSide == pb.BattleSide.north)
        {
            _sideObj.transform.localPosition = new Vector3(-0.002f, 0.125f, 0.004f);
        }
    }

    private void UpdateRoleInRoom()
    {
        _sortedSideListFromSelf = BattleManager.Instance.GetSortSideListFromSelf(); //东南西北排序
        rotateTable();
        List<PlayerInfo> playerList = new List<PlayerInfo>();
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            PlayerInfo player = BattleManager.Instance.GetPlayerInfoBySide(_sortedSideListFromSelf[i]);
            if (player != null)
            {
                playerList.Add(player);
            }
        }
        Debug.Log("UpdateRoleInRoom=> current player count:" + playerList.Count);
        hideAllRoleItem();
        Vector3[] _roleItemPos = { new Vector3(-555, -155, 0), new Vector3(495, 95, 0), new Vector3(285, 270, 0), new Vector3(-555, 95, 0) };
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] == null)
            {
                Debug.LogError("UpdateRoleInRoom=> player" + i + " info is null.");
                continue;
            }
            Debug.Log("UpdateRoleInRoom=> i=" + i + ", playerId:" + playerList[i].NickName);
            Item_role itemScript = getRoleItem(i);
            if (itemScript != null)
            {
                itemScript.gameObject.SetActive(true);
                itemScript.UpdateUI(playerList[i]);
                itemScript.gameObject.transform.localPosition = _roleItemPos[i];
            }
            else
            {
                Debug.LogError("UpdateRoleInRoom=> get roleItem failed.");
            }
        }
    }

    #region game start animation
    private void PlayGamePrepareAni()
    {
        _battleProcess = BattleProcess.PlayTableAniStart;
        _hasPlayStartAni = true;
        ShowDealer();
    }

    private void ShowDealer()
    {
        for (int i = 0; i < _roleItemList.Count; i++)
        {
            if (BattleManager.Instance.DealerID == _roleItemList[i].PlayerInfo.OID)
            {
                _roleItemList[i].ShowDealer();
                break;
            }
        }
    }

    private void PlayTableAni()
    {
        Debug.Log("play table ani start...");
        _tableAni.Play();
        Invoke("PaiShownAni", 0.5f);
    }

    private void PaiShownAni()
    {
        for (int i = 0; i < _sidePaiWallList.Count; i++)
        {
            _sidePaiWallList[i].gameObject.SetActive(true);
            iTween.MoveTo(_sidePaiWallList[i].gameObject, iTween.Hash("y", -0.32f, "islocal", true, "easytype", iTween.EaseType.easeOutQuad,
                "time", 2.5f));
        }
        Invoke("PlayTableAniOver", 2.5f);
    }

    private void PlayTableAniOver()
    {
        _battleProcess = BattleProcess.PlayingTableAniOver;
    }

    private void PlayShaiZiAni()
    {
        Debug.Log("play shaizi ani start...");
        _shaiziValue[0] = Random.Range(1, 6);
        _shaiziValue[1] = Random.Range(1, 6);
        Debug.Log("shaizi value=" + _shaiziValue[0] + ", " + _shaiziValue[1]);
        _battleProcess = BattleProcess.PlayShaiZiAniOver;
    }

    private void PlayStartDrawPaiAni()
    {
        Debug.Log("play draw pai ani start...");
        PlayDrawPaiAni();
    }

    private int getSideIndexFromSelf(pb.BattleSide side)
    {
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            if (side == _sortedSideListFromSelf[i])
            {
                return i;
            }
        }
        return -1;
    }

    private void PlayDrawPaiAni()
    {
        Debug.Log("PlayDrawPaiAni");
        //初始化抓牌人方位
        _drawerSide_prepareAni = BattleManager.Instance.GetDealerSide();
        Debug.Log("庄家方位：" + _drawerSide_prepareAni.ToString());
        //初始化从哪个方位抓牌
        int maxShaiZi = Mathf.Max(_shaiziValue[0], _shaiziValue[1]);
        pb.BattleSide curPaiDrawnSide = BattleManager.Instance.GetPaiDrawnSideByShaiZi(_drawerSide_prepareAni, maxShaiZi); //从该方位抓牌
        Debug.Log("从" + curPaiDrawnSide.ToString() + "开始抓牌");
        _drawnSideIndex_prepareAni = getSideIndexFromSelf(curPaiDrawnSide);
        _drawnCardOffsetIndex_prepareAni = Mathf.Min(_shaiziValue[0], _shaiziValue[1]) * 2; //从0开始
        _drawCardSumCount_prepareAni = 0;
        _drawRound_prepareAni = 0;
        //开始抓牌
        hideAllSelf2DCardItem();
        hideAllOther3DCardObj();
        OnceDrawPaiAni();
    }

    private void hideAllOther3DCardObj()
    {
        foreach (List<Item_pai_3d> objList in _sideCardObjDict.Values)
        {
            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].gameObject.SetActive(false);
            }
        }
    }

    private Item_pai_3d getOtherCardObjBySide(int sideIndex, int itemIndex)
    {
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        if (!_sideCardObjDict.ContainsKey(side))
        {
            _sideCardObjDict.Add(side, new List<Item_pai_3d>());
        }
        List<Item_pai_3d> itemList = _sideCardObjDict[side];
        if (itemIndex < itemList.Count)
        {
            return itemList[itemIndex];
        }
        GameObject root = _otherCardObjRoot[sideIndex - 1];
        Item_pai_3d script = UIManager.AddChild<Item_pai_3d>(root);
        itemList.Add(script);
        return script;
    }

    private void hideAllSelf2DCardItem()
    {
        for (int i = 0; i < _self2DCardItemList.Count; i++)
        {
            _self2DCardItemList[i].gameObject.SetActive(false);
        }
    }

    private Item_pai getSelf2DCardItem(int index)
    {
        if (index < _self2DCardItemList.Count)
        {
            return _self2DCardItemList[index];
        }
        Item_pai script = UIManager.AddChild<Item_pai>(_self2DCardItemRoot);
        _self2DCardItemList.Add(script);
        return script;
    }

    private int drawOnePai(int wallIndex, int paiIndex)
    {
        int turnRound = 0;
        bool drawSuc = _sidePaiWallList[wallIndex].HidePaiInWallByIndex(paiIndex);
        while (!drawSuc)
        {
            //Debug.LogError("换抓牌方位");
            wallIndex++;
            if (wallIndex >= 4)
            {
                wallIndex = 0;
            }
            paiIndex = 0;
            turnRound++;
            drawSuc = _sidePaiWallList[wallIndex].HidePaiInWallByIndex(paiIndex);
        }
        return turnRound;
    }

    private void OnceDrawPaiAni()
    {
        Debug.Log("当前抓牌人方位：" + _drawerSide_prepareAni.ToString() + ", 已经抓的牌数：" + _drawCardSumCount_prepareAni);
        if (_drawCardSumCount_prepareAni >= (13 * 4 + 1))
        {
            _battleProcess = BattleProcess.PlayStartDrawAniOver;
            return;
        }
        int drawCountCurRound = 4; //当前需要抓牌的张数
        if (_drawRound_prepareAni >= 12)
        {
            drawCountCurRound = 1;
        }
        //摸牌
        for (int i = 0; i < drawCountCurRound; i++)
        {
            //Debug.LogError("draw pai=> 从" + _sortedSideListFromSelf[_curPaiDrawnSideIndex_prepareAni].ToString()
            //    + "方位抓牌, drawOffsetIndex=" + _drawOffsetIndex_prepareAni);
            int turnRound = drawOnePai(_drawnSideIndex_prepareAni, _drawnCardOffsetIndex_prepareAni);
            _drawCardSumCount_prepareAni++;
            _drawnCardOffsetIndex_prepareAni++;
            if (turnRound > 0)
            {
                //抓牌方位有变化
                _drawnSideIndex_prepareAni += turnRound;
                if (_drawnSideIndex_prepareAni >= 4)
                {
                    _drawnSideIndex_prepareAni = _drawnSideIndex_prepareAni % 4;
                }
                _drawnCardOffsetIndex_prepareAni = turnRound;
            }
        }
        //摆牌
        Debug.Log("place pai to side[" + _drawerSide_prepareAni.ToString() + "] when game start");
        int drawSideIndex = getSideIndexFromSelf(_drawerSide_prepareAni);
        for (int index_pai = 0; index_pai < drawCountCurRound; index_pai++)
        {
            int itemIndex = _placedPaiCount_prepareAni[drawSideIndex];
            _placedPaiCount_prepareAni[drawSideIndex]++;
            if (drawSideIndex == 0)
            {
                //自己    
                Item_pai script = getSelf2DCardItem(itemIndex);
                if (script != null)
                {
                    script.gameObject.SetActive(true);
                    Pai pai = BattleManager.Instance.GetPaiInfoByIndexAndSide(_drawerSide_prepareAni, itemIndex);
                    script.UpdateUI(pai, _drawerSide_prepareAni);
                    script.transform.localScale = Vector3.one * 0.88f;
                    script.transform.localPosition = new Vector3(-440 + itemIndex * 64, -250, 0);
                }
                else
                {
                    Debug.LogError("create self item_pai obj fail.");
                }
            }
            else
            {
                //其他人
                Item_pai_3d script = getOtherCardObjBySide(drawSideIndex, itemIndex);
                script.gameObject.SetActive(true);
                if (script != null)
                {
                    script.UpdatePaiMian();
                    script.SetSide(_drawerSide_prepareAni);
                    if (drawSideIndex == 1)
                    {
                        script.transform.localScale = Vector3.one;
                        script.transform.localEulerAngles = new Vector3(-90, -90, 0);
                        script.transform.localPosition = new Vector3(0.45f, 0.05f, -0.235f + 0.035f * itemIndex);
                    }
                    else if (drawSideIndex == 2)
                    {
                        script.transform.localScale = new Vector3(1.4f, 1, 1);
                        script.transform.localEulerAngles = new Vector3(-90, 180, 0);
                        script.transform.localPosition = new Vector3(0.32f - 0.034f * itemIndex * 1.4f, 0.05f, 0.33f);
                    }
                    else if (drawSideIndex == 3)
                    {
                        script.transform.localScale = Vector3.one;
                        script.transform.localEulerAngles = new Vector3(-90, 90, 0);
                        script.transform.localPosition = new Vector3(-0.45f, 0.05f, 0.235f - 0.035f * itemIndex);
                    }
                }
                else
                {
                    Debug.LogError("create 3d_pai obj fail.");
                }
            }
        }

        //本轮抓牌完毕，换抓牌人
        _drawRound_prepareAni++;
        _drawerSide_prepareAni++;
        if (_drawerSide_prepareAni > pb.BattleSide.north)
        {
            _drawerSide_prepareAni = pb.BattleSide.east;
        }

        Invoke("OnceDrawPaiAni", 0.3f);
    }

    private void SortSelfPaiPrepareAni()
    {
        List<Pai> selfList = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.InHand);
        selfList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        hideAllSelf2DCardItem();
        for (int i = 0; i < selfList.Count; i++)
        {
            Item_pai script = getSelf2DCardItem(i);
            if (script != null)
            {
                script.gameObject.SetActive(true);
                script.transform.localScale = Vector3.one * 0.88f;
                script.transform.localPosition = new Vector3(-440 + i * 64, -250, 0);
                script.UpdateUI(selfList[i], _sortedSideListFromSelf[0]);
            }
            else
            {
                Debug.LogError("self pai" + i + " item is null.");
            }
        }
        _battleProcess = BattleProcess.SortPaiOver;
    }
    #endregion

    #region select exchange three

    private void SelectExchangeCard()
    {
        BattleManager.Instance.CurProcess = BattleProcess.SelectingExchangeCard;

        _exchangeCardContainer.SetActive(true);
        _exchangingContainer.SetActive(true);
        _afterExchangeContainer.SetActive(false);
        _exchangeTipsAniTime.Clear();
        for (int i = 0; i < _exchangeTips.Count; i++)
        {
            _exchangeTips[i].text = "选择中...";
            _exchangeTipsAniTime.Add(System.DateTime.Now);
        }
    }

    private void UpdateBtnExchangeCard(bool enable)
    {
        //Debug.Log("btn exchange enable=" + enable.ToString());
        _btnEnsureSelectExchange.isEnabled = enable;
    }

    private void OnClickEnsureExchange(GameObject go)
    {
        Debug.Log("OnClickEnsureExchange");
        _battleProcess = BattleProcess.WaitingExchangeCardOver;
        BattleManager.Instance.CurProcess = BattleProcess.WaitingExchangeCardOver;
        List<Pai> list = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.Exchange);
        if (list != null)
        {
            GameMsgHandler.Instance.SendMsgC2GSExchangeCard(list);
            _exchangingContainer.SetActive(false);
            PlayExchangePlaceCardAni(list);
        }
        else
        {
            Debug.LogError("exchange card list is null.");
        }
    }

    private void PlayExchangePlaceCardAni(List<Pai> list)
    {
        Debug.Log("PlayExchangePlaceCardAni");
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = 0; j < _self2DCardItemList.Count; j++)
            {
                if (_self2DCardItemList[j].Info.OID == list[i].OID)
                {
                    _self2DCardItemList[j].gameObject.SetActive(false);
                    break;
                }
            }
        }
        PlaceSelfExchange3DCards();
        SortCardAfterExchange();
    }

    private Item_pai_3d getSelf3DCard(int index)
    {
        if (index < _self3DCardItemList.Count)
        {
            return _self3DCardItemList[index];
        }
        GameObject pai = UIManager.AddGameObject("3d/model/pai", _self3DCardItemRoot);
        Item_pai_3d script = pai.AddComponent<Item_pai_3d>();
        _self3DCardItemList.Add(script);
        return script;
    }

    private void hideAllSelf3DCard()
    {
        for (int i = 0; i < _self3DCardItemList.Count; i++)
        {
            _self3DCardItemList[i].gameObject.SetActive(false);
        }
    }

    private void PlaceSelfExchange3DCards()
    {
        hideAllSelf3DCard();
        List<Item_pai_3d> exchangeObjList = new List<Item_pai_3d>();
        Vector3 startPos = new Vector3(-0.047f, 0.04f, -0.17f);
        for (int i = 0; i < 3; i++)
        {
            Item_pai_3d item = getSelf3DCard(i);
            item.UpdatePaiMian();
            item.transform.localScale = new Vector3(1.4f, 1, 1);
            item.transform.localEulerAngles = new Vector3(180, 0, 0);
            item.transform.localPosition = startPos + new Vector3(i * 0.047f, 0.21f, -0.085f);
            Vector3 toPos = startPos + i * new Vector3(0.047f, 0, 0);
            item.gameObject.SetActive(true);
            iTween.MoveTo(item.gameObject, iTween.Hash("position", toPos, "islocal", true, "time", 0.5f));
        }
    }

    private void SortCardAfterExchange()
    {
        List<Pai> selfList = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.InHand);
        selfList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        hideAllSelf2DCardItem();
        for (int i = 0; i < selfList.Count; i++)
        {
            Item_pai script = getSelf2DCardItem(i);
            if (script != null)
            {
                script.gameObject.SetActive(true);
                script.transform.localScale = Vector3.one * 0.88f;
                script.transform.localPosition = new Vector3(-440 + i * 64, -250, 0);
                script.UpdateUI(selfList[i], _sortedSideListFromSelf[0]);
            }
            else
            {
                Debug.LogError("self pai" + i + " item is null.");
            }
        }
    }

    private void UpdateCardInfoAfterExchange(pb.ExchangeType type)
    {
        _battleProcess = BattleProcess.PlayingExchangeAni;
        for (int i = 0; i < _exchangeTips.Count; i++)
        {
            _exchangeTips[i].text = "";
        }
        OthersPlaceExchangeCard();
        _exchangeType = type;
        Invoke("PlayExchangeAni", 0.8f);
    }

    private void OthersPlaceExchangeCard()
    {
        Debug.Log("OthersPlaceExchangeCard");
        Vector3[] fromPos = { new Vector3(0.3f, 0.21f, -0.043f), new Vector3(-0.047f, 0.21f, 0.2f), new Vector3(-0.3f, 0.21f, -0.043f) };
        Vector3[] toPos = { new Vector3(0.22f, 0.04f, -0.043f), new Vector3(-0.047f, 0.04f, 0.14f), new Vector3(-0.22f, 0.04f, -0.043f) };
        Vector3[] spacePos = { new Vector3(0, 0, 0.033f), new Vector3(0.047f, 0, 0), new Vector3(0, 0, 0.033f) };
        Vector3[] rotate = { new Vector3(0, 90, 180), new Vector3(180, 0, 0), new Vector3(180, 90, 0) };
        for (int i = 1; i < _sortedSideListFromSelf.Count; i++)
        {
            List<Item_pai_3d> objList = _sideCardObjDict[_sortedSideListFromSelf[i]];
            for (int j = 1; j < 4; j++)
            {
                Item_pai_3d obj = objList[objList.Count - j];
                obj.transform.localEulerAngles = rotate[i - 1];
                obj.transform.localPosition = fromPos[i - 1] + spacePos[i - 1] * (j - 1);
                iTween.MoveTo(obj.gameObject, iTween.Hash("position", toPos[i - 1] + spacePos[i - 1] * (j - 1), "islocal", true, "time", 0.5f));
            }
        }
    }

    private void hideAllExchangeArrow()
    {
        for (int i = 0; i < _exchangeArrows.Count; i++)
        {
            _exchangeArrows[i].gameObject.SetActive(false);
        }
    }

    private Item_exchangeArrow getItemExchangeArrow(int index)
    {
        if (index < _exchangeArrows.Count)
        {
            return _exchangeArrows[index];
        }
        Item_exchangeArrow item = UIManager.AddChild<Item_exchangeArrow>(_afterExchangeContainer);
        return item;
    }

    private void PlayExchangeAni()
    {
        Debug.Log("PlayExchangeAni, exchangeType:" + _exchangeType.ToString());
        _afterExchangeContainer.SetActive(true);
        int count = _exchangeType == pb.ExchangeType.Opposite ? 2 : 4;
        hideAllExchangeArrow();
        for (int i = 0; i < count; i++)
        {
            Item_exchangeArrow script = getItemExchangeArrow(i);
            script.gameObject.SetActive(true);
            script.UpdateUI(_exchangeType, i);
        }
        Invoke("ArrowAniOver", 2f);
    }

    private void ArrowAniOver()
    {
        Debug.Log("ArrowAniOver");
        _exchangeCardContainer.SetActive(false);
        // self
        hideAllSelf3DCard();
        List<Pai> handCards = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.InHand);
        handCards.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        List<Pai> exchangeCards = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.Exchange);
        exchangeCards.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        Debug.Log("self hand card count=" + handCards.Count + ", exchange card count=" + exchangeCards.Count);
        int index = 0;
        hideAllSelf2DCardItem();
        for (; index < handCards.Count; index++)
        {
            Item_pai script = getSelf2DCardItem(index);
            script.gameObject.SetActive(true);
            script.UpdateUI(handCards[index], _sortedSideListFromSelf[0]);
            script.transform.localPosition = new Vector3(-440 + 64 * index, -250, 0);
        }
        for (int i = 0; i < exchangeCards.Count; i++, index++)
        {
            Item_pai script = getSelf2DCardItem(index);
            script.gameObject.SetActive(true);
            script.UpdateUI(exchangeCards[i], _sortedSideListFromSelf[0]);
            script.transform.localPosition = new Vector3(-440 + 64 * index, -230, 0);
            iTween.MoveTo(script.gameObject, iTween.Hash("y", -250, "islocal", true, "time", 0.5f, "delay", 0.5f));
            exchangeCards[i].Status = PaiStatus.InHand;
        }
        // others
        hideAllOther3DCardObj();
        for (int i = 1; i < 4; i++)
        {
            List<Pai> otherHandCards = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[i], PaiStatus.InHand);
            Debug.Log("i:" + i + ", hand card count=" + otherHandCards.Count);
            for (int j = 0; j < otherHandCards.Count; j++)
            {
                Item_pai_3d script = getOtherCardObjBySide(i, j);
                script.gameObject.SetActive(true);
                script.UpdatePaiMian();
                script.SetSide(_sortedSideListFromSelf[i]);
                float upOffset = 0;
                if (j >= (otherHandCards.Count - 3))
                {
                    upOffset = 0.01f;   //最后三张为交换牌
                    iTween.MoveTo(script.gameObject, iTween.Hash("y", 0.05f, "islocal", true, "time", 0.5f, "delay", 0.5f));
                }
                if (i == 1)
                {
                    script.transform.localScale = Vector3.one;
                    script.transform.localEulerAngles = new Vector3(-90, -90, 0);
                    script.transform.localPosition = new Vector3(0.45f, 0.05f + upOffset, -0.235f + 0.035f * j);
                }
                else if (i == 2)
                {
                    script.transform.localScale = new Vector3(1.4f, 1, 1);
                    script.transform.localEulerAngles = new Vector3(-90, 180, 0);
                    script.transform.localPosition = new Vector3(0.32f - 0.034f * j * 1.4f, 0.05f + upOffset, 0.33f);
                }
                else if (i == 3)
                {
                    script.transform.localScale = Vector3.one;
                    script.transform.localEulerAngles = new Vector3(-90, 90, 0);
                    script.transform.localPosition = new Vector3(-0.45f, 0.05f + upOffset, 0.235f - 0.035f * j);
                }
            }
        }
        Invoke("SortCardAfterExchangeSuccess", 1f);
    }

    private void SortCardAfterExchangeSuccess()
    {
        Debug.Log("SortCardAfterExchangeSuccess");
        List<Pai> list = BattleManager.Instance.GetCardListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.InHand);
        list.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        Debug.Log("inhand card count=" + list.Count);
        hideAllSelf2DCardItem();
        for (int i = 0; i < list.Count; i++)
        {
            Item_pai script = getSelf2DCardItem(i);
            script.gameObject.SetActive(true);
            script.UpdateUI(list[i], _sortedSideListFromSelf[0]);
            script.transform.localPosition = new Vector3(-440 + 64 * i, -250, 0);
        }
        _battleProcess = BattleProcess.PlayExchangeAniOver;
    }
    #endregion

    #region select lack
    private void hideAllItemLack()
    {
        for (int i = 0; i < _itemLackList.Count; i++)
        {
            _itemLackList[i].gameObject.SetActive(false);
        }
    }

    private Item_lack getItemLack(int index)
    {
        if (index < _itemLackList.Count)
        {
            return _itemLackList[index];
        }
        Item_lack script = UIManager.AddChild<Item_lack>(_selectLackContainer);
        _itemLackList.Add(script);
        return script;
    }

    private void StartSelectLackPai()
    {
        Debug.Log("select lack card start...");
        _selectLackContainer.SetActive(true);
        _exchangeTipsAniTime.Clear();
        for (int i = 0; i < _exchangeTips.Count; i++)
        {
            _exchangeTips[i].text = "定缺中...";
            _exchangeTipsAniTime.Add(System.DateTime.Now);
        }
        hideAllItemLack();
        for (int i = 0; i < 3; i++)
        {
            Item_lack script = getItemLack(i);
            script.gameObject.SetActive(true);
            script.UpdateUI(i);
        }
    }

    private void SelectLack(pb.CardType type)
    {
        Debug.Log("select lack=" + type.ToString());
        for (int i = 0; i < _itemLackList.Count; i++)
        {
            _itemLackList[i].UpdateWord(type == _itemLackList[i].Type);
        }
    }

    private void EnsureLack(pb.CardType type)
    {
        Debug.Log("ensure lack=" + type.ToString());
        GameMsgHandler.Instance.SendMsgC2GSSelectLack(type);
        _selectLackContainer.SetActive(false);
        _battleProcess = BattleProcess.WaitingLackCardInfo;
    }

    private void ShowLackCard()
    {
        Debug.Log("ShowLackCard");
        _battleProcess = BattleProcess.PlayingLackAni;
        for (int i = 0; i < _exchangeTips.Count; i++)
        {
            _exchangeTips[i].text = "";
        }
        for (int i = 0; i < _roleItemList.Count; i++)
        {
            _roleItemList[i].ShowLackIcon();
        }
        Invoke("SelectLackOver", 0.5f);
    }

    private void SelectLackOver()
    {
        BattleManager.Instance.TurnToNextPlayer(BattleManager.Instance.DealerID);
        sortAndPlaceSelfInHandCard(true);
        _discardItemIndex = 0;
        _battleProcess = BattleProcess.SortCardOver;
    }
    #endregion


    #region playing
    private void TurnToPlayer(pb.BattleSide side)
    {
        Debug.Log("current play side=" + side.ToString());
        int sideIndex = getSideIndexFromSelf(side);
        Vector3[] pos = { new Vector3(0, 35, 0), new Vector3(68, 82, 0), new Vector3(0, 118, 0), new Vector3(-68, 82, 0) };
        _curSideFlag.SetActive(true);
        _curSideFlag.transform.localPosition = pos[sideIndex];
        iTween.MoveTo(_curSideFlag, iTween.Hash("y", pos[sideIndex].y - 10, "islocal", true, "time", 1f, 
            "looptype", iTween.LoopType.loop));        
    }

    #region sort self card(inhand、peng、gang、discard)    
    //showDiscard: 最右侧的牌是否显示为提示出牌
    private float sortAndPlaceSelfInHandCard(bool showDiscard)
    {
        pb.BattleSide side = _sortedSideListFromSelf[0];
        Debug.Log("sortAndPlaceInHandCard=> side:" + side.ToString());
        List<Pai> inhandList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.InHand);
        int lackType = (int)BattleManager.Instance.GetLackCardTypeBySide(side);
        inhandList.Sort((x, y) =>
        {
            int result = 0;
            int type1 = Mathf.FloorToInt(x.Id / 10) + 1;
            int type2 = Mathf.FloorToInt(y.Id / 10) + 1;
            if (type1 != lackType && type2 == lackType)
            {
                result = -1;
            }
            else if (type1 == lackType && type2 != lackType)
            {
                return 1;
            }
            else
            {
                result = x.Id.CompareTo(y.Id);
            }
            return result;
        });

        string str = "side[" + side.ToString() + "]=> inhand: ";
        for (int n = 0; n < inhandList.Count; n++)
        {
            str += inhandList[n].Id + ", ";
        }
        Debug.Log(str);

        Vector3 offset = new Vector3(64, 0, 0);
        Vector3 curPos = new Vector3(-440, -250, 0) - offset;
        hideAllSelf2DCardItem();
        _curSelf2DItemIndex_playing = 0;
        for (int i = 0; i < inhandList.Count; i++, _curSelf2DItemIndex_playing++)
        {
            Item_pai script = getSelf2DCardItem(_curSelf2DItemIndex_playing);
            script.gameObject.SetActive(true);
            script.UpdateUI(inhandList[i], side);
            script.transform.localScale = Vector3.one * 0.88f;
            if (showDiscard && i == inhandList.Count - 1)
            {
                curPos += (offset / 5);
            }
            curPos += offset;
            script.transform.localPosition = curPos;
        }
        return curPos.x;
    }

    private float sortAndPlaceSelfPengCard(float offsetX)
    {
        pb.BattleSide side = _sortedSideListFromSelf[0];
        Debug.Log("sortAndPlaceSelfPengCard=> side:" + side.ToString());
        List<Pai> pList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Peng);
        pList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });

        string str = "side[" + side.ToString() + "]=> peng: ";
        for (int n = 0; n < pList.Count; n++)
        {
            str += pList[n].Id + ", ";
        }
        Debug.Log(str);

        Vector3 offset = new Vector3(50, 0, 0);
        Vector3 curPos = new Vector3(offsetX, -250, 0) - offset;
        if (pList.Count > 0)
        {
            int curPaiId = pList[0].Id;
            for (int i = 0; i < pList.Count; i++, _curSelf2DItemIndex_playing++)
            {
                if (pList[i].Id != curPaiId)
                {
                    curPos += new Vector3(5, 0, 0);
                    curPaiId = pList[i].Id;
                }
                Item_pai script = getSelf2DCardItem(_curSelf2DItemIndex_playing);
                script.gameObject.SetActive(true);
                script.UpdateUI(pList[i], side);
                script.transform.localScale = Vector3.one * 0.88f;
                curPos += offset;
                script.transform.localPosition = curPos;
            }
        }
        return curPos.x;
    }

    private float sortAndPlaceSelfGangCard(float offsetX)
    {
        pb.BattleSide side = _sortedSideListFromSelf[0];
        Debug.Log("sortAndPlaceSelfGangCard=> side:" + side.ToString());
        List<Pai> gList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Gang);
        gList.Sort((x, y) => {
            if (x.IsFromOther && !y.IsFromOther)
            {
                return 1;
            }
            return x.Id.CompareTo(y.Id);
        });

        string str = "side[" + side.ToString() + "]=> gang: ";
        for (int n = 0; n < gList.Count; n++)
        {
            str += gList[n].Id + ", ";
        }
        Debug.Log(str);

        Dictionary<int, List<Pai>> gDict = new Dictionary<int, List<Pai>>();
        for (int i = 0; i < gList.Count; i++)
        {
            if (gDict.ContainsKey(gList[i].Id))
            {
                gDict[gList[i].Id].Add(gList[i]);
            }
            else
            {
                List<Pai> list = new List<Pai>();
                list.Add(gList[i]);
                gDict.Add(gList[i].Id, list);
            }
        }

        Vector3 offset = new Vector3(50, 0, 0);
        Vector3 curPos = new Vector3(offsetX, -250, 0) - offset;
        foreach (List<Pai> list in gDict.Values)
        {
            bool isSelfGang = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsFromOther)
                {
                    isSelfGang = false;
                    break;
                }
            }
            curPos += new Vector3(5, 0, 0);
            for (int i = 0; i < list.Count; i++, _curSelf2DItemIndex_playing++)
            {
                Item_pai script = getSelf2DCardItem(_curSelf2DItemIndex_playing);
                script.gameObject.SetActive(true);
                script.UpdateGangCard(gList[i], side, isSelfGang && i == 0);
                script.transform.localScale = Vector3.one * 0.88f;
                curPos += offset;
                script.transform.localPosition = curPos;
            }
        }
        return curPos.x;
    }

    private int _discardItemIndex;
    private void sortAndPlaceSelfDiscard()
    {
        pb.BattleSide side = _sortedSideListFromSelf[0];
        Debug.Log("sortAndPlaceSelfDiscard=> side:" + side.ToString());
        List<Pai> dList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Discard);

        string str = "side[" + side.ToString() + "]=> discard: ";
        for (int n = 0; n < dList.Count; n++)
        {
            str += dList[n].Id + ", ";
        }
        Debug.Log(str);

        hideAllSelf3DCard();
        Vector3[] vecs = getNonDiscardVecBySideIndex(0);
        for (int i = 0; i < dList.Count; i++)
        {
            Item_pai_3d script = getSelf3DCard(i);
            script.gameObject.SetActive(true);
            script.SetInfo(dList[i]);
            script.SetSide(side);
            script.UpdatePaiMian();
            script.transform.localEulerAngles = vecs[0];
            script.transform.localScale = new Vector3(1.4f, 1, 1);
            int index = i % 10;
            int line = i / 10;
            vecs[1] += line * vecs[3];
            script.transform.localPosition = vecs[1] + index * vecs[2];
        }
    }

    private void SortSelfCardAfterEnsureDiscard()
    {
        // inhand
        float offsetX = sortAndPlaceSelfInHandCard(false);
        // peng
        offsetX = sortAndPlaceSelfPengCard(offsetX + 10);
        // gang
        offsetX = sortAndPlaceSelfGangCard(offsetX + 5);
    }

    private void SortSelfCard(bool showDiscard = true)
    {
        // inhand
        float offsetX = sortAndPlaceSelfInHandCard(showDiscard);
        // peng
        offsetX = sortAndPlaceSelfPengCard(offsetX + 10);
        // gang
        offsetX = sortAndPlaceSelfGangCard(offsetX + 5);
        // discard
        sortAndPlaceSelfDiscard();
    }
    #endregion

    #region sort others card(inhand、peng、gang、discard)
    //startPos、rotate、offsetInLine
    private Vector3[] getNonDiscardVecBySideIndex(int sideIndex)
    {
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero };
        if (sideIndex == 1)
        {
            result[0] = new Vector3(0.45f, 0.05f, -0.235f);
            result[1] = new Vector3(-90, -90, -90);
            result[2] = new Vector3(0, 0, 0.035f);
        }
        else if (sideIndex == 2)
        {
            result[0] = new Vector3(0.32f, 0.05f, 0.33f);
            result[1] = new Vector3(-90, 180, -90);
            result[2] = new Vector3(-0.035f, 0, 0);
        }
        else if (sideIndex == 3)
        {
            result[0] = new Vector3(-0.45f, 0.05f, 0.235f);
            result[1] = new Vector3(-90, 90, -90);
            result[2] = new Vector3(0, 0, -0.035f);
        }
        return result;
    }

    private Vector3 sortAndPlaceOtherInHandCard(int sideIndex)
    {
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        Debug.Log("sortAndPlaceOtherInHandCard=> side:" + side.ToString());
        List<Pai> inhandList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.InHand);
        Debug.Log("hand card count=" + inhandList.Count);

        Vector3[] vecs = getNonDiscardVecBySideIndex(sideIndex);
        hideAllOther3DCardObj();
        _curOther3DItemIndex_playing = 0;
        for (int i = 0; i < inhandList.Count; i++, _curOther3DItemIndex_playing++)
        {
            Item_pai_3d script = getOtherCardObjBySide(sideIndex, _curOther3DItemIndex_playing);
            script.gameObject.SetActive(true);
            script.UpdatePaiMian();
            script.transform.localEulerAngles = vecs[1];
            script.transform.localScale = sideIndex % 2 == 0 ? new Vector3(1.4f, 1, 1) : Vector3.one;
            script.transform.localPosition = vecs[0] + i * vecs[2];
            if (i == inhandList.Count - 1)
            {
                script.transform.localPosition += (vecs[2] / 10);
            }
        }
        return vecs[0] + (inhandList.Count - 1) * vecs[2];
    }

    private Vector3 sortAndPlaceOtherPengCard(int sideIndex, Vector3 lastEndPos)
    {
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        Debug.Log("sortAndPlaceOtherPengCard=> side:" + side.ToString());
        List<Pai> pList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Peng);

        string str = "side[" + side.ToString() + "]=> peng: ";
        for (int n = 0; n < pList.Count; n++)
        {
            str += pList[n].Id + ", ";
        }
        Debug.Log(str);

        Vector3[] vecs = getNonDiscardVecBySideIndex(sideIndex);
        Vector3 startPos = lastEndPos + vecs[2] / 10;
        if (pList.Count > 0)
        {
            int curPaiId = pList[0].Id;
            for (int i = 0; i < pList.Count; i++, _curOther3DItemIndex_playing++)
            {
                if (pList[i].Id != curPaiId)
                {
                    startPos += (vecs[3] / 10);
                    curPaiId = pList[i].Id;
                }
                Item_pai_3d script = getOtherCardObjBySide(sideIndex, _curOther3DItemIndex_playing);
                script.gameObject.SetActive(true);
                script.SetInfo(pList[i]);
                script.SetSide(side);
                script.UpdatePaiMian();
                script.transform.localEulerAngles = vecs[1];
                script.transform.localScale = sideIndex % 2 == 0 ? new Vector3(1.4f, 1, 1) : Vector3.one;
                script.transform.localPosition = startPos + i * vecs[2];
            }
        }
        return startPos + (pList.Count - 1) * vecs[2];
    }

    private Vector3 sortAndPlaceOtherGangCard(int sideIndex, Vector3 lastEndPos)
    {
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        Debug.Log("sortAndPlaceOtherGangCard=> side:" + side.ToString());
        List<Pai> gList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Gang);
        gList.Sort((x, y) => {
            if (x.IsFromOther && !y.IsFromOther)
            {
                return 1;
            }
            return x.Id.CompareTo(y.Id);
        });

        string str = "side[" + side.ToString() + "]=> gang: ";
        for (int n = 0; n < gList.Count; n++)
        {
            str += gList[n].Id + ", ";
        }
        Debug.Log(str);

        Dictionary<int, List<Pai>> gDict = new Dictionary<int, List<Pai>>();
        for (int i = 0; i < gList.Count; i++)
        {
            if (gDict.ContainsKey(gList[i].Id))
            {
                gDict[gList[i].Id].Add(gList[i]);
            }
            else
            {
                List<Pai> list = new List<Pai>();
                list.Add(gList[i]);
                gDict.Add(gList[i].Id, list);
            }
        }

        Vector3[] vecs = getNonDiscardVecBySideIndex(sideIndex);
        Vector3 startPos = lastEndPos + vecs[0] / 10;
        int index = 0;
        foreach (List<Pai> list in gDict.Values)
        {
            bool isSelfGang = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsFromOther)
                {
                    isSelfGang = false;
                    break;
                }
            }
            startPos += (vecs[3] / 10);
            for (int i = 0; i < list.Count; i++, _curSelf2DItemIndex_playing++, index++)
            {
                Item_pai_3d script = getOtherCardObjBySide(sideIndex, _curSelf2DItemIndex_playing);
                script.gameObject.SetActive(true);
                script.SetInfo(list[i]);
                script.SetSide(side);
                script.UpdatePaiMian();
                if (isSelfGang && i != 0)
                {
                    script.ShownBack(sideIndex);
                }
                script.transform.localEulerAngles = vecs[1];
                script.transform.localScale = sideIndex % 2 == 0 ? new Vector3(1.4f, 1, 1) : Vector3.one;
                script.transform.localPosition = startPos + index * vecs[2];
            }
        }
        return startPos + (index - 1) * vecs[2];
    }

    private void hideOther3DDiscardObjBySide(pb.BattleSide side)
    {
        if (_other3DDiscard.ContainsKey(side))
        {
            for (int i = 0; i < _other3DDiscard[side].Count; i++)
            {
                _other3DDiscard[side][i].gameObject.SetActive(false);
            }
        }
    }

    private Item_pai_3d getOther3DDiscardObj(int sideIndex, int index)
    {
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        if (!_other3DDiscard.ContainsKey(side))
        {
            _other3DDiscard.Add(side, new List<Item_pai_3d>());
        }
        List<Item_pai_3d> objList = _other3DDiscard[side];
        if (index < objList.Count)
        {
            return objList[index];
        }
        else
        {
            Item_pai_3d script = UIManager.AddChild<Item_pai_3d>(_otherCardObjRoot[sideIndex - 1]);
            objList.Add(script);
            return script;
        }
    }

    //rotate、startPos、offsetInline、offsetBetweenLine
    private Vector3[] getDiscardVecsBySideIndex(int sideIndex)
    {
        Vector3[] vec = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        if (sideIndex == 0)
        {
            vec[0] = new Vector3(0, 0, 0);
            vec[1] = new Vector3(-0.1f, 0.04f, -0.1f);
            vec[2] = new Vector3(0.035f, 0, 0);
            vec[3] = new Vector3(0, 0, -0.035f);
        }
        else if (sideIndex == 1)
        {
            vec[0] = new Vector3(-90, -90, 0);
            vec[1] = new Vector3(0.1f, 0.04f, -0.1f);
            vec[2] = new Vector3(0, 0, 0.035f);
            vec[3] = new Vector3(0.035f, 0, 0);
        }
        else if (sideIndex == 2)
        {
            vec[0] = new Vector3(0, 180, -90);
            vec[1] = new Vector3(0.1f, 0.04f, 0.1f);
            vec[2] = new Vector3(-0.035f, 0, 0);
            vec[3] = new Vector3(0, 0, 0.035f);
        }
        else if (sideIndex == 3)
        {
            vec[0] = new Vector3(-90, 90, 0);
            vec[1] = new Vector3(-0.1f, 0.04f, 0.1f);
            vec[2] = new Vector3(0, 0, -0.035f);
            vec[3] = new Vector3(-0.035f, 0, -0.035f);
        }
        return vec;
    }

    private void sortAndPlaceOtherDiscard(int sideIndex)
    {
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        Debug.Log("sortAndPlaceOtherDiscard=> side:" + side.ToString());
        List<Pai> dList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Discard);

        string str = "side[" + side.ToString() + "]=> discard: ";
        for (int n = 0; n < dList.Count; n++)
        {
            str += dList[n].Id + ", ";
        }
        Debug.Log(str);

        hideOther3DDiscardObjBySide(side);
        Vector3[] vecs = getDiscardVecsBySideIndex(sideIndex);
        for (int i = 0; i < dList.Count; i++)
        {
            Item_pai_3d script = getOther3DDiscardObj(sideIndex, i);
            script.gameObject.SetActive(true);
            script.SetInfo(dList[i]);
            script.SetSide(side);
            script.UpdatePaiMian();
            script.transform.localEulerAngles = vecs[0];
            script.transform.localScale = sideIndex % 2 == 0 ? new Vector3(1.4f, 1, 1) : Vector3.one;
            int index = i % 10;
            int line = i / 10;
            vecs[1] += line * vecs[3];
            script.transform.localPosition = vecs[1] + vecs[2] * index;
        }
    }

    private void SortOtherCard(int sideIndex)
    {
        // inhand
        Vector3 offsetPos = sortAndPlaceOtherInHandCard(sideIndex);
        // peng
        offsetPos = sortAndPlaceOtherPengCard(sideIndex, offsetPos);
        // gang
        offsetPos = sortAndPlaceOtherGangCard(sideIndex, offsetPos);
        // discard
        sortAndPlaceOtherDiscard(sideIndex);
    }
    #endregion

    private void SortCard()
    {
        Debug.Log("sort card");
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            pb.BattleSide side = _sortedSideListFromSelf[i];
            if (i == 0)
            {
                SortSelfCard();
            }
            else
            {
                SortOtherCard(i);
            }
        }
        _battleProcess = BattleProcess.SortCardOver;
    }

    #region check card(hu、gang、peng)
    private void checkHu()
    {
        if (BattleManager.Instance.CanHu())
        {
            Debug.Log("胡牌！！！");
            _battleProcess = BattleProcess.EnsureHuStart;
        }
        else
        {
            _battleProcess = BattleProcess.CheckingGang;
            checkGang();
        }
    }

    private void checkGang()
    {
        if (BattleManager.Instance.CanGang())
        {
            Debug.Log("能杠，显示杠牌按钮");
            _battleProcess = BattleProcess.EnsureGangStart;
        }
        else
        {
            Debug.Log("自己手牌不能自杠，进入出牌阶段");
            _battleProcess = BattleProcess.SelectingDiscard;
            BattleManager.Instance.CurProcess = BattleProcess.SelectingDiscard;
        }
    }
    #endregion

    //startPos、rotate、offsetInline、offsetBetweenLine、offsetAni
    private Vector3[] getDiscardVecBySideIndex(int sideIndex)
    {
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        if (sideIndex == 0)
        {
            result[0] = new Vector3(-0.07f, 0.04f, -0.12f);
            result[1] = new Vector3(0, 0, 0);
            result[2] = new Vector3(0.033f, 0, 0);
            result[3] = new Vector3(0, 0, -0.035f);
            result[4] = new Vector3(0.09f, 0.16f, -0.13f);
        }
        else if (sideIndex == 1)
        {
            result[0] = new Vector3(0.45f, 0.05f, -0.235f);
            result[1] = new Vector3(-90, -90, -90);
            result[2] = new Vector3(0, 0, 0.035f);
            result[3] = new Vector3(0.035f, 0, 0);
        }
        else if (sideIndex == 2)
        {
            result[0] = new Vector3(0.32f, 0.05f, 0.33f);
            result[1] = new Vector3(-90, 180, -90);
            result[2] = new Vector3(-0.035f, 0, 0);
            result[3] = new Vector3(0, 0, 0.035f);
        }
        else if (sideIndex == 3)
        {
            result[0] = new Vector3(-0.45f, 0.05f, 0.235f);
            result[1] = new Vector3(-90, 90, -90);
            result[2] = new Vector3(0, 0, -0.035f);
            result[3] = new Vector3(0, 0, -0.035f);
        }
        return result;
    }

    private void UnSelectOtherDiscard(Pai cardInfo)
    {
        for (int i = 0; i < _self2DCardItemList.Count; i++)
        {
            if (cardInfo.OID != _self2DCardItemList[i].Info.OID)
            {
                _self2DCardItemList[i].UnSelect();
            }
        }
    }

    private void EnsureDiscard(Pai cardInfo)
    {
        //排序
        SortSelfCardAfterEnsureDiscard();

        //播放出牌动画
        Debug.Log("出牌：" + cardInfo.OID + "[" + cardInfo.Id + "], 播放出牌动画.");
        Item_pai_3d script = getSelf3DCard(_discardItemIndex);
        script.gameObject.SetActive(true);
        script.SetInfo(cardInfo);
        script.SetSide(_sortedSideListFromSelf[0]);
        script.UpdatePaiMian();
        script.transform.localScale = Vector3.one;
        Vector3[] vecs = getDiscardVecBySideIndex(0);
        script.transform.localEulerAngles = vecs[1];
        int index = _discardItemIndex % 10;
        int line = _discardItemIndex / 10;
        Vector3 targetPos = vecs[0] + vecs[2] * index + vecs[3] * line;
        script.transform.localPosition = targetPos + vecs[4];
        iTween.MoveTo(script.gameObject, iTween.Hash("position", targetPos, "islocal", true, "time", 0.2f));
        _discardItemIndex++;

        //发送出牌信息
        _battleProcess = BattleProcess.WaitingDiscardRet;
        BattleManager.Instance.CurProcess = BattleProcess.WaitingDiscardRet;
        GameMsgHandler.Instance.SendMsgC2GSDiscard(cardInfo.OID);
    }

    #endregion

    public override void OnUpdate()
    {
        base.OnUpdate();
        ProcessBattle();
        ExchangeCardWaitingAni();
    }

    private void ProcessBattle()
    {
        switch (_battleProcess)
        {
            case BattleProcess.PlayTableAniStart:
                _battleProcess = BattleProcess.PlayingTableAni;
                PlayTableAni();
                break;
            case BattleProcess.PlayingTableAniOver:
                _battleProcess = BattleProcess.PlayShaiZiAniStart;
                break;
            case BattleProcess.PlayShaiZiAniStart:
                _battleProcess = BattleProcess.PlayingShaiZiAni;
                PlayShaiZiAni();
                break;
            case BattleProcess.PlayShaiZiAniOver:
                _battleProcess = BattleProcess.PlayStartDrawAniStart;
                break;
            case BattleProcess.PlayStartDrawAniStart:
                _battleProcess = BattleProcess.PlayingStartDrawAni;
                PlayStartDrawPaiAni();
                break;
            case BattleProcess.PlayStartDrawAniOver:
                _battleProcess = BattleProcess.SortPai;
                SortSelfPaiPrepareAni();
                break;
            case BattleProcess.SortPaiOver:
                _battleProcess = BattleProcess.SelectingExchangeCard;
                SelectExchangeCard();
                break;
            case BattleProcess.PlayExchangeAniOver:
                _battleProcess = BattleProcess.SelectingLackCard;
                StartSelectLackPai();
                break;
            case BattleProcess.BattleReady:
                _battleProcess = BattleProcess.SortingCard;
                SortCard();
                break;
            case BattleProcess.SortCardOver:                
                if (BattleManager.Instance.DealerID == Player.Instance.PlayerInfo.OID)
                {
                    Debug.Log("dealer is self.");
                    _battleProcess = BattleProcess.CheckingHu;
                    checkHu();
                }
                break;
            default:
                break;
        }
    }

    private void ExchangeCardWaitingAni()
    {
        if (BattleProcess.SelectingExchangeCard == _battleProcess || BattleProcess.WaitingExchangeCardOver == _battleProcess
            || BattleProcess.SelectingLackCard == _battleProcess || BattleProcess.WaitingLackCardInfo == _battleProcess)
        {
            for (int i = 0; i < _exchangeTipsAniTime.Count; i++)
            {
                if (System.DateTime.Now.Subtract(_exchangeTipsAniTime[i]).TotalMilliseconds >= 1000 && _exchangeTips[i].text.Length >= 3)
                {
                    _exchangeTipsAniTime[i] = System.DateTime.Now;
                    if (_exchangeTips[i].text.Length >= 6)
                    {
                        _exchangeTips[i].text = _exchangeTips[i].text.Substring(0, 4);
                    }
                    else
                    {
                        _exchangeTips[i].text += ".";
                    }
                }
            }
        }
    }
}
