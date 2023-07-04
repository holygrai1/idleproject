using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

/// <summary>
/// 窗口层次
/// </summary>
public enum CanvasLayer
{
    below,      // 手柄窗口层
    main,       // main窗口所在层
    normal,     // 普通窗口层 
    above,
    top,

    msgBox,     // 信息窗口层
    floatMsg,   // 飘字层
    wait,       // 等待窗口层

    max
}

/// <summary>
/// 窗口类型
/// </summary>
public enum WndType
{
    waitWnd,
    msgBoxYesWnd,
    loginWnd,
    mainWnd,
    testToolWnd,
    settingWnd,

    // battle相关
    killDragonBattleWnd,
    killDragonBattleResultWnd,

    battleActorInfoWnd,

    max
}

/// <summary>
/// 传递给窗口得信息类型
/// </summary>
public enum WndMsgType
{
    initContent,
    updateContent,
    showRank,

    max
}

/// <summary>
/// 飘字类型
/// </summary>
public enum FloatMessageType
{
    tips,
    warning,
    max
}

/// <summary>
/// 窗口配置信息
/// </summary>
[Serializable]
public class sWndAssetRef
{
    /// <summary>
    /// 窗口类型
    /// </summary>
    public WndType wndType;
    /// <summary>
    /// 窗口资源地址
    /// </summary>
    public string assetAddress;
};

