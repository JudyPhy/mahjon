using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aaaaa : MonoBehaviour {



    void Awake()
    {
        MeshFilter m = GetComponent<MeshFilter>();

        int nx = 1;
        int ny = 1;
        Vector2 dx = new Vector2(1f / 9f, 1f / 6f);
        Vector2 off = new Vector2(0,0);
        Vector2 start = new Vector2(0, 0/6f);
        Vector2 vs = start + new Vector2(off.x * (nx - 1), off.y * ny);
        Vector2[] new_uv = new Vector2[m.mesh.uv.Length];
        for (int i = 0; i < m.mesh.uv.Length; i++)
        {
            //牌面顶点顺序如下
            // 56-----58
            //  |     |
            //  |     |
            //  |     |
            // 59-----57
            new_uv[i] = m.mesh.uv[i];
            if (i == 56)
            {
                new_uv[i] = new Vector2(0, 2f / 6f);
            }
            if (i == 57)
            {
                new_uv[i] = new Vector2(1f / 9f, 2f / 6f);
            }
            if (i == 58)
            {
                new_uv[i] = new Vector2(1f / 9f, 0 + 1f / 6f);
            }
            if (i == 59)
            {
                new_uv[i] = new Vector2(0, 0 + 1f / 6f);
            }
        }
        m.mesh.uv = new_uv;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
