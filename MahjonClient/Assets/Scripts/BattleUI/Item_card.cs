using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class Item_card : MonoBehaviour
{
    private UISprite _card;
    private UISprite _bg;
    private BoxCollider _collider;

    public Card Info
    {
        get { return _info; }
    }
    private Card _info;

    private bool _preDiscard;

    private string[] bgName = { "self", "flank", "front", "flank" };
    private string[] bgPGName = { "self_front", "flank_front", "self_front1", "flank_front" };
    private string[] discardBgName = { "self_front1", "flank_front", "front_front", "flank_front" };

    private int m_sideIndex;
    

    void Awake()
    {
        _card = transform.FindChild("pai").GetComponent<UISprite>();
        _bg = transform.FindChild("bg").GetComponent<UISprite>();
        _collider = transform.GetComponent<BoxCollider>();
        UIEventListener.Get(gameObject).onClick = OnClickCard;
        _preDiscard = false;
    }

    public void UpdateUI(int sidendex, Card card)
    {
        _info = card;
        m_sideIndex = sidendex;
        _collider.enabled = m_sideIndex == 0;        
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
                case CardStatus.Deal:                    
                    _bg.spriteName = bgName[m_sideIndex];
                    _bg.transform.localEulerAngles = m_sideIndex == 3 ? new Vector3(0, 180, 0) : Vector3.zero;
                    _bg.MakePixelPerfect();
                    _bg.depth = 10;

                    _card.gameObject.SetActive(m_sideIndex == 0);
                    _card.spriteName = _info.Id.ToString();
                    _card.MakePixelPerfect();
                    _card.transform.localScale = Vector3.one;
                    _card.transform.localPosition = Vector3.zero;                    
                    break;
                case CardStatus.Peng:
                    _bg.spriteName = bgPGName[m_sideIndex];
                    _bg.MakePixelPerfect();

                    _card.gameObject.SetActive(true);
                    _card.spriteName = _info.Id.ToString();                    
                    _card.MakePixelPerfect();
                    _card.transform.localPosition = new Vector3(0, 20, 0);
                    _card.transform.localScale = Vector3.one * 0.9f;                    
                    break;
                case CardStatus.Discard:                    
                    _bg.spriteName = discardBgName[m_sideIndex];
                    _bg.MakePixelPerfect();

                    _card.gameObject.SetActive(true);
                    _card.spriteName = _info.Id.ToString();
                    _card.MakePixelPerfect();
                    int[] rotate = { 0, 90, 180, -90 };
                    _card.transform.localEulerAngles = new Vector3(0, 0, rotate[m_sideIndex]);
                    float[] scaleRate = { 0.5f, 0.5f, 0.4f, 0.5f };
                    _card.transform.localScale = Vector3.one * scaleRate[m_sideIndex];
                    int[] cardPosX = { 0, -5, 0, 4 };
                    int[] cardPosY = { 11, 6, 4, 6 };
                    _card.transform.localPosition = new Vector3(cardPosX[m_sideIndex], cardPosY[m_sideIndex], 0);
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
        _collider.enabled = sideIndex == 0;
        string[] bgName = { "front_back", "flank_back", "front_back", "flank_back" };
        _bg.spriteName = bgName[sideIndex];
        _bg.MakePixelPerfect();

        _card.gameObject.SetActive(false);
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
        Debug.Log("click pai, status=" + _info.Status + ", id=" + _info.Id + ", sideIndex=" + m_sideIndex.ToString()
            + ", curProcess=" + BattleManager.Instance.CurProcess);        
        if (BattleManager.Instance.CurProcess == BattleProcess.ExchangCard)
        {
            // 选择交换牌阶段            
            OnSelectExchangeCard();
        }
        else if (BattleManager.Instance.CurProcess == BattleProcess.Discard)
        {
            if (_info.Status == CardStatus.InHand || _info.Status == CardStatus.Deal)
            {
                // 出牌阶段
                OnChooseDiscard();
            }
        }
        else if (BattleManager.Instance.CurProcess == BattleProcess.SelfGangChoose)
        {
            if (_info.Status == CardStatus.InHand || _info.Status == CardStatus.Deal)
            {
                // 自杠阶段
                int count = BattleManager.Instance.GetCardCount(_info.PlayerID, _info.Id);
                if (count == 4)
                {
                    BattleManager.Instance.ProcCard = _info;
                    EventDispatcher.TriggerEvent(EventDefine.UpdateSelfGangCard, _info);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
