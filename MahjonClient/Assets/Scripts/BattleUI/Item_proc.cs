using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTransmit;

public class Item_proc : MonoBehaviour {

    private UISprite _word;

    private pb.ProcType _type;

    void Awake()
    {
        _word = GetComponent<UISprite>();
        UIEventListener.Get(gameObject).onClick = OnClickProc;
    }

    public void UpdateUI(pb.ProcType type)
    {
        _type = type;
        switch (_type)
        {
            case pb.ProcType.Proc_Peng:
                _word.spriteName = "text_peng_s";
                break;
            case pb.ProcType.Proc_Gang:
                _word.spriteName = "text_gang_s";
                break;
            case pb.ProcType.Proc_Hu:
                _word.spriteName = "text_hu_s";
                break;
            case pb.ProcType.Proc_Pass:
                _word.spriteName = "text_hu_s";
                break;
            default:
                break;
        }
        _word.MakePixelPerfect();
    }

    private void OnClickProc(GameObject go)
    {
        EventDispatcher.TriggerEvent<pb.ProcType>(EventDefine.EnsureProcHPG, _type);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
