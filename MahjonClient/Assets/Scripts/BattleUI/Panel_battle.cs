using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventTransmit;

public class Panel_battle : WindowsBasePanel
{
    // table
    private GameObject _tableRoot;
    private Animation _tableAni;
    private List<SidePai> _sidePaiList = new List<SidePai>(); //从自己方位开始顺时针旋转

    private GameObject _playerRoot;
    private GameObject _prepareContainer;
    private List<Item_role> _roleList = new List<Item_role>();

    public override void OnAwake()
    {
        base.OnAwake();
        // table
        _tableRoot = GameObject.Find("TableRoot"); //find时必须激活预制
        GameObject table = UIManager.AddGameObject("3d/model/table", _tableRoot);
        _tableAni = table.GetComponent<Animation>();
        _tableAni.Stop();
        for (int i = 0; i < 4; i++)
        {
            SidePai pai = UIManager.AddChild<SidePai>(_tableRoot);
            pai.UpdatePai(i);
            _sidePaiList.Add(pai);
        }

        _playerRoot = transform.FindChild("").gameObject;
    }

    public override void OnStart()
    {
        base.OnStart();
        PlayGameStartAni();
    }

    private void PlayGameStartAni()
    {
        _tableAni.Play();
        Invoke("PaiShownAni", 0.5f);
    }

    private void PaiShownAni()
    {
        for (int i = 0; i < _sidePaiList.Count; i++)
        {
            _sidePaiList[i].gameObject.SetActive(true);
            iTween.MoveTo(_sidePaiList[i].gameObject, iTween.Hash("y", -0.32f, "islocal", true, "easytype", iTween.EaseType.linear,
                "time", 2.5f));
        }
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
