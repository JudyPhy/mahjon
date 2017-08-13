using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_role : MonoBehaviour
{
    private UILabel _name;
    private UISprite _headIcon;
    private UILabel _score;
    private GameObject _owner;
    private GameObject _dealer;
    private UISprite _lack;

    private pb.PlayerInfo _playerInfo;

    public pb.PlayerInfo PlayerInfo
    {
        get { return _playerInfo; }
    }

    void Awake()
    {
        _name = transform.FindChild("name").GetComponent<UILabel>();
        _headIcon = transform.FindChild("headicon/icon").GetComponent<UISprite>();
        _score = transform.FindChild("score").GetComponent<UILabel>();
        _owner = transform.FindChild("owner").gameObject;
        _owner.gameObject.SetActive(false);
        _dealer = transform.FindChild("dealer").gameObject;
        _dealer.SetActive(false);
        _lack = transform.FindChild("lack").GetComponent<UISprite>();
        _lack.gameObject.SetActive(false);
    }

    public void UpdateUI(pb.PlayerInfo player)
    {
        _playerInfo = player;
        _name.text = _playerInfo.NickName;
        _headIcon.spriteName = string.IsNullOrEmpty(_playerInfo.HeadIcon) ? "headIcon_default_s" : _playerInfo.HeadIcon;
        _score.text = "";
        _owner.SetActive(player.IsOwner);
    }

    public string getSpriteNameByType(pb.CardType type)
    {
        switch (type)
        {
            case pb.CardType.Wan:
                return "room_color2";
            case pb.CardType.Tiao:
                return "room_color3";
            case pb.CardType.Tong:
                return "room_color1";
            default:
                return "";
        }
    }

    //public void ShowLackIcon()
    //{
    //    //Debug.Log("player name=" + PlayerInfo.NickName + " lack icon ani...");
    //    pb.CardType type = BattleManager.Instance.GetLackCardTypeByPlayerId(_playerInfo.OID);
    //    _owner.spriteName = getSpriteNameByType(type);
    //    _owner.gameObject.SetActive(true);
    //    //animation
    //    _owner.transform.localPosition = Vector3.zero;
    //    _owner.transform.localScale = Vector3.one * 2;
    //    iTween.MoveTo(_owner.gameObject, iTween.Hash("position", new Vector3(55, 30, 0), "islocal", true, "time", 0.5f, "easytype", iTween.EaseType.easeOutExpo));
    //    iTween.ScaleTo(_owner.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easytype", iTween.EaseType.easeOutExpo));
    //}

    //public void ShowDealer()
    //{
    //    _dealer.SetActive(true);
    //}

    // Update is called once per frame
    void Update()
    {

    }
}
