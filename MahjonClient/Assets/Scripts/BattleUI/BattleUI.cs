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
}
