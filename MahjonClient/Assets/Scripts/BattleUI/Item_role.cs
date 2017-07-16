using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_role : MonoBehaviour
{
    private UILabel _nameText;
    private UISprite _headIcon;
    private UILabel _goldText;

    private PlayerInfo _playerInfo;
    public PlayerInfo PlayerInfo
    {
        get { return _playerInfo; }
    }

    void Awake()
    {
        _nameText = transform.FindChild("name").GetComponent<UILabel>();
        _headIcon = transform.FindChild("headicon").GetComponent<UISprite>();
        _goldText = transform.FindChild("gold").GetComponent<UILabel>();
    }

    public void UpdateUI(PlayerInfo player)
    {
        _playerInfo = player;
        _nameText.text = _playerInfo.NickName;
        _headIcon.spriteName = "head_img_male";// _playerInfo.HeadIcon;
        //_headIcon.MakePixelPerfect();
        _goldText.text = _playerInfo.Gold.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
