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
        _btnEnterGame = transform.FindChild("EnterButton").gameObject;

        UIEventListener.Get(_btnEnterGame).onClick = OnEnterGame;
    }

    private void OnEnterGame(GameObject go)
    {
        if (string.IsNullOrEmpty(_accountInput.value))
        {
            UIManager.Instance.ShowTips(TipsType.text, "账号不能为空");
            return;
        }
        if (string.IsNullOrEmpty(_passwordInput.value))
        {
            UIManager.Instance.ShowTips(TipsType.text, "密码不能为空");
            return;
        }
        GameMsgHandler.Instance.SendMsgC2GSLogin(_accountInput.value, _passwordInput.value);
    }
}
