using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 加血技能
/// </summary>
public class HealSkill : Skill
{
    private SkillInfo.HealInfo mHealInfo;

    // 初始化
    public override void Init(SkillInfo info, BattleCreature skillOwner)
    {
        base.Init(info, skillOwner);

        mHealInfo = info.heal;
    }

    // 判断能否使用
    public override bool CanUse()
    {
        if (mSkillOwner.Dead == true || mSkillOwner.Destroyed == true)
        {
            return false;
        }

        return true;
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);
    }

    // 使用
    public async override void Use(params object[] extParams)
    {
        BattleFixMagic magic = (BattleFixMagic)await BattleThingFactory.Instance.GetMagic(mInfo.magicAssetAddress, null);
        if (mSkillOwner == null || mSkillOwner.Destroyed == true)
        {
            magic.Destroy();
            return;
        }

        var slotTrans = mSkillOwner.GetSlotByType(CreatureSlotType.body);
        var pos = slotTrans.position;
        magic.InitMagic(pos, 3f);
        magic.RegisterFinishCallback(OnMagicFinish);
        magic.EnterBattle(mSkillOwner.Battle);

        int likeNumAcc = mSkillOwner.LikeNumAcc;
        int per = Helpers.GetWithinArray(mHealInfo.healPers, likeNumAcc);
        float maxHP = mSkillOwner.GetAttributeValueByType(CreatureAttributeType.hp);
        float healHP = maxHP * per * 0.0001f;
        mSkillOwner.BeHeal(healHP, mSkillOwner, this);
    }

    private void OnMagicFinish(BattleMagic magic)
    {
        magic.UnregisterFinishCallback(OnMagicFinish);
        Destroy();
    }
}
