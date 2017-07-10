using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : WindowsBasePanel
{
    //top
    private UISprite _headicon;
    private UILabel _playerName;
    private UILabel _gold;
    private UILabel _diamond;
    private UIButton _btnAddGold;
    private UIButton _btnAddDiamond;

    // center
    private UIButton _btnCreateRoom;
    private UIButton _btnJoinRoom;
    private UIButton _btnQuickGame;

    // input roomId
    private GameObject _inputRoomIDContainer;
    private UIButton _btnClose;
    private GameObject _inputRoomIdMask;
    private UILabel _roomID;
    private UIButton _btnDel;
    private UIButton _btnEnsure;
    private List<UIButton> _btnNumList = new List<UIButton>();

    private string _curRoomId;

    public override void OnAwake()
    {
        base.OnAwake();
        // top
        Transform _playerContainer = transform.FindChild("PlayerInfoContainer");
        _headicon = _playerContainer.transform.FindChild("headIcon").GetComponent<UISprite>();
        _playerName = _playerContainer.transform.FindChild("name").GetComponent<UILabel>();
        _gold = _playerContainer.transform.FindChild("gold/Label").GetComponent<UILabel>();
        _btnAddGold = _playerContainer.transform.FindChild("gold/AddButton").GetComponent<UIButton>();
        _diamond = _playerContainer.transform.FindChild("diamond/Label").GetComponent<UILabel>();
        _btnAddDiamond = _playerContainer.transform.FindChild("diamond/AddButton").GetComponent<UIButton>();
        UIEventListener.Get(_btnAddGold.gameObject).onClick = OnClikAddGold;
        UIEventListener.Get(_btnAddDiamond.gameObject).onClick = OnClikAddDiamond;

        // center   
        _btnCreateRoom = transform.FindChild("CenterContainer/ButtonCreate").GetComponent<UIButton>();
        _btnJoinRoom = transform.FindChild("CenterContainer/ButtonJoin").GetComponent<UIButton>();
        _btnQuickGame = transform.FindChild("CenterContainer/ButtonQuick").GetComponent<UIButton>();
        UIEventListener.Get(_btnCreateRoom.gameObject).onClick = OnClikCreateRoom;
        UIEventListener.Get(_btnJoinRoom.gameObject).onClick = OnClikJoinRoom;
        UIEventListener.Get(_btnQuickGame.gameObject).onClick = OnClikQuickGame;

        //input roomId
        _inputRoomIDContainer = transform.FindChild("RoomIdContainer").gameObject;
        _inputRoomIDContainer.SetActive(false);
        _btnClose = _inputRoomIDContainer.transform.FindChild("CloseButton").GetComponent<UIButton>();
        _inputRoomIdMask = _inputRoomIDContainer.transform.FindChild("BG/Mask").gameObject;
        _roomID = _inputRoomIDContainer.transform.FindChild("NumContainer/Value").GetComponent<UILabel>();
        _curRoomId = "";
        _roomID.text = _curRoomId;
        _btnDel = _inputRoomIDContainer.transform.FindChild("NumContainer/ButtonDel").GetComponent<UIButton>();
        _btnEnsure = _inputRoomIDContainer.transform.FindChild("NumContainer/ButtonEnsure").GetComponent<UIButton>();
        _btnNumList.Clear();
        for (int i = 0; i < 10; i++)
        {
            UIButton btn = _inputRoomIDContainer.transform.FindChild("NumContainer/Button" + i.ToString()).GetComponent<UIButton>();
            UIEventListener.Get(btn.gameObject).onClick = OnClickBtnNum;
            _btnNumList.Add(btn);
        }
        UIEventListener.Get(_inputRoomIdMask).onClick = OnClikCloseRoomIdContainer;
        UIEventListener.Get(_btnClose.gameObject).onClick = OnClikCloseRoomIdContainer;
        UIEventListener.Get(_btnDel.gameObject).onClick = OnClikDelNumRoom;
        UIEventListener.Get(_btnEnsure.gameObject).onClick = OnClikEnsureEnterRoom;
    }

    public override void OnEnableWindow()
    {
        base.OnEnableWindow();
        UpdatePlayerInfoUI();
    }

    private void UpdatePlayerInfoUI()
    {
        _headicon.spriteName = Player.Instance.PlayerInfo.HeadIcon;
        _headicon.MakePixelPerfect();
        _playerName.text = Player.Instance.PlayerInfo.NickName;
        _gold.text = Player.Instance.PlayerInfo.Gold.ToString();
        _diamond.text = Player.Instance.PlayerInfo.Diamond.ToString();
    }

    private void OnClikAddGold(GameObject go)
    {

    }

    private void OnClikAddDiamond(GameObject go)
    {

    }

    private void OnClikCreateRoom(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.CreateRoom);
        BattleManager.Instance.IsWaitingEnterRoomRet = true;
        UIManager.Instance.ShowMainWindow<Panel_loading>(eWindowsID.LoadingUI);
    }

    private void OnClikJoinRoom(GameObject go)
    {
        _inputRoomIDContainer.SetActive(true);
    }

    private void OnClikEnsureEnterRoom(GameObject go)
    {
        if (string.IsNullOrEmpty(_curRoomId))
        {
            return;
        }
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.JoinRoom, _curRoomId);
    }

    private void OnClikQuickGame(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.QuickEnter);
    }

    private void OnClickBtnNum(GameObject go)
    {
        for (int i = 0; i < _btnNumList.Count; i++)
        {
            if (_btnNumList[i].gameObject == go)
            {
                _curRoomId += i.ToString();
                break;
            }
        }
        //Debug.LogError("click num, _curRoomId=" + _curRoomId);
        _roomID.text = _curRoomId;
    }

    private void OnClikDelNumRoom(GameObject go)
    {
        if (_curRoomId.Length < 1)
        {
            return;
        }
        _curRoomId = _curRoomId.Substring(0, _curRoomId.Length - 1);
        //Debug.LogError("delete num, _curRoomId=" + _curRoomId);
        _roomID.text = _curRoomId;
    }

    private void OnClikCloseRoomIdContainer(GameObject go)
    {
        _inputRoomIDContainer.SetActive(false);
        _curRoomId = "";
        _roomID.text = _curRoomId;
    }



}
