using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_exchangeArrow : MonoBehaviour
{
    private Vector3[] pos = { new Vector3(0, 0, 0), new Vector3(275, 155, 0), new Vector3(0, 275, 0), new Vector3(-270, 155, 0) };
    private Vector3[] angle = { new Vector3(0, 0, 0), new Vector3(0, 0, 104), new Vector3(0, 0, 180), new Vector3(0, 0, -104) };
    private int _index;
    private pb.ExchangeType _type;

    private void Awake()
    {
    }

    void Start()
    {

    }

    public void UpdateUI(pb.ExchangeType type, int index)
    {
        _index = index;
        _type = type;
        if (_type == pb.ExchangeType.Opposite)
        {
            transform.localEulerAngles = _index % 2 == 0 ? new Vector3(0, 0, 90) : new Vector3(0, 0, -90);
            transform.localPosition = _index % 2 == 0 ? new Vector3(-135, 155, 0) : new Vector3(135, 155, 0);
        }
        else
        {
            transform.localEulerAngles = new Vector3(0, 0, 90 * _index);
            if (type == pb.ExchangeType.AntiClock)
            {
                transform.localEulerAngles += new Vector3(0, 0, 180);
            }
            transform.localPosition = pos[index];
        }
        PlayDirectionAni();
    }

    private void PlayDirectionAni()
    {
        float angle = transform.localEulerAngles.z;
        if (angle >= -1 && angle <= 1)
        {
            // left
            iTween.MoveTo(gameObject, iTween.Hash("x", transform.localPosition.x - 50, "islocal", true, "time", 1f, "looptype", iTween.LoopType.loop));
        }
        else if (Mathf.Abs(angle) == 180)
        {
            // right
            iTween.MoveTo(gameObject, iTween.Hash("x", transform.localPosition.x + 50, "islocal", true, "time", 1f, "looptype", iTween.LoopType.loop));
        }
        else if (angle >= 89 && angle <= 91)
        {
            // down
            iTween.MoveTo(gameObject, iTween.Hash("y", transform.localPosition.y - 50, "islocal", true, "time", 1f, "looptype", iTween.LoopType.loop));
        }
        else if ((angle >= -91 && angle <= -89) || (angle >= 269 && angle <= 271))
        {
            //up
            iTween.MoveTo(gameObject, iTween.Hash("y", transform.localPosition.y + 50, "islocal", true, "time", 1f, "looptype", iTween.LoopType.loop));
        }
        Invoke("ItweenStop", 2f);
    }

    private void ItweenStop()
    {
        iTween.Stop();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
