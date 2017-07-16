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

    GameStart,
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

    //抓牌动画  
    private int[] _shaiziValue = new int[2];    
    private int _drawPaiSumCount_gameStart = 0;  //抓牌总张数
    private int _drawRound_gameStart = 0;  //抓牌次数
    private pb.BattleSide _drawSide;  //抓牌人方位
    private int[] _placedPaiCound_gameStart = { 0, 0, 0, 0 }; //各个方位已经放置的牌张数
    private int _curPaiDrawnSideIndex_gameStart;
    private int _drawOffsetIndex_gameStart;

    //游戏中
    private pb.BattleSide _curPlaySide; //当前操作方

    private bool _hasPlayStartAni = false;


    public override void OnAwake()
    {
        base.OnAwake();
        _roomId = transform.FindChild("RoomID").GetComponent<UILabel>();

        // table
        GameObject _tableRoot = GameObject.Find("TableRoot");
        _tableAni = _tableRoot.transform.FindChild("table(Clone)").GetComponent<Animation>();
        _tableAni.Stop();
        _sideObj = _tableAni.transform.FindChild("Dummy001/Bone009").gameObject;
        for (int i = 0; i < 4; i++)
        {
            SidePai pai = UIManager.AddChild<SidePai>(_tableRoot);
            pai.UpdatePai(i);
            _sidePaiWallList.Add(pai);
        }

        // players info
        _playerRoot = transform.FindChild("RootPlayer").gameObject;

        //pai
        _selfPaiRoot = transform.FindChild("Root_pai/Side0").gameObject;
        for (int i = 1; i < 4; i++)
        {
            GameObject root = _tableRoot.transform.FindChild("Side" + i.ToString()).gameObject;
            _otherPaiRoot.Add(root);
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
        EventDispatcher.AddEventListener(EventDefine.PlayGameStartAni, PlayGameStartAni);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoleInRoom, UpdateRoleInRoom);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGameStartAni, PlayGameStartAni);
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

    private void RotateTable()
    {
        pb.BattleSide selfSide = _sortedSideListFromSelf[0];
        Vector3 origAngle = _sideObj.transform.localEulerAngles;
        _sideObj.transform.localEulerAngles = new Vector3(origAngle.x, (selfSide - pb.BattleSide.east) * 90, origAngle.z);        
        if (selfSide == pb.BattleSide.east)
        {
            _sideObj.transform.localPosition = new Vector3(0.0005f, 0.124f, -0.003f);
        }
        else if (selfSide == pb.BattleSide.south)
        {
            _sideObj.transform.localPosition = new Vector3(-0.001f, 0.123f, 0.002f);
        }
        else if (selfSide == pb.BattleSide.west)
        {
            _sideObj.transform.localPosition = new Vector3(0.003f, 0.124f, 0.004f);
        }
        else if (selfSide == pb.BattleSide.north)
        {
            _sideObj.transform.localPosition = new Vector3(0.0045f, 0.125f, -0.0015f);
        }
    }

    private void UpdateRoleInRoom()
    {
        _sortedSideListFromSelf = BattleManager.Instance.GetSortSideListFromSelf();
        RotateTable();
        List<PlayerInfo> playerList = new List<PlayerInfo>();
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            PlayerInfo player = BattleManager.Instance.GetPlayerInfoBySide(_sortedSideListFromSelf[i]);
            playerList.Add(player);
        }
        Debug.Log("UpdateRoleInRoom=> current player count:" + playerList.Count);
        Vector3[] _roleItemPos = { new Vector3(-555, -155, 0), new Vector3(-555, 95, 0), new Vector3(265, 280, 0), new Vector3(515, 95, 0) };
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] == null)
            {
                Debug.LogError("player" + i + " info is null.");
                continue;
            }
            Item_role itemScript = getRoleItem(i);
            if (itemScript != null)
            {
                itemScript.UpdateUI(playerList[i]);
                itemScript.gameObject.transform.localPosition = _roleItemPos[i];
            }
            else
            {
                Debug.LogError("get roleItem failed.");
            }
        }
    }

    #region game start animation
    private void PlayGameStartAni()
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
            iTween.MoveTo(_sidePaiWallList[i].gameObject, iTween.Hash("y", -0.32f, "islocal", true, "easytype", iTween.EaseType.linear,
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
        HidePaiInWallByGameStart();
    }

    private int GetSideIndexFromSelf(pb.BattleSide side)
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

    private void HidePaiInWallByGameStart()
    {
        Debug.Log("HidePaiInWallByGameStart");
        //初始化抓牌人方位
        _drawSide = BattleManager.Instance.GetDealerSide();
        Debug.Log("庄家方位：" + _drawSide.ToString());
        //初始化从哪个方位抓牌
        int maxShaiZi = Mathf.Max(_shaiziValue[0], _shaiziValue[1]);
        pb.BattleSide curPaiDrawnSide = BattleManager.Instance.GetPaiDrawnSideByShaiZi(_drawSide, maxShaiZi); //从该方位抓牌
        Debug.Log("从" + curPaiDrawnSide.ToString() + "开始抓牌");
        _curPaiDrawnSideIndex_gameStart = GetSideIndexFromSelf(curPaiDrawnSide);
        _drawOffsetIndex_gameStart = Mathf.Min(_shaiziValue[0], _shaiziValue[1]) * 2; //从0开始
        _drawPaiSumCount_gameStart = 0;
        _drawRound_gameStart = 0;
        //开始抓牌
        DrawPaiAniGameStart();
    }

    private GameObject GetItemPaiBySide(pb.BattleSide side, int sideIndex,int itemIndex)
    {
        if (!_sidePaiDict.ContainsKey(side))
        {
            _sidePaiDict.Add(side, new List<GameObject>());
        }
        List<GameObject> itemList = _sidePaiDict[side];
        if (side == _sortedSideListFromSelf[0])
        {
            //自己 2D牌
            if (itemIndex < itemList.Count)
            {
                return itemList[itemIndex];
            }
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
            return pai;
        }
    }

    private void DrawPaiAniGameStart()
    {
        Debug.Log("当前抓牌人方位：" + _drawSide.ToString()+", 已经抓的牌数："+ _drawPaiSumCount_gameStart);
        if (_drawPaiSumCount_gameStart >= (13 * 4 + 1))
        {
            _battleProcess = BattleProcess.PlayStartDrawAniOver;
            return;
        }
        int drawCountCurRound = 4; //当前需要抓牌的张数
        if (_drawRound_gameStart >= 12)
        {
            drawCountCurRound = 1;
        }
        //摸牌
        for (int i = 0; i < drawCountCurRound; i++)
        {
            //Debug.LogError("draw pai=> 从" + _sortedSideListFromSelf[_curPaiDrawnSideIndex_gameStart].ToString()
            //    + "方位抓牌, drawOffsetIndex=" + _drawOffsetIndex_gameStart);
            bool drawSuc = _sidePaiWallList[_curPaiDrawnSideIndex_gameStart].HideDrawStartPai(_drawOffsetIndex_gameStart);
            if (drawSuc)
            {
                _drawPaiSumCount_gameStart++;
                _drawOffsetIndex_gameStart++;
            }
            else
            {
                i--;
                _curPaiDrawnSideIndex_gameStart++; //换抓牌方位
                _drawOffsetIndex_gameStart = 0;
                if (_curPaiDrawnSideIndex_gameStart >= _sidePaiWallList.Count)
                {
                    _curPaiDrawnSideIndex_gameStart = 0;
                }
            }
        }
        //摆牌
        Debug.Log("place pai to side[" + _drawSide.ToString() + "] when game start");
        int drawSideIndex = GetSideIndexFromSelf(_drawSide);
        for (int index_pai = 0; index_pai < drawCountCurRound; index_pai++)
        {
            int itemIndex = _placedPaiCound_gameStart[drawSideIndex];
            GameObject item = GetItemPaiBySide(_drawSide, drawSideIndex, itemIndex);
            _placedPaiCound_gameStart[drawSideIndex]++;
            if (drawSideIndex == 0)
            {
                //自己
                Item_pai script = item.GetComponent<Item_pai>();
                if (script != null)
                {
                    Pai pai = BattleManager.Instance.GetPaiInfoByIndexAndSide(_drawSide, itemIndex);
                    script.UpdateUI(pai, _drawSide);
                    item.transform.localScale = Vector3.one * 1.7f;
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
                    script.SetSide(_drawSide);
                    if (drawSideIndex == 1)
                    {
                        item.transform.localEulerAngles = new Vector3(-90, 180, 0);
                        item.transform.localPosition = new Vector3(0.27f - 0.035f * itemIndex, 0.04f, 0.33f);
                    }
                    else if (drawSideIndex == 2)
                    {
                        item.transform.localEulerAngles = new Vector3(-90, -90, 0);
                        item.transform.localPosition = new Vector3(0.33f, 0.04f, 0.25f - 0.035f * itemIndex);
                    }
                    else if (drawSideIndex == 3)
                    {
                        item.transform.localEulerAngles = new Vector3(-90, 90, 0);
                        item.transform.localPosition = new Vector3(-0.33f, 0.04f, 0.25f - 0.035f * itemIndex);
                    }
                }
                else
                {
                    Debug.LogError("create 3d_pai obj fail.");
                }
            }
        }

        //本轮抓牌完毕，换人
        _drawRound_gameStart++;
        _drawSide--;
        if (_drawSide < pb.BattleSide.east)
        {
            _drawSide = pb.BattleSide.north;
        }

        Invoke("DrawPaiAniGameStart", 0.5f);
    }

    private void SortSelfPaiByGameStart()
    {
        List<Pai> selfList = BattleManager.Instance.GetPaiListBySide(_sortedSideListFromSelf[0]);
        selfList.Sort(new PaiCompare());
        for (int i = 0; i < selfList.Count; i++)
        {
            GameObject obj = GetItemPaiBySide(_sortedSideListFromSelf[0], 0, i);
            Item_pai script = obj.GetComponent<Item_pai>();
            if (script != null)
            {
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

    private void GameStart()
    {
        Debug.Log("game start...");
        BattleManager.Instance.CurPlaySide = BattleManager.Instance.GetDealerSide();
    }

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
                SortSelfPaiByGameStart();
                break;
            case BattleProcess.SortPaiOver:
                _battleProcess = BattleProcess.GameStart;
                GameStart();
                break;
            default:
                break;
        }
    }

}
