using UnityEngine;
using System.Collections;
using System.Collections.Generic;

<<<<<<< HEAD
//=========================================================================================================
//
//逻辑思路：一级窗口：不可堆叠，任何时刻只存在一个。
//             ↓
//             ↓
//          子窗口：隶属于打开其的一级窗口，可堆叠或隐藏显示。
//
//=========================================================================================================

public class UIManager : MonoBehaviour {
=======
public class UIManager : MonoBehaviour
{
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50

    public static UIManager Instance;
    //UI摄像机
    public static Camera UICamera_;
<<<<<<< HEAD
=======
    //Center Root
    private GameObject _centerRoot;
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    //当前显示窗口
    private WindowsBasePanel CurShowingWindow_;
    private bool IsShowingWindow_ = false;
    //等待删除的窗口
    public Dictionary<eWindowsID, WindowsBasePanel> DeletingWindowsDict_ = new Dictionary<eWindowsID, WindowsBasePanel>();

<<<<<<< HEAD
    void Awake() {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        UICamera_ = this.transform.FindChild("Camera").GetComponent<Camera>();
    }

    public void ShowWindow<T>(eWindowsID windowId) where T : WindowsBasePanel {
        if (!ConfigData.Instance.UIWindowsDataDict_.ContainsKey((int)windowId)) {
            Debug.LogError("窗口Id [" + windowId + "] 不存在，请检查配表。");
            return;
        }
        UIWindowsData windowData = ConfigData.Instance.UIWindowsDataDict_[(int)windowId];
        if (windowData != null) {
            switch (windowData._opentype) {
                case 1: {
                        ShowMainWindow<T>(windowId);
                    }
                    break;
                case 2: {

                    } break;
                case 3: {
                    } break;
                default:
                    break;
            }
        } else {
            Debug.LogError("窗口Id [" + windowId + "] 配表数据为空。");
        }
    }