/// <summary>
/// 窗口管理器
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static UIManager sInstance;
    public static UIManager Instance => sInstance;
    private void Awake()
    {
        if (sInstance != null && sInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sInstance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    #region Field
    private List<sWndAssetRef> mWndAssetRefs;
    private Dictionary<WndType, WndBase> mDicInstantiatedWnd;
    private Canvas mCurCanvas;
    private Camera mUICamera;
    private Dictionary<WndType, AsyncOperationHandle<GameObject>> mDicWndInstantiatingAsyncOP;
    private HashSet<WndType> mSetNeedHideWnds;
    private Dictionary<CanvasLayer, RectTransform> mDicLayerTrans;
    private bool mInited = false;
    private Queue<MsgBubble> mFreeMsgBubbles = new Queue<MsgBubble>();
    #endregion

    #region getter
    public Canvas CurCanvas { get => mCurCanvas; }
    public Camera UICamera { get => mUICamera; }
    public RectTransform GetLayer(CanvasLayer layer)
    {
        RectTransform rect = null;
        mDicLayerTrans.TryGetValue(layer, out rect);
        return rect;
    }
    public bool inited { get => mInited; }
    #endregion

    public async Task<bool> Init()
    {
        mDicInstantiatedWnd = new Dictionary<WndType, WndBase>();
        mWndAssetRefs = new List<sWndAssetRef>();
        mDicWndInstantiatingAsyncOP = new Dictionary<WndType, AsyncOperationHandle<GameObject>>();
        mSetNeedHideWnds = new HashSet<WndType>();

        LocaleManager.Instance.RegisterLocaleChangeEvent(OnLocaleChangeEventHandle, false);

        SetUILayer();

        AddWndAssetRef(WndType.waitWnd, "WaitWnd");
        AddWndAssetRef(WndType.msgBoxYesWnd, "MsgBoxYesWnd");
        AddWndAssetRef(WndType.loginWnd, "LoginWnd");
        AddWndAssetRef(WndType.mainWnd, "MainWnd");
        AddWndAssetRef(WndType.settingWnd, "SettingWnd");
        AddWndAssetRef(WndType.killDragonBattleWnd, "KillDragonBattleWnd");
        AddWndAssetRef(WndType.killDragonBattleResultWnd, "KillDragonBattleResultWnd");
        AddWndAssetRef(WndType.battleActorInfoWnd, "BattleActorInfoWnd");

        await SetUpWnd(WndType.waitWnd);
        await SetUpWnd(WndType.msgBoxYesWnd);
        await SetUpWnd(WndType.loginWnd);
        await SetUpWnd(WndType.mainWnd);
        await InitMsgBubbles();

        SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;

        mInited = true;

        return true;
    }

    async void RequestLateBindingAtlas(string tag, Action<SpriteAtlas> act)
    {
        using (var op = Addressables.LoadAssetAsync<SpriteAtlas>(tag).Task)
        {
            var asset = await op;
            act(asset);
        }
    }

    void SetUILayer()
    {
        mDicLayerTrans = new Dictionary<CanvasLayer, RectTransform>();
        float zPos = 0;
        for (int i = 0, max = (int)CanvasLayer.max; i < max; ++i)
        {
            CanvasLayer layer = (CanvasLayer)i;
            GameObject layerGO = new GameObject();
            layerGO.layer = LayerMask.NameToLayer("UI");
            layerGO.name = layer.ToString() + "Layer";
            var trans = layerGO.AddComponent<RectTransform>();
            mDicLayerTrans.Add(layer, trans);
            trans.Translate(new Vector3(0, 0, zPos));
            zPos -= 200;
        }
        mCurCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();

        mDicLayerTrans[CanvasLayer.floatMsg].gameObject.AddComponent<Canvas>();

        RefreshLayers();
    }

    async Task<bool> SetUpWnd(WndType wndType)
    {
        for (int i = 0, max = mWndAssetRefs.Count; i < max; ++i)
        {
            if (mWndAssetRefs[i].wndType == wndType)
            {
                string addresss = mWndAssetRefs[i].assetAddress;
                string addressLocale = LocaleManager.Instance.GetExistingLocaleAddress(addresss);
                var op = Addressables.InstantiateAsync(addressLocale);
                mDicWndInstantiatingAsyncOP.Add(wndType, op);
                var wndGO = await op.Task;
                mDicWndInstantiatingAsyncOP.Remove(wndType);

                var wnd = wndGO.GetComponent<WndBase>();
                if (wnd == null)
                {
                    LogManager.Error(wndGO.name + " has not wndBase subclass");
                }

                await wnd.Init(mWndAssetRefs[i]);
                var layerTrans = mDicLayerTrans[wnd.CurLayer];
                wnd.Trans.SetParent(layerTrans, false);
                mDicInstantiatedWnd.Add(wndType, wnd);
                wndGO.SetActive(false);
                return true;
            }
        }

        return false;
    }

    void RefreshLayers()
    {
        var canvasTrans = CurCanvas.transform;
        foreach (var kv in mDicLayerTrans)
        {
            var layerTrans = kv.Value;
            layerTrans.SetParent(canvasTrans, false);
            StretchInsideParent(layerTrans);
        }
    }

    void Update()
    {
        if (mUICamera == null)
        {
            mUICamera = GetComponentInChildren<Camera>();
        }
    }

    /// <summary>
    /// 添加窗口相关信息， 根据信息创建窗口
    /// </summary>
    /// <param name="wndType"></param>
    /// <param name="assetAddress"></param>
    public void AddWndAssetRef(WndType wndType, string assetAddress)
    {
        mWndAssetRefs.Add(new sWndAssetRef() { wndType = wndType, assetAddress = assetAddress });
    }

    /// <summary>
    /// 获取窗口信息
    /// </summary>
    /// <param name="wndType"></param>
    /// <returns></returns>
    public sWndAssetRef GetWndAssetRef(WndType wndType)
    {
        return mWndAssetRefs.Find((sWndAssetRef asset) =>
        {
            if (asset.wndType == wndType)
            {
                return true;
            }
            else
            {
                return false;
            }
        });
    }

    /// <summary>
    /// 扩展到父的大小
    /// </summary>
    /// <param name="childTrans"></param>
    public void StretchInsideParent(RectTransform childTrans)
    {
        childTrans.anchorMin = Vector2.zero;
        childTrans.anchorMax = Vector2.one;
        childTrans.offsetMin = Vector2.zero;
        childTrans.offsetMax = Vector2.zero;
        childTrans.ForceUpdateRectTransforms();
    }

    /// <summary>
    /// 显示等待窗口
    /// </summary>
    public void ShowWait()
    {
        var waitWnd = mDicInstantiatedWnd[WndType.waitWnd] as WaitWnd;
        waitWnd.OnShow();
    }
    /// <summary>
    /// 隐藏等待窗口
    /// </summary>
    public void HideWait()
    {
        HideWnd(WndType.waitWnd, false);
    }

    /// <summary>
    /// 设置用于显示窗口的画板
    /// </summary>
    /// <param name="canvas"></param>
    public void SetCanvas(Canvas canvas)
    {
        mCurCanvas = canvas;

        RefreshLayers();
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="wndType"></param>
    /// <param name="isNeedTransition"></param>
    /// <param name="bringToTop"></param>
    /// <param name="waitTransitionDone"></param>
    public async void ShowWnd(WndType wndType, bool isNeedTransition = true, bool bringToTop = true, bool waitTransitionDone = true)
    {
        await ShowWndAsync(wndType, isNeedTransition, bringToTop, waitTransitionDone);
    }

    public async Task<WndBase> ShowWndAsync(WndType wndType, bool isNeedTransition = true, bool bringToTop = true, bool waitTransitionDone = true)
    {
        if (mSetNeedHideWnds.Contains(wndType) == true)
        {
            mSetNeedHideWnds.Remove(wndType);
        }

        WndBase wnd = null;
        if (mDicInstantiatedWnd.TryGetValue(wndType, out wnd) == true)
        {
            if (bringToTop == true)
            {
                wnd.Trans.SetAsLastSibling();
            }

            wnd.OnShow(isNeedTransition);
            if (waitTransitionDone == true)
            {
                await WaitWndShowTransitionCompleted(wnd);
            }

            return wnd;
        }
        else if (mDicWndInstantiatingAsyncOP.ContainsKey(wndType) == true)
        {
            wnd = await WaitWndInstantiated(wndType);

            if (bringToTop == true)
            {
                wnd.Trans.SetAsLastSibling();
            }

            wnd.OnShow(isNeedTransition);
            if (waitTransitionDone == true)
            {
                await WaitWndShowTransitionCompleted(wnd);
            }

            return wnd;
        }
        else
        {
            for (int i = 0, max = mWndAssetRefs.Count; i < max; ++i)
            {
                if (mWndAssetRefs[i].wndType == wndType)
                {
                    string addresss = mWndAssetRefs[i].assetAddress;
                    string addressLocale = LocaleManager.Instance.GetExistingLocaleAddress(addresss);
                    var op = Addressables.InstantiateAsync(addressLocale);
                    mDicWndInstantiatingAsyncOP.Add(wndType, op);
                    var wndGO = await op.Task;
                    mDicWndInstantiatingAsyncOP.Remove(wndType);

                    wnd = wndGO.GetComponent<WndBase>();
                    if (wnd == null)
                    {
                        LogManager.Error(wndGO.name + " has not wndBase subclass");
                    }

                    await wnd.Init(mWndAssetRefs[i]);
                    var layerTrans = mDicLayerTrans[wnd.CurLayer];
                    wnd.Trans.SetParent(layerTrans, false);
                    mDicInstantiatedWnd.Add(wndType, wnd);

                    if (mSetNeedHideWnds.Contains(wndType) == false)
                    {
                        if (bringToTop == true)
                        {
                            wnd.Trans.SetAsLastSibling();
                        }

                        wnd.OnShow(isNeedTransition);
                        if (waitTransitionDone == true)
                        {
                            await WaitWndShowTransitionCompleted(wnd);
                        }
                    }
                    else
                    {
                        mSetNeedHideWnds.Remove(wndType);
                    }

                    return wnd;

                }
            }

            LogManager.Error(wndType.ToString() + " has not asset ref");
            return null;
        }
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="wndType"></param>
    /// <param name="isNeedTransition"></param>
    public void HideWnd(WndType wndType, bool isNeedTransition = true)
    {
        WndBase wnd = null;
        if (mDicInstantiatedWnd.TryGetValue(wndType, out wnd) == true)
        {
            wnd.OnHide(isNeedTransition);
            return;
        }

        if (mDicWndInstantiatingAsyncOP.ContainsKey(wndType) == true)
        {
            if (mSetNeedHideWnds.Contains(wndType) == false)
            {
                mSetNeedHideWnds.Add(wndType);
            }
        }
    }

    /// <summary>
    /// 删除窗口
    /// </summary>
    /// <param name="wndType"></param>
    public async void DestroyWnd(WndType wndType)
    {
        WndBase wnd = null;
        if (mDicInstantiatedWnd.TryGetValue(wndType, out wnd) == true)
        {
            Destroy(wnd.GO);
            mDicInstantiatedWnd.Remove(wndType);
            mSetNeedHideWnds.Remove(wndType);
        }
        else
        {
            if (mDicWndInstantiatingAsyncOP.ContainsKey(wndType) == true)
            {
                await WaitWndInstantiated(wndType);
                DestroyWnd(wndType);
            }
        }
    }

    /// <summary>
    /// 发送信息到制定窗口
    /// </summary>
    /// <param name="wndType"></param>
    /// <param name="msgType"></param>
    /// <param name="msgParams"></param>
    public async void SendMsg(WndType wndType, WndMsgType msgType, params object[] msgParams)
    {
        await SendMsgAsync(wndType, msgType, msgParams);
    }
    public async Task<bool> SendMsgAsync(WndType wndType, WndMsgType msgType, params object[] msgParams)
    {
        WndBase wnd = null;
        if (mDicInstantiatedWnd.TryGetValue(wndType, out wnd) == true)
        {
            wnd.OnMsg(msgType, msgParams);
            return true;
        }
        else if (mDicWndInstantiatingAsyncOP.ContainsKey(wndType) == false)
        {
            Debug.LogError(wndType.ToString() + " has not started instantiating");
            return false;
        }
        else
        {
            wnd = (WndBase)(await WaitWndInstantiated(wndType));
            wnd.OnMsg(msgType, msgParams);
            return true;
        }
    }

    async Task<WndBase> WaitWndInstantiated(WndType wndType)
    {
        while (mDicInstantiatedWnd.ContainsKey(wndType) == false)
        {
            await new WaitForUpdate();
        }

        var wnd = mDicInstantiatedWnd[wndType];
        return wnd;
    }

    async Task WaitWndShowTransitionCompleted(WndBase wnd)
    {
        while (wnd.IsShowTransitionCompleted == false)
        {
            await new WaitForUpdate();
        }
    }

    public void OnDestroy()
    {
        SpriteAtlasManager.atlasRequested -= RequestLateBindingAtlas;
        LocaleManager.Instance?.UnregisterLocaleChangeEvent(OnLocaleChangeEventHandle);
    }

    public WndBase GetWnd(WndType type)
    {
        WndBase wnd = null;
        mDicInstantiatedWnd.TryGetValue(type, out wnd);
        return wnd;
    }

    public async Task InitMsgBubbles()
    {
        for (int i = 0; i < 30; ++i)
        {
            var go = await AssetManager.Instantiate("MsgBubble", null);
            mFreeMsgBubbles.Enqueue(go.GetComponent<MsgBubble>());
        }
    }
    public MsgBubble GetMsgBubble(CanvasLayer locateAdd)
    {
        if (mFreeMsgBubbles.Count > 0)
        {
            var bubble = mFreeMsgBubbles.Dequeue();
            bubble.transform.SetParent(GetLayer(locateAdd), false);
            return bubble;
        }
        return null;
    }
    public void ReleaseMsgBubble(MsgBubble bubble)
    {
        bubble.gameObject.SetActive(false);
        mFreeMsgBubbles.Enqueue(bubble);
    }

    #region Test
    public async void TestShowWnd()
    {
        ShowWait();
        await ShowWndAsync(WndType.testToolWnd);
        HideWait();
        SendMsg(WndType.testToolWnd, WndMsgType.initContent);
    }
    public void TestDestroyWnd()
    {
        DestroyWnd(WndType.testToolWnd);
    }
    #endregion

    #region Localization Update
    private bool OnLocaleChangeEventHandle(string preLocale, string curLocale)
    {
        LogManager.Log("ui OnLocaleChangeEventHandle");

        // 某些窗口需要载入新资源，都在这里处理， 先把老的Destroy后再Create
        //if (mDicInstantiatedWnd.ContainsKey())
        //HideWnd(WndTypeConst.SettingWnd);
        //DestroyWnd(WndTypeConst.SettingWnd);
        //await ShowWndAsync(WndTypeConst.SettingWnd);

        foreach (var kv in mDicInstantiatedWnd)
        {
            kv.Value.OnLocaleUpdate(preLocale, curLocale);
        }

        return true;
    }
    #endregion
}
