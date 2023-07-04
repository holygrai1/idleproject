using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.U2D;
using UnityEngine.UI;
using static AssetManager;

public class Main : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static Main sInstance;
    public static Main Instance => sInstance;
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

    class LoginState
    {
        public static int newUser = 0;
        public static int oldUser = 1;
        public static int loginFailure = 2;
    }
    public delegate void ApplicatiQuitHandler();

    public static bool ApplicationIsQuitting = false;
    bool mInited = false;
    bool mPaused = false;
    ApplicatiQuitHandler mApplicationQuitHandler;

    async void Start()
    {
        Text loadTxt = this.transform.Find("Canvas/LoadPan/LoadImg/LoadTxt").GetComponent<Text>();
        Text loadPer = this.transform.Find("Canvas/LoadPan/LoadImg/LoadPer").GetComponent<Text>();
        Slider loadUI = this.transform.Find("Canvas/LoadPan/Slider").GetComponent<Slider>();

        loadTxt.text = "正在载入......";        
        SDKManager.Instance.PreInit();
        SDKManager.Instance.LogEvent("login_sideBar");

        Application.lowMemory += OnLowMemory;

        SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;
        SDKManager.Instance.LogEvent("login_title");

        SDKManager.Instance.LogEvent("login_bundle_init");
        await ScheduleManager.Instance.Init();

        loadUI.value = 15;
        loadPer.text = "15%";

        int downNum = 0, downOver = 0;
        await AssetManager.Instance.Init();
        await AssetManager.Instance.UpdateAssets((AssetManager.AssetUpdateStep step, int number) => {
            if (step == AssetUpdateStep.initialize)
            {
                //loadTxt.text = "??????????????......";
                loadUI.value = 20;
                loadPer.text = "20%";
            }
            else if (step == AssetUpdateStep.checkForCatalogUpdate)
            {
                //loadTxt.text = "????????????????......";
            }
            else if (step == AssetUpdateStep.UpdateCatalogs)
            {
                //loadTxt.text = "????????????????????......";
                loadUI.value = 30;
                loadPer.text = "30%";
            }
            else if (step == AssetUpdateStep.StartDownload)
            {
                downNum = number;
                downOver = downNum >= 20 ? 20 : downNum;
                //loadTxt.text = "????????????????????,??[1/" + downOver + "]......";
                loadUI.value = 40;
                loadPer.text = "40%";
            }
            else if (step == AssetUpdateStep.Download)
            {                
                int num = Convert.ToInt32(Convert.ToDouble(number) / Convert.ToDouble(downNum) * downOver);                
                //loadTxt.text = "????????????????????,??[" + (num < 1 ? 1 : num) + "/" + downOver + "]......";
                loadUI.value = 40 + num;
                loadPer.text = 40 + num + "%";
            }
        });

        //loadTxt.text = "????????????,????????????????......";
        loadUI.value = 60;
        loadPer.text = "60%";

        SDKManager.Instance.LogEvent("login_locale");
        if (await LocaleManager.Instance.Init() == false)
        {
            return;
        }

        loadUI.value = 70;
        loadPer.text = "70%";
        SDKManager.Instance.LogEvent("login_data");
        if (await DataManager.Instance.Init() == false)
        {
            return;
        }

        await Resources.UnloadUnusedAssets();

        Shader.WarmupAllShaders();

        loadUI.value = 75;
        loadPer.text = "75%";
        SDKManager.Instance.LogEvent("login_ui");
        if (await UIManager.Instance.Init() == false)
        {
            return;
        }

        loadUI.value = 80;
        loadPer.text = "80%";
        SDKManager.Instance.LogEvent("login_view");
        if (await ViewManager.Instance.Init() == false)
        {
            return;
        }


        loadUI.value = 85;
        loadPer.text = "85%";
        SDKManager.Instance.LogEvent("login_client");

        if (await ClientManager.Instance.Init() == false)
        {
            return;
        }

        loadUI.value = 90;
        loadPer.text = "90%";
        SDKManager.Instance.LogEvent("login_sdk");
        if (await SDKManager.Instance.Init() == false)
        {
            return;
        }

        loadUI.value = 95; 
        loadPer.text = "95%";
        SDKManager.Instance.LogEvent("login_preloadMusic");  

        loadUI.value = 100;        
        loadPer.text = "100%";

        this.gameObject.SetActive(false);

        // await ClientManager.Instance.Login("1", "");

        // test
//        ClientManager.Instance.TestData();
        UIManager.Instance.ShowWnd(WndType.loginWnd);
    }

    public void OnLowMemory()
    {
        var preUsedSize = Profiler.GetMonoUsedSizeLong();
        var preMemorySize = System.GC.GetTotalMemory(true);

        ViewManager.Instance.DestroyFreeCaches();
        GC.Collect();
        Resources.UnloadUnusedAssets();

        var nowMemorySize = System.GC.GetTotalMemory(true);
        var nowUsedSize = Profiler.GetMonoUsedSizeLong();

        Debug.Log("Size Now: " + preMemorySize + " after: " + nowMemorySize);
        Debug.Log("UsedSize Now: " + preUsedSize + " after: " + nowUsedSize);
    }


    public void AddApplicationQuitHandler(ApplicatiQuitHandler handler)
    {
        mApplicationQuitHandler += handler;
    }
    public void RemoveApplicationQuitHandler(ApplicatiQuitHandler handler)
    {
        mApplicationQuitHandler -= handler;
    }

    async void OnApplicationQuit()
    {
        Application.lowMemory -= OnLowMemory;

        mApplicationQuitHandler?.Invoke();
        mApplicationQuitHandler = null;
    }

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        Application.quitting += async () =>
        {
            Debug.Log("application quitting");

            ApplicationIsQuitting = true;

           await ClientManager.Instance.Logout();

            Debug.Log("logout done");
        };
    }

    async void RequestLateBindingAtlas(string tag, System.Action<SpriteAtlas> action)
    {
        using (var op = AssetManager.LoadAssetAsync<SpriteAtlas>(tag))
        {
            await op;
            action(op.Result);
        }
    }
}
