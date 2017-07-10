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
    private List<SidePai> _sidePaiList = new List<SidePai>(); //从自己方位(0)开始，顺时针旋转

    // players
    private GameObject _playerRoot;
    private List<Item_role> _roleItemList = new List<Item_role>();
    private List<pb.BattleSide> _sortedSideListFromSelf = new List<pb.BattleSide>(); //从自己方位(0)开始，顺时针旋转
    private Vector3[] _roleItemPos = { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) }; //从自己方位开始顺时针旋转

    // side pai

        
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
            _sidePaiList.Add(pai);
        }

        // players info
        _playerRoot = transform.FindChild("").gameObject;
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
        _tableAni.Play();
        Invoke("PaiShownAni", 0.5f);
    }

    private void PaiShownAni()
    {
        for (int i = 0; i < _sidePaiList.Count; i++)
        {
            _sidePaiList[i].gameObject.SetActive(true);
            iTween.MoveTo(_sidePaiList[i].gameObject, iTween.Hash("y", -0.32f, "islocal", true, "easytype", iTween.EaseType.linear,
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
        _shaiziValue[0] = Random.Range(1, 6);
        _shaiziValue[1] = Random.Range(1, 6);
    }

    //顺时针摸牌
    private void PlayStartDrawPaiAni()
    {
        HidePaiInWallByGameStart();
        DealPaiByGameStart();
    }

    private void HidePaiInWallByGameStart()
    {
        pb.BattleSide dealerSide = BattleManager.Instance.GetDealerSide();
        int curSidePaiIndex = -1;
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            if (dealerSide == _sortedSideListFromSelf[i])
            {
                curSidePaiIndex = i;
                break;
            }
        }
        if (curSidePaiIndex == -1)
        {
            Debug.LogError("dealer side is none.");
            return;
        }
        int drawOffsetIndex = Mathf.Min(_shaiziValue[0], _shaiziValue[1]) * 2 + 1;
        int drawPaiCount = 0;
        while (drawPaiCount < 13 * 4 + 1)
        {
            bool hideSuc = _sidePaiList[curSidePaiIndex].HideDrawStartPai(drawOffsetIndex);
            if (hideSuc)
            {
                drawPaiCount++;
                drawOffsetIndex++;
            }
            else
            {
                curSidePaiIndex++;
                drawOffsetIndex = 0;
                if (curSidePaiIndex >= _sidePaiList.Count)
                {
                    curSidePaiIndex = 0;
                }
            }
        }
    }

    private void DealPaiByGameStart()
    {
        for (int i = 0; i < _sortedSideListFromSelf.Count; i++)
        {
            List<Pai> list = BattleManager.Instance.GetPaiListBySide(_sortedSideListFromSelf[i]);
            
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
