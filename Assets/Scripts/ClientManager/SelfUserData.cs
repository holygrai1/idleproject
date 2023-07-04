using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主播数据
/// </summary>
public class SelfUserData
{
    // 主播id
    private string mID;
    // 主播名字
    private string mName = "";
    // 主播头像url
    private string mHeadPicUrl = "";

    // 副本相关数据
    // 挑战玩法最大关卡数
    private int mMaxChallengeLevelNum;
    // 挑战玩法当前关卡数
    private int mCurChallengeLevelNum;
    // 屠龙BOSS玩家ID
    private string mKillDragonUserID;
    // 屠龙BOSS玩家昵称
    private string mKillDragonUserName;
    // 屠龙BOSS玩家头像
    private string mKillDragonUserHeadPicUrl;
    // 屠龙宝箱已击杀数
    private int mKillDragonTreasureNum;
    // 下标索引
    private int mNextCursor;
    // 房间id
    private string mRoomID;

    #region 完成战斗时需提交部分, 这里暂存
    // 经验
    private int mKillDragonUserExp = 0;
    // 累积伤害
    private int mKillDragonUserDmg = 0;
    // 屠龙胜利次数
    private int mKillDragonUserKillDragonNum = 0;
    // 屠龙参与次数
    private int mKillDragonUserJoinDragonNum = 0;
    #endregion

    #region getter
    public string ID => mID;
    public string Name => mName;
    public string HeadPicUrl => mHeadPicUrl;
    public int MaxChallengeLevelNum => mMaxChallengeLevelNum;
    public int CurChallenegeLevelNum => mCurChallengeLevelNum;
    public string KillDragonUserID => mKillDragonUserID;
    public string KillDragonUserName => mKillDragonUserName;
    public string KillDragonUserHeadUrl => mKillDragonUserHeadPicUrl;
    public int KillDragonTreasureNum => mKillDragonTreasureNum;
    public int NextCursor => mNextCursor;
    public string RoomID => mRoomID;

    public int KillDragonUserExp => mKillDragonUserExp;
    public int KillDragonUserDmg => mKillDragonUserDmg;
    public int KillDragonUserKillDragonNum => mKillDragonUserKillDragonNum;
    public int KillDragonUserJoinDragonNum => mKillDragonUserJoinDragonNum;

    #endregion

    public bool Init(JToken json)
    {
        try
        {
            //if (json == null)
            //{
            //    // use test data
            //    mID = "1";
            //    mName = "1用户";
            //    mHeadPicUrl = "";
            //    mKillDragonUserID = "1";
            //    return true;
            //}
            JToken token = null;
            if ((token = json["nextCursor"]) != null)
            {
                mNextCursor = (int)token;
            }

            mID = (string)json["id"];
            if (string.IsNullOrEmpty(mID) == true)
            {
                return false;
            }

            if ((token = json["name"]) != null)
            {
                mName = (string)token;
            }
            if ((token = json["headPic"]) != null)
            {
                mHeadPicUrl = (string)token;
            }

            //if ((token = json["maxChallengeLevelNum"]) != null)
            //{
            //    mMaxChallengeLevelNum = (int)token;
            //}
            //if ((token = json["curChallengeLevelNum"]) != null)
            //{
            //    mCurChallengeLevelNum = (int)token;
            //}
            if ((token = json["killDragonUserID"]) != null)
            {
                mKillDragonUserID = (string)token;
            }
            if ((token = json["killDragonUserName"]) != null)
            {
                mKillDragonUserName = (string)token;
            }
            if ((token = json["killDragonUserHeadPic"]) != null)
            {
                mKillDragonUserHeadPicUrl = (string)token;
            }
            if ((token = json["killDragonTreasureNum"]) != null)
            {
                mKillDragonTreasureNum = (int)token;
            }
            if ((token = json["roomId"]) != null)
            {
                mRoomID = (string)token;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public void SetKillDragonUser(UserData userData)
    {
        mKillDragonUserID = userData.id;
        mKillDragonUserHeadPicUrl = userData.headPic;
        mKillDragonUserName = userData.name;
        mKillDragonUserDmg = userData.dmg;
        mKillDragonUserExp = userData.exp;
        mKillDragonUserJoinDragonNum = userData.joinDragonNum;
        mKillDragonUserKillDragonNum = userData.killDragonNum;

        EventManager.Instance.DispatchSelfUserDataChangeEvent(this);
    }

    public void SetNextCursor(int cursor)
    {
        mNextCursor = cursor;
    }
}
