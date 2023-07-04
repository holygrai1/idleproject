using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 送礼的加血技能
/// </summary>
public class PrizeHealSkill : Skill
{
    enum State
    {
        // 开始攻击
        startAttack,
        // 已经达到攻击帧
        reachAttackFrame,

        max
    }

    private class GiftInfo
    {
        public string imgUrl;
        public int shakeMoney;
    }

    // 总使用技能时间
    private float mTimeAcc;
    // 下一次攻击剩余冷却时间
    private float mAttackCD;
    // 攻击帧剩余冷却时间
    private float mAttackFrameDelayCD;
    private SkillInfo.PrizeHealInfo mPrizeHealInfo;
    private State mState;
    private float mNextBallCD;
    private Dictionary<BattleMagic, GiftInfo> mRunningGifts = new Dictionary<BattleMagic, GiftInfo>();
    private Queue<GiftInfo> mWaitingGifts = new Queue<GiftInfo>();

    // 初始化
    public override void Init(SkillInfo info, BattleCreature skillOwner)
    {
        base.Init(info, skillOwner);

        mPrizeHealInfo = info.prizeHeal;

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
        //    mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
        //    mSkillOwner.SetState(BattleCreatureState.idle);
        }
        foreach (var kv in mRunningGifts)
        {
            var magic = kv.Key;
            magic.UnregisterFinishCallback(OnMagicHitTarget);
        }
        mRunningGifts.Clear();
        mWaitingGifts.Clear();

        base.Destroy();
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        mTimeAcc += delta;
        mAttackCD -= delta;
        mNextBallCD -= delta;

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
            if (mTimeAcc >= mPrizeHealInfo.duration && mRunningGifts.Count <= 0 && mWaitingGifts.Count <= 0)
            {
                Destroy();
                return;
            }

            if (mAttackCD <= 0 && mNextBallCD <= 0 && mWaitingGifts.Count > 0)
            {
              //  mSkillOwner.SetState(BattleCreatureState.skill);
              //  mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
              //  mSkillOwner.RegisterAnimationCompleteEvent(OnAttackComplete);
                //var aniTrack = mSkillOwner.SkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.attack, false);
                //var animationTime = aniTrack.Animation.Duration;
                //var speedRate = Helpers.GetAniSpeedByAttackSpeed(animationTime, animationTime, mPrizeHealInfo.animationTime);
                //if (speedRate > 1)
                //{
                //    // 加速
                //    aniTrack.TimeScale = speedRate;
                //}
                //else
                //{
                //    aniTrack.TimeScale = 1;
                //}
                mAttackCD = mPrizeHealInfo.animationTime;
                mAttackFrameDelayCD = mPrizeHealInfo.attackFrameDeldy;
                mNextBallCD = mPrizeHealInfo.nextCD;

                mState = State.startAttack;
                return;
            }
        }
    }

    // 运行中的技能再次被调用
    public override void Reuse(params object[] extParams)
    {
        var userData = (UserData)extParams[0];

        for (int i = 0; i < userData.giftInfoData.giftCount; ++i)
        {
            mWaitingGifts.Enqueue(new GiftInfo() { imgUrl = userData.giftInfoData.giftUrl, shakeMoney = userData.giftInfoData.shakeMoney });
        }

        userData.AddExp(userData.giftInfoData.shakeMoney * userData.giftInfoData.giftCount * 3);
    }

    // 使用
    public override void Use(params object[] extParams)
    {
        var userData = (UserData)extParams[0];
        for (int i = 0; i < userData.giftInfoData.giftCount; ++i)
        {
            mWaitingGifts.Enqueue(new GiftInfo() { imgUrl = userData.giftInfoData.giftUrl, shakeMoney = userData.giftInfoData.shakeMoney });
        }

        userData.AddExp(userData.giftInfoData.shakeMoney * userData.giftInfoData.giftCount * 3);
        // mSkillOwner.SetState(BattleCreatureState.skill);
        // mSkillOwner.UnregisterAnimationCompleteEvent(OnAttackComplete);
        // mSkillOwner.RegisterAnimationCompleteEvent(OnAttackComplete);
        //var aniTrack = mSkillOwner.SkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.attack, false);
        //var animationTime = aniTrack.Animation.Duration;
        //var speedRate = Helpers.GetAniSpeedByAttackSpeed(animationTime, animationTime, mPrizeHealInfo.animationTime);
        //if (speedRate > 1)
        //{
        //    // 加速
        //    aniTrack.TimeScale = speedRate;
        //}
        //else
        //{
        //    aniTrack.TimeScale = 1;
        //}
        mTimeAcc = 0;
        mAttackCD = mPrizeHealInfo.animationTime;
        mAttackFrameDelayCD = mPrizeHealInfo.attackFrameDeldy;
        mNextBallCD = mPrizeHealInfo.nextCD;
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

        if (mWaitingGifts.Count <= 0)
        {
            return;
        }

        // 飞出奖励
        {
            BattlePrizeHealMagic magic = (BattlePrizeHealMagic)await BattleThingFactory.Instance.GetMagic(mInfo.magicAssetAddress, null);
            if (mSkillOwner.Dead == true || mSkillOwner.Destroyed == true)
            {
                magic.Destroy();
                return;
            }

            var startPos = mSkillOwner.GetSlotByType(CreatureSlotType.body).position;
            var boss = mSkillOwner as BattleMonster;
            var endPos = boss.HPAndTimePartTrans.position;
            magic.InitMagic(startPos, endPos, mInfo.speed);
            magic.RegisterFinishCallback(OnMagicHitTarget);
            magic.EnterBattle(mSkillOwner.Battle);

            var giftInfo = mWaitingGifts.Dequeue();
            mRunningGifts.Add(magic, giftInfo);
            var spriteRender = magic.GetComponentInChildren<SpriteRenderer>();
            if (spriteRender != null)
            {
                Helpers.SetSpriteRenderFromURL(giftInfo.imgUrl, spriteRender);
            }
        }
    }

    // 命中回调
    private void OnMagicHitTarget(BattleMagic magic)
    {
        magic.UnregisterFinishCallback(OnMagicHitTarget);

        GiftInfo giftInfo = null;
        mRunningGifts.TryGetValue(magic, out giftInfo);

        if (mSkillOwner == null || giftInfo == null)
        {
            return;
        }

        mRunningGifts.Remove(magic);

        float healNum = Mathf.Pow(giftInfo.shakeMoney, 1.0f / mPrizeHealInfo.pow) * mSkillOwner.GetAttributeValueByType(CreatureAttributeType.hp) * mPrizeHealInfo.hpPer * 0.0001f;
        int iDmg = (int)healNum;
        mSkillOwner.BeHeal(iDmg, mSkillOwner, this);

        //if (string.IsNullOrEmpty(mSkillOwner.UserID) == false)
        //{
        //    var userData = ClientManager.Instance.GetUserData(mSkillOwner.UserID);
        //    if (userData != null)
        //    {
        //        userData.AddExp(giftInfo.shakeMoney * 3);
        //    }
        //}

        if (mTimeAcc >= mPrizeHealInfo.duration  && mWaitingGifts.Count <= 0 && mRunningGifts.Count <= 0)
        {
            Destroy();
        }
    }
}
