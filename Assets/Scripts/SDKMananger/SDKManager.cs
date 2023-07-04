using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

public class SDKManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static SDKManager sInstance;
    public static SDKManager Instance => sInstance;
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

    private bool m_bInit = false;
    private bool m_bPreInit = false;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void iOSShowRewardAd();

    [DllImport("__Internal")]
    private static extern void iOSShowBanner();

    [DllImport("__Internal")]
    private static extern void iOSShowInterstitialAd();

    [DllImport("__Internal")]
    private static extern void iOSLogEvent(string jsonEvent);

#elif UNITY_ANDROID
    AndroidJavaClass mAndroidClass = null;
#endif



    public bool PreInit()
    {
        if (m_bPreInit)
        {
            return true;
        }

        m_bPreInit = true;
//#if UNITY_EDITOR
//#elif UNITY_IOS
//#elif UNITY_ANDROID
//        mAndroidClass = new AndroidJavaClass("com.tinyhorse.practice.NativeBridge");
//#endif

        return true;
    }

    public async Task<bool> Init()
    {
        if (m_bInit)
        {
            return true;
        }

        m_bInit = true;

        return true;
    }

    void OnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        JToken token = null;
        if ((token = json["type"]) == null)
        {
            return;
        }

        string type = (string)token;
        if (type == "showBannerDone")
        {
            if ((token = json["ret"]) == null)
            {
                return;
            }

            bool ret = (bool)token;
            mShowBannerRet = ret ? 0 : 1;
        }
        else if (type == "showInterstitialAdDone")
        {
            if ((token = json["ret"]) == null)
            {
                return;
            }

            bool ret = (bool)token;
            mShowInterstitialAdRet = ret ? 0 : 1;
        }
        else if (type == "initSDKFailed")
        {

        }
        else if (type == "showAd")
        {
            if ((token = json["ret"]) == null)
            {
                return;
            }

            bool ret = (bool)token;
            mShowRewardRet = ret ? 0 : 1;
        }
    }

#region 广告
    private int mShowRewardRet = -1;
    private string mRewarAdType = "";
    public async Task<bool> ShowRewardAd(string type)
    {
        mRewarAdType = type;
#if UNITY_EDITOR
        Debug.Log("playAd:"+type);
        return true;
//#elif UNITY_ANDROID
//        mShowRewardRet = -1;
//        mAndroidClass.CallStatic("ShowRewardAd");
//        while (mShowRewardRet == -1)
//        {
//            await new WaitForEndOfFrame();
//        }

//        return mShowRewardRet==0?true:false;

//#elif UNITY_IOS
//        mShowRewardRet = -1;
//        iOSShowRewardAd();
//        while (mShowRewardRet == -1)
//       {
//            await new WaitForEndOfFrame();
//        }

//        return mShowRewardRet==0?true:false;
#else

        return true;
#endif
    }

    public async void ShowRewardAdLua(string type, Action<bool> callback)
    {
        var ret = await ShowRewardAd(type);
        callback?.Invoke(ret);
    }

    // banner
    private int mShowBannerRet = -1;
    private string mBannerType = "";
    public async Task<bool> ShowBanner(string type)
    {
        mBannerType = type;
#if UNITY_EDITOR
        Debug.Log("showBanner:" + type);
        return true;
//#elif UNITY_ANDROID
//        mShowBannerRet = -1;
//        mAndroidClass.CallStatic("ShowBanner");
//        while (mShowBannerRet == -1)
//        {
//            await new WaitForEndOfFrame();
//        }

//        return mShowBannerRet==0?true:false;

//#elif UNITY_IOS
//        mShowBannerRet = -1;
//        iOSShowBanner();
//        while (mShowBannerRet == -1)
//        {
//            await new WaitForEndOfFrame();
//        }

//        return mShowBannerRet==0?true:false;
#else

        return true;
#endif
    }

    public async void ShowBannerLua(string type, Action<bool> callback)
    {
        var ret = await ShowBanner(type);
        callback?.Invoke(ret);
    }

    // 插屏广告
    private int mShowInterstitialAdRet = -1;
    private string mInterstitialAdType = "";
    public async Task<bool> ShowInterstitialAd(string type)
    {
        mInterstitialAdType = type;
#if UNITY_EDITOR
        Debug.Log("ShowInterstitialAd:" + type);
        return true;
//#elif UNITY_ANDROID
//        mShowInterstitialAdRet = -1;
//        mAndroidClass.CallStatic("ShowInterstitialAd");
//        while (mShowInterstitialAdRet == -1)
//        {
//            await new WaitForEndOfFrame();
//        }

//        return mShowInterstitialAdRet==0?true:false;

//#elif UNITY_IOS
//        mShowInterstitialAdRet = -1;
//        iOSShowInterstitialAd();
//        while (mShowInterstitialAdRet == -1)
//        {
//            await new WaitForEndOfFrame();
//        }

//        return mShowInterstitialAdRet==0?true:false;
#else
    return true;
#endif
    }

    public async void ShowInterstitialAdLua(string type, Action<bool> callback)
    {
        var ret = await ShowInterstitialAd(type);
        callback?.Invoke(ret);
    }
