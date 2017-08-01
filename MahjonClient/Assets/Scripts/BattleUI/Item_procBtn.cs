using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProcBtnType
{
    Peng,
    Gang,
    Hu,
    Pass,
}

public class Item_procBtn : MonoBehaviour {

    private UISprite _sprite;
    private BoxCollider _collider;

    private ProcBtnType _type;

    private void Awake() {
        _sprite = transform.GetComponent<UISprite>();
        _collider = transform.GetComponent<BoxCollider>();
        UIEventListener.Get(gameObject).onClick = OnClickProcBtn;
    }

	// Use this for initialization
	void Start () {
		
	}

    public void UpdateUI(ProcBtnType type)
    {
        _type = type;
        switch (_type)
        {
            case ProcBtnType.Peng:
                _sprite.spriteName = "Button_PengAction";
                break;
            case ProcBtnType.Gang:
                _sprite.spriteName = "Button_GangAction";
                break;
            case ProcBtnType.Hu:
                _sprite.spriteName = "Button_HuAction";
                break;
            case ProcBtnType.Pass:
                _sprite.spriteName = "Button_GuoAction";
                break;
            default:
                break;
        }
        _sprite.MakePixelPerfect();
    }

    public void EnableClick(bool enable)
    {
        _collider.enabled = enabled;
    }

    private void OnClickProcBtn(GameObject go)
    {
        Debug.Log("OnClickProcBtn, procType=" + _type.ToString());
        switch (_type)
        {
            case ProcBtnType.Peng:
                GameMsgHandler.Instance.SendMsgC2GSProcPG(pb.ProcType.Peng);
                break;
            case ProcBtnType.Gang:
                if (BattleManager.Instance.CurPlaySide == BattleManager.Instance.GetSideByPlayerOID(Player.Instance.PlayerInfo.OID))
                {
                    GameMsgHandler.Instance.SendMsgC2GSProcPG(pb.ProcType.SelfGang);
                }
                else
                {
                    GameMsgHandler.Instance.SendMsgC2GSProcPG(pb.ProcType.GangOther);
                }
                break;
            case ProcBtnType.Hu:
                if (BattleManager.Instance.CurPlaySide == BattleManager.Instance.GetSideByPlayerOID(Player.Instance.PlayerInfo.OID))
                {
                    GameMsgHandler.Instance.SendMsgC2GSProcPG(pb.ProcType.SelfHu);
                }
                else
                {
                    GameMsgHandler.Instance.SendMsgC2GSProcPG(pb.ProcType.HuSelf);
                }
                break;
            case ProcBtnType.Pass:
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
