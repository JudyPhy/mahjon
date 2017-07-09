using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Panel_loading : WindowsBasePanel
{

    private UILabel _tips;
    private DateTime _curTime;

    public override void OnAwake()
    {
        base.OnAwake();
        _tips = transform.FindChild("Label").GetComponent<UILabel>();
        _tips.text = "加载中...";
    }

    public override void OnStart()
    {
        base.OnStart();
        _curTime = DateTime.Now;
    }

    private bool IsOverTime()
    {
        double interval = DateTime.Now.Subtract(_curTime).TotalMilliseconds;
        if (interval >= 100)
        {
            return true;
        }
        return false;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (IsOverTime())
        {
            _curTime = DateTime.Now;
            if (_tips.text == "加载中...")
            {
                _tips.text = "加载中.";
            }
            else if (_tips.text == "加载中.")
            {
                _tips.text = "加载中..";
            }
            else if (_tips.text == "加载中..")
            {
                _tips.text = "加载中...";
            }
        }
    }

}
