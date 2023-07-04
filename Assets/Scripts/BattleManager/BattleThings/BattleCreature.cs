using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗生物类, 是角色和怪物的父类
/// </summary>
public class BattleCreature : BattleThing
{
    // 生物配置信息
    protected CreatureInfo mCreatureInfo;
    // 当前状态
    protected BattleCreatureState mState = BattleCreatureState.max;
    // 当前目标
    protected BattleCreature mTarget = null;
    // 是否已死亡
    protected bool mDead = false;
    // spine动做
    protected Spine.Unity.SkeletonAnimation mSkeletonAnimation;
    // spine动做完成回调
    protected Spine.AnimationState.TrackEntryDelegate mSkeletonAnimationCompleteCallback;
    // spine动作事件回调
    protected Spine.AnimationState.TrackEntryEventDelegate mSkeletonAnimationEventCallback;
    // 角色动作完成处理函数
    protected Action<string> mCurAnimationCompleteHandlers;
    // 骨骼Transform
    protected Transform mSkeletonTrans;

    // 角色动作事件处理函数
    protected Action<string, string> mCurAnimationEventHandlers;
    // 所有生物属性
    protected Dictionary<CreatureAttributeType, CreatureAttribute> mAllAttributes = new Dictionary<CreatureAttributeType, CreatureAttribute>();
    // 当前血量
    protected float mCurHP;
    // 技能控制器
    protected SkillController mSkillController;
    // 普攻冷却剩余时间
    protected float mNormalAttackCD;
    // 配置脚本设置的普攻速度, 每次攻击的时间
    protected float mCreatureCfgAttackTime;
    // 当前普攻的技能id
    protected string mCurNormalAttackID;
    // 当前普攻加速比
    protected float mCurNormalAttackSkillSpeedScale = 1;
    //// 普攻动作最长播放时间
    //protected float mMaxNormalAttackTime = 1;
    protected float mAttackFrameDelay;

    // 击杀此生物的生物
    protected BattleCreature mKillerCreature;
    // 击杀此生物的生物名字
    protected string mKillerName;
    // 用户id
    protected string mUserID;
    // 头像
    protected Canvas mCanvas;
    protected float mInitCanvasScale;
    protected Image mUserHeadSprite;
    protected Image mUserHeadFg;
    // 头顶信息框
    protected MsgBubble mMsgBubble;
    // 头顶信息框位置
    protected Transform mMsgBubblePos;
    // 头顶信息框原始位置
    protected Vector3 mMsgBubbleInitPos;

    // 槽点
    protected Dictionary<CreatureSlotType, Transform> mSlots = new Dictionary<CreatureSlotType, Transform>();
    // 累积的无操作时间
    protected float mNoActionTimeAcc;
    // 被击动作cd
    protected float mBeHitCD;
    // 被击动作下次最快可播放时间
    protected float mBeHitNextTime = 0.35f;
    // 累积点赞次数
    protected int mLikeNumAcc = 0;
    // 本场战斗的累积伤害值
    protected float mDmgAcc = 0;
    // 累积的评论数
    protected int mCommentNumAcc = 0;
    // 累积的送礼数
    protected int mPrizeNumAcc = 0;
    // 显示评论持续时间
    protected float mShowCommentTimeAcc = 0;
    // 显示评论最长时间
    protected float mShowCommentMaxTime = 3;

