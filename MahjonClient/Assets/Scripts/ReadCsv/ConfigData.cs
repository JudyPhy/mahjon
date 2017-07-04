using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfigData {

    private static ConfigData instance;
    public static ConfigData Instance {
        get {
            if (instance == null) {
                instance = new ConfigData();
            }
            return instance;
        }
    }

    //UIWindows
    public Dictionary<int, UIWindowsData> UIWindowsDataDict_ = new Dictionary<int, UIWindowsData>();
    //BattleSatge
    public Dictionary<int, BattleSatgeConfigData> BattleSatgeConfigDict = new Dictionary<int, BattleSatgeConfigData>();
    //HeroType
    public Dictionary<int, HeroConfigData> HeroConfigDict = new Dictionary<int, HeroConfigData>();

    public void LoadConfigs() {
        //UIWindows配置表
        ReadCsv config = new ReadCsv("UIWindows");
        for (int i = 3; i < config.GetRow(); i++) {
            UIWindowsData data = new UIWindowsData(config, i);
            this.UIWindowsDataDict_.Add(data._id, data);
        }

        //BattleSatge配置表
        config = new ReadCsv("BattleSatge");
        for (int i = 3; i < config.GetRow(); i++) {
            BattleSatgeConfigData data = new BattleSatgeConfigData(config, i);
            this.BattleSatgeConfigDict.Add(data._id, data);
        }

        //HeroConfigData配置表
        config = new ReadCsv("Hero");
        for (int i = 3; i < config.GetRow(); i++) {
            HeroConfigData data = new HeroConfigData(config, i);
            this.HeroConfigDict.Add(data._id, data);
        }
    }
}

public class UIWindowsData {
    public int _id;
    public string _name;
    public int _opentype;
    public string _prefabsPath;

    public UIWindowsData(ReadCsv config, int row) {
        _id = int.Parse(config.GetDataByRowAndName(row, "ID"));
        _name = config.GetDataByRowAndName(row, "WindowName");
        _opentype = int.Parse(config.GetDataByRowAndName(row, "OpenType"));
        _prefabsPath = config.GetDataByRowAndName(row, "PrefabPath");
    }
}

public class BattleSatgeConfigData {
    public int _id;
    public string _name;
    public int[] _pos = new int[9];

    public BattleSatgeConfigData(ReadCsv config, int row) {
        _id = int.Parse(config.GetDataByRowAndName(row, "ID"));
        _name = config.GetDataByRowAndName(row, "Name");
        string[] posKey = { "Pos1", "Pos2", "Pos3", "Pos4", "Pos5", "Pos6", "Pos7", "Pos8", "Pos9" };
        for (int i = 0; i < posKey.Length; i++) {
            _pos[i] = int.Parse(config.GetDataByRowAndName(row, posKey[i]));
        }
    }
}

public class HeroConfigData {
    public int _id;
    public string _name;
    public int _modelId;
    public int _atkType;
    public int _hp;
    public int _atk;
    public int _def;
    public int _speed;
    public int _atkRange;

    public HeroConfigData(ReadCsv config, int row) {
        _id = int.Parse(config.GetDataByRowAndName(row, "ID"));
        _name = config.GetDataByRowAndName(row, "Name");
        _modelId = int.Parse(config.GetDataByRowAndName(row, "ModelID"));
        _atkType = int.Parse(config.GetDataByRowAndName(row, "AtkType"));
        _hp = int.Parse(config.GetDataByRowAndName(row, "HP"));
        _atk = int.Parse(config.GetDataByRowAndName(row, "Atk"));
        _def = int.Parse(config.GetDataByRowAndName(row, "Def"));
        _speed = int.Parse(config.GetDataByRowAndName(row, "Speed"));
        _atkRange = int.Parse(config.GetDataByRowAndName(row, "AtkRange"));
    }
}