#endregion

#region 事件
    public void LogEvent(string evt){
//#if UNITY_EDITOR
//        return;
//#elif UNITY_ANDROID
//        JObject json = new JObject();
//        json["event"] = evt;
//        mAndroidClass.CallStatic("LogEvent", json.ToString());
//        return;
//#elif UNITY_IOS
//        JObject json = new JObject();
//        json["event"] = evt;
//        iOSLogEvent(json.ToString());

//        return;
//#endif
    }

    public void LogEventEx(string data)
    {
//#if UNITY_EDITOR
//        Debug.Log(data);
//        return;
//#elif UNITY_ANDROID
//        mAndroidClass.CallStatic("LogEvent", data);
//        return;
//#else
//        return;
//#endif
    }

#endregion

#region 杂类
    public string GetVersion()
    {
        return "0.1";

        //#if UNITY_EDITOR
        //        return "0.1";
        //#elif UNITY_ANDROID
        //        return mAndroidClass.CallStatic<string>("GetVersion");
        //#else
        //        return "";
        //#endif
    }

    public void Vibrate(int time)
    {
    //    Handheld.Vibrate();
        //#if UNITY_EDITOR
        //        //Debug.Log("vibrate:"+time);
        //#elif UNITY_ANDROID
        //        mAndroidClass.CallStatic("Vibrate", time);
        //#else

        //#endif

    }

    /// <summary>
    /// 手机粘贴功能
    /// </summary>
    /// <returns></returns>
    public string Paste()
    {
        string value = "";
#if UNITY_EDITOR
        //        //Debug.Log("vibrate:"+time);
#elif UNITY_ANDROID
               try
        {
            var javaClass = new AndroidJavaObject("com.tw.project.MainActivity");
            value = javaClass.Call<string>("onPaste");                        
        }
        catch (System.Exception ex)
        {            
        }
        //#else

#endif
        return value;
    }

    /// <summary>
    /// 手机复制功能
    /// </summary>
    /// <returns></returns>
    public string Copy(string val)
    {
        string value = "";
#if UNITY_EDITOR
        //        //Debug.Log("vibrate:"+time);
#elif UNITY_ANDROID
               try
        {
            var javaClass = new AndroidJavaObject("com.tw.project.MainActivity");
            value = javaClass.Call<string>("onCopy",val);                        
        }
        catch (System.Exception ex)
        {            
        }
        //#else

#endif
        return value;
    }

    #endregion

    #region 热云统计
    private const string androidClassName = "com.reyun.game.sdk.Tracking";
     
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport ("__Internal")]
    private static extern void _internalInitWithAppIdAndChannelId_GAME (string appId, string channelId);

    [DllImport ("__Internal")]
    private static extern void _internalSetRegister_GAME
     (string accountId, string accountType, int gender, string age, string serverId, string role);

    [DllImport ("__Internal")]
    private static extern void _internalSetLogin_GAME 
    (string accountId, int level, string serverId, string roleName, int gender, string age);

    [DllImport ("__Internal")]
    private static extern void _internalSetZF_GAME 
    (string tID, string zfType, string hbType, float hbAmount, 
    float virtualCoinAmount, string iapName, long iapAmount, int level,string campID);

    [DllImport ("__Internal")]
    private static extern void _internalSetEvent_GAME (string EventName,string dictJson);

    [DllImport ("__Internal")]
    private static extern void _internalSetEconomy_GAME 
    (string itemName, int itemAmount, float itemTotalPrice, int level, string campID);

    [DllImport ("__Internal")]
    private static extern void _internalSetQuest_GAME (string questId, int questStatu, string questType,int level);

    [DllImport ("__Internal")]
    private static extern string _internalGetDeviceId_GAME ();

    [DllImport ("__Internal")]
    private static extern void _internalSetPrintLog_GAME (bool printLog);
#endif


#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaObject getApplicationContext ()
    {
        using (AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer")) {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject> ("currentActivity")) {
                return jo.Call<AndroidJavaObject> ("getApplicationContext");
            }
        }
        return null;
    }
