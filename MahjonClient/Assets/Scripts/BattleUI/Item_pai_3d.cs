using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_pai_3d : MonoBehaviour {

    private MeshFilter _mesh;
    private Pai _info;
    private pb.BattleSide _side;

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>();
    }

	// Use this for initialization
	void Start () {
		
	}

    private int[] GetPaiUVIndex()
    {

        int[] result = new int[2];
        if (_info != null)
        {
            result[0] = _info.Id % 10 - 1;
            switch (Mathf.FloorToInt(_info.Id / 10))
            {
                case 0:
                    //万
                    result[1] = 4;
                    break;
                case 1:
                    //条                
                    result[1] = 5;
                    break;
                case 2:
                    //筒
                    result[1] = 3;
                    break;
                default:
                    result[1] = 0;
                    break;
            }
        }
        else
        {
            result[0] = 0;
            result[1] = 0;
        }
        return result;
    }

    public void SetSide(pb.BattleSide side)
    {
        _side = side;
    }

    public void UpdatePaiMian()
    {
        //牌面顶点顺序如下
        // 56-----58
        //  |     |
        //  |     |
        //  |     |
        // 59-----57
        int[] uvIndex = GetPaiUVIndex();
        Vector2[] new_uv = new Vector2[_mesh.mesh.uv.Length];
        for (int i = 0; i < _mesh.mesh.uv.Length; i++)
        {
            new_uv[i] = _mesh.mesh.uv[i];
            if (i == 56)
            {
                new_uv[i] = new Vector2(uvIndex[0] / 9f, (uvIndex[1] + 1) / 6f);
            }
            if (i == 57)
            {
                new_uv[i] = new Vector2((uvIndex[0] + 1) / 9f, uvIndex[1] / 6f);
            }
            if (i == 58)
            {
                new_uv[i] = new Vector2((uvIndex[0] + 1) / 9f, (uvIndex[1] + 1) / 6f);
            }
            if (i == 59)
            {
                new_uv[i] = new Vector2(uvIndex[0] / 9f, uvIndex[1] / 6f);
            }
        }
        _mesh.mesh.uv = new_uv;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
