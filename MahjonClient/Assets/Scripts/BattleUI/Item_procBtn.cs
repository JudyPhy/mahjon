using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProcBtnType {
Peng,
Gang,
Hu,
Pass,
}

public class Item_procBtn : MonoBehaviour {

    private UISprite _sprite;

    private ProcBtnType _type;

    private void Awake() {
        _sprite = transform.GetComponent<UISprite>();
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
	
	// Update is called once per frame
	void Update () {
		
	}
}
