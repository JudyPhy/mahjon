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
        EventDispatcher.AddEventListener<pb.RoleInfo>(EventDefine.AddRoleToRoom, AddRoleToRoom);
    }

    public override void OnRemoveEvent()
    {
        base.OnRemoveEvent();
        EventDispatcher.RemoveEventListener<pb.RoleInfo>(EventDefine.AddRoleToRoom, AddRoleToRoom);
    }

    private Item_role getRoleItem(pb.RoleInfo role)
    {
        for (int i = 0; i < _roleList.Count; i++)
        {
            if (_roleList[i].gameObject.activeSelf && _roleList[i].RoleInfo.side == role.side)
            {
                return _roleList[i];
            }
        }
        for (int i = 0; i < _roleList.Count; i++)
        {
            if (!_roleList[i].gameObject.activeSelf)
            {
                _roleList[i].gameObject.SetActive(true);
                _roleList[i].RoleInfo = role;
                return _roleList[i];
            }
        }
        Item_role script = UIManager.AddChild<Item_role>(_playerRoot);
        script.RoleInfo = role;
        _roleList.Add(script);
        return script;
    }

    private void AddRoleToRoom(pb.RoleInfo role)
    {
        Debug.Log("AddRoleToRoom=>" + role.nickName);
        Item_role itemScript = getRoleItem(role);
        if (itemScript != null)
        {
            itemScript.UpdateUI();
        }
        else
        {
            Debug.LogError("player " + role.nickName + " item obj is null.");
        }
    }


}
