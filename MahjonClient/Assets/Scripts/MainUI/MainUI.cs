using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : WindowsBasePanel
{
    //top
    private UISprite _headicon;
    private UILabel _playerName;
    private UILabel _gold;
    private UILabel _fangka;

    //main ui
    private GameObject _mainui;
    private UITexture _bgMainui;
    private GameObject _btnJoinXueLiu;
    private GameObject _btnJoinXueZhan;
    private GameObject _btnCreateRoom;

    private GameObject _friendList;
    private UIGrid _gridFriend;
    //private List<Item_friend> _friendItemList = new List<Item_friend>();

    //create room
    private GameObject _createRoom;
    private GameObject _btnBack;
    private GameObject _friendRoomList;
    private GameObject _btnTuidaohu;
    private GameObject _btnDoudizhu;
    private GameObject _btnEnterRoom;

    private string _curRoomId;

    public override void OnAwake()
    {
        base.OnAwake();
        //top
       Transform _playerContainer = transform.FindChild("TopLeftAnchor/PlayerInfoContainer");
        _headicon = _playerContainer.transform.FindChild("headIcon/icon").GetComponent<UISprite>();
        _playerName = _playerContainer.transform.FindChild("name").GetComponent<UILabel>();
        _gold = _playerContainer.transform.FindChild("gold/value").GetComponent<UILabel>();
        _fangka = _playerContainer.transform.FindChild("fangka/value").GetComponent<UILabel>();

        //main ui
        _mainui = transform.FindChild("MainContainer").gameObject;
        _bgMainui = transform.FindChild("BG").GetComponent<UITexture>();
        _btnJoinXueLiu = _mainui.transform.FindChild("RightAnchor/ButtonXueZhan").gameObject;
        _btnJoinXueZhan = _mainui.transform.FindChild("RightAnchor/ButtonXueLiu").gameObject;
        _btnCreateRoom = _mainui.transform.FindChild("RightAnchor/ButtonCreate").gameObject;
        UIEventListener.Get(_btnJoinXueLiu).onClick = OnClikJoinXueLiu;
        UIEventListener.Get(_btnJoinXueZhan.gameObject).onClick = OnClikJoinXueZhan;
        UIEventListener.Get(_btnCreateRoom.gameObject).onClick = OnClikCreateRoom;

        _friendList = _mainui.transform.FindChild("LeftAnchor").gameObject;
        _gridFriend = _friendList.transform.FindChild("friendPanel/Grid").GetComponent<UIGrid>();

        //create room
        _createRoom = transform.FindChild("CreateRoomContainer").gameObject;        
        _friendRoomList = _createRoom.transform.FindChild("LeftAnchor").gameObject;
        _btnTuidaohu = _createRoom.transform.FindChild("LeftAnchor/ButtonTuidaohu").gameObject;
        _btnDoudizhu = _createRoom.transform.FindChild("LeftAnchor/ButtonDoudizhu").gameObject;
        _btnEnterRoom = _createRoom.transform.FindChild("RightAnchor/ButtonCreate").gameObject;
        _btnBack = _createRoom.transform.FindChild("RightAnchor/ButtonBack").gameObject;
        UIEventListener.Get(_btnTuidaohu).onClick = OnClickTuidao;
        UIEventListener.Get(_btnDoudizhu).onClick = OnClickDoudizhu;
        UIEventListener.Get(_btnEnterRoom).onClick = OnClickEnterRoom;
        UIEventListener.Get(_btnBack).onClick = OnClickBackMainUI;
    }

    public override void OnEnableWindow()
    {
        base.OnEnableWindow();
        UpdatePlayerInfoUI();
        ShowMainUI();
    }

    private void UpdatePlayerInfoUI()
    {
        _headicon.spriteName = Player.Instance.HeadIcon;
        _headicon.MakePixelPerfect();
        _playerName.text = Player.Instance.NickName;
        _gold.text = Player.Instance.GetGold();
        _fangka.text = Player.Instance.Fangka.ToString() + "张";
    }

    private void OnClikJoinXueLiu(GameObject go)
    {
        UIManager.Instance.ShowTips(TipsType.text, "功能暂未开放");
    }

    private void OnClikJoinXueZhan(GameObject go)
    {
        UIManager.Instance.ShowTips(TipsType.text, "功能暂未开放");
    }

    private void OnClikCreateRoom(GameObject go)
    {
        ShowCreateRoomUI();
    }

    private void ShowCreateRoomUI()
    {
        _bgMainui.mainTexture = Resources.Load("BG/bg_mainui") as Texture;
        _mainui.SetActive(false);
        _createRoom.SetActive(true);
        _friendRoomList.transform.localScale = Vector3.zero;
        _btnEnterRoom.transform.localPosition = new Vector3(200, 36, 0);
        _btnBack.SetActive(false);
        iTween.MoveTo(_btnEnterRoom, iTween.Hash("x", -260, "islocal", true, "time", 0.5f, "easytype", iTween.EaseType.easeOutBack));
        Invoke("ShowFriendRoomBtn", 0.5f);
    }

    private void ShowFriendRoomBtn()
    {
        iTween.ScaleTo(_friendRoomList, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easytype", iTween.EaseType.easeOutBack));
        Invoke("ShowBtnBack", 0.5f);
    }

    private void ShowBtnBack()
    {
        _btnBack.SetActive(true);
    }

    private void ShowMainUI()
    {
        _bgMainui.mainTexture = Resources.Load("BG/bg_mainui2") as Texture;
        _createRoom.SetActive(false);
        _mainui.SetActive(true);
        _friendList.transform.localScale = Vector3.zero;
        _btnJoinXueLiu.transform.localPosition = new Vector3(200, 170, 0);
        _btnJoinXueZhan.transform.localPosition = new Vector3(200, 18, 0);
        _btnCreateRoom.transform.localPosition = new Vector3(200, -134, 0);
        iTween.MoveTo(_btnJoinXueLiu, iTween.Hash("x", -276, "islocal", true, "time", 0.5f, "easytype", iTween.EaseType.easeOutBack));
        iTween.MoveTo(_btnJoinXueZhan, iTween.Hash("x", -276, "islocal", true, "time", 0.5f, "delay", 0.3f, "easytype", iTween.EaseType.easeOutBack));
        iTween.MoveTo(_btnCreateRoom, iTween.Hash("x", -276, "islocal", true, "time", 0.5f, "delay", 0.6f, "easytype", iTween.EaseType.easeOutBack));
        Invoke("ShowFriendList", 1f);
    }

    private void ShowFriendList()
    {
        iTween.ScaleTo(_friendList, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easytype", iTween.EaseType.easeOutBack));
    }

    private void OnClickBackMainUI(GameObject go)
    {
        ShowMainUI();
    }

    private void OnClickTuidao(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameType.XueZhan, pb.EnterMode.CreateRoom);
    }

    private void OnClickDoudizhu(GameObject go)
    {
        UIManager.Instance.ShowTips(TipsType.text, "功能暂未开放");
    }

    private void OnClickEnterRoom(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameType.XueZhan, pb.EnterMode.QuickEnter);
    }



}
