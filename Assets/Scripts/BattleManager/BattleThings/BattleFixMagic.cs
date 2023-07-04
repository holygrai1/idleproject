using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 固定位置魔法
/// </summary>
public class BattleFixMagic : BattleMagic
{
    private Vector3 mPos;
    // -1无限时间, 需外部处理移除的逻辑. 
    private float mDuration;
    private float mTimeAcc;
    // spine动做
    protected Spine.Unity.SkeletonAnimation mSkeletonAnimation;

    public override bool InitMagic(params object[] extParams)
    {
        if (base.InitMagic(extParams) == false)
        {
            return false;
        }

        mPos = (Vector3)extParams[0];
        mDuration = (float)extParams[1];
        mSkeletonAnimation = Go.GetComponentInChildren<Spine.Unity.SkeletonAnimation>();

        return true;
    }

    public override BattleMagicType GetMagicType()
    {
        return BattleMagicType.fix;
    }

    public override void EnterBattle(Battle battle)
    {
        base.EnterBattle(battle);
        Trans.position = mPos;
        Go.SetActive(true);
        mTimeAcc = 0;

        if (battle.Started == true)
        {
            OnStartBattle();

            if (mSkeletonAnimation != null)
            {
                var current = mSkeletonAnimation.AnimationState.GetCurrent(0);
                if (current != null)
                {
                    mSkeletonAnimation.AnimationState.SetAnimation(0, current.Animation.Name, true);
                }
            }
        }
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        mTimeAcc += delta;
        if (mDuration >= 0 && mTimeAcc >= mDuration)
        {
            CacheOrDestroy();
        }
    }

    public void CacheOrDestroy()
    {
        DispatchFinishCallback();

        if (BattleThingFactory.Instance.TryCacheMagic(AssetAddress, this) == false)
        {
            Destroy();
        }
        else
        {
            mFinishCallback = null;
            Go.SetActive(false);
            LeaveBattle();
        }
    }

    public void ResetTimeAcc()
    {
        mTimeAcc = 0;
    }
}