    #region getter
    // 获取生物配置信息
    public CreatureInfo CreatureInfo => mCreatureInfo;
    // 获取当前状态
    public BattleCreatureState State => mState;
    // 获取当前目标
    public BattleCreature Target => mTarget;
    // 获取是否死亡
    public bool Dead => mDead;
    // 获取当前剩余血量
    public float CurHP => mCurHP;
    // 获取技能控制器
    public SkillController SkillController => mSkillController;
    // 获取当前所有生物属性
    public Dictionary<CreatureAttributeType, CreatureAttribute> AllAttributes => mAllAttributes;
    // 获取参数指定的生物属性值
    public float GetAttributeValueByType(CreatureAttributeType type)
    {
        CreatureAttribute att = null;
        if (mAllAttributes.TryGetValue(type, out att) == true)
        {
            return att.Value;
        }
        return 0;
    }
    // 获取参数指定的生物属性
    public CreatureAttribute GetAttributeByType(CreatureAttributeType type)
    {
        CreatureAttribute att = null;
        mAllAttributes.TryGetValue(type, out att);
        return att;
    }
    // 获取参数指定的生物属性
    public CreatureAttribute GetAttributeByChangeType(CreatureAttributeChangeType changeType)
    {
        var type = CreatureAttributeOperand.GetTypeByChangeType(changeType);
        return GetAttributeByType(type);
    }
    // 获取用户id
    public string UserID => mUserID;
    // 获取当前攻击帧延迟时间
    public float AttackFrameDelay => mAttackFrameDelay;
    // 获取槽点
    public Transform GetSlotByType(CreatureSlotType type)
    {
        Transform slotTrans = null;
        mSlots.TryGetValue(type, out slotTrans);
        if (slotTrans == null)
        {
            return Trans;
        }

        return slotTrans;
    }
    public Dictionary<CreatureSlotType, Transform> Slots => mSlots;
    // 获取动画控件
    public Spine.Unity.SkeletonAnimation SkeletonAnimation => mSkeletonAnimation;
    // 获取累积无操作时间值
    public float NoActionTimeAcc => mNoActionTimeAcc;
    // 获取累积点赞次数
    public int LikeNumAcc => mLikeNumAcc;
    // 获取本次战斗累积伤害值
    public float DmgAcc => mDmgAcc;
    // 获取本次战斗累积评论数
    public int CommentNumAcc => mCommentNumAcc;
    // 获取本次战斗累积送礼数
    public int PrizeNumAcc => mPrizeNumAcc;
    public Transform MsgBubblePos => mMsgBubblePos;
    public Transform SkeletonTrans => mSkeletonTrans;
    #endregion

    // 初始化
    public virtual bool InitCreature(ThingType thingType, CreatureInfo creatureInfo, string userID)
    {
        if (base.Init(thingType) == false)
        {
            return false;
        }
        mCreatureInfo = creatureInfo;
        InitAttributes();
        InitAnimation();
        InitSlots();
        InitSkills();

        mUserID = userID;
        mUserHeadSprite = Trans.Find("Canvas/HeadBg/HeadIcon")?.GetComponent<Image>();
        mUserHeadFg = Trans.Find("Canvas/HeadFg")?.GetComponent<Image>();
        mCanvas = Trans.Find("Canvas")?.GetComponent<Canvas>();
        mInitCanvasScale = mCanvas.transform.localScale.x;

        var canvs = Trans.Find("Canvas");
        if (canvs != null)
        {
            canvs.GetComponent<Canvas>().worldCamera = Camera.main;
        }

        mCommentNumAcc = 0;
        mLikeNumAcc = 0;
        mPrizeNumAcc = 0;

        mMsgBubblePos = Trans.Find("MsgBubblePos");
        mMsgBubbleInitPos = mMsgBubblePos.localPosition;
        // todo status

        return true;
    }

    // 删除
    public override void Destroy()
    {
        if (Destroyed == true)
        {
            return;
        }

        if (mMsgBubble != null)
        {
            mMsgBubble.Release();
            mMsgBubble = null;
        }

        mCurAnimationCompleteHandlers = null;
        mCurAnimationEventHandlers = null;
        if (mSkeletonAnimation != null)
        {
            ResumeAnimation();
            mSkeletonAnimation.AnimationState.Complete -= mSkeletonAnimationCompleteCallback;
            mSkeletonAnimation.AnimationState.Event -= mSkeletonAnimationEventCallback;
            mSkeletonAnimation = null;
        }
        mSkeletonAnimationCompleteCallback = null;
        mSkeletonAnimationEventCallback = null;

        EventManager.Instance.UnregisterCreatureAttributeDirtyEvent(OnAttributeChangedHandle);

        base.Destroy();
    }


