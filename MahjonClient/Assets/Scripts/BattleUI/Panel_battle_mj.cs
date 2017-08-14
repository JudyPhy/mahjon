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

    private GameObject _playerObj;
    private GameObject _playerRootLeft;
    private GameObject _playerRootRight;
    private List<Item_role> _playerItems = new List<Item_role>();

    private GameObject _btnReady;

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

        _playerObj = transform.FindChild("Players").gameObject;
        _playerRootLeft = transform.FindChild("Players/LeftAnchor").gameObject;
        _playerRootRight = transform.FindChild("Players/RightAnchor").gameObject;

        _btnReady = transform.FindChild("ButtonReady").gameObject;
        UIEventListener.Get(_btnReady).onClick = OnClickReady;
    }

    public override void OnRegisterEvent()
    {
        base.OnRegisterEvent();
        EventDispatcher.AddEventListener(EventDefine.UpdateRoomMember, UpdateRoomMember);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener(EventDefine.UpdateRoomMember, UpdateRoomMember);
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
        return item;
    }

    private void UpdateRoomMember()
    {
        if (_roomProcess == RoomProcess.PlayingEnterRoomAni)
        {
            Debug.Log("Is playing enter room ani, can't show player item.");
            return;
        }
        Debug.Log("UpdateRoomMember");
        RefreshRoleItems();
    }

    private void RefreshRoleItems()
    {
        List<SideInfo> list = BattleManager.Instance.GetRoomMembers();
        Debug.Log("current member count:" + list.Count);
        Vector3[] pos = { new Vector3(65, -116, 0), new Vector3(-65, 10, 0), new Vector3(-65, 180, 0), new Vector3(65, 50, 0) };
        hideAllPlayerItems();
        int n = 0;
        for (int i = 0; i < list.Count; i++)
        {
            int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(list[i].Side);
            Debug.Log("sideIndex:" + sideIndex + ", side:" + list[i].Side.ToString());
            Item_role role = getItemRole(n, sideIndex);
            n++;
            role.gameObject.SetActive(true);
            role.UpdateUI(list[i]);
            role.transform.localPosition = pos[sideIndex];
        }
    }

}
