using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    CheckingPG,
    CheckOver,
    IsPenging,
    PengOver,
    IsGanging,
    GangOver,    
}

public class Panel_battle : WindowsBasePanel
{
    private BattleProcess _battleProcess = BattleProcess.Default;

    private UILabel _roomId;

    // table
    private Animation _tableAni;
    private GameObject _sideObj;
    private List<SidePai> _sidePaiWallList = new List<SidePai>(); //从自己方位(0)开始，逆时针旋转

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

    //after exchange card
    private GameObject _afterExchangeContainer;
    private List<Item_exchangeArrow> _exchangeArrows = new List<Item_exchangeArrow>();
    private pb.ExchangeType _exchangeType;

    // select lack
    private GameObject _selectLackContainer;
    private List<UIButton> _btnLack = new List<UIButton>();

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
        _selectLackContainer = transform.FindChild("btnLackContainer").gameObject;
        _selectLackContainer.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            UIButton btn = _selectLackContainer.transform.FindChild("btnLack" + (i + 1).ToString()).GetComponent<UIButton>();
            _btnLack.Add(btn);
            UIEventListener.Get(_btnLack[i].gameObject).onClick = OnClickLack;
        }
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
        EventDispatcher.AddEventListener(EventDefine.ShowLackCard, ShowLackCard);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoleInRoom, UpdateRoleInRoom);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);
        EventDispatcher.RemoveEventListener<bool>(EventDefine.UpdateBtnExchangeCard, UpdateBtnExchangeCard);
        EventDispatcher.RemoveEventListener(EventDefine.ReExchangeCard, SelectExchangeCard);
        EventDispatcher.RemoveEventListener<pb.ExchangeType>(EventDefine.UpdateCardInfoAfterExchange, UpdateCardInfoAfterExchange);
        EventDispatcher.RemoveEventListener(EventDefine.ShowLackCard, ShowLackCard);
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
        Vector3[] _roleItemPos = { new Vector3(-555, -155, 0), new Vector3(-555, 95, 0), new Vector3(285, 280, 0), new Vector3(495, 95, 0) };
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
        List<Pai> selfList = BattleManager.Instance.GetAllInHandPaiListBySide(_sortedSideListFromSelf[0]);
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
        List<Pai> list = BattleManager.Instance.GetExchangeCardListBySide(_sortedSideListFromSelf[0]);
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
        List<Pai> selfList = BattleManager.Instance.GetAllInHandPaiListBySide(_sortedSideListFromSelf[0]);
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
        _exchangeType = pb.ExchangeType.Opposite;//test
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
        return;
        Debug.Log("ArrowAniOver");
        _exchangeCardContainer.SetActive(false);
        // self
        hideAllSelf3DCard();
        hideAllOther3DCardObj();
        List<Pai> handCards = BattleManager.Instance.GetAllInHandPaiListBySide(_sortedSideListFromSelf[0]);
        List<Pai> exchangeCards = BattleManager.Instance.GetExchangeCardListBySide(_sortedSideListFromSelf[0]);
        Debug.Log("hand card count=" + handCards.Count + ", exchange card count=" + exchangeCards.Count);
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
            iTween.MoveTo(script.gameObject, iTween.Hash("y", -250, "islocal", true, "time", 0.2f, "delay", 1f));
            exchangeCards[i].Status = PaiStatus.InHand;
        }
        Invoke("SortInHandCard", 1.2f);
    }

    private void SortCardAfterExchangeSuccess()
    {
        Debug.Log("SortCardAfterExchangeSuccess");
        List<Pai> list = BattleManager.Instance.GetAllInHandPaiListBySide(_sortedSideListFromSelf[0]);
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
    private void StartSelectLackPai()
    {
        Debug.Log("select lack card start...");
        BattleManager.Instance.CurProcess = BattleProcess.SelectingLackCard;
        _selectLackContainer.SetActive(true);
        _exchangeTipsAniTime.Clear();
        for (int i = 0; i < _exchangeTips.Count; i++)
        {
            _exchangeTips[i].text = "选择中...";
            _exchangeTipsAniTime.Add(System.DateTime.Now);
        }
    }

    private void OnClickLack(GameObject go)
    {
        for (int i = 0; i < _btnLack.Count; i++)
        {
            if (_btnLack[i].gameObject == go)
            {
                GameMsgHandler.Instance.SendMsgC2GSSelectLack((pb.CardType)(i + 1));
                _selectLackContainer.SetActive(false);
                BattleManager.Instance.CurProcess = BattleProcess.WaitingLackCardInfo;
                break;
            }
        }
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
        BattleManager.Instance.CurProcess = BattleProcess.BattleReady;
    }
    #endregion

    /*
    #region playing
    private void hide3DPaiObjBySide(pb.BattleSide side)
    {
        List<GameObject> list = _sideCardObjDict[side];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].SetActive(false);
        }
    }

    private Vector3 getStartPosBySideAndStatus(int sideIndex, PaiStatus status)
    {
        switch (status)
        {
            case PaiStatus.InHand:
                if (sideIndex == 0)
                {
                    return new Vector3(-440 - 64, -250, 0);
                }
                else if (sideIndex == 1)
                {
                    return new Vector3(0.45f, 0.05f, -0.235f - 0.035f);
                }
                else if (sideIndex == 2)
                {
                    return new Vector3(0.32f + 0.034f * 1.4f, 0.05f, 0.33f);
                }
                else if (sideIndex == 3)
                {
                    return new Vector3(-0.45f, 0.05f, 0.235f + 0.035f);
                }
                break;
            case PaiStatus.Exchange:
                if (sideIndex == 0)
                {
                    return new Vector3(-440 - 64, -250, 0);
                }
                else if (sideIndex == 1)
                {
                    return new Vector3(0.45f, 0.05f, -0.235f - 0.035f);
                }
                else if (sideIndex == 2)
                {
                    return new Vector3(0.32f + 0.034f * 1.4f, 0.05f, 0.33f);
                }
                else if (sideIndex == 3)
                {
                    return new Vector3(-0.45f, 0.05f, 0.235f + 0.035f);
                }
                break;
            default:
                break;

        }
        return Vector3.zero;
    }

    private Vector3 getOffsetVecBySideAndStatus(int sideIndex, PaiStatus status)
    {
        switch (status)
        {
            case PaiStatus.InHand:
                if (sideIndex == 0)
                {
                    return new Vector3(64, 0, 0);
                }
                else if (sideIndex == 1)
                {
                    return new Vector3(0, 0, 0.035f);
                }
                else if (sideIndex == 2)
                {
                    return new Vector3(-0.034f * 1.4f, 0, 0);
                }
                else if (sideIndex == 3)
                {
                    return new Vector3(0, 0, -0.035f);
                }
                break;
            default:
                break;

        }
        return Vector3.zero;
    }

    private void placeSortedCard(pb.BattleSide side, int sideIndex, List<Pai> list, bool lastShown = true)
    {
        Debug.Log("placeSortedCard=> sideIndex:" + sideIndex.ToString());
        PaiStatus status = list[0].Status;
        Vector3 startPos = getStartPosBySideAndStatus(sideIndex, status);
        Vector3 offset = getOffsetVecBySideAndStatus(sideIndex, status);
        Vector3 lastOffset = Vector3.zero;
        if (sideIndex == 0)
        {
            hideAllSelfPaiObj();
            for (int i = 0; i < list.Count; i++)
            {
                GameObject item = getOtherCardObjBySide(side, sideIndex, i);
                item.SetActive(true);
                Item_pai pai = item.GetComponent<Item_pai>();
                pai.UpdateUI(list[i], side);
                lastOffset = lastShown ? new Vector3(10, 0, 0) : Vector3.zero;
                startPos = i < list.Count - 1 ? startPos + offset : startPos + offset + lastOffset;
                pai.transform.localPosition = startPos;
            }
        }
        else
        {
            hide3DPaiObjBySide(side);
            for (int i = 0; i < list.Count; i++)
            {
                GameObject item = getOtherCardObjBySide(side, sideIndex, i);
                item.gameObject.SetActive(true);
                Item_pai_3d script = item.GetComponent<Item_pai_3d>();
                script.SetInfo(list[i]);
                script.SetSide(side);
                script.UpdatePaiMian();
                if (sideIndex == 1)
                {
                    item.transform.localScale = Vector3.one;
                    item.transform.localEulerAngles = new Vector3(-90, -90, 0);
                    lastOffset = lastShown ? new Vector3(0, 0, 0.005f) : Vector3.zero;
                }
                else if (sideIndex == 2)
                {
                    item.transform.localScale = new Vector3(1.4f, 1, 1);
                    item.transform.localEulerAngles = new Vector3(-90, 180, 0);
                    lastOffset = lastShown ? new Vector3(-0.005f, 0, 0) : Vector3.zero;
                }
                else if (sideIndex == 3)
                {
                    item.transform.localScale = Vector3.one;
                    item.transform.localEulerAngles = new Vector3(-90, 90, 0);
                    lastOffset = lastShown ? new Vector3(0, 0, -0.01f) : Vector3.zero;
                }
                startPos = i < list.Count - 1 ? startPos + offset : startPos + offset + lastOffset;
                item.transform.localPosition = startPos;
            }
        }
    }

    private void sortAndPlaceInHandCard(pb.BattleSide side, int sideIndex)
    {
        Debug.Log("sortAndPlaceInHandCard=> side:" + side.ToString());
        List<Pai> inhandList = BattleManager.Instance.GetAllInHandPaiListBySide(side);
        int lackType = (int)BattleManager.Instance.GetLackCardTypeBySide(side);
        inhandList.Sort((x, y) =>
        {
            int result = 0;
            int type1 = Mathf.FloorToInt(x.Id / 10) + 1;
            int type2 = Mathf.FloorToInt(y.Id / 10) + 1;
            if (type1 != lackType && type2 == lackType)
            {
                result = 1;
            }
            if (result == 0)
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

        placeSortedCard(side, sideIndex, inhandList);
    }

    private void sortAndPlacePGCard(pb.BattleSide side, int sideIndex)
    {
        Debug.Log("sortAndPlacePGCard=> side:" + side.ToString());

    }

    private void SortCard()
    {
        Debug.Log("sort card");
        BattleManager.Instance.PlayingProcess = BattleProcess.SortingCard;
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            pb.BattleSide side = _sortedSideListFromSelf[i];
            // inhand
            sortAndPlaceInHandCard(side, i);
            // peng、gang
            sortAndPlacePGCard(side, i);
        }
    }
    #endregion
    */

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
