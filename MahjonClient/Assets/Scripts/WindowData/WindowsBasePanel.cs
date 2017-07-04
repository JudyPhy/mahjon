using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WindowsBasePanel : MonoBehaviour
{

    public eWindowsID WindowID;
    public int Depth_ = 0;
    public System.DateTime CloseTime_;

    void Awake()
    {
        OnAwake();
        OnRegisterEvent();
    }

    void OnEnable()
    {
        OnEnableWindow();
    }

    // Use this for initialization
    void Start()
    {
        OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        OnUpdate();
    }

    void OnDisable()
    {
        
    }

    void OnDestroy()
    {
        OnRemoveEvent();
    }

    public virtual void OnAwake()
    {
        
    }

    //每次激活窗口时执行
    public virtual void OnEnableWindow()
    {

    }

    //打开窗口时执行
    public virtual void OnStart()
    {

    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnRegisterEvent()
    {

    }

    public virtual void OnRemoveEvent()
    {

    }

    public void CloseWindow()
    {
        this.CloseTime_ = System.DateTime.Now;
        this.gameObject.SetActive(false);
    }

}
