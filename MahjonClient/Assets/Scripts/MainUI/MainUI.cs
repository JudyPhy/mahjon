using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : WindowsBasePanel
{
    private UIButton _btnEnterRoom;
    private UIButton _btnCreateRoom;
    private UIButton _btnQuickGame;

    // enter game
    private GameObject _panel_inputRoomID;
    private UIInput _inputRoomID;
    private UIButton _btnEnsureEnterRoom;

    public override void OnAwake()
    {
        base.OnAwake();
        _btnEnterRoom = transform.FindChild("").GetComponent<UIButton>();
        _btnCreateRoom = transform.FindChild("").GetComponent<UIButton>();
        _btnQuickGame = transform.FindChild("").GetComponent<UIButton>();

        _panel_inputRoomID = transform.FindChild("").gameObject;
        _inputRoomID = transform.FindChild("").GetComponent<UIInput>();
        _btnEnsureEnterRoom = transform.FindChild("").GetComponent<UIButton>();

        UIEventListener.Get(_btnEnterRoom.gameObject).onClick = OnClikEnterRoom;
        UIEventListener.Get(_btnEnsureEnterRoom.gameObject).onClick = OnClikEnsureEnterRoom;
        UIEventListener.Get(_btnCreateRoom.gameObject).onClick = OnClikCreateRoom;
        UIEventListener.Get(_btnQuickGame.gameObject).onClick = OnClikQuickGame;
    }

    private void OnClikEnterRoom(GameObject go)
    {
        _panel_inputRoomID.SetActive(true);
    }

    private void OnClikEnsureEnterRoom(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.JoinRoom);
    }

    private void OnClikCreateRoom(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.CreateRoom);
    }

    private void OnClikQuickGame(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.QuickEnter);        
    }

}