#endif

    /// <summary>
    /// 初始化方法   
    /// </summary>
    /// <param name="appId">appId</param>
    /// <param name="channelId">标识推广渠道的字符</param>
    public void Init(string appId, string channelId)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _internalInitWithAppIdAndChannelId_GAME(appId,channelId);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass gameAnalysys = new AndroidJavaClass (androidClassName)) {
            gameAnalysys.CallStatic ("initWithKeyAndChannelId", getApplicationContext (), appId, channelId);
        }
#else
        //PC and other
#endif
    }


    public enum Gender
    {
        m = 0,//Male 男性
        f = 1,//female 女性
        o = 2//其他
    }

    public enum QuestStatus
    {
        a,//开始
        c,//结束
        f //失败
    }

    /// <summary>
    /// 玩家服务器注册
    /// </summary>
    /// <param name="accountId">账号</param>
    /// <param name="gender">玩家性别</param>
    /// <param name="age">年龄</param>
    /// <param name="serverId">玩家登陆的区服</param>
    /// <param name="accountType">账号的类型</param>
    /// <param name="role">人物的名称</param>
    public void Register(string accountId, string accountType, Gender gender, string age, string serverId, string role)
    {
//#if UNITY_IOS && !UNITY_EDITOR
//        _internalSetRegister_GAME(accountId, accountType, (int)gender, age, serverId, role);
//#elif UNITY_ANDROID && !UNITY_EDITOR
//        using (AndroidJavaClass gameAnalysys = new AndroidJavaClass(androidClassName))
//        {
//            string strGender = System.Enum.GetName(typeof(Gender), gender);
//            gameAnalysys.CallStatic("setNRegisterWithAccountID", accountId, accountType, strGender, age, serverId, role);
//        }
//#else
//        //PC and other
//#endif
    }

    /// <summary>
    /// 玩家的账号登陆服务器
    /// </summary>
    /// <param name="accountId">账号</param>
    /// <param name="level">玩家等级</param>
    /// <param name="serverId">登陆的区服</param>
    /// <param name="roleName">玩家名称</param>
    /// <param name="gender">性别</param>
    /// <param name="age">年龄</param>
    public void Login(string accountId, int level, string serverId, string roleName, Gender gender, string age)
    {
//#if UNITY_IOS && !UNITY_EDITOR
//        _internalSetLogin_GAME(accountId, level, serverId, roleName,(int)gender, age);
//#elif UNITY_ANDROID && !UNITY_EDITOR
//        using (AndroidJavaClass gameAnalysys = new AndroidJavaClass(androidClassName))
//        {
//            string strGender = System.Enum.GetName(typeof(Gender), gender);
//            gameAnalysys.CallStatic("setNLoginWithAccountID", accountId, level, serverId, roleName, strGender, age);
//        }
//#else
//        //PC and other
//#endif
    }

    /// <summary>
    /// 玩家充值数据
    /// </summary>
    /// <param name="tID">交易的流水号</param>
    /// <param name="zfType">支付类型</param>
    /// <param name="hbType">货币类型</param>
    /// <param name="hbAmount">支付的真实货币的金额</param>
    /// <param name="virtualCoinAmount">通过充值获得的游戏内货币的数量</param>
    /// <param name="iapName">游戏内购买道具的名称</param>
    /// <param name="iapAmount">游戏内购买道具的数量</param>
    /// <param name="level">玩家角色等级</param>
    /// <param name="campID">活动ID</param>
    public void SetZF(string tID, string zfType, string hbType, float hbAmount,
    float virtualCoinAmount, string iapName, long iapAmount, int level, string campID)
    {
//#if UNITY_IOS && !UNITY_EDITOR
//        _internalSetZF_GAME(tID, zfType, hbType, hbAmount, virtualCoinAmount, iapName, iapAmount, level, campID);
//#elif UNITY_ANDROID && !UNITY_EDITOR
//        using (AndroidJavaClass andClass = new AndroidJavaClass(androidClassName))
//        {
//            andClass.CallStatic("setPayment", tID, zfType, hbType, hbAmount, virtualCoinAmount, iapName, iapAmount,level);
//        }
//#else
//        //PC and other
//#endif

    }

    /// <summary>
    /// 游戏内的虚拟交易数据
    /// </summary>
    /// <param name="itemName">游戏内虚拟物品的名称/ID</param>
    /// <param name="itemAmount">交易的数量</param>
    /// <param name="itemTotalPrice">交易的总价</param>
    /// <param name="level">用户等级</param>
    /// <param name="campID">活动 ID</param>
    public void SetEconomy(string itemName, int itemAmount, float itemTotalPrice, int level, string campID)
    {
//#if UNITY_IOS && !UNITY_EDITOR
//        _internalSetEconomy_GAME(itemName, itemAmount, itemTotalPrice, level, campID);
//#elif UNITY_ANDROID && !UNITY_EDITOR
//        using (AndroidJavaClass andClass = new AndroidJavaClass(androidClassName))
//        {
//            andClass.CallStatic("setEconomy", itemName, itemAmount, itemTotalPrice, level);
//        }
//#else
//        //PC and other
//#endif
    }

    /// <summary>
    /// 玩家的任务、副本数据
    /// </summary>
    /// <param name="questId">当前任务/关卡/副本的编号或名称</param>
    /// <param name="questStatu">当前任务/关卡/副本的状态</param>
    /// <param name="questType">当前任务/关卡/副本的类型</param>
    public void SetQuest(string questId, QuestStatus questStatu, string questType, int level)
    {
//#if UNITY_IOS && !UNITY_EDITOR
//        _internalSetQuest_GAME(questId, (int)questStatu, questType, level);
//#elif UNITY_ANDROID && !UNITY_EDITOR
//        using (AndroidJavaClass andClass = new AndroidJavaClass(androidClassName))
//        {
//            string strQuest = System.Enum.GetName(typeof(QuestStatus), questStatu);
//            andClass.CallStatic("setNQuest", questId, strQuest, questType,level);
//        }
//#else
//        //PC and other
//#endif
    }

    /// <summary>
    /// 统计玩家的自定义事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="dict"></param>
    public void SetEvent(string eventName, Dictionary<string, string> dict)
    {
//#if UNITY_IOS && !UNITY_EDITOR
//        if (dict == null || dict.Count == 0)
//        {
//            _internalSetEvent_GAME(eventName, "{}");
//        }
//        else
//        {
//            int nLength = dict.Count;
//            List<string> dicKey = new List<string>(dict.Keys);
//            List<string> dicValue = new List<string>(dict.Values);
//            string json = "{";
//            for (int i = 0; i < nLength; i++)
//            {
//                string subKeyValue = "\"" + dicKey[i] + "\"" + ":" + "\"" + dicValue[i] + "\"";
//                json += subKeyValue;
//                if (i != nLength - 1)
//                {
//                    json += ",";
//                }
//            }
//            json += "}";
//            _internalSetEvent_GAME(eventName, json);
//        }
//#elif UNITY_ANDROID && !UNITY_EDITOR
//        try
//        {
//            if (dict == null)
//            {
//                using (AndroidJavaClass gameAnalysys = new AndroidJavaClass(androidClassName))
//                {
//                    gameAnalysys.CallStatic("setEvent", eventName, null);
//                }
//            }
//            else
//            {
//                using (AndroidJavaClass gameAnalysys = new AndroidJavaClass(androidClassName))
//                {
//                    using (AndroidJavaObject obj_HashMap = new AndroidJavaObject("java.util.HashMap"))
//                    {
//                        System.IntPtr method_Put = AndroidJNIHelper.GetMethodID(obj_HashMap.GetRawClass(), "put",
//                                                                                "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
//                        object[] args = new object[2];
//                        foreach (KeyValuePair<string, string> kvp in dict)
//                        {
//                            using (AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key))
//                            {
//                                using (AndroidJavaObject v = new AndroidJavaObject("java.lang.String", kvp.Value))
//                                {
//                                    args[0] = k;
//                                    args[1] = v;
//                                    AndroidJNI.CallObjectMethod(obj_HashMap.GetRawObject(),
//                                                                method_Put, AndroidJNIHelper.CreateJNIArgArray(args));

//                                }
//                            }
//                        }
//                        gameAnalysys.CallStatic("setEvent", eventName, obj_HashMap);
//                    }
//                }
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogError("SetEvent exception:"+e.Message);
//        }
//#else
//        //PC and other
//#endif
    }

    /// <summary>
    /// 获取用户的设备ID信息
    /// </summary>
    public string GetDeviceId()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _internalGetDeviceId_GAME();
#elif UNITY_ANDROID && !UNITY_EDITOR
        string str = "unknown";

        using (AndroidJavaClass gameAnalysys = new AndroidJavaClass (androidClassName)) {
            str = gameAnalysys.CallStatic<string> ("getDeviceId");
        }
        return str;
#else
        //PC and other
        return "unknown";
#endif
    }

    /// <summary>
    /// 开启日志打印 
    /// 传入 true 表示开启  false表示关闭 
    /// 上线前请将该参数置为false  开发阶段可设置true方便调试
    /// </summary>
    public void SetPrintLog(bool print)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _internalSetPrintLog_GAME(print);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass gameAnalysys = new AndroidJavaClass (androidClassName)) {
            gameAnalysys.CallStatic ("setDebugMode",print);
        }
#else
        //PC and other
#endif
    }
    #endregion
}
