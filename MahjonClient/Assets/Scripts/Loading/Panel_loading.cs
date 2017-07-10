﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Panel_loading : WindowsBasePanel
{

    private UILabel _tips;
    private DateTime _curTime;
    private bool _loadResOver = false;

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
        StartCoroutine(LoadTable());
    }

    private IEnumerator LoadTable()
    {
        GameObject root = GameObject.Find("TableRoot");
        GameObject table = UIManager.AddGameObject("3d/model/table", root);
        Animation ani = table.GetComponent<Animation>();
        ani.Stop();
        _loadResOver = true;        

        yield return 0;
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
        if (_loadResOver)
        {
            UIManager.Instance.ShowMainWindow<Panel_battle>(eWindowsID.BattleUI);
            return;
        }
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
