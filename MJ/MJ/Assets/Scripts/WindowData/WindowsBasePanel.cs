using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WindowsBasePanel : MonoBehaviour {

    public UIWindowsData WindowData_;
    public int Depth_ = 0;
    public System.DateTime CloseTime_;

    void Awake() {
        OnAwake();
    }

    void OnEnable() {
        OnInitWindow();
    }

    // Use this for initialization
    void Start() {
        OnStart();
    }

    // Update is called once per frame
    void Update() {
        OnUpdate();
    }

    void OnDisable() {

    }

    void OnDestroy() {

    }

    public virtual void OnAwake() {

    }

    //每次激活窗口时执行
    public virtual void OnInitWindow() {

    }

    //打开窗口时执行
    public virtual void OnStart() {

    }

    public virtual void OnUpdate() {

    }

    public void CloseWindow() {
        this.CloseTime_ = System.DateTime.Now;
        this.gameObject.SetActive(false);
    }




}
