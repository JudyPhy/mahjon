using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleSide
{
    Self = 0,
    SelfRight,
    SelfFront,
    SelfLeft,
}

public class Item_pai_hide : MonoBehaviour {

    private BattleSide _side;
    private int _sideIndex;
    private UISprite _paiDi;
    private UIWidget _container;

    void Awake()
    {
        _container = transform.GetComponent<UIWidget>();
        _paiDi = transform.FindChild("di").GetComponent<UISprite>();
    }

    // Use this for initialization
    void Start () {
		
	}

    public void Init(BattleSide side, int sideIndex)
    {
        this._side = side;
        this._sideIndex = sideIndex;
        string paiSpriteName = "";
        switch (this._side)
        {
            case BattleSide.Self:
            case BattleSide.SelfFront:
                paiSpriteName = "dipai_09";
                break;
            case BattleSide.SelfRight:
            case BattleSide.SelfLeft:
                paiSpriteName = "paidimian_17";
                break;
            default:
                break;
        }
        _paiDi.spriteName = paiSpriteName;
        _paiDi.MakePixelPerfect();
        UpdatePos();
    }

    private void UpdatePos()
    {
        int sideCount = ((int)_side) % 2 == 0 ? 28 : 26;
        int xOffset = 0;
        int yOffset = 0;
        switch (_side)
        {
            case BattleSide.Self:
                xOffset = _sideIndex < sideCount / 2 ? (-242 + 37 * _sideIndex) : (-242 + 37 * (_sideIndex - sideCount / 2));
                yOffset = _sideIndex < sideCount / 2 ? -200 : -188;
                break;
            case BattleSide.SelfFront:
                xOffset = _sideIndex < sideCount / 2 ? (-242 + 37 * _sideIndex) : (-242 + 37 * (_sideIndex - sideCount / 2));
                yOffset = _sideIndex < sideCount / 2 ? 200 : 212;
                break;
            case BattleSide.SelfRight:
                xOffset = 300;
                yOffset = _sideIndex < sideCount / 2 ? (-180 + 29 * _sideIndex) : (-193 + 29 * (_sideIndex - sideCount / 2));
                _paiDi.depth = sideCount - _sideIndex;
                break;
            case BattleSide.SelfLeft:
                xOffset = -300;
                yOffset = _sideIndex < sideCount / 2 ? (-180 + 29 * _sideIndex) : (-193 + 29 * (_sideIndex - sideCount / 2));
                _paiDi.depth = sideCount - _sideIndex;
                break;
            default:
                break;
        }
        transform.localPosition = new Vector3(xOffset, yOffset, 0);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
