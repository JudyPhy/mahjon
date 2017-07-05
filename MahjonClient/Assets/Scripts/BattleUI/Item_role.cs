using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_role : MonoBehaviour {

    private Vector3[] _roleItemPos = { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };

    private pb.RoleInfo _roleInfo;
    public pb.RoleInfo RoleInfo
    {
        set { _roleInfo = value; }
        get { return _roleInfo; }
    }

    private UILabel _nameText;
    private UISprite _headIcon;
    private UILabel _levText;

    void Awake()
    {
        _nameText = transform.FindChild("").GetComponent<UILabel>();
        _headIcon = transform.FindChild("").GetComponent<UISprite>();
        _levText = transform.FindChild("").GetComponent<UILabel>();
    }

    public void UpdateUI()
    {
        _nameText.text = _roleInfo.nickName;
        _levText.text = _roleInfo.lev.ToString();
        int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(_roleInfo.side);
        if (sideIndex >= 0 && sideIndex < _roleItemPos.Length)
        {
            transform.localPosition = _roleItemPos[sideIndex];
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
