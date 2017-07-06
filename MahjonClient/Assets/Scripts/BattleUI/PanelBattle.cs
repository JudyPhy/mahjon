using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class PanelBattle : WindowsBasePanel
{
    private GameObject _playerRoot;
    private GameObject _prepareContainer;
    private List<Item_role> _roleList = new List<Item_role>();

    public override void OnAwake()
    {
        base.OnAwake();
        _playerRoot = transform.FindChild("").gameObject;
    }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnRegisterEvent()
    {
        base.OnRegisterEvent();
        EventDispatcher.AddEventListener<pb.BattlePlayerInfo>(EventDefine.AddRoleToRoom, AddRoleToRoom);
        EventDispatcher.AddEventListener<int>(EventDefine.PlayGameStartAni, PlayGameStartAni);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener<pb.BattlePlayerInfo>(EventDefine.AddRoleToRoom, AddRoleToRoom);
        EventDispatcher.RemoveEventListener<int>(EventDefine.PlayGameStartAni, PlayGameStartAni);
    }

    private Item_role getRoleItem(pb.BattlePlayerInfo role)
    {
        for (int i = 0; i < _roleList.Count; i++)
        {
            if (_roleList[i].gameObject.activeSelf && _roleList[i].BattlePlayerInfo.side == role.side)
            {
                return _roleList[i];
            }
        }
        for (int i = 0; i < _roleList.Count; i++)
        {
            if (!_roleList[i].gameObject.activeSelf)
            {
                _roleList[i].gameObject.SetActive(true);
                _roleList[i].BattlePlayerInfo = role;
                return _roleList[i];
            }
        }
        Item_role script = UIManager.AddChild<Item_role>(_playerRoot);
        script.BattlePlayerInfo = role;
        _roleList.Add(script);
        return script;
    }

    private void AddRoleToRoom(pb.BattlePlayerInfo role)
    {
        Debug.Log("AddRoleToRoom=>" + role.player.nickName);
        Item_role itemScript = getRoleItem(role);
        if (itemScript != null)
        {
            itemScript.UpdateUI();
        }
        else
        {
            Debug.LogError("player " + role.player.nickName + " item obj is null.");
        }
    }

    private void PlayGameStartAni(int dealerId)
    {

    }

}
