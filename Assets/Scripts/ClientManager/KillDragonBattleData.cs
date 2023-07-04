using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 屠龙战斗数据
/// </summary>
public class KillDragonBattleData
{
    // 额外补充人数
    private int mExtFillUserNum;
    // 比例系数
    private float mRatio;
    // 随机波动
    private float mRand;
    // 战斗时长, sec
    private float mDuration;
    // 高等级区人数上线
    private int mHighLevelAreaMaxUserNum;
    // 高等级区最低等级要求
    private int mHighLevelAreaLevelNeed;

    #region getter
    public int extFillUserNum => mExtFillUserNum;
    public float ratio => mRatio;
    public float rand => mRand;
    public float duration => mDuration;
    public int highLevelAreaMaxUserNum => mHighLevelAreaMaxUserNum;
    public int highLevelAreaLevelNeed => mHighLevelAreaLevelNeed;
    #endregion

    public bool Init(JToken json)
    {
        try
        {
            JToken JContent = json["content"];
            if (JContent != null)
            {
                JToken token = null;
                if ((token = JContent["livePlatform"]) != null)
                {

                }
                if ((token = JContent["anchorId"]) != null)
                {

                }

                if ((token = JContent["extFillUserNum"]) != null)
                {
                    mExtFillUserNum = (int)token;
                }
                if ((token = JContent["ratio"]) != null)
                {
                    mRatio = (float)token;
                }
                if ((token = JContent["rand"]) != null)
                {
                    mRand = (float)token;
                }
                if ((token = JContent["duration"]) != null)
                {
                    mDuration = (float)token;
                }
                if ((token = JContent["highLevelAreaMaxUserNum"]) != null)
                {
                    mHighLevelAreaMaxUserNum = (int)token;
                }
                if ((token = JContent["highLevelAreaLevelNeed"]) != null)
                {
                    mHighLevelAreaLevelNeed = (int)token;
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}
