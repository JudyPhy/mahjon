using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aaaaa : MonoBehaviour {



    void Awake()
    {
        MeshFilter _mesh = GetComponent<MeshFilter>();
        
        int[] uvIndex = { 8, 4 };
        Vector2[] new_uv = new Vector2[_mesh.mesh.uv.Length];
        float width = 110.5f;
        for (int i = 0; i < _mesh.mesh.uv.Length; i++)
        {
            new_uv[i] = _mesh.mesh.uv[i];
            if (i == 56)
            {
                new_uv[i] = new Vector2(uvIndex[0] * width / 1024f, (uvIndex[1] + 1) / 6f);
            }
            if (i == 57)
            {
                new_uv[i] = new Vector2((uvIndex[0] + 1) * width / 1024f, (uvIndex[1] + 1) / 6f);
            }
            if (i == 58)
            {
                new_uv[i] = new Vector2((uvIndex[0] + 1) * width / 1024f, uvIndex[1] / 6f);
            }
            if (i == 59)
            {
                new_uv[i] = new Vector2(uvIndex[0] * width / 1024f, uvIndex[1] / 6f);
            }
        }
        _mesh.mesh.uv = new_uv;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
