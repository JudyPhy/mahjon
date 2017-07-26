using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class Item_pai : MonoBehaviour
{
    private UISprite _pai;
    private UISprite _bg;

    private Pai _info;
    public Pai Info
    {
        get { return _info; }
    }
    private pb.BattleSide _side;

    void Awake()
    {
        _pai = transform.FindChild("pai").GetComponent<UISprite>();
        _bg = transform.FindChild("bg").GetComponent<UISprite>();
        UIEventListener.Get(gameObject).onClick = OnClickPai;
    }

    // Use this for initialization
    void Start()
    {

    }

    public void UpdateUI(Pai pai, pb.BattleSide side, bool isSelfPG = false)
    {
        _info = pai;
        _side = side;
        if (_info == null)
        {
            Debug.LogError("self pai info is null.");
            _pai.gameObject.SetActive(false);
            _bg.spriteName = "inhand_bg_back2";
            _bg.MakePixelPerfect();
        }
        else
        {
            //Debug.LogError("self pai id:" + _info.Id + ", status:" + _info.Status.ToString());
            switch (_info.Status)
            {
                case PaiStatus.InHand:
                case PaiStatus.Exchange:
                    _pai.spriteName = "b" + _info.Id.ToString();
                    _pai.gameObject.SetActive(true);
                    _bg.spriteName = "inhand_bg2";
                    break;
                case PaiStatus.Peng:
                    _pai.spriteName = "b" + _info.Id.ToString();
                    _pai.gameObject.SetActive(true);
                    _bg.spriteName = "inhand_bg2";
                    break;
                default:
                    break;
            }
        }
    }

    public void UpdateGangCard(Pai pai, pb.BattleSide side, bool showFront)
    {
        _info = pai;
        _side = side;
        _pai.gameObject.SetActive(showFront);
        if (showFront)
        {
            _pai.spriteName = "b" + _info.Id.ToString();            
            _bg.spriteName = "inhand_bg2";
        }
        else
        {
            _bg.spriteName = "inhand_bg2";
        }
    }

    private void OnSelectExchangeCard()
    {
        Debug.Log("select card as exchange card, oid=" + _info.OID);
        if (_info.Status == PaiStatus.Exchange)
        {
            _info.Status = PaiStatus.InHand;
            iTween.MoveTo(gameObject, iTween.Hash("y", -250, "islocal", true, "time", 0.2f));
            EventDispatcher.TriggerEvent<bool>(EventDefine.UpdateBtnExchangeCard, false);
        }
        else
        {
            pb.CardType exchangeType = BattleManager.Instance.GetExchangeTypeBySide(_side);
            pb.CardType curType = (pb.CardType)Mathf.CeilToInt(_info.Id / 10);
            //Debug.Log("curType=" + curType.ToString() + ", exchangeType=" + exchangeType.ToString());
            if (exchangeType != pb.CardType.None && curType != exchangeType)
            {
                UIManager.Instance.ShowTips(TipsType.text, "必须选择同花色的牌");
            }
            else
            {
                int count = BattleManager.Instance.GetExchangeCardCountBySide(_side);
                if (count >= 3)
                {
                    UIManager.Instance.ShowTips(TipsType.text, "只能交换三张牌");
                }
                else
                {
                    _info.Status = PaiStatus.Exchange;
                    iTween.MoveTo(gameObject, iTween.Hash("y", -230, "islocal", true, "time", 0.2f));
                    count++;
                    EventDispatcher.TriggerEvent<bool>(EventDefine.UpdateBtnExchangeCard, count >= 3);
                }
            }
        }
    }

    private void OnClickPai(GameObject go)
    {
        Debug.Log("click pai, status=" + _info.Status + ", id=" + _info.Id + ", side=" + _side.ToString());
        if (BattleManager.Instance.CurProcess == BattleProcess.SelectingExchangeCard)
        {
            // 选择交换牌阶段            
            OnSelectExchangeCard();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
