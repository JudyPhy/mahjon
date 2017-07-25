using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_exchangeArrow : MonoBehaviour
{
    private Vector3[] pos = { new Vector3(0, -300, 0), new Vector3(300, 0, 0), new Vector3(0, 300, 0), new Vector3(-300, 0, 0) };
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
            transform.localEulerAngles = _index % 2 == 0 ? Vector3.zero : new Vector3(0, 0, 180);
            transform.localPosition = _index % 2 == 0 ? new Vector3(-100, 0, 0) : new Vector3(100, 0, 0);
        }
        else
        {
            transform.localEulerAngles = new Vector3(0, 0, -90 * _index);
            if (type == pb.ExchangeType.AntiClock)
            {
                index += 2;
                if (index > 3)
                {
                    index -= 4;
                }
            }
            transform.localPosition = pos[index];
        }
        PlayDirectionAni();
    }

    private void PlayDirectionAni()
    {
        float angle = transform.localEulerAngles.z;
        int direction = (int)(angle / 90);
        if (direction == 0)
        {
            iTween.MoveTo(gameObject, iTween.Hash("x", transform.localPosition.x - 50, "islocal", true, "time", 0.5f, "looptype", iTween.LoopType.loop));
        }
        else if (direction == -1)
        {
            iTween.MoveTo(gameObject, iTween.Hash("y", transform.localPosition.y - 50, "islocal", true, "time", 0.5f, "looptype", iTween.LoopType.loop));
        }
        else if (direction == -2)
        {
            iTween.MoveTo(gameObject, iTween.Hash("x", transform.localPosition.x + 50, "islocal", true, "time", 0.5f, "looptype", iTween.LoopType.loop));
        }
        else if (direction == -3)
        {
            iTween.MoveTo(gameObject, iTween.Hash("y", transform.localPosition.y + 50, "islocal", true, "time", 0.5f, "looptype", iTween.LoopType.loop));
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
