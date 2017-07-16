using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_pai : MonoBehaviour
{
    private UISprite _pai;
    private UISprite _bg;

    private Pai _info;
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

    public void UpdateUI(Pai pai, pb.BattleSide side)
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
            Debug.LogError("self pai id:" + _info.Id + ", status:" + _info.Status.ToString());
            switch (_info.Status)
            {
                case pb.CardStatus.inHand:
                    _pai.spriteName = "b" + _info.Id.ToString();
                    _pai.gameObject.SetActive(true);
                    _bg.spriteName = "inhand_bg2";
                    break;
                default:
                    break;
            }
        }
    }

    private void OnClickPai(GameObject go)
    {
        Debug.Log("click pai, status=" + _info.Status + ", id=" + _info.Id + ", side=" + _side.ToString());
        if (BattleManager.Instance.CurPlaySide != _side)
        {
            Debug.Log("current side is " + BattleManager.Instance.CurPlaySide.ToString() + ", is not self round.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
