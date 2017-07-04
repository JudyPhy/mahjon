using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBattle : WindowsBasePanel
{
    private List<GameObject> _rootPai = new List<GameObject>();
    private Dictionary<pb.BattleSide, List<Item_pai_hide>> _drawCardsDict = new Dictionary<pb.BattleSide, List<Item_pai_hide>>();

    public override void OnAwake()
    {
        base.OnAwake();
        for (int i = 0; i < 4; i++)
        {
            string path = "Root_pai/Side" + i.ToString();
            _rootPai.Add(transform.FindChild(path).gameObject);
        }
    }

    public override void OnStart()
    {
        base.OnStart();
        BattlePrepareStart();
    }

    private void BattlePrepareStart()
    {
        Debug.Log("[Battle] prepare start...");
        PlaceWholePai();
    }

    private void PlaceWholePai()
    {
        /*_drawCardsDict.Clear();
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            int count = i % 2 == 0 ? 28 : 26;
            for (int j = 0; j < count; j++)
            { 
                Item_pai_hide item = UIManager.Instance.AddChild<Item_pai_hide>(_rootPai[i]);
                item.Init((BattleSide)i, j);
                if (_drawCardsDict.ContainsKey((BattleSide)i))
                {
                    _drawCardsDict[(BattleSide)i].Add(item);
                }
                else
                {
                    List<Item_pai_hide> list = new List<Item_pai_hide>();
                    list.Add(item);
                    _drawCardsDict.Add((BattleSide)i, list);
                }
                index++;
            }
        }
        PlayPaiEnterAni();*/
    }

    private void PlayPaiEnterAni()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i % 2 == 0)
            {

            }
            else
            {
            }
        }
    }

}
