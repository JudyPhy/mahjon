using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Login : WindowsBasePanel
{
    private GameObject _btnEnterGame;

    public override void OnAwake()
    {
        base.OnAwake();
        _btnEnterGame = transform.FindChild("btn_enterGame").gameObject;
        UIEventListener.Get(_btnEnterGame).onClick = OnEnterGame;
    }

    private void OnEnterGame(GameObject go)
    {
        Debug.Log("OnEnterGame");
        NetworkManager.Instance.ConnectGameServer("127.0.0.1", 3563);
    }
}
