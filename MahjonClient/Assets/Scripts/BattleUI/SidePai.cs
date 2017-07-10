﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidePai : MonoBehaviour
{
    private int _index; // 从自己方位开始顺时针旋转
    private pb.BattleSide _side;
    List<MeshRenderer> _paiList = new List<MeshRenderer>();

    Vector3[] pos = { new Vector3(-0.314f, -0.4f, 0.005f), new Vector3(0.54f, -0.4f, 0.29f),
            new Vector3(0.315f, -0.4f, 0.005f), new Vector3(-0.018f, -0.4f, 0.29f) };
    Vector3[] angle = { Vector3.zero, new Vector3(0, 90, 0), new Vector3(0, -180, 0), new Vector3(0, 90, 0) };

    private void Awake()
    {
        _paiList.Clear();
        for (int i = 0; i < 28; i++)
        {
            MeshRenderer mesh = transform.FindChild("pai_" + (i + 1).ToString()).GetComponent<MeshRenderer>();
            _paiList.Add(mesh);
        }
    }

    public void UpdatePai(int index)
    {
        _index = index;
        if (_index % 2 != 0)
        {
            _paiList[_paiList.Count - 2].gameObject.SetActive(false);
            _paiList[_paiList.Count - 1].gameObject.SetActive(false);
        }
        transform.localPosition = pos[_index];
        transform.localEulerAngles = angle[_index];
        for (int i = 0; i < _paiList.Count; i++)
        {
            _paiList[i].material.SetTextureOffset("_MainTex", Vector2.zero);
        }
        gameObject.SetActive(false);
    }

    void Start()
    {

    }

    public bool HideDrawStartPai(int index_pai)
    {
        int limiteIndex = 28;
        if (_index % 2 != 0)
        {
            limiteIndex = 26;
        }
        if (index_pai < limiteIndex)
        {
            _paiList[index_pai].gameObject.SetActive(false);
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