    // 固定间隔更新
    public override void OnUpdate(float delta, float unscaleDelta)
    {
        if (Paused == false || Destroyed == true)
        {
            base.OnUpdate(delta, unscaleDelta);
            mNoActionTimeAcc += delta;
            mNormalAttackCD -= delta;
            mBeHitCD -= delta;
            mShowCommentTimeAcc += delta;

            mMsgBubble?.UpdatePos(mMsgBubblePos);

            if (mShowCommentTimeAcc >= mShowCommentMaxTime)
            {
                if (mMsgBubble != null)
                {
                    mMsgBubble.Release();
                    mMsgBubble = null;
                }
            }

            UpdateTarget();

            if (mState == BattleCreatureState.wait)
            {
            }
            else if (mState == BattleCreatureState.attack)
            {
                mSkillController.OnUpdate(delta, unscaleDelta);
            }
            else if (mState == BattleCreatureState.enter)
            {

            }
            else if (mState == BattleCreatureState.idle)
            {
                mSkillController.OnUpdate(delta, unscaleDelta);

                if (mNormalAttackCD <= 0 && mTarget != null)
                {
                    mSkillController.UseSkill(mCreatureInfo.normalAttackSkillID);
                }
            }
            else if (mState == BattleCreatureState.skill)
            {
                mSkillController.OnUpdate(delta, unscaleDelta);
            }
        }
    }


