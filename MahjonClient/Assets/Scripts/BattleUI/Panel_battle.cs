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

    StartSelectLackPai,
    SelectingLackPai,
    SelectLackPaiOver,

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
    private Dictionary<pb.BattleSide, List<GameObject>> _sidePaiDict = new Dictionary<pb.BattleSide, List<GameObject>>();
    private GameObject _selfPaiRoot;
    private List<GameObject> _otherPaiRoot = new List<GameObject>();

    //prepare ani  
    private bool _hasPlayStartAni = false;
    private int[] _shaiziValue = new int[2];    
    private int _drawPaiSumCount_prepareAni = 0;  //抓牌总张数
    private int _drawRound_prepareAni = 0;  //抓牌次数
    private pb.BattleSide _drawSide_prepareAni;  //抓牌人方位
    private int[] _placedPaiCount_prepareAni = { 0, 0, 0, 0 }; //各个方位已经放置的牌张数
    private int _curPaiDrawnSideIndex_prepareAni;
    private int _drawOffsetIndex_prepareAni;

    //select lack
    private GameObject _selectLackContainer;
    private List<UIButton> _btnLack = new List<UIButton>();

    public override void OnAwake()
    {
        base.OnAwake();
        _roomId = transform.FindChild("RoomID").GetComponent<UILabel>();

        // table
        GameObject _tableRoot = GameObject.Find("TableRoot");        
        _tableAni = _tableRoot.transform.FindChild("table(Clone)").GetComponent<Animation>();
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
        _selfPaiRoot = transform.FindChild("Root_pai/Side0").gameObject;
        for (int i = 1; i < 4; i++)
        {
            GameObject root = _tableRoot.transform.FindChild("Side" + i.ToString()).gameObject;
            _otherPaiRoot.Add(root);
        }

        //lack
        _selectLackContainer = transform.FindChild("").gameObject;
        _selectLackContainer.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            _btnLack[i] = transform.FindChild("").GetComponent<UIButton>();
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
        EventDispatcher.AddEventListener(EventDefine.ShowLackCard, ShowLackCard);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoleInRoom, UpdateRoleInRoom);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);
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
            playerList.Add(player);
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
        _drawSide_prepareAni = BattleManager.Instance.GetDealerSide();
        Debug.Log("庄家方位：" + _drawSide_prepareAni.ToString());
        //初始化从哪个方位抓牌
        int maxShaiZi = Mathf.Max(_shaiziValue[0], _shaiziValue[1]);
        pb.BattleSide curPaiDrawnSide = BattleManager.Instance.GetPaiDrawnSideByShaiZi(_drawSide_prepareAni, maxShaiZi); //从该方位抓牌
        Debug.Log("从" + curPaiDrawnSide.ToString() + "开始抓牌");
        _curPaiDrawnSideIndex_prepareAni = getSideIndexFromSelf(curPaiDrawnSide);
        _drawOffsetIndex_prepareAni = Mathf.Min(_shaiziValue[0], _shaiziValue[1]) * 2; //从0开始
        _drawPaiSumCount_prepareAni = 0;
        _drawRound_prepareAni = 0;
        //开始抓牌
        hideAllPaiObj();
        OnceDrawPaiAni();
    }

    private void hideAllPaiObj()
    {
        foreach (List<GameObject> objList in _sidePaiDict.Values)
        {
            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].SetActive(false);
            }
        }
    }

    private GameObject getItemPaiObjBySide(pb.BattleSide side, int sideIndex,int itemIndex)
    {
        if (!_sidePaiDict.ContainsKey(side))
        {
            _sidePaiDict.Add(side, new List<GameObject>());
        }
        List<GameObject> itemList = _sidePaiDict[side];
        if (itemIndex < itemList.Count)
        {
            return itemList[itemIndex];
        }
        if (side == _sortedSideListFromSelf[0])
        {
            //自己 2D牌            
            Item_pai script = UIManager.AddChild<Item_pai>(_selfPaiRoot);
            itemList.Add(script.gameObject);
            return script.gameObject;
        }
        else
        {
            //其余玩家 3D牌
            GameObject root = _otherPaiRoot[sideIndex - 1];
            GameObject pai = UIManager.AddGameObject("3d/model/pai", root);
            pai.AddComponent<Item_pai_3d>();
            itemList.Add(pai);
            return pai;
        }
    }

    private int drawOnePai(int wallIndex, int paiIndex)
    {
        int turnRound = 0;
        bool drawSuc = _sidePaiWallList[wallIndex].HidePaiInWallByIndex(paiIndex);
        while (!drawSuc)
        {
            wallIndex++;
            paiIndex = 0;
            turnRound++;
            drawSuc = _sidePaiWallList[wallIndex].HidePaiInWallByIndex(paiIndex);
        }
        return turnRound;
    }

    private void OnceDrawPaiAni()
    {
        Debug.Log("当前抓牌人方位：" + _drawSide_prepareAni.ToString()+", 已经抓的牌数："+ _drawPaiSumCount_prepareAni);
        if (_drawPaiSumCount_prepareAni >= (13 * 4 + 1))
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
            //Debug.LogError("draw pai=> 从" + _sortedSideListFromSelf[_curPaiDrawnSideIndex_gameStart].ToString()
            //    + "方位抓牌, drawOffsetIndex=" + _drawOffsetIndex_gameStart);
            int turnRound = drawOnePai(_curPaiDrawnSideIndex_prepareAni, _drawOffsetIndex_prepareAni);
            _drawPaiSumCount_prepareAni++;
            _drawOffsetIndex_prepareAni++;            
            if (turnRound > 0)
            {
                //抓牌方位有变化
                _curPaiDrawnSideIndex_prepareAni += turnRound;
                if (_curPaiDrawnSideIndex_prepareAni >= 4)
                {
                    _curPaiDrawnSideIndex_prepareAni -= _curPaiDrawnSideIndex_prepareAni - 4;
                }
                _drawOffsetIndex_prepareAni = 0;
            }
        }
        //摆牌
        Debug.Log("place pai to side[" + _drawSide_prepareAni.ToString() + "] when game start");
        int drawSideIndex = getSideIndexFromSelf(_drawSide_prepareAni);
        for (int index_pai = 0; index_pai < drawCountCurRound; index_pai++)
        {
            int itemIndex = _placedPaiCount_prepareAni[drawSideIndex];
            GameObject item = getItemPaiObjBySide(_drawSide_prepareAni, drawSideIndex, itemIndex);
            item.SetActive(true);
            _placedPaiCount_prepareAni[drawSideIndex]++;
            if (drawSideIndex == 0)
            {
                //自己
                Item_pai script = item.GetComponent<Item_pai>();
                if (script != null)
                {
                    Pai pai = BattleManager.Instance.GetPaiInfoByIndexAndSide(_drawSide_prepareAni, itemIndex);
                    script.UpdateUI(pai, _drawSide_prepareAni);
                    item.transform.localScale = Vector3.one * 0.88f;
                    item.transform.localPosition = new Vector3(-440 + itemIndex * 64, -250, 0);
                }
                else
                {
                    Debug.LogError("create self item_pai obj fail.");
                }
            }
            else
            {
                //其他人
                Item_pai_3d script = item.GetComponent<Item_pai_3d>();
                if (script != null)
                {
                    script.UpdatePaiMian();
                    script.SetSide(_drawSide_prepareAni);
                    if (drawSideIndex == 1)
                    {
                        item.transform.localScale = Vector3.one;
                        item.transform.localEulerAngles = new Vector3(-90, -90, 0);
                        item.transform.localPosition = new Vector3(0.45f, 0.05f, -0.235f + 0.035f * itemIndex);                        
                    }
                    else if (drawSideIndex == 2)
                    {
                        item.transform.localScale = new Vector3(1.4f, 1, 1);
                        item.transform.localEulerAngles = new Vector3(-90, 180, 0);
                        item.transform.localPosition = new Vector3(0.32f - 0.034f * itemIndex * 1.4f, 0.05f, 0.33f);
                    }
                    else if (drawSideIndex == 3)
                    {
                        item.transform.localScale = Vector3.one;
                        item.transform.localEulerAngles = new Vector3(-90, 90, 0);
                        item.transform.localPosition = new Vector3(-0.45f, 0.05f, 0.235f - 0.035f * itemIndex);
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
        _drawSide_prepareAni++;
        if (_drawSide_prepareAni > pb.BattleSide.north)
        {
            _drawSide_prepareAni = pb.BattleSide.east;
        }

        Invoke("OnceDrawPaiAni", 0.5f);
    }

    private void hideAllSelfPaiObj()
    {
        for (int i = 0; i < _sidePaiDict[_sortedSideListFromSelf[0]].Count; i++)
        {
            _sidePaiDict[_sortedSideListFromSelf[0]][i].SetActive(false);
        }
    }

    private void SortSelfPaiPrepareAni()
    {
        hideAllSelfPaiObj();
        List<Pai> selfList = BattleManager.Instance.GetAllInHandPaiListBySide(_sortedSideListFromSelf[0]);
        selfList.Sort((x, y) => { return x.Id.CompareTo(y.Id); });
        for (int i = 0; i < selfList.Count; i++)
        {
            GameObject obj = getItemPaiObjBySide(_sortedSideListFromSelf[0], 0, i);
            Item_pai script = obj.GetComponent<Item_pai>();
            if (script != null)
            {
                script.gameObject.SetActive(true);
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

    #region select lack
    private void StartSelectLackPai()
    {
        Debug.Log("select lack pai start...");
        BattleManager.Instance.CurProcess = BattleProcess.SelectingLackPai;
        _selectLackContainer.SetActive(true);
    }

    private void OnClickLack(GameObject go)
    {
        for (int i = 0; i < _btnLack.Count; i++)
        {
            if (_btnLack[i].gameObject == go)
            {                
                GameMsgHandler.Instance.SendMsgC2GSSelectLack((pb.CardType)(i + 1));
                _selectLackContainer.SetActive(false);
                break;
            }
        }
    }

    private void ShowLackCard()
    {
        Debug.Log("ShowLackCard");
        for (int i = 0; i < _roleItemList.Count; i++)
        {
            _roleItemList[i].ShowLackIcon();
        }
        Invoke("SelectLackOver", 0.2f);
    }

    private void SelectLackOver()
    {
        BattleManager.Instance.CurProcess = BattleProcess.SelectLackPaiOver;
    }
    #endregion

    #region playing
    private void SortCard()
    {
        Debug.Log("sort card");
        BattleManager.Instance.PlayingProcess = BattleProcess.SortingCard;
        
    }
    #endregion

    public override void OnUpdate()
    {
        base.OnUpdate();
        ProcessBattle();
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
                _battleProcess = BattleProcess.StartSelectLackPai;
                StartSelectLackPai();
                break;
            case BattleProcess.SelectLackPaiOver:
                _battleProcess = BattleProcess.BattleReady;
                SortCard();
                break;
            default:
                break;
        }
    }

}
