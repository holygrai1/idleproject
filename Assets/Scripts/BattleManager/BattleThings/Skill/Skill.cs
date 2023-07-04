using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能
/// </summary>
public class Skill
{
    private static long sUID = 0;
    // 唯一uid
    protected readonly long mUID = ++sUID;
    // 配置信息
    protected SkillInfo mInfo;
    // 当前剩余冷却时间
    protected float mCD;
    // 技能拥有者
    protected BattleCreature mSkillOwner;

    #region getter
    // 获取uid
    public long UID => mUID;
    // 获取配置信息
    public SkillInfo Info => mInfo;
    // 获取剩余冷却时间
    public float CD => mCD;
    // 获取拥有者
    public BattleCreature SkillOwner => mSkillOwner;
    #endregion

    // 初始化
    public virtual void Init(SkillInfo info, BattleCreature skillOwner)
    {
        mInfo = info;
        mSkillOwner = skillOwner;
        mCD = 0;
    }

    // 删除
    public virtual void Destroy()
    {
        if (mSkillOwner != null)
        {
            mSkillOwner.SkillController.OnRunningSkillDetached(this);
            mSkillOwner = null;
        }
        mInfo = null;
    }

    // 更新
    public virtual void OnUpdate(float delta, float unscaleDelta)
    {

    }

    // 判断能否使用
    public virtual bool CanUse()
    {
        if (mCD > 0)
        {
            return false;
        }

        return true;
    }

    // 使用(无需判断直接使用)
    public virtual void Use(params object[] extParams)
    {
    }

    // 运行中的技能再次被调用
    public virtual void Reuse(params object[] extParams)
    {

    }

    // 设置剩余冷却时间
    public void SetCD(float time)
    {
        mCD = time;
    }
}