    // 进入战场
    public override void EnterBattle(Battle battle)
    {
        base.EnterBattle(battle);
        mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);
    }

    // 开始战斗回调
    public override void OnStartBattle()
    {
        mNormalAttackCD = UnityEngine.Random.Range(0.1f, GetAttributeValueByType(CreatureAttributeType.attackSpeed));

        SetState(BattleCreatureState.idle);

        base.OnStartBattle();
    }

    // 暂停
    public override void Pause()
    {
        if (Paused == true)
        {
            return;
        }

        PauseAnimation();
        base.Pause();
    }

    // 暂停恢复
    public override void Resume()
    {
        if (Paused == false)
        {
            return;
        }

        ResumeAnimation();
        base.Resume();
    }

    // 设置大小
    public virtual void SetScale(bool big)
    {
        float scale = big == true ? mCreatureInfo.scaleBig : mCreatureInfo.scaleSmall;
        if (SkeletonTrans.localScale.x > 0)
        {
            SkeletonTrans.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            SkeletonTrans.localScale = new Vector3(-scale, scale, scale);
        }

        if (mCanvas != null)
        {
            mCanvas.transform.localScale = new Vector3(mInitCanvasScale * scale, mInitCanvasScale * scale, mInitCanvasScale * scale);
        }

        mMsgBubblePos.localPosition = new Vector3(mMsgBubbleInitPos.x * scale, mMsgBubbleInitPos.y * scale, mMsgBubbleInitPos.z * scale);
    }

    // 设置正面朝向
    public virtual void FaceRight(bool right)
    {
        var localScale = SkeletonTrans.localScale;

        if (right == true)
        {
            SkeletonTrans.localScale = new Vector3(Math.Abs(localScale.x), localScale.y, localScale.z);
        }
        else
        {
            SkeletonTrans.localScale = new Vector3(-Math.Abs(localScale.x), localScale.y, localScale.z);
        }
    }

    // 根据用户行为类型选择使用相应的技能
    public virtual void OnUserActionTypeHandle(UserActionType actionType, string extParam = "")
    {
        if (mState == BattleCreatureState.wait || Battle.Started == false || mState == BattleCreatureState.max)
        {
            if (actionType == UserActionType.comment)
            {
                if (string.IsNullOrEmpty(extParam) == false)
                {
                    if (mMsgBubble == null)
                    {
                        mMsgBubble = UIManager.Instance.GetMsgBubble(CanvasLayer.below);
                    }

                    if (mMsgBubble != null)
                    {
                        mMsgBubble.UpdatePos(mMsgBubblePos);
                        mMsgBubble.SetMsg(extParam);
                        mShowCommentTimeAcc = 0;
                    }
                }
            }
            return;
        }

        if (actionType == UserActionType.comment)
        {
     //       Debug.Log("comment " + extParam);
            var skill = mSkillController.GetRunningSkillByID(mCreatureInfo.commentSkillID);
            if (skill != null)
            {
                skill.Reuse();
            }
            else
            {
                mSkillController.UseSkill(mCreatureInfo.commentSkillID);
            }

            if (string.IsNullOrEmpty(extParam) == false)
            {
                if (mMsgBubble == null)
                {
                    mMsgBubble = UIManager.Instance.GetMsgBubble(CanvasLayer.below);
                }

                if (mMsgBubble != null)
                {
                    mMsgBubble.UpdatePos(mMsgBubblePos);
                    mMsgBubble.SetMsg(extParam);
                    mShowCommentTimeAcc = 0;
                }
            }

            ++mCommentNumAcc;
        }
        else if (actionType == UserActionType.like)
        {
            var skill = mSkillController.GetRunningSkillByID(mCreatureInfo.likeSkillID);
            if (skill != null)
            {
                skill.Reuse();
            }
            else
            {
                mSkillController.UseSkill(mCreatureInfo.likeSkillID);
            }

            ++mLikeNumAcc;
        }
        else if (actionType == UserActionType.prize)
        {
            var skill = mSkillController.GetRunningSkillByID(mCreatureInfo.prizeSkillID);
            var userData = ClientManager.Instance.GetUserData(mUserID);
            if (skill != null)
            {
                skill.Reuse(userData);
            }
            else
            {
                mSkillController.UseSkill(mCreatureInfo.prizeSkillID, userData);
            }

            ++mPrizeNumAcc;
        }

        mNoActionTimeAcc = 0;
    }

    // 设置当前状态
    public virtual void SetState(BattleCreatureState newState)
    {
        var preState = mState;

        if (mDead == true || Destroyed == true)
        {
            return;
        }

        if (newState == BattleCreatureState.die && mState != newState)
        {
            mDead = true;

            ResumeAnimation();
            mCurAnimationCompleteHandlers = null;
            mCurAnimationEventHandlers = null;

            mCurAnimationCompleteHandlers += (string animationName) => {
                if (animationName == CreatureAnimationName.dead)
                {
                }
            };

            EventManager.Instance.DispatchCreatureDieEvent(this, mKillerCreature, mKillerName);
            mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.dead, false);
        }
        else if (newState == BattleCreatureState.idle)
        {
            mCurAnimationCompleteHandlers = null;
            mCurAnimationEventHandlers = null;
            mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);
        }
        else if (newState == BattleCreatureState.attack)
        {
            mCurAnimationCompleteHandlers = null;
            mCurAnimationEventHandlers = null;
            mCurAnimationCompleteHandlers += ((string animationName) => {
                if (animationName == CreatureAnimationName.attack)
                {
                    SetState(BattleCreatureState.idle);
                }
            });
            var aniTrack = mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.attack, false);
            var normalAttackTime = GetAttributeValueByType(CreatureAttributeType.attackSpeed);
            var animationTime = aniTrack.Animation.Duration;
            var speedRate = Helpers.GetAniSpeedByAttackSpeed(animationTime, animationTime, normalAttackTime);
            if (speedRate > 1)
            {
                // 加速
                aniTrack.TimeScale = speedRate;
                mCurNormalAttackSkillSpeedScale = speedRate;
            }
            else
            {
                //if (normalAttackTime > mMaxNormalAttackTime)
                //{
                //    speedRate = Helpers.GetAniSpeedByAttackSpeed(animationTime, animationTime, mMaxNormalAttackTime);
                //}
                // aniTrack.TimeScale = speedRate;
                //mCurNormalAttackSkillSpeedScale = speedRate;

                aniTrack.TimeScale = 1;
                mCurNormalAttackSkillSpeedScale = 1;
            }
            mNormalAttackCD = normalAttackTime;
        }
        else if (newState == BattleCreatureState.skill)
        {
            mCurAnimationCompleteHandlers = null;
            mCurAnimationEventHandlers = null;
        }
        else if (newState == BattleCreatureState.wait)
        {
            mCurAnimationCompleteHandlers = null;
            mCurAnimationEventHandlers = null;
            mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);
        }

        mState = newState;
        EventManager.Instance.DispatchCreatureStateChangeEvent(this, preState, newState);
    }

    // 设置目标
    public virtual void SetTarget(BattleCreature newTarget)
    {
        if (newTarget == mTarget)
        {
            return;
        }

        mTarget = newTarget;
    }

    // 更新目标
    public virtual void UpdateTarget()
    {
        if (mTarget != null)
        {
            if (mTarget.Dead == true || mTarget.Destroyed == true)
            {
                SetTarget(null);
            }
        }
    }

    // 被击
    public virtual void BeHit(float damage, BattleCreature attacker, object ext = null)
    {
        if (Dead == true || Destroyed == true)
        {
            return;
        }

        if (State == BattleCreatureState.enter || State == BattleCreatureState.wait || State == BattleCreatureState.max)
        {
            return;
        }

        if (Battle.State != BattleState.run)
        {
            return;
        }

        mCurHP -= damage;
        if (mCurHP <= 0)
        {
            mCurHP = 0;
            SetState(BattleCreatureState.die);
        }
        else if (damage > 0)
        {
            if (mBeHitCD <= 0)
            {
                mCurAnimationCompleteHandlers = null;
                mCurAnimationEventHandlers = null;
                mCurAnimationCompleteHandlers += ((string animationName) => {
                    if (animationName == CreatureAnimationName.beHit)
                    {
                        SetState(BattleCreatureState.idle);
                    }
                });
                mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.beHit, false);
                mBeHitCD = mBeHitNextTime;
            }
        }
    }

    // 被治疗
    public virtual void BeHeal(float heal, BattleCreature healer, object ext = null)
    {
        if (Dead == true || Destroyed == true)
        {
            return;
        }

        float maxHP = GetAttributeValueByType(CreatureAttributeType.hp);
        // 可治疗/已损失血量
        float canHealNum = maxHP - mCurHP;
        // 溢出值
        float healLeft = 0;
        if (canHealNum < heal)
        {
            healLeft = heal - canHealNum;
            heal = canHealNum;
        }

        // 实际加的值
        if (heal > 0)
        {
            mCurHP += heal;
            if (mCurHP > maxHP)
            {
                mCurHP = maxHP;
            }
        }
    }

    // 初始化生物属性
    public virtual void InitAttributes()
    {
        mAllAttributes.Clear();
        mAllAttributes.Add(CreatureAttributeType.hp, new CreatureAttributeMulThenAdd(CreatureAttributeType.hp, 1));
        mAllAttributes.Add(CreatureAttributeType.attackPower, new CreatureAttributeMulThenAdd(CreatureAttributeType.attackPower, 1));
        mAllAttributes.Add(CreatureAttributeType.attackSpeed, new CreatureAttributeDiv(CreatureAttributeType.attackSpeed, 1));
        mCurHP = GetAttributeValueByType(CreatureAttributeType.hp);
        EventManager.Instance.RegisterCreatureAttributeDirtyEvent(OnAttributeChangedHandle);
    }

    // 属性改变事件处理
    public virtual void OnAttributeChangedHandle(CreatureAttribute att)
    {
        if (mAllAttributes[CreatureAttributeType.attackSpeed] == att)
        {
            mNormalAttackCD = att.Value;
            var normalAttackInfo = DataManager.Instance.GetSkillInfo(mCreatureInfo.normalAttackSkillID);
            if (normalAttackInfo != null)
            {
                var ratio = mNormalAttackCD / normalAttackInfo.normalAttack.animationTime;
                mAttackFrameDelay = normalAttackInfo.normalAttack.attackFrameDeldy * ratio;
                // 只影响到下一个到普攻
            }
        }
        else if (mAllAttributes[CreatureAttributeType.hp] == att)
        {
            mCurHP = GetAttributeValueByType(CreatureAttributeType.hp);
        }
    }

    // 更新所有生物属性
    public virtual void UpdateAllAttributes()
    {
        foreach (var kv in mAllAttributes)
        {
            kv.Value.Update();
        }
    }

    // 初始化动画
    public virtual void InitAnimation()
    {
        if (mSkeletonAnimation != null)
        {
            if (mSkeletonAnimationCompleteCallback != null)
            {
                mSkeletonAnimation.AnimationState.Complete -= mSkeletonAnimationCompleteCallback;
            }
            if (mSkeletonAnimationEventCallback != null)
            {
                mSkeletonAnimation.AnimationState.Event -= mSkeletonAnimationEventCallback;
            }
        }

        mSkeletonAnimation = Go.GetComponentInChildren<Spine.Unity.SkeletonAnimation>();

        if (mSkeletonAnimationEventCallback == null)
        {
            mSkeletonAnimationEventCallback = new Spine.AnimationState.TrackEntryEventDelegate((Spine.TrackEntry trackEntry, Spine.Event spineEvent) => {
                mCurAnimationEventHandlers?.Invoke(trackEntry.Animation.Name, spineEvent.String);
            });
        }

        if (mSkeletonAnimationCompleteCallback == null)
        {
            mSkeletonAnimationCompleteCallback = new Spine.AnimationState.TrackEntryDelegate((Spine.TrackEntry trackEntry) => {
                mCurAnimationCompleteHandlers?.Invoke(trackEntry.Animation.Name);
            });
        }

        mSkeletonAnimation.AnimationState.Complete += mSkeletonAnimationCompleteCallback;
        mSkeletonAnimation.AnimationState.Event += mSkeletonAnimationEventCallback;

        mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);

        mSkeletonTrans = mSkeletonAnimation.transform;
    }

    // 暂停spine动作
    public void PauseAnimation()
    {
        mSkeletonAnimation.timeScale = 0;
    }

    // 恢复播放spine动作
    public void ResumeAnimation()
    {
        mSkeletonAnimation.timeScale = 1;
    }

    // 注册动做播放完成事件
    public void RegisterAnimationCompleteEvent(Action<string> handler)
    {
        mCurAnimationCompleteHandlers += handler;
    }
    // 注销动作播放完成事件
    public void UnregisterAnimationCompleteEvent(Action<string> handler)
    {
        mCurAnimationCompleteHandlers -= handler;
    }
    // 注册动作事件
    public void RegisterAnimationEvent(Action<string, string> handler)
    {
        mCurAnimationEventHandlers += handler;
    }
    // 注销动作事件
    public void UnregisterAnimationEvent(Action<string, string> handler)
    {
        mCurAnimationEventHandlers -= handler;
    }

    // 初始化技能
    public virtual void InitSkills()
    {
        mSkillController = new SkillController();
        mSkillController.Init(this);

        if (string.IsNullOrEmpty(mCreatureInfo.normalAttackSkillID) == false)
        {
            mSkillController.LearnSkill(mCreatureInfo.normalAttackSkillID);

            var normalAttackInfo = DataManager.Instance.GetSkillInfo(mCreatureInfo.normalAttackSkillID);
            var attackSpeedAtt = GetAttributeByType(CreatureAttributeType.attackSpeed);
            attackSpeedAtt.SetBaseValue(normalAttackInfo.normalAttack.animationTime);
            mAttackFrameDelay = normalAttackInfo.normalAttack.attackFrameDeldy;
        }
        if (string.IsNullOrEmpty(mCreatureInfo.commentSkillID) == false)
        {
            mSkillController.LearnSkill(mCreatureInfo.commentSkillID);
        }
        if (string.IsNullOrEmpty(mCreatureInfo.likeSkillID) == false)
        {
            mSkillController.LearnSkill(mCreatureInfo.likeSkillID);
        }
        if (string.IsNullOrEmpty(mCreatureInfo.prizeSkillID) == false)
        {
            mSkillController.LearnSkill(mCreatureInfo.prizeSkillID);
        }
    }

    // 初始化槽点
    public virtual void InitSlots()
    {
        var bodySlot = Trans.Find(CreatureSlotType.body.ToString());
        if (bodySlot != null)
        {
            mSlots.Add(CreatureSlotType.body, bodySlot);
        }

        var headSlot = Trans.Find(CreatureSlotType.head.ToString());
        if (headSlot != null)
        {
            mSlots.Add(CreatureSlotType.head, headSlot);
        }
    }

    // 增加本次伤害累积值
    public void AddDmg(float dmg)
    {
        mDmgAcc += dmg;
    }
    public void ClearDmg()
    {
        mDmgAcc = 0;
    }
}
