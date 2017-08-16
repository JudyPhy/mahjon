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

    private GameObject _btnReady;

    private Dictionary<pb.MahjonSide, GameObject> _sideCardsRoot = new Dictionary<pb.MahjonSide, GameObject>();

    private RoomProcess _roomProcess;
    private Dictionary<pb.MahjonSide, List<Item_card>> _sideCardsDict = new Dictionary<pb.MahjonSide, List<Item_card>>();


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

        _btnReady = transform.FindChild("ButtonReady").gameObject;
        UIEventListener.Get(_btnReady).onClick = OnClickReady;

        for (pb.MahjonSide i = pb.MahjonSide.EAST; i <= pb.MahjonSide.NORTH; i++)
        {
            int curIndex = (int)(i - pb.MahjonSide.EAST);
            GameObject root = transform.FindChild("CardRoot/Anchor" + curIndex.ToString()).gameObject;
            _sideCardsRoot.Add(i, root);
        }
    }

    public override void OnRegisterEvent()
    {
        base.OnRegisterEvent();
        EventDispatcher.AddEventListener(EventDefine.UpdateRoomMember, UpdateRoomMember);
        EventDispatcher.AddEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoomMember, UpdateRoomMember);
        EventDispatcher.RemoveEventListener(EventDefine.PlayGamePrepareAni, PlayGamePrepareAni);
    }

    public override void OnEnableWindow()
    {
        base.OnEnableWindow();

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
        RefreshRoleItems();
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

    private void OnClickReady(GameObject go)
    {

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

    private void UpdateRoomMember()
    {
        if (_roomProcess == RoomProcess.PlayingEnterRoomAni)
        {
            Debug.Log("Is playing enter room ani, can't show player item.");
            return;
        }
        RefreshRoleItems();
    }

    private void RefreshRoleItems()
    {
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
    private int _curSideIndex;
    private int rightItemDepth = 20;
    private void PlayGamePrepareAni()
    {
        Debug.Log("PlayGamePrepareAni...");
        _timerObj.SetActive(true);
        _restCard.text = "108";
        _restRound.text = (108 / 4).ToString();
        ShowDealer();

        hideAllSideCardItem();
        _curSideIndex = (int)BattleManager.Instance.GetSideByPlayerOID(BattleManager.Instance.DealerID);
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
        pb.MahjonSide curSide = (pb.MahjonSide)_curSideIndex;
        Debug.Log("PlayDrawCardsAni, _turns:" + _turns + ", curSide:" + curSide);
        if (_turns > 16)
        {
            Debug.Log("draw animation over, start exchange cards...");
            return;
        }
        int curDrawCount = _turns > 11 ? 1 : 4;
        Debug.Log("current draw card " + curDrawCount);
        Vector3[] itemAttr = getCardsItemAttr(curSide);
        for (int i = 0; i < curDrawCount; i++)
        {
            Card card = BattleManager.Instance.GetDrawCardInfo(curSide, _drawItemIndex[curSide]);
            Item_card item = getCardsItem(curSide, _drawItemIndex[curSide]);
            item.gameObject.SetActive(true);
            item.UpdateUI(curSide, card);
            item.transform.localPosition = itemAttr[0] + itemAttr[1] * _drawItemIndex[curSide];
            if (_curSideIndex == 3)
            {
                //右侧item要修改depth
                int curDepth = rightItemDepth - _drawItemIndex[curSide];
                Debug.LogError("curDepth=" + curDepth);
                item.SetDepth(curDepth);
            }
            _drawItemIndex[curSide]++;           
        }
        _turns++;
        _curSideIndex = _curSideIndex == 5 ? 2 : _curSideIndex + 1;
        Invoke("PlayDrawCardsAni", 0.5f);
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
            return _sideCardsDict[side][index];
        }
        else {
            Item_card item = UIManager.AddChild<Item_card>(_sideCardsRoot[side]);
            _sideCardsDict[side].Add(item);
            return item;
        }
    }

    private Vector3[] getCardsItemAttr(pb.MahjonSide side)
    {
        //inhand_startPos、inhand_inlineOffset、inhand_outLineOffset
        int sideIndexFromSelf = BattleManager.Instance.GetSideIndexFromSelf(side);
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        switch (sideIndexFromSelf)
        {
            case 0:
                result[0] = new Vector3(-453, 80, 0);
                result[1] = new Vector3(75, 0, 0);
                result[2] = new Vector3(0, 0, 0);
                break;
            case 1:
                result[0] = new Vector3(-168, -180, 0);
                result[1] = new Vector3(0, 28, 0);
                result[2] = new Vector3(0, 0, 0);
                break;
            case 2:
                result[0] = new Vector3(228, -65, 0);
                result[1] = new Vector3(-38, 0, 0);
                result[2] = new Vector3(0, 0, 0);
                break;
            case 3:
                result[0] = new Vector3(160, 210, 0);
                result[1] = new Vector3(0, -28, 0);
                result[2] = new Vector3(0, 0, 0);
                break;
            default:
                break;
        }
        return result;
    }

}
