using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Login : WindowsBasePanel
{
    private UIInput _accountInput;
    private UIInput _passwordInput;
    private GameObject _btnEnterGame;

    public override void OnAwake()
    {
        base.OnAwake();
        _accountInput = transform.FindChild("AccountInput").GetComponent<UIInput>();
        _passwordInput = transform.FindChild("PasswordInput").GetComponent<UIInput>();
        _btnEnterGame = transform.FindChild("btn_enterGame").gameObject;

        UIEventListener.Get(_btnEnterGame).onClick = OnEnterGame;
    }

    private void OnEnterGame(GameObject go)
    {
        Debug.Log("OnEnterGame");
        if (string.IsNullOrEmpty(_accountInput.value))
        {
            return;
        }
        if (string.IsNullOrEmpty(_passwordInput.value))
        {
            return;
        }
        GameMsgHandler.Instance.SendMsgC2GSLogin(_accountInput.value, _passwordInput.value);
    }
}
