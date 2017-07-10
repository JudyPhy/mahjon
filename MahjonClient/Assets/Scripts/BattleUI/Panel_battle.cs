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
}

public class Panel_battle : WindowsBasePanel
{
    private BattleProcess _battleProcess = BattleProcess.Default;

    // table
    private Animation _tableAni;
    private List<SidePai> _sidePaiWallList = new List<SidePai>(); //从自己方位(0)开始，顺时针旋转

    // players
    private GameObject _playerRoot;
    private List<Item_role> _roleItemList = new List<Item_role>();
    private List<pb.BattleSide> _sortedSideListFromSelf = new List<pb.BattleSide>(); //从自己方位(0)开始，顺时针旋转    

    // side pai
    private Dictionary<pb.BattleSide, List<Item_pai>> _sidePaiDict = new Dictionary<pb.BattleSide, List<Item_pai>>();
    private Dictionary<pb.BattleSide, GameObject> _sideItemPaiRoot = new Dictionary<pb.BattleSide, GameObject>();
        
    private int[] _shaiziValue = new int[2];

    public override void OnAwake()
    {
        base.OnAwake();
        // table
        GameObject _tableRoot = GameObject.Find("TableRoot");
        _tableAni = _tableRoot.transform.FindChild("table").GetComponent<Animation>();
        _tableAni.Stop();
        for (int i = 0; i < 4; i++)
        {
            SidePai pai = UIManager.AddChild<SidePai>(_tableRoot);
            pai.UpdatePai(i);
            _sidePaiWallList.Add(pai);
        }

        // players info
        _playerRoot = transform.FindChild("").gameObject;

        //pai
        for (int i = 0; i < 4; i++)
        {
            GameObject root = transform.FindChild("").gameObject;
            _sideItemPaiRoot.Add((pb.BattleSide)(i + 1), root);
        }
    }

    public override void OnStart()
    {
        base.OnStart();
        UpdateRoleInRoom();
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

    private void UpdateRoleInRoom()
    {
        _sortedSideListFromSelf = BattleManager.Instance.GetSortSideListFromSelf();
        List<PlayerInfo> playerList = new List<PlayerInfo>();
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            PlayerInfo player = BattleManager.Instance.GetPlayerInfoBySide(_sortedSideListFromSelf[i]);
            playerList.Add(player);
        }
        Debug.Log("UpdateRoleInRoom=> current player count:" + playerList.Count);
        Vector3[] _roleItemPos = { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] == null)
                continue;
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

    private void PlayGameStartAni()
    {
        _battleProcess = BattleProcess.PlayTableAniStart;
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
    }
    
    private void PlayStartDrawPaiAni()
    {
        Debug.Log("play draw pai ani start...");
        HidePaiInWallByGameStart();
        DrawPaiByGameStart();
    }

    private void HidePaiInWallByGameStart()
    {
        pb.BattleSide dealerSide = BattleManager.Instance.GetDealerSide();
        int curSidePaiIndex = GetSideIndexFromSelf(dealerSide);
        if (curSidePaiIndex == -1)
        {
            Debug.LogError("dealer side is none.");
            return;
        }
        int drawOffsetIndex = Mathf.Min(_shaiziValue[0], _shaiziValue[1]) * 2 + 1;
        int drawPaiCount = 0;
        while (drawPaiCount < 13 * 4 + 1)
        {
            bool hideSuc = _sidePaiWallList[curSidePaiIndex].HideDrawStartPai(drawOffsetIndex);
            if (hideSuc)
            {
                drawPaiCount++;
                drawOffsetIndex++;
            }
            else
            {
                curSidePaiIndex++;
                drawOffsetIndex = 0;
                if (curSidePaiIndex >= _sidePaiWallList.Count)
                {
                    curSidePaiIndex = 0;
                }
            }
        }
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

    private Item_pai GetItemPaiBySide(pb.BattleSide side, int index)
    {
        List<Item_pai> itemList = _sidePaiDict[side];
        if (index < itemList.Count)
        {
            return itemList[index];
        }
        Item_pai script = UIManager.AddChild<Item_pai>(_sideItemPaiRoot[side]);
        itemList.Add(script);
        return script;
    }

    private void DrawPaiByGameStart()
    {
        float[] xoffset = { };
        float[] yoffset = { };
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            List<Pai> list = BattleManager.Instance.GetPaiListBySide(_sortedSideListFromSelf[i]);
            int sideIndex = GetSideIndexFromSelf(_sortedSideListFromSelf[i]);
            float xInterval = 0;
            float yInterval = 0;
            for (int index_pai = 0; index_pai < list.Count; index_pai++)
            {
                Item_pai item = GetItemPaiBySide(_sortedSideListFromSelf[i], index_pai);
                item.UpdateUI(list[index_pai]);
                item.transform.localPosition = new Vector3(xoffset[sideIndex] + index_pai * xInterval, yoffset[sideIndex] + index_pai * yInterval, 0);
            }
        }
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
            /*case BattleProcess.PlayDealPaiAniOver:
                _battleProcess = BattleProcess.PlayDealPaiAniStart;
                break;*/
            default:
                break;
        }
    }

}
