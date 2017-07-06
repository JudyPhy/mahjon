using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_role : MonoBehaviour {

    private Vector3[] _roleItemPos = { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };

    private pb.BattlePlayerInfo _battlePlayerInfo;
    public pb.BattlePlayerInfo BattlePlayerInfo
    {
        set { _battlePlayerInfo = value; }
        get { return _battlePlayerInfo; }
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
        _nameText.text = _battlePlayerInfo.player.nickName;
        _levText.text = _battlePlayerInfo.player.lev.ToString();
        int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(_battlePlayerInfo.side);
        if (sideIndex >= 0 && sideIndex < _roleItemPos.Length)
        {
            transform.localPosition = _roleItemPos[sideIndex];
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
