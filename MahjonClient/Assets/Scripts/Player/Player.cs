using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{

    private static Player _instance;
    public static Player Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Player();
            return _instance;
        }
    }

    private RoleInfo _info;
    public RoleInfo RoleInfo
    {
        set { _info = value; }
        get { return _info; }
    }
}
