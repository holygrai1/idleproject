using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

// 玩家数据
public class UserData
{
    // id
    private string mID = "";
    // 名字
    private string mName = "";
    // 头像url
    private string mHeadPicUrl = "";
    // 经验
    private int mExp = 0;
    // 累积伤害
    private int mDmg = 0;
    // 屠龙胜利次数
    private int mKillDragonNum = 0;
    // 屠龙参与次数
    private int mJoinDragonNum = 0;
    // 下标索引值
    private int mNextCursor;
    // 评论内容
    private string mComment= "";

    // 用户礼物信息
    public class GiftInfoData
    {
        private string mGiftId;
        private string mGiftName;
        private string mGiftUrl;
        private int mGiftCount;
        private int mShakeMoney;
        private int mTotalShakeMoney;

        public string giftId => mGiftId;
        public string giftName => mGiftName;
        public string giftUrl => mGiftUrl;
        public int giftCount => mGiftCount;
        public int shakeMoney => mShakeMoney;
        public int totalShakeMoney => mTotalShakeMoney;

        public void Init(JToken json)
        {
            JToken token = null;
            if ((token = json["giftId"]) != null)
            {
                mGiftId = (string)token;
            }
            if ((token = json["giftName"]) != null)
            {
                mGiftName = (string)token;
            }
            if ((token = json["giftUrl"]) != null)
            {
                mGiftUrl = (string)token;
            }
            if ((token = json["giftCount"]) != null)
            {
                mGiftCount = (int)token;
            }
            if ((token = json["shakeMoney"]) != null)
            {
                mShakeMoney = (int)token;
            }
            if ((token = json["totalShakeMoney"]) != null)
            {
                mTotalShakeMoney = (int)token;
            }
        }

        public void Test(UserData data)
        {
            mGiftId = "1";
            mGiftName = "name1";
            mGiftUrl = data.mHeadPicUrl;
            mGiftCount = 5;
            mShakeMoney = 1;
            mTotalShakeMoney = 10;
        }
    }
    private GiftInfoData mGiftInfoData = new GiftInfoData();

    #region getter
    public string id => mID;
    public string name => mName;
    public string headPic => mHeadPicUrl;
    public int exp => mExp;
    public int dmg => mDmg;
    public int killDragonNum => mKillDragonNum;
    public int joinDragonNum => mJoinDragonNum;
    [JsonIgnore]
    public int NextCursor => mNextCursor;
    [JsonIgnore]
    public string Comment => mComment;
    [JsonIgnore]
    public GiftInfoData giftInfoData => mGiftInfoData;
    #endregion

    #region 方法
    /// <summary>
    /// 初始化属性
    /// </summary>
    /// <param name="obj">字符串或者是JObject</param>
    /// <returns></returns>
    public bool Init(JToken json)
    {
        //try
        //{
            mID = (string)json["id"];
            JToken token = null;
            if ((token = json["name"]) != null)
            {
                mName = (string)token;
            }
            if ((token = json["headPic"]) != null)
            {
                mHeadPicUrl = (string)token;
            }
            if ((token = json["exp"]) != null)
            {
                mExp = (int)token;
            }
            if ((token = json["dmg"]) != null)
            {
                mDmg = (int)token;
            }
            if ((token = json["killDragonNum"]) != null)
            {
                mKillDragonNum = (int)token;
            }
            if ((token = json["joinDragonNum"]) != null)
            {
                mJoinDragonNum = (int)token;
            }
            if ((token = json["nextCursor"]) != null)
            {
                mNextCursor = (int)token;
            }

            if ((token = json["comment"]) != null && token.HasValues == true)
            {
                mComment = (string)token;
            }

            if ((token = json["giftInfoData"]) != null && token.HasValues == true)
            {
                mGiftInfoData.Init(token);
            }
        //}
        //catch (Exception)
        //{
        //    return false;
        //}

        return true;
    }

    /// <summary>
    /// 排行时用到的用户数据初始化
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public bool InitRank(JToken json)
    {
        try
        {
            mID = (string)json["id"];
            JToken token = null;
            if ((token = json["name"]) != null)
            {
                mName = (string)token;
            }
            if ((token = json["headPic"]) != null)
            {
                mHeadPicUrl = (string)token;
            }
            if ((token = json["exp"]) != null)
            {
                mExp = (int)token;
            }
            if ((token = json["dmg"]) != null)
            {
                mDmg = (int)token;
            }
            if ((token = json["killDragonNum"]) != null)
            {
                mKillDragonNum = (int)token;
            }
            if ((token = json["joinDragonNum"]) != null)
            {
                mJoinDragonNum = (int)token;
            }
            if ((token = json["comment"]) != null)
            {
                mComment = (string)token;
            }
            if ((token = json["giftInfoData"]) != null)
            {
                mGiftInfoData.Init(token);
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public void UpdateData(JToken json)
    {
        try
        {
            JToken token = null;
            if ((token = json["name"]) != null)
            {
                mName = (string)token;
            }
            if ((token = json["headPic"]) != null)
            {
                mHeadPicUrl = (string)token;
            }
            //if ((token = json["exp"]) != null)
            //{
            //    mExp = (float)token;
            //}
            //if ((token = json["dmg"]) != null)
            //{
            //    mDmg = (float)token;
            //}
            if ((token = json["killDragonNum"]) != null)
            {
                mKillDragonNum = (int)token;
            }
            if ((token = json["joinDragonNum"]) != null)
            {
                mJoinDragonNum = (int)token;
            }
            if ((token = json["nextCursor"]) != null)
            {
                mNextCursor = (int)token;
            }

            if ((token = json["giftInfoData"]) != null)
            {
                mGiftInfoData.Init(token);
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// 清理玩家数据
    /// </summary>
    public void Clear()
    {
        mID = "";
        mName = "";
        mHeadPicUrl = "";
        mExp = 0;
        mDmg = 0;
        mKillDragonNum = 0;
        mJoinDragonNum = 0;
    }

    // 加经验
    public void AddExp(int num)
    {
        mExp += num;
        EventManager.Instance.DispatchUserDataChangeEvent(this);
    }
    // 加累积伤害
    public void AddDmg(int num)
    {
        mDmg += num;
        EventManager.Instance.DispatchUserDataChangeEvent(this);
    }
    // 增加屠龙成功次数
    public void AddKillDragonNum(int num)
    {
        mKillDragonNum += num;
    }
    // 增加参与屠龙次数
    public void AddJoinDragonNum(int num)
    {
        mJoinDragonNum += num;
    }

    public void TestPrize()
    {
        mGiftInfoData.Test(this);
    }

    #endregion
}
