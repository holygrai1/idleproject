using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 连续n秒的快速光球攻击技能
/// </summary>
public class FastLightBallSkill : Skill
{
    enum State
    {
        // 开始攻击
        startAttack,
        // 已经达到攻击帧
        reachAttackFrame,
        max
    }

    // 总使用技能时间
    private float mTimeAcc;
    // 下一次攻击剩余冷却时间
    private float mAttackCD;
    // 攻击帧剩余冷却时间
    private float mAttackFrameDelayCD;
    private SkillInfo.FastLightBallInfo mFastLightBallInfo;
    private State mState;
    private int mFlyingBallNum;

    // 初始化
    public override void Init(SkillInfo info, BattleCreature skillOwner)
    {
        base.Init(info, skillOwner);

        mFastLightBallInfo = info.fastLightBall;
        mFlyingBallNum = 0;
        mState = State.max;
    }

    // 判断能否使用
    public override bool CanUse()
    {
        if (mSkillOwner.Dead == true || mSkillOwner.Destroyed == true)
        {
            return false;
        }

        //if (mSkillOwner.State != BattleCreatureState.idle && mSkillOwner.State != BattleCreatureState.attack)
        //{
        //    return false;
        //}

        return true;
    }

    public override void Destroy()
    {
        if (mSkillOwner != null)
        {
            mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
            if (mSkillOwner.State != BattleCreatureState.wait)
            {
                mSkillOwner.SetState(BattleCreatureState.idle);
            }
        }

        base.Destroy();
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        mTimeAcc += delta;
        mAttackCD -= delta;

        if (mState == State.startAttack)
        {
            if (mAttackFrameDelayCD > 0)
            {
                mAttackFrameDelayCD -= delta;
                if (mAttackFrameDelayCD <= 0)
                {
                    OnAttackFrameEvent();
                }
            }
        }
        else if (mState == State.reachAttackFrame)
        {
            if (mTimeAcc >= mFastLightBallInfo.duration)
            {
                if (mFlyingBallNum <= 0)
                {
                    Destroy();
                }
                return;
            }

            if (mAttackCD <= 0)
            {
                mSkillOwner.SetState(BattleCreatureState.skill);
                mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
                mSkillOwner.RegisterAnimationCompleteEvent(OnAttackComplete);
                var aniTrack = mSkillOwner.SkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.attack, false);
                var animationTime = aniTrack.Animation.Duration;
                var speedRate = Helpers.GetAniSpeedByAttackSpeed(animationTime, animationTime, mFastLightBallInfo.animationTime);
                if (speedRate > 1)
                {
                    // 加速
                    aniTrack.TimeScale = speedRate;
                }
                else
                {
                    aniTrack.TimeScale = 1;
                }
                mAttackCD = mFastLightBallInfo.animationTime;
                mAttackFrameDelayCD = mFastLightBallInfo.attackFrameDeldy;

                mState = State.startAttack;

                return;
            }
        }
    }

    // 运行中的技能再次被调用
    public override void Reuse(params object[] extParams)
    {
        mTimeAcc = 0;
    }

    // 使用
    public override void Use(params object[] extParams)
    {
        mTimeAcc = 0;

        mSkillOwner.SetState(BattleCreatureState.skill);
        mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
        mSkillOwner.RegisterAnimationCompleteEvent(OnAttackComplete);
        var aniTrack = mSkillOwner.SkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.attack, false);
        var animationTime = aniTrack.Animation.Duration;
        var speedRate = Helpers.GetAniSpeedByAttackSpeed(animationTime, animationTime, mFastLightBallInfo.animationTime);
        if (speedRate > 1)
        {
            // 加速
            aniTrack.TimeScale = speedRate;
        }
        else
        {
            aniTrack.TimeScale = 1;
        }
        mAttackCD = mFastLightBallInfo.animationTime;
        mAttackFrameDelayCD = mFastLightBallInfo.attackFrameDeldy;
        mState = State.startAttack;
    }

    private void OnAttackComplete(string animationName)
    {
        if (animationName == CreatureAnimationName.attack)
        {
            mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
            mSkillOwner.SkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);
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

        ++mFlyingBallNum;

        var startPos = mSkillOwner.Trans.position;
        var slotTrans = mSkillOwner.Target.GetSlotByType(CreatureSlotType.head);
        var endPos = slotTrans.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
        magic.InitMagic(startPos, endPos, mInfo.speed);
        magic.RegisterFinishCallback(OnMagicHitTarget);
        magic.EnterBattle(mSkillOwner.Battle);

    }

    // 命中回调
    private void OnMagicHitTarget(BattleMagic magic)
    {
        --mFlyingBallNum;

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

        if (mTimeAcc >= mFastLightBallInfo.duration && mFlyingBallNum <= 0)
        {
            Destroy();
        }
    }
}
