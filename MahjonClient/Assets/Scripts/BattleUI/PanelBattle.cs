using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBattle : WindowsBasePanel
{
    private List<GameObject> _rootPai = new List<GameObject>();
    private Dictionary<pb.BattleSide, List<Item_pai_hide>> _drawCardsDict = new Dictionary<pb.BattleSide, List<Item_pai_hide>>();

    public override void OnAwake()
    {
        base.OnAwake();
    }

    public override void OnStart()
    {
        base.OnStart();
        UpdateRoleUI();
    }

    private void UpdateRoleUI()
    {
        Debug.Log("UpdateRoleUI");
        //for (int i=0;i< BattleManager.Instance.UpdatePlayerInfo)
    }

}
