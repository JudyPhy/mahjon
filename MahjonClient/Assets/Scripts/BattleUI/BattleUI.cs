using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : WindowsBasePanel
{
    public override void OnAwake()
    {
        base.OnAwake();

    }

    public override void OnStart()
    {
        base.OnStart();
        WaitGameStart();
    }

    private void WaitGameStart()
    {

    }

<<<<<<< HEAD
    public override void OnInitWindow()
    {
        base.OnInitWindow();
    }
=======
    
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
}
