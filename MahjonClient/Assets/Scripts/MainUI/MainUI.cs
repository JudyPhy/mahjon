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
        UIManager.Instance.ShowMainWindow<BattleUI>(eWindowsID.BattleUI);
    }

    private void OnClikCreateRoom(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.CreateRoom);
        UIManager.Instance.ShowMainWindow<BattleUI>(eWindowsID.BattleUI);
    }

    private void OnClikQuickGame(GameObject go)
    {
        GameMsgHandler.Instance.SendMsgC2GSEnterGame(pb.GameMode.QuickEnter);
        UIManager.Instance.ShowMainWindow<BattleUI>(eWindowsID.BattleUI);
    }

}
