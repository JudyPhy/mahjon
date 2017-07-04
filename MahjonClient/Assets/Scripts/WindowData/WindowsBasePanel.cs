using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

<<<<<<< HEAD
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
=======
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
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
        OnStart();
    }

    // Update is called once per frame
<<<<<<< HEAD
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
=======
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
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50

    }

    //打开窗口时执行
<<<<<<< HEAD
    public virtual void OnStart() {

    }

    public virtual void OnUpdate() {

    }

    public void CloseWindow() {
        this.CloseTime_ = System.DateTime.Now;
        this.gameObject.SetActive(false);
    }



=======
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
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50

}
