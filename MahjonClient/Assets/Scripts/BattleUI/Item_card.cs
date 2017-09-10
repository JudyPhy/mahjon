using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class Item_card : MonoBehaviour
{
    private UISprite _card;
    private UISprite _bg;
    private BoxCollider _collider;

    private Card _info;
    public Card Info
    {
        get { return _info; }
    }
    private pb.MahjonSide _side;

    private bool _preDiscard;
    public bool IsSelected
    {
        set { _preDiscard = value; }
    }

    void Awake()
    {
        _card = transform.FindChild("pai").GetComponent<UISprite>();
        _bg = transform.FindChild("bg").GetComponent<UISprite>();
        _collider = transform.GetComponent<BoxCollider>();
        UIEventListener.Get(gameObject).onClick = OnClickCard;
        _preDiscard = false;
    }

    // Use this for initialization
    void Start()
    {

    }

    public void UpdateUI(pb.MahjonSide side, Card card)
    {
        _info = card;
        _side = side;
        int sideIndex = BattleManager.Instance.GetSideIndexFromSelf(_side);
        _collider.enabled = sideIndex == 0;
        string[] bgName = { "self", "flank", "front", "flank" };
        if (_info == null)
        {
            Debug.LogError("self pai info is null.");
            _card.gameObject.SetActive(false);
            _bg.spriteName = "inhand_bg_back2";
            _bg.MakePixelPerfect();
        }
        else
        {
            switch (_info.Status)
            {
                case CardStatus.InHand:
                case CardStatus.Exchange:
                    if (sideIndex == 0)
                    {
                        _card.gameObject.SetActive(true);
                        _card.spriteName = _info.Id.ToString();
                        _card.MakePixelPerfect();
                    }
                    else
                    {
                        _card.gameObject.SetActive(false);
                    }
                    _bg.spriteName = bgName[sideIndex];
                    _bg.transform.localEulerAngles = sideIndex == 3 ? new Vector3(0, 180, 0) : Vector3.zero;
                    _bg.MakePixelPerfect();
                    _bg.depth = 10;
                    break;
                case CardStatus.Peng:
                    _card.spriteName = _info.Id.ToString();
                    _card.gameObject.SetActive(true);
                    _card.MakePixelPerfect();
                    _card.transform.localPosition = new Vector3(0, 11, 0);
                    break;
                default:
                    break;
            }
        }
    }

    public void SetDepth(int depth)
    {
        _bg.depth = depth;
    }

    public void ShowBack(int sideIndex)
    {
        _card.gameObject.SetActive(false);
        _collider.enabled = sideIndex == 0;
        string[] bgName = { "front_back", "flank_back", "front_back", "flank_back" };
        _bg.spriteName = bgName[sideIndex];
        _bg.MakePixelPerfect();
    }

    private void OnSelectExchangeCard()
    {
        Debug.Log("select card as exchange card, oid=" + _info.OID);
        if (_info.Status == CardStatus.Exchange)
        {
            _info.Status = CardStatus.InHand;
            iTween.MoveTo(gameObject, iTween.Hash("y", 80, "islocal", true, "time", 0.2f));
        }
        else
        {
            List<Card> list = BattleManager.Instance.GetCardList(Player.Instance.OID, CardStatus.Exchange);
            if (list.Count >= 3)
            {
                UIManager.Instance.ShowTips(TipsType.text, "只能交换3张牌");
            }
            else if (list.Count <= 0)
            {
                _info.Status = CardStatus.Exchange;
                iTween.MoveTo(gameObject, iTween.Hash("y", 105, "islocal", true, "time", 0.2f));
            }
            else
            {
                pb.CardType exchangeType = (pb.CardType)Mathf.CeilToInt(list[0].Id / 10);
                pb.CardType curType = (pb.CardType)Mathf.CeilToInt(_info.Id / 10);
                if (exchangeType != curType)
                {
                    UIManager.Instance.ShowTips(TipsType.text, "必须选择同花色的牌");
                }
                else
                {
                    _info.Status = CardStatus.Exchange;
                    iTween.MoveTo(gameObject, iTween.Hash("y", 105, "islocal", true, "time", 0.2f));
                }
            }
        }
    }

    public void UnChoose()
    {
        if (_preDiscard)
        {
            _preDiscard = false;
            iTween.MoveTo(gameObject, iTween.Hash("y", 80, "islocal", true, "time", 0.2f));
        }
    }

    private void OnChooseDiscard()
    {        
        if (_preDiscard)
        {
            _preDiscard = false;
            _info.Status = CardStatus.Discard;
            EventDispatcher.TriggerEvent<Card>(EventDefine.EnsureDiscard, _info);
        }
        else
        {
            Debug.Log("prepare discard, oid=" + _info.OID);
            _preDiscard = true;
            iTween.MoveTo(gameObject, iTween.Hash("y", 105, "islocal", true, "time", 0.2f));
            EventDispatcher.TriggerEvent<Card>(EventDefine.UnSelectOtherDiscard, _info);
        }
    }

    private void OnClickCard(GameObject go)
    {
        Debug.Log("click pai, status=" + _info.Status + ", id=" + _info.Id + ", side=" + _side.ToString()
            + ", curProcess=" + BattleManager.Instance.CurProcess);
        if (BattleManager.Instance.CurProcess == BattleProcess.ExchangCard)
        {
            // 选择交换牌阶段            
            OnSelectExchangeCard();
        }
        else if (BattleManager.Instance.CurProcess == BattleProcess.Discard)
        {
            if (_info.Status != CardStatus.InHand || _info.Status != CardStatus.Deal)
            {
                // 出牌阶段
                OnChooseDiscard();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
