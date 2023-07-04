using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 客户端网络数据管理器
/// </summary>
public class ClientManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static ClientManager sInstance;
    public static ClientManager Instance => sInstance;
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

    #region 属性
    // 服务器地址
    private string mServerAddress = "";

    private bool mInited = false;
    private bool mLogined = false;
    private bool mLogining = false;
    private int mRetCode = 0;
    private string mErrorMsg = "";
    private string mKey = "";

    // 主播数据
    private SelfUserData mSelfUserData = new SelfUserData();
    // 当前观众数据
    private List<UserData> mUserDatas = new List<UserData>();
    // 屠龙战斗数据
    private KillDragonBattleData mKillDragonBattleData = new KillDragonBattleData();

    // 心跳数据
    private int mBeatHeartDelay = 1;
    #endregion

    #region getter
    public bool HasLogined => mLogined;
    public bool Logining => mLogining;
    public SelfUserData SelfUserData => mSelfUserData;
    public UserData GetUserData(string id)
    {
        foreach (var userData in mUserDatas)
        {
            if (userData.id == id)
            {
                return userData;
            }
        }

        return null;
    }
    public List<UserData> AllUserDatas => mUserDatas;
    public int ErrorCode => mRetCode;
    public string ServerAddress => mServerAddress;
    public KillDragonBattleData KillDragonBattleData => mKillDragonBattleData;
    public string ErrorMsg => mErrorMsg;
    #endregion

    #region 方法
    public async Task<bool> Init()
    {
        if (mInited)
        {
            return true;
        }

        try
        {
            using (var op = AssetManager.LoadAssetAsync<TextAsset>("ServerConfig"))
            {
                await op;
                JObject json = JObject.Parse(op.Result.text);

                mServerAddress = (string)json["serverURL"];

                mInited = true;

                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ClientMananger create fail: " + e.Message);
            return false;
        }
    }

    private void OnLogin()
    {
        mLogined = true;
        ScheduleManager.Instance.On(mBeatHeartDelay, OnBeatHeartTimer);
    }

    private void OnLogout()
    {
        mLogined = false;
        ScheduleManager.Instance.Off(OnBeatHeartTimer);
    }

    private async void ParseEvent(JToken json)
    {
        if (json == null)
        {
            return;
        }

        JArray contentArray = null;
        if ((contentArray = (JArray)json["content"]) != null)
        {
            foreach (var contentJson in contentArray)
            {
                JToken token = null;
                if ((token = contentJson["nextCursor"]) != null)
                {
                    mSelfUserData.SetNextCursor((int)token);
                }

                if ((token = contentJson["id"]) != null)
                {
                    string userID = (string)token;
                    if (string.IsNullOrEmpty(userID) == true)
                    {
                        continue;
                    }

                    if ((token = contentJson["actionType"]) != null)
                    {
                        UserActionType actionType = (UserActionType)(int)token;
                        if (actionType == UserActionType.leave || actionType == UserActionType.enter)
                        {
                            continue;
                        }

                        UserData userData = ClientManager.Instance.GetUserData(userID);
                        if (userData == null)
                        {
                            // 新用户
                            userData = new UserData();
                            if (userData.Init(contentJson) == false)
                            {
                                continue;
                            }

                            mUserDatas.Add(userData);
                            EventManager.Instance.DispatchUserEnterEvent(userData);

                           // Debug.Log("新用户");
                        }
                        else
                        {
                            userData.UpdateData(contentJson);
                        }

                        if (actionType == UserActionType.comment)
                        {
                            // 用户评论
                            string commnet = (string)contentJson["comment"];
                            EventManager.Instance.DispatchUserCommentEvent(userData, commnet);

                          //  Debug.Log(userData.name + ": 评论");

                        }
                        else if (actionType == UserActionType.like)
                        {
                            // 用户点赞
                            EventManager.Instance.DispatchUserLikeEvent(userData);

                            //Debug.Log(userData.name + ": 点赞");
                        }
                        else if (actionType == UserActionType.prize)
                        {
                            // 用户送礼物
                            EventManager.Instance.DispatchUserPrizeEvent(userData);

//                            Debug.Log(userData.name + ": 送礼");
                        }
                    }
                }
            }
        }
    }

    private float mKillDragonBattleCallCD = 10;
    private async void OnBeatHeartTimer(float detal, float unscaleDeltaTime)
    {
        mKillDragonBattleCallCD -= detal;

        await BeatHeart();

        if (mKillDragonBattleCallCD <= 0)
        {
            StartKillDragonBattle();
            mKillDragonBattleCallCD = 10;
        }
    }

    private bool InitSelfUserData(JToken json)
    {
        return mSelfUserData.Init(json);
    }

    private bool InitUserDatas(JToken json)
    {
        JToken userDataArray = null;
        if ((userDataArray = json["userDatas"]) != null)
        {
            bool failed = false;
            foreach (JToken userToken in userDataArray)
            {
                var userData = new UserData();
                if (false == userData.Init(userToken))
                {
                    failed = true;
                }
                else
                {
                    var existingUserData = GetUserData(userData.id);
                    if (existingUserData != null)
                    {
                        existingUserData.UpdateData(userToken);
                    }
                    else
                    {
                        mUserDatas.Add(userData);
                    }
                }
            }

            if (failed == true) return false;
        }

        return true;
    }

    // 通用返回分析
    JToken ParseRet(string data)
    {
        try
        {
            JObject ret = JObject.Parse(data);
            mRetCode = (int)ret["statusCode"];
            mErrorMsg = (string)ret["message"];
            if (mRetCode != 200)
            {
                if (mRetCode == -99)
                {
                    // 该用户没有查询到区服信息
                }

                return null;
            }



            return ret;

        }
        catch (Exception e)
        {
        //    Debug.LogError("parse ret fail:" + e.Message);
            return null;
        }
    }
    #endregion

    #region 服务器请求


    // 请求发送测试
    public async Task<bool> TestTools(int type, params string[] param)
    {
        if (mInited == false || mLogined == false)
        {
            return false;
        }

        string url = mServerAddress + "gm/testTools";

        JObject data = new JObject();
        data.Add("key", JToken.FromObject(mKey));
        data.Add("type", JToken.FromObject(type));
        for (int i = 0; i < param.Length; ++i)
        {
            data.Add("param" + (i + 1), JToken.FromObject(param[i]));
        }

        var www = Helpers.Post(url, data.ToString());
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return false;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return false;
            }

            return true;
        }
    }

    // 登陆请求, liveId就是userID
    public async Task<bool> Login(string liveId, string anchorID = "")
    {
        if (mInited == false || mLogined == true)
        {
            return false;
        }

        mLogining = true;

        string url = mServerAddress + "live/game/user/login";

        WWWForm form = new WWWForm();
        form.AddField("liveId", liveId);

        if (string.IsNullOrEmpty(anchorID) == false)
        {
            form.AddField("anchorId", anchorID);
        }

        string timeStamp = Helpers.GetTimeStamp().ToString();
        form.AddField("timestamp", timeStamp);

        string sign = Helpers.GetSign(anchorID, liveId, timeStamp);
        form.AddField("sign", sign);

        var www = Helpers.PostForm(url, form);
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                mLogining = false;
                return false;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                mLogining = false;
                return false;
            }

            JToken content = json["content"];
            if (false == InitSelfUserData(content))
            {
                mLogining = false;
                return false;
            }

            PlayerPrefs.SetString("roomID", mSelfUserData.RoomID);
                
            if (false == InitUserDatas(content))
            {
                mLogining = false;

                return false;
            }

            OnLogin();
            mLogining = false;

            return true;
        }
    }

    // 登出请求
    public async Task<bool> Logout()
    {
        string roomID = "";
        if (PlayerPrefs.HasKey("roomID") == true)
        {
            roomID = PlayerPrefs.GetString("roomID");
        }
        else
        {
            return false;
        }

        string url = mServerAddress + "live/game/user/logout";

        WWWForm form = new WWWForm();
        form.AddField("roomId", roomID);
        string timeStamp = Helpers.GetTimeStamp().ToString();
        form.AddField("timestamp", timeStamp);
        string sign = Helpers.GetSign(roomID, timeStamp);
        form.AddField("sign", sign);
        var www = Helpers.PostForm(url, form);
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return false;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return false;
            }

            mLogined = false;

            PlayerPrefs.DeleteKey("roomID");

            return true;
        }
    }

    // 发送心跳包
    public async Task<JToken> BeatHeart()
    {
        if (mInited == false || mLogined == false)
        {
            return null;
        }

        string url = mServerAddress + "live/game/user/beat/heart";

        WWWForm form = new WWWForm();
        string cursor = mSelfUserData.NextCursor.ToString();
        form.AddField("cursor", cursor);
        form.AddField("roomId", mSelfUserData.RoomID);
        string timeStamp = Helpers.GetTimeStamp().ToString();
        form.AddField("timestamp", timeStamp);
        string sign = Helpers.GetSign(cursor, mSelfUserData.RoomID, timeStamp);
        form.AddField("sign", sign);
        var www = Helpers.PostForm(url, form);
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return null;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return null;
            }

            ParseEvent(json);

            return json;
        }
    }

    // 开始屠龙战斗请求
    public async Task<bool> StartKillDragonBattle()
    {
        if (mInited == false || mLogined == false)
        {
            return false;
        }

        string url = mServerAddress + "live/game/user/start/battle";
        WWWForm form = new WWWForm();
        byte livePlatform = 1;
        form.AddField("livePlatform", livePlatform);
        var liveID = PlayerPrefs.GetString("id");
        form.AddField("liveId", liveID);
        string timeStamp = Helpers.GetTimeStamp().ToString();
        form.AddField("timestamp", timeStamp);
        string sign = Helpers.GetSign(liveID, livePlatform.ToString(), timeStamp);
        form.AddField("sign", sign);
        var www = Helpers.PostForm(url, form);

        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return false;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return false;
            }

            // 解析出屠龙战斗数据
            mKillDragonBattleData.Init(json);

            return true;
        }
    }
    
    // 完成战斗
    public async Task<bool> FinishBattle()
    {
        if (mInited == false || mLogined == false)
        {
            return false;
        }

        string url = mServerAddress + "live/game/user/finish/battle";

        JObject data = new JObject();
        byte livePlatform = 1;
        data.Add("livePlatform", JToken.FromObject(livePlatform));
        data.Add("anchorId", JToken.FromObject(mSelfUserData.ID));
        data.Add("killDragonUserId", JToken.FromObject(mSelfUserData.KillDragonUserID));
        data.Add("killDragonUserName", JToken.FromObject(mSelfUserData.KillDragonUserName));
        data.Add("killDragonUserHeadPic", JToken.FromObject(mSelfUserData.KillDragonUserHeadUrl));
        data.Add("killDragonTreasureNum", JToken.FromObject(mSelfUserData.KillDragonTreasureNum));
        data.Add("exp", JToken.FromObject(mSelfUserData.KillDragonUserExp));
        data.Add("dmg", JToken.FromObject(mSelfUserData.KillDragonUserDmg));
        data.Add("killDragonNum", JToken.FromObject(mSelfUserData.KillDragonUserKillDragonNum));
        data.Add("joinDragonNum", JToken.FromObject(mSelfUserData.KillDragonUserJoinDragonNum));
        string timeStamp = Helpers.GetTimeStamp().ToString();
        data.Add("timestamp", JToken.FromObject(timeStamp));
        JToken jUserDatas = JToken.FromObject(mUserDatas);
        data.Add("userDatas", jUserDatas);

        List<KeyValuePair<string, string>> keyvalues = new List<KeyValuePair<string, string>>();
        keyvalues.Add(new KeyValuePair<string, string>("livePlatform", livePlatform.ToString()));
        keyvalues.Add(new KeyValuePair<string, string>("anchorId", mSelfUserData.ID));
        keyvalues.Add(new KeyValuePair<string, string>("killDragonUserId", mSelfUserData.KillDragonUserID));
        keyvalues.Add(new KeyValuePair<string, string>("killDragonUserName", mSelfUserData.KillDragonUserName));
        keyvalues.Add(new KeyValuePair<string, string>("killDragonUserHeadPic", mSelfUserData.KillDragonUserHeadUrl));
        keyvalues.Add(new KeyValuePair<string, string>("killDragonTreasureNum", mSelfUserData.KillDragonTreasureNum.ToString()));
        keyvalues.Add(new KeyValuePair<string, string>("exp", mSelfUserData.KillDragonUserExp.ToString()));
        keyvalues.Add(new KeyValuePair<string, string>("dmg", mSelfUserData.KillDragonUserDmg.ToString()));
        keyvalues.Add(new KeyValuePair<string, string>("killDragonNum", mSelfUserData.KillDragonUserKillDragonNum.ToString()));
        keyvalues.Add(new KeyValuePair<string, string>("joinDragonNum", mSelfUserData.KillDragonUserJoinDragonNum.ToString()));
        keyvalues.Add(new KeyValuePair<string, string>("timestamp", timeStamp.ToString()));
       // keyvalues.Add(new KeyValuePair<string, string>("userDatas", jUserDatas.ToString()));
        string sign = Helpers.GetSign(keyvalues);
        data.Add("sign", JToken.FromObject(sign));

   //     Debug.Log("sign: " + JToken.FromObject(sign));
   //     Debug.Log(data.ToString());

        PlayerPrefs.SetString("data", data.ToString());

        var www = Helpers.Post(url, data.ToString());
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return false;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return false;
            }

            return true;
        }
    }

    // 发送获取用户数据请求
    public async Task<UserData> GetUserDataAysnc(string userID)
    {
        if (mInited == false || mLogined == false)
        {
            return null;
        }

        string url = mServerAddress + "user/getUserData";
        JObject param = new JObject();
        param.Add("key", JToken.FromObject(mKey));
        param.Add("id", JToken.FromObject(mSelfUserData.ID));
        param.Add("userID", JToken.FromObject(userID));

        var www = Helpers.Post(url, param.ToString());
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return null;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return null;
            }

            var userData = new UserData();
            if (userData.Init(json) == false)
            {
                return null;
            }

            return userData;
        }
    }

    // 发送获取排行请求
    // 登陆请求, liveId就是userID, rankType: 1为等级, 2为伤害排行
    public async Task<List<UserData>> GetRank(string anchorID, int rankType)
    {
        if (mInited == false || mLogined == false)
        {
            return null;
        }
        string url = mServerAddress + "live/game/user/top/rank";

        WWWForm form = new WWWForm();
        byte livePlatform = 1;
        form.AddField("livePlatform", livePlatform);
        form.AddField("anchorId", anchorID);
        form.AddField("rankType", rankType);
        string timeStamp = Helpers.GetTimeStamp().ToString();
        form.AddField("timestamp", timeStamp);
        string sign = Helpers.GetSign(anchorID, livePlatform.ToString(), rankType.ToString(), timeStamp);
        form.AddField("sign", sign);

        var www = Helpers.PostForm(url, form);
        using (www)
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return null;
            }

            JToken json = ParseRet(www.downloadHandler.text);
            if (json == null)
            {
                return null;
            }

            List<UserData> userDatas = new List<UserData>();
            JArray contentArray = null;
            JToken jToken = null;
            if ((jToken = json["content"]) != null)
            {
                contentArray = jToken.ToObject<JArray>();
                if (contentArray != null)
                {
                    foreach (var contentJson in contentArray)
                    {
                        JToken token = null;
                        UserData userData = null;
                        if ((token = contentJson["id"]) != null)
                        {
                            string userID = (string)token;
                            if (string.IsNullOrEmpty(userID) == true)
                            {
                                continue;
                            }
                            userData = new UserData();
                            userData.InitRank(contentJson);
                            userDatas.Add(userData);
                        }
                    }
                }
            }

            return userDatas;
        }
    }

    #endregion
}