    private void ShowMainWindow<T>(eWindowsID windowId) where T : WindowsBasePanel {
        this.IsShowingWindow_ = true;
        if (this.CurShowingWindow_ != null) {
            if (this.CurShowingWindow_.WindowData_._id == (int)windowId) {
                this.IsShowingWindow_ = false;
                return;
            } else {
                CloseMainWindow((eWindowsID)this.CurShowingWindow_.WindowData_._id);
            }
        }
        if (this.DeletingWindowsDict_.ContainsKey(windowId)) {
            this.CurShowingWindow_ = this.DeletingWindowsDict_[windowId];
            this.CurShowingWindow_.gameObject.SetActive(true);
            this.DeletingWindowsDict_.Remove(windowId);
        } else {
            UIWindowsData windowData = ConfigData.Instance.UIWindowsDataDict_[(int)windowId];
            T windowScript = AddChild<T>(this.gameObject);
            if (windowScript != null) {
                windowScript.gameObject.SetActive(true);
                windowScript.WindowData_ = windowData;
                this.CurShowingWindow_ = windowScript;
            } else {
                Debug.LogError("窗口[" + windowData._name + "] 创建失败，请检查预制路径或实例化是否成功。");
=======
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        UICamera_ = this.transform.FindChild("Camera").GetComponent<Camera>();
        _centerRoot = this.transform.FindChild("CenterRoot").gameObject;
    }

    public void ShowMainWindow<T>(eWindowsID windowId) where T : WindowsBasePanel
    {
        this.IsShowingWindow_ = true;
        if (this.CurShowingWindow_ != null)
        {
            if (this.CurShowingWindow_.WindowID == windowId)
            {
                this.IsShowingWindow_ = false;
                return;
            }
            else
            {
                CloseMainWindow(this.CurShowingWindow_.WindowID);
            }
        }
        if (this.DeletingWindowsDict_.ContainsKey(windowId))
        {
            this.CurShowingWindow_ = this.DeletingWindowsDict_[windowId];
            this.CurShowingWindow_.gameObject.SetActive(true);
            this.DeletingWindowsDict_.Remove(windowId);
        }
        else
        {
            T windowScript = AddChild<T>(_centerRoot);
            if (windowScript != null)
            {
                windowScript.gameObject.SetActive(true);
                windowScript.WindowID = windowId;
                this.CurShowingWindow_ = windowScript;
            }
            else
            {
                Debug.LogError("窗口[" + windowId.ToString() + "] 创建失败，请检查预制路径或实例化是否成功。");
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
            }
        }
        this.IsShowingWindow_ = false;
    }

<<<<<<< HEAD
    public void CloseMainWindow(eWindowsID windowId) {
        if (this.CurShowingWindow_ == null) {
            Debug.LogError("当前没有显示的窗口，严重bug!!!!!!!");
            return;
        }
        if (this.CurShowingWindow_.WindowData_._id != (int)windowId) {
            Debug.LogError("当前显示的窗口 [" + (eWindowsID)this.CurShowingWindow_.WindowData_._id + "] 与想要关闭的窗口 [" + windowId + "] 不一致。");
=======
    public void CloseMainWindow(eWindowsID windowId)
    {
        if (this.CurShowingWindow_ == null)
        {
            Debug.LogError("当前没有显示的窗口，严重bug!!!!!!!");
            return;
        }
        if (this.CurShowingWindow_.WindowID != windowId)
        {
            Debug.LogError("当前显示的窗口 [" + this.CurShowingWindow_.WindowID + "] 与想要关闭的窗口 [" + windowId + "] 不一致。");
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
            return;
        }
        this.CurShowingWindow_.CloseWindow();
        this.DeletingWindowsDict_.Add(windowId, this.CurShowingWindow_);
    }

    //prefabPath后期会改为资源列表中的路径
<<<<<<< HEAD
    public T AddItemToList<T>(string prefabPath, GameObject parentObj) {
=======
    public T AddItemToList<T>(string prefabPath, GameObject parentObj)
    {
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
        GameObject obj = ResourcesManager.Instance.GetUIPrefabs(prefabPath);
        obj.AddComponent(typeof(T));
        AddGameObject(parentObj, obj);
        return obj.GetComponent<T>();
    }

<<<<<<< HEAD
    private void Update() {
        if (!this.IsShowingWindow_) {
            foreach (eWindowsID id in this.DeletingWindowsDict_.Keys) {
                System.DateTime now = System.DateTime.Now;
                if ((now - this.DeletingWindowsDict_[id].CloseTime_).TotalMilliseconds > 10000) {
=======
    private void Update()
    {
        if (!this.IsShowingWindow_)
        {
            foreach (eWindowsID id in this.DeletingWindowsDict_.Keys)
            {
                System.DateTime now = System.DateTime.Now;
                if ((now - this.DeletingWindowsDict_[id].CloseTime_).TotalMilliseconds > 10000)
                {
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
                    GameObject obj = this.DeletingWindowsDict_[id].gameObject;
                    this.DeletingWindowsDict_.Remove(id);
                    DestroyImmediate(obj);
                    break;
                }
            }
        }
    }

<<<<<<< HEAD
    private static T AddChild<T>(GameObject parent) {
        string prefabName = typeof(T).Name;
        string prefabPath = ResourcesManager.Instance.GetResPath(prefabName);
        GameObject obj = ResourcesManager.Instance.GetUIPrefabs(prefabPath);
        if (obj != null) {
=======
    private static T AddChild<T>(GameObject parent)
    {
        string prefabName = typeof(T).Name;
        string prefabPath = ResourcesManager.Instance.GetResPath(prefabName);
        GameObject obj = ResourcesManager.Instance.GetUIPrefabs(prefabPath);
        if (obj != null)
        {
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
            obj.AddComponent(typeof(T));
            AddGameObject(parent, obj);
            return obj.GetComponent<T>();
        }
        return default(T);
    }

<<<<<<< HEAD
    private static void AddGameObject(GameObject parentObj, GameObject obj) {
=======
    private static void AddGameObject(GameObject parentObj, GameObject obj)
    {
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
        obj.transform.parent = parentObj.transform;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }

}
