using UnityEngine;
using System.Collections;
using System.IO;

public class ResourcesManager {

    private static ResourcesManager instance;
    public static ResourcesManager Instance {
        get {
            if (instance == null) {
                instance = new ResourcesManager();
            }
            return instance;
        }
    }

    public static string GetCsvConfigFilePath(string csvName) {
        string path = Application.persistentDataPath + "/lulu/CSVConfig/" + csvName + ".csv";
        if (!File.Exists(path)) {
            path = Application.streamingAssetsPath + "/CSVConfig/" + csvName + ".csv";
        }
        return path;
    }

    public string GetResPath(string resName) {        
        switch (resName) {
            case "GameStartPanel":
                return "Prefab/GameStartPanel";
            case "LoginPanel":
                return "Prefab/LoginPanel";
            case "LoadingUI":
                return "Prefab/LoadingUI";
            case "CreateRoleUI":
                return "Prefab/CreateRoleUI";
            case "Warrior":
                return "Model/Warrior";
            case "Assassin":
                return "Model/Assassin";
            case "Armourer":
                return "Model/Armourer";
            case "BattlePanel":
                return "Prefabs/UI/Battle/BattlePanel";
            case "MainPanel":
                return "Prefabs/UI/Main/MainPanel";
            case "SelecteRoleItem":
                return "Prefabs/UI/SelectRole/SelectRoleItem";
            case "SelectRolePanel":
                return "Prefabs/UI/SelectRole/SelectRolePanel";
            case "OfflineBattlePanel":
                return "Prefabs/UI/Battle/OfflineBattlePanel";
        }
        return "";
    }

    public GameObject GetUIPrefabs(string path) {
        //Debug.LogError("path:" + path);
        Object obj = Resources.Load(path);
        GameObject go = GameObject.Instantiate(obj) as GameObject;
        return go;
    }

    public GameObject GetModelPrefab(string modelName) {
        string path = GetResPath(modelName);
        GameObject obj = GetUIPrefabs(path);
        return obj;
    }

}
