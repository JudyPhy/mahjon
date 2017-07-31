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
    private TweenPosition _curSideFlag;

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
    private int _wallIndex;  //牌墙方位
    private int _offsetIndexInWall;   //牌墙内抓到第几张牌

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
    private int[] _curOther3DItemIndex_playing; //其他人3d牌index
    private Dictionary<pb.BattleSide, List<Item_pai_3d>> _other3DDiscard = new Dictionary<pb.BattleSide, List<Item_pai_3d>>();
    private int[] _discardItemIndex;    //出牌堆item index

    // peng、gang proc container
    private UIGrid _procGrid;
    private List<Item_procBtn> _procBtns = new List<Item_procBtn>();

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
        _curSideFlag = transform.FindChild("CurSide").GetComponent<TweenPosition>();
        _curSideFlag.gameObject.SetActive(false);

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

        //proc container
        _procGrid = transform.FindChild("GridProc").GetComponent<UIGrid>();
        _procGrid.gameObject.SetActive(false);
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
        EventDispatcher.AddEventListener<pb.BattleSide, pb.CardInfo>(EventDefine.TurnToPlayer, TurnToPlayer);
        EventDispatcher.AddEventListener<Pai>(EventDefine.EnsureDiscard, EnsureDiscard);
        EventDispatcher.AddEventListener<Pai>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
        EventDispatcher.AddEventListener<int>(EventDefine.DiscardRet, PlayDiscardAni);
        EventDispatcher.AddEventListener<pb.BattleSide, pb.CardStatus>(EventDefine.SomePlayerPG, SomePlayerPG);
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
        EventDispatcher.RemoveEventListener<pb.BattleSide, pb.CardInfo>(EventDefine.TurnToPlayer, TurnToPlayer);
        EventDispatcher.RemoveEventListener<Pai>(EventDefine.EnsureDiscard, EnsureDiscard);
        EventDispatcher.RemoveEventListener<Pai>(EventDefine.UnSelectOtherDiscard, UnSelectOtherDiscard);
        EventDispatcher.RemoveEventListener<int>(EventDefine.DiscardRet, PlayDiscardAni);
        EventDispatcher.RemoveEventListener<pb.BattleSide, pb.CardStatus>(EventDefine.SomePlayerPG, SomePlayerPG);
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
        _wallIndex = getSideIndexFromSelf(curPaiDrawnSide);
        _offsetIndexInWall = Mathf.Min(_shaiziValue[0], _shaiziValue[1]) * 2; //从0开始
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
            int turnRound = drawOnePai(_wallIndex, _offsetIndexInWall);
            _drawCardSumCount_prepareAni++;
            _offsetIndexInWall++;
            if (turnRound > 0)
            {
                //抓牌方位有变化
                _wallIndex += turnRound;
                if (_wallIndex >= 4)
                {
                    _wallIndex = _wallIndex % 4;
                }
                _offsetIndexInWall = turnRound;
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

    //开始第一轮
    private void SelectLackOver()
    {
        _discardItemIndex = new int[4] { 0, 0, 0, 0 };
        _curOther3DItemIndex_playing = new int[4] { 0, 0, 0, 0 };
        BattleManager.Instance.TurnToNextPlayer(BattleManager.Instance.DealerID, null);
    }
    #endregion

    private void TurnToPlayer(pb.BattleSide side, pb.CardInfo newCard)
    {
        Debug.Log("current play side=" + side.ToString());

        //当前操作方标记动画
        int sideIndex = getSideIndexFromSelf(side);
        Vector3[] pos = { new Vector3(0, 35, 0), new Vector3(68, 82, 0), new Vector3(0, 118, 0), new Vector3(-68, 82, 0) };
        Debug.Log("sideIndex=" + sideIndex);
        _curSideFlag.gameObject.SetActive(true);
        _curSideFlag.transform.localPosition = pos[sideIndex];
        Debug.Log("pos[sideIndex]=" + pos[sideIndex]);
        _curSideFlag.from = pos[sideIndex];
        _curSideFlag.to = pos[sideIndex] + new Vector3(0, -10, 0);

        //牌墙中去除一张牌
        if (newCard != null)
        {
            Debug.LogError("draw pai=> 从" + _sortedSideListFromSelf[_wallIndex].ToString()
                + "方位抓牌, drawOffsetIndex=" + _offsetIndexInWall);
            if (_offsetIndexInWall >= 108)
            {
                Debug.LogError("牌墙为空");
                return;
            }
            hideCardInWall(1);
        }

        //摆牌
        if (BattleManager.Instance.CurPlaySide == _sortedSideListFromSelf[0])
        {
            sortAndPlaceSelfCard(newCard, true);
        }
        else
        {
            sortAndPlaceOtherCard(sideIndex, true);
        }

        //判断胡、碰、杠情况
        if (BattleManager.Instance.CurPlaySide == _sortedSideListFromSelf[0])
        {
            Debug.Log("当前我方操作");
            _battleProcess = BattleProcess.CheckingHu;

            List<int> inhandList = BattleManager.Instance.GetCardIdListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.InHand);
            List<int> pList = BattleManager.Instance.GetCardIdListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.Peng);
            List<int> gList = BattleManager.Instance.GetCardIdListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.Gang);
            if (BattleManager.Instance.CanHu(inhandList, pList, gList))
            {
                Debug.Log("胡牌！！！");
                _battleProcess = BattleProcess.EnsureHuStart;
            }
            else
            {
                _battleProcess = BattleProcess.CheckingGang;
                if (BattleManager.Instance.CanGang(inhandList))
                {
                    Debug.Log("能杠，显示杠牌按钮");
                    _battleProcess = BattleProcess.EnsureGangStart;
                }
                else
                {
                    Debug.Log("不能自杠，则进入出牌阶段");
                    _battleProcess = BattleProcess.SelectingDiscard;
                    BattleManager.Instance.CurProcess = BattleProcess.SelectingDiscard;
                }
            }
        }
        else
        {
            Debug.Log("当前为别人操作");
        }
    }

    private void hideCardInWall(int hideCount)
    {
        for (int i = 0; i < hideCount; i++)
        {
            int turnRound = drawOnePai(_wallIndex, _offsetIndexInWall);
            _drawCardSumCount_prepareAni++;
            _offsetIndexInWall++;
            if (turnRound > 0)
            {
                //抓牌方位有变化
                _wallIndex += turnRound;
                if (_wallIndex >= 4)
                {
                    _wallIndex = _wallIndex % 4;
                }
                _offsetIndexInWall = turnRound;
            }
        }
    }

    #region sort self card(inhand、peng、gang)    
    //showDiscard: 最右侧的牌是否显示为提示出牌
    private void sortAndPlaceSelfCard(pb.CardInfo rightCard, bool showDiscard)
    {
        pb.BattleSide side = _sortedSideListFromSelf[0];
        //手牌
        List<Pai> inhandList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.InHand);
        int lackType = (int)BattleManager.Instance.GetLackCardTypeBySide(side);
        InHandCardSort inhandSort = new InHandCardSort();
        inhandSort.lackType = lackType;
        inhandSort.rightCard = rightCard;
        inhandList.Sort(inhandSort);

        string str = "side[" + side.ToString() + "]=> inhand: ";
        for (int n = 0; n < inhandList.Count; n++)
        {
            str += inhandList[n].Id + ", ";
        }
        Debug.Log(str);

        Vector3 offset_inhand = new Vector3(64, 0, 0);
        Vector3 curPos = new Vector3(-440, -250, 0);
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
                curPos += (offset_inhand / 5);
            }
            script.transform.localPosition = curPos;
            curPos += offset_inhand;
        }

        //碰牌
        List<Pai> pList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Peng);
        pList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });

        str = "side[" + side.ToString() + "]=> peng: ";
        for (int n = 0; n < pList.Count; n++)
        {
            str += pList[n].Id + ", ";
        }
        Debug.Log(str);

        Vector3 offset_p = new Vector3(50, 0, 0);
        for (int i = 0; i < pList.Count; i++, _curSelf2DItemIndex_playing++)
        {
            if (i % 3 == 0)
            {
                curPos += new Vector3(5, 0, 0);
            }
            Item_pai script = getSelf2DCardItem(_curSelf2DItemIndex_playing);
            script.gameObject.SetActive(true);
            script.UpdateUI(pList[i], side);
            script.transform.localScale = Vector3.one * 0.88f;
            script.transform.localPosition = curPos;
            curPos += offset_p;
        }

        //杠牌
        List<Pai> gList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Gang);
        gList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });

        str = "side[" + side.ToString() + "]=> gang: ";
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

        Vector3 offset_g = new Vector3(50, 0, 0);
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
            list.Sort((x, y) => { return x.IsFromOther.CompareTo(y.IsFromOther); });
            for (int i = 0; i < list.Count; i++, _curSelf2DItemIndex_playing++)
            {
                Item_pai script = getSelf2DCardItem(_curSelf2DItemIndex_playing);
                script.gameObject.SetActive(true);
                script.UpdateGangCard(gList[i], side, isSelfGang && i == 0);
                script.transform.localScale = Vector3.one * 0.88f;
                script.transform.localPosition = curPos;
                curPos += offset_g;
            }
        }
    }
    #endregion

    #region sort others card(inhand、peng、gang)
    //手牌区坐标向量: startPos、rotate、offsetInLine
    private Vector3[] getNonDiscardVecBySideIndex(int sideIndex)
    {
        Debug.Log("getNonDiscardVecBySideIndex, sideIndex=" + sideIndex);
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero };
        if (sideIndex == 1)
        {
            result[0] = new Vector3(0.45f, 0.05f, -0.235f);
            result[1] = new Vector3(-90, -90, 0);
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

    private Vector3 getNonDiscardPengSpaceVecBySideIndex(int sideIndex)
    {
        Vector3 result = Vector3.zero;
        if (sideIndex == 1)
        {
            result = new Vector3(0, 0, -0.235f);
        }
        else if (sideIndex == 2)
        {
            result = new Vector3(0.32f, 0, 0);
        }
        else if (sideIndex == 3)
        {
            result = new Vector3(0, 0, 0.235f);
        }
        return result;
    }

    private Vector3 getNonDiscardGangSpaceVecBySideIndex(int sideIndex)
    {
        Vector3 result = Vector3.zero;
        if (sideIndex == 1)
        {
            result = new Vector3(0, 0, -0.235f);
        }
        else if (sideIndex == 2)
        {
            result = new Vector3(0.32f, 0, 0);
        }
        else if (sideIndex == 3)
        {
            result = new Vector3(0, 0, 0.235f);
        }
        return result;
    }

    private void hideOther3DCardObjBySide(pb.BattleSide side)
    {        
        if (_sideCardObjDict.ContainsKey(side))
        {
            List<Item_pai_3d> objList = _sideCardObjDict[side];
            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].gameObject.SetActive(false);
            }
            Debug.Log("hideOther3DCardObjBySide, sum count=" + objList.Count);
        }
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

    private void sortAndPlaceOtherCard(int sideIndex, bool shownDiscard)
    {
        Debug.Log("Other sort (normal), sideIndex=" + sideIndex + ", shownDiscard=" + shownDiscard.ToString());
        //手牌
        pb.BattleSide side = _sortedSideListFromSelf[sideIndex];
        hideOther3DCardObjBySide(side);

        List<Pai> inhandList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.InHand);
        Vector3[] vecs = getNonDiscardVecBySideIndex(sideIndex);    //startPos、rotate、offsetInLine        
        _curOther3DItemIndex_playing[sideIndex] = 0;
        Vector3 curPos = vecs[0];
        for (int i = 0; i < inhandList.Count; i++, _curOther3DItemIndex_playing[sideIndex]++)
        {
            Item_pai_3d script = getOtherCardObjBySide(sideIndex, _curOther3DItemIndex_playing[sideIndex]);
            script.gameObject.SetActive(true);
            script.UpdatePaiMian();
            script.transform.localEulerAngles = vecs[1];
            script.transform.localScale = sideIndex % 2 == 0 ? new Vector3(1.4f, 1, 1) : Vector3.one;
            if (shownDiscard && i == inhandList.Count - 1)
            {
                curPos += (vecs[2] / 5);
            }
            script.transform.localPosition = curPos;
            curPos += vecs[2];
        }
        Debug.Log("place inhand count=" + _curOther3DItemIndex_playing[sideIndex]);

        //碰牌
        List<Pai> pList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Peng);
        pList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });

        string str = "side[" + side.ToString() + "]=> peng: ";
        for (int n = 0; n < pList.Count; n++)
        {
            str += pList[n].Id + ", ";
        }
        Debug.Log(str);

        Vector3 pSpace = getNonDiscardPengSpaceVecBySideIndex(sideIndex);
        for (int i = 0; i < pList.Count; i++, _curOther3DItemIndex_playing[sideIndex]++)
        {
            if (i % 3 == 0)
            {
                curPos += pSpace;
            }
            Item_pai_3d script = getOtherCardObjBySide(sideIndex, _curOther3DItemIndex_playing[sideIndex]);
            script.gameObject.SetActive(true);
            script.SetInfo(pList[i]);
            script.SetSide(side);
            script.UpdatePaiMian();
            script.transform.localEulerAngles = vecs[1];
            script.transform.localScale = sideIndex % 2 == 0 ? new Vector3(1.4f, 1, 1) : Vector3.one;
            script.transform.localPosition = curPos;
            curPos += vecs[2];
        }
        Debug.Log("place inhand+peng count=" + _curOther3DItemIndex_playing[sideIndex]);

        //杠牌
        List<Pai> gList = BattleManager.Instance.GetCardListBySideAndStatus(side, PaiStatus.Gang);
        gList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });

        str = "side[" + side.ToString() + "]=> gang: ";
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

        Vector3 gSpace = getNonDiscardGangSpaceVecBySideIndex(sideIndex);
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
            curPos += gSpace;
            list.Sort((x, y) => { return x.IsFromOther.CompareTo(y.IsFromOther); });
            for (int i = 0; i < list.Count; i++, _curOther3DItemIndex_playing[sideIndex]++)
            {
                Item_pai_3d script = getOtherCardObjBySide(sideIndex, _curOther3DItemIndex_playing[sideIndex]);
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
                script.transform.localPosition = curPos;
                curPos += vecs[2];
            }
        }
        Debug.Log("place inhand+peng+gang count=" + _curOther3DItemIndex_playing[sideIndex]);
    }
    #endregion

    #region discard    
    //startPos、rotate、offsetInline、offsetBetweenLine、offsetAni
    private Vector3[] getDiscardVecBySideIndex(int sideIndex)
    {
        Debug.Log("getDiscardVecBySideIndex, sideIndex=" + sideIndex);
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        if (sideIndex == 0)
        {
            result[0] = new Vector3(-0.085f, 0.04f, -0.12f);
            result[1] = new Vector3(0, 0, 0);
            result[2] = new Vector3(0.033f, 0, 0);
            result[3] = new Vector3(0, 0, -0.045f);
            result[4] = new Vector3(0f, 0.2f, -0.257f);
        }
        else if (sideIndex == 1)
        {
            result[0] = new Vector3(0.13f, 0.04f, -0.068f);
            result[1] = new Vector3(0, -90, 0);
            result[2] = new Vector3(0, 0, 0.035f);
            result[3] = new Vector3(0.045f, 0, 0);
            result[4] = new Vector3(0.439f, 0.08f, 0);
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

    //我方出牌
    private void EnsureDiscard(Pai cardInfo)
    {
        //排序
        sortAndPlaceSelfCard(null, false);

        //发送出牌信息
        _battleProcess = BattleProcess.WaitingDiscardRet;
        BattleManager.Instance.CurProcess = BattleProcess.WaitingDiscardRet;
        GameMsgHandler.Instance.SendMsgC2GSDiscard(cardInfo.OID);
    }

    //收到服务器出牌成功的反馈，播放牌进入弃牌堆动画
    private void PlayDiscardAni(int cardOid)
    {
        Debug.Log("出牌反馈后，播放出牌动画，discardOid=" + cardOid);
        Pai cardInfo = BattleManager.Instance.GetCardInfoByCardOid(cardOid);
        cardInfo.Status = PaiStatus.Discard;
        Debug.LogError("cardInfo id=" + cardInfo.Id);
        pb.BattleSide side = BattleManager.Instance.GetSideByPlayerOID(cardInfo.PlayerID);
        int sideIndex = getSideIndexFromSelf(side);
        Item_pai_3d script = null;
        if (cardInfo.PlayerID == Player.Instance.PlayerInfo.OID)
        {
            script = getSelf3DCard(_discardItemIndex[0]);
            _discardItemIndex[0]++;
        }
        else
        {
            script = getOther3DDiscardObj(sideIndex, _discardItemIndex[sideIndex]);
            _discardItemIndex[sideIndex]++;
        }
        if (script == null)
        {
            Debug.LogError("未获取到对应item");
        }
        else
        {
            script.gameObject.SetActive(true);
            script.SetInfo(cardInfo);
            script.SetSide(side);
            script.UpdatePaiMian();
            script.transform.localScale = Vector3.one;
            Vector3[] vecs = getDiscardVecBySideIndex(sideIndex);   //startPos、rotate、offsetInline、offsetBetweenLine、offsetAni
            script.transform.localEulerAngles = vecs[1];
            int index = _discardItemIndex[sideIndex] % 10 - 1;
            int line = _discardItemIndex[sideIndex] / 10;
            Vector3 targetPos = vecs[0] + vecs[2] * index + vecs[3] * line;
            Debug.Log("targetPos=" + targetPos.x + ", " + targetPos.y + ", " + targetPos.z);
            script.transform.localPosition = vecs[4];
            iTween.MoveTo(script.gameObject, iTween.Hash("position", targetPos, "islocal", true, "time", 0.5f));

            AfterDiscard(cardInfo);
        }
    }

    //收到出牌信息后
    private void AfterDiscard(Pai discardInfo)
    {
        Debug.Log("CurPlaySide:" + BattleManager.Instance.CurPlaySide);
        if (BattleManager.Instance.CurPlaySide != _sortedSideListFromSelf[0])
        {
            //不是自己出牌，隐藏出牌方出的牌
            int sideIndex = getSideIndexFromSelf(BattleManager.Instance.CurPlaySide);
            sortAndPlaceOtherCard(sideIndex, false);

            Debug.Log("不是自己出牌，判断胡牌、杠、碰情况");
            _battleProcess = BattleProcess.CheckingHu;

            List<int> inhandList = BattleManager.Instance.GetCardIdListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.InHand);
            inhandList.Add(discardInfo.Id);
            List<int> pList = BattleManager.Instance.GetCardIdListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.Peng);
            List<int> gList = BattleManager.Instance.GetCardIdListBySideAndStatus(_sortedSideListFromSelf[0], PaiStatus.Gang);
            if (BattleManager.Instance.CanHu(inhandList, pList, gList))
            {
                Debug.Log("胡牌！！！");
                _battleProcess = BattleProcess.EnsureHuStart;
            }
            else
            {
                _battleProcess = BattleProcess.CheckingGang;
                if (BattleManager.Instance.CanGang(inhandList))
                {
                    Debug.Log("能杠，显示碰、杠牌按钮");
                    showProcGang();
                    _battleProcess = BattleProcess.EnsureGangStart;
                }
                else
                {
                    _battleProcess = BattleProcess.CheckingPeng;
                    if (BattleManager.Instance.CanPeng(inhandList, discardInfo.Id))
                    {
                        Debug.Log("能碰，显示碰牌按钮");
                        showProcPeng();
                        _battleProcess = BattleProcess.EnsurePengStart;
                    }
                    else
                    {
                        Debug.Log("自己无法操作，发送我方本轮结束消息");
                        Invoke("sendTurnOver", 1f);                        
                    }
                }
            }
        }
        else
        {
            Debug.Log("自己出牌结束，等待服务器切换操作方消息");
            _battleProcess = BattleProcess.SelfTurnOver;
        }
    }

    private void sendTurnOver()
    {
        GameMsgHandler.Instance.SendMsgC2GSCurTurnOver();
    }

    private void hideAllProcItem()
    {
        for (int i = 0; i < _procBtns.Count; i++)
        {
            _procBtns[i].gameObject.SetActive(false);
        }
    }

    private Item_procBtn getProcBtnItem(int index)
    {
        if (index < _procBtns.Count)
        {
            return _procBtns[index];
        }
        Item_procBtn script = UIManager.AddChild<Item_procBtn>(_procGrid.gameObject);
        _procBtns.Add(script);
        return script;
    }

    private void showProcPeng()
    {
        _procGrid.gameObject.SetActive(true);
        ProcBtnType[] types = { ProcBtnType.Peng, ProcBtnType.Pass };
        for (int i = 0; i < 2; i++)
        {
            Item_procBtn script = getProcBtnItem(i);
            script.gameObject.SetActive(true);
            script.UpdateUI(types[i]);
        }
        _procGrid.repositionNow = true;
    }

    private void showProcGang()
    {
        _procGrid.gameObject.SetActive(true);
        ProcBtnType[] types = { ProcBtnType.Gang, ProcBtnType.Peng, ProcBtnType.Pass };
        for (int i = 0; i < 3; i++)
        {
            Item_procBtn script = getProcBtnItem(i);
            script.gameObject.SetActive(true);
            script.UpdateUI(types[i]);
        }
        _procGrid.repositionNow = true;
    }
    #endregion

    private void SomePlayerPG(pb.BattleSide side, pb.CardStatus type)
    {
        Debug.Log("有玩家碰或杠，播放其碰杠动画，且重新排列手牌");

    }

    private void OnClickProcBtn(GameObject go)
    {

    }

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
