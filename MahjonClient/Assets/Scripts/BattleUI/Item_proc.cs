using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_proc : MonoBehaviour {

    private UISprite _word;

    private pb.ProcType _type;

    void Awake()
    {
        _word = GetComponent<UISprite>();
        UIEventListener.Get(gameObject).onClick = OnClickProc;
    }

    //public void UpdateUI(pb.ProcType type)
    //{
    //    _type = type;
    //    switch (_type)
    //    {
    //        case pb.ProcType.Peng:
    //            _word.spriteName = "text_peng_s";
    //            break;
    //        case pb.ProcType.GangOther:
    //        case pb.ProcType.SelfGang:
    //            _word.spriteName = "text_gang_s";
    //            break;
    //        case pb.ProcType.HuOther:
    //        case pb.ProcType.SelfHu:
    //            _word.spriteName = "text_hu_s";
    //            break;
    //        default:
    //            break;
    //    }
    //    _word.MakePixelPerfect();
    //}

    private void OnClickProc(GameObject go)
    {

    }

    // Update is called once per frame
    void Update () {
		
	}
}
