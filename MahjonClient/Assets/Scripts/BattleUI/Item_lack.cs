using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class Item_lack : MonoBehaviour {

    private UISprite _word;
    private pb.CardType _type;
    public pb.CardType Type
    {
        set { _type = value; }
        get { return _type; }
    }

    private bool _isSelected;

    private void Awake()
    {
        _word = transform.FindChild("word").GetComponent<UISprite>();
        UIEventListener.Get(gameObject).onClick = OnClickWord;
    }

	// Use this for initialization
	void Start () {
		
	}

    public void UpdateUI(int index)
    {
        switch (index)
        {
            case 0:
                _type = pb.CardType.Wan;
                _word.spriteName = "quewan1";
                transform.localPosition = new Vector3(-160, 0, 0);
                break;
            case 1:
                _type = pb.CardType.Tiao;
                _word.spriteName = "quetiao1";
                transform.localPosition = Vector3.zero;
                break;
            case 2:
                _type = pb.CardType.Tong;
                _word.spriteName = "quetong1";
                transform.localPosition = new Vector3(160, 0, 0);
                break;
            default:
                break;
        }
        UpdateWord(false);        
    }

    public void UpdateWord(bool selected)
    {
        _isSelected = selected;
        string spriteName = "";
        switch (_type)
        {
            case pb.CardType.Wan:
                _word.spriteName = _isSelected ? "quewan2" : "quewan1";
                break;
            case pb.CardType.Tiao:
                _word.spriteName = _isSelected ? "quetiao2" : "quetiao1";
                break;
            case pb.CardType.Tong:
                _word.spriteName = _isSelected ? "quetong2" : "quetong1";
                break;
            default:
                break;
        }
        _word.MakePixelPerfect();
    }

    private void OnClickWord(GameObject go)
    {
        Debug.Log("select lack[" + _type.ToString() + "], current selected=" + _isSelected.ToString());
        if (_isSelected)
        {
            EventDispatcher.TriggerEvent<pb.CardType>(EventDefine.EnsureLack, _type);
        }
        else
        {
            EventDispatcher.TriggerEvent<pb.CardType>(EventDefine.SelectLack, _type);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
