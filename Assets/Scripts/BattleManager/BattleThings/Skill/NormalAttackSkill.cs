using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普攻技能
/// </summary>
public class NormalAttackSkill : Skill
{
    enum State
    {
        // 开始攻击
        startAttack,
        // 已经达到攻击帧
        reachAttackFrame,
        max
    }

    private float mAttackFrameCD;
    private State mState;

    public override void Init(SkillInfo info, BattleCreature skillOwner)
    {
        base.Init(info, skillOwner);

        mState = State.max;
    }

    // 判断能否使用
    public override bool CanUse()
    {
        if (mSkillOwner.Dead == true || mSkillOwner.Destroyed == true)
        {
            return false;
        }

        if (mSkillOwner.State != BattleCreatureState.idle)
        {
            return false;
        }

        return true;
    }

    // 使用
    public override void Use(params object[] extParams)
    {
        mSkillOwner.SetState(BattleCreatureState.attack);
        mAttackFrameCD = mSkillOwner.AttackFrameDelay;
        mState = State.startAttack;
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        if (mSkillOwner != null && mSkillOwner.Dead == false && mSkillOwner.Destroyed == false)
        {
            if (mState == State.startAttack)
            {
                mAttackFrameCD -= delta;
                if (mAttackFrameCD <= 0)
                {
                    OnAttackFrameEvent();
                }
            }
        }
    }

    // 攻击帧事件
    private async void OnAttackFrameEvent()
    {
        mState = State.reachAttackFrame;

        BattleFlyMagic magic = (BattleFlyMagic)await BattleThingFactory.Instance.GetMagic(mInfo.magicAssetAddress, null);
        if (mSkillOwner.Target == null || mSkillOwner.Target.Dead == true || mSkillOwner.Target.Destroyed == true)
        {
            magic.Destroy();
            return;
        }

        var startPos = mSkillOwner.GetSlotByType(CreatureSlotType.body).position;
        var slotTrans = mSkillOwner.Target.GetSlotByType(CreatureSlotType.head);
        var endPos = slotTrans.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
        magic.InitMagic(startPos, endPos, mInfo.speed);
        magic.RegisterFinishCallback(OnMagicHitTarget);
        magic.EnterBattle(mSkillOwner.Battle);
    }

    // 命中回调
    private void OnMagicHitTarget(BattleMagic magic)
    {
        magic.UnregisterFinishCallback(OnMagicHitTarget);

        if (mSkillOwner == null)
        {
            return;
        }

        if (mSkillOwner.Target != null)
        {
            mSkillOwner.Target.BeHit(mInfo.damage, mSkillOwner, this);
            mSkillOwner.AddDmg(mInfo.damage);

            if (string.IsNullOrEmpty(mSkillOwner.UserID) == false)
            {
                var userData = ClientManager.Instance.GetUserData(mSkillOwner.UserID);
                userData.AddDmg(mInfo.damage);
            }
        }

        Destroy();
    }
}
