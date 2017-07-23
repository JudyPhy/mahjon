using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_role : MonoBehaviour
{
    private UILabel _nameText;
    private UISprite _headIcon;
    private UILabel _goldText;
    private UISprite _cardTypeIcon;

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
        _cardTypeIcon = transform.FindChild("lack").GetComponent<UISprite>();
        _cardTypeIcon.gameObject.SetActive(false);
    }

    public void UpdateUI(PlayerInfo player)
    {
        _playerInfo = player;
        _nameText.text = _playerInfo.NickName;
        _headIcon.spriteName = "head_img_male";// _playerInfo.HeadIcon;
        //_headIcon.MakePixelPerfect();
        _goldText.text = _playerInfo.Gold.ToString();
    }

    public void ShowLackIcon()
    {
        pb.CardType type = BattleManager.Instance.GetLackCardTypeByPlayerId(_playerInfo.OID);
        _cardTypeIcon.spriteName = "";
        _cardTypeIcon.gameObject.SetActive(true);
        //animation
        _cardTypeIcon.transform.localPosition = Vector3.zero;
        _cardTypeIcon.transform.localScale = Vector3.zero;
        iTween.MoveTo(_cardTypeIcon.gameObject, iTween.Hash("position", new Vector3(0, 0, 0), "islocal", true, "time", 0.2f, "easytype", iTween.EaseType.easeOutExpo));
        iTween.ScaleTo(_cardTypeIcon.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.2f, "easytype", iTween.EaseType.easeOutExpo));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
