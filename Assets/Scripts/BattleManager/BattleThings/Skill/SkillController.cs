using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能控制器
/// </summary>
public class SkillController
{
    // 拥有者
    private BattleCreature mOwner;
    // 已学习技能
    private Dictionary<string, Skill> mLearnedSkills = new Dictionary<string, Skill>();
    // 运行中的技能
    private Dictionary<long, Skill> mRunningSkills = new Dictionary<long, Skill>();
    // 待移除的运行中技能
    private HashSet<Skill> mToRemovedRunningSkills = new HashSet<Skill>();

    #region getter
    public BattleCreature Owner => mOwner;
    public Dictionary<string, Skill> LearnedSkills => mLearnedSkills;
    public Dictionary<long, Skill> RunningSkills => mRunningSkills;
    #endregion

    // 初始化
    public void Init(BattleCreature owner)
    {
        mOwner = owner;
    }

    // 删除
    public void Destroy()
    {
        foreach (var kv in mLearnedSkills)
        {
            kv.Value.Destroy();
        }
        mLearnedSkills.Clear();

        foreach(var kv in mRunningSkills)
        {
            kv.Value.Destroy();
        }
        mRunningSkills.Clear();

        mToRemovedRunningSkills.Clear();

        mOwner = null;
    }

    // 学习技能
    public void LearnSkill(string skillID)
    {
        if (mLearnedSkills.ContainsKey(skillID) == true)
        {
            return;
        }
        
        var skillInfo = DataManager.Instance.GetSkillInfo(skillID);
        var skill = CreateSkillByType(skillInfo.type);
        skill.Init(skillInfo, mOwner);
        mLearnedSkills.Add(skillID, skill);
    }

    // 判断是否可使用技能
    public bool CanUseSkill(string skillID)
    {
        if (mLearnedSkills.TryGetValue(skillID, out Skill learnSkill) == false)
        {
            return false;
        }

        if (learnSkill.CanUse() == false)
        {
            return false;
        }

        return true;
    }

    // 使用技能
    public bool UseSkill(string skillID, params object[] extParams)
    {
        if (CanUseSkill(skillID) == false)
        {
            return false;
        }

        var skillInfo = DataManager.Instance.GetSkillInfo(skillID);
        var skillLearned = mLearnedSkills[skillID];
        skillLearned.SetCD(skillInfo.cd);

        var skill = CreateSkillByType(skillInfo.type);
        skill.Init(skillInfo, mOwner);
        mRunningSkills.Add(skill.UID, skill);
        skill.Use(extParams);

        return true;
    }

    // 停止所有正在运行的技能
    public void StopAllRunningSkill()
    {
        foreach (var kv in mRunningSkills)
        {
            var skill = kv.Value;
            skill.Destroy();
        }

        mRunningSkills.Clear();
    }

    // 获取运行中的技能
    public Skill GetRunningSkillByID(string id)
    {
        foreach (var kv in mRunningSkills)
        {
            var skill = kv.Value;
            if (mToRemovedRunningSkills.Contains(skill) == false && skill.Info.id == id)
            {
                return skill;
            }
        }

        return null;
    }

    // 更新
    public void OnUpdate(float delta, float unscaleDelta)
    {
        foreach (var kv in mLearnedSkills)
        {
            var skillLearn = kv.Value;
            skillLearn.SetCD(skillLearn.CD - delta);
        }

        foreach (var kv in mRunningSkills)
        {
            var skillRunning = kv.Value;
            if (mToRemovedRunningSkills.Contains(skillRunning) == false)
            {
                skillRunning.OnUpdate(delta, unscaleDelta);
            }
        }

        if (mToRemovedRunningSkills.Count > 0)
        {
            foreach (var skill in mToRemovedRunningSkills)
            {
                mRunningSkills.Remove(skill.UID);
            }
            mToRemovedRunningSkills.Clear();
        }
    }

    // 创建技能
    public Skill CreateSkillByType(SkillType type)
    {
        if (type == SkillType.normalAttack)
        {
            return new NormalAttackSkill();
        }
        else if (type == SkillType.fastLightBallAttack)
        {
            return new FastLightBallSkill();
        }
        else if (type == SkillType.heartBallAttack)
        {
            return new HeartBallSkill();
        }
        else if (type == SkillType.heal)
        {
            return new HealSkill();
        }
        else if (type == SkillType.prizeBall)
        {
            return new PrizeBallSkill();
        }
        else if (type == SkillType.prizeHeal)
        {
            return new PrizeHealSkill();
        }

        return null;
    }

    // 运行中技能移除回调
    public void OnRunningSkillDetached(Skill skill)
    {
        if (mRunningSkills.ContainsKey(skill.UID) == false && mToRemovedRunningSkills.Contains(skill) == false)
        {
            return;
        }
        mToRemovedRunningSkills.Add(skill);
    }
}
