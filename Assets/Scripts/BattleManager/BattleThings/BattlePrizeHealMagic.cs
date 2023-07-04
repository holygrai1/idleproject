using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 奖励的加血魔法
/// </summary>
public class BattlePrizeHealMagic : BattleMagic
{
    private enum State
    {
        // 进场, 原地放大
        enter,
        // 原地停留
        hold,
        // 飞行
        fly,
        // 目标原地停留
        targetHold,
        // 原地放大
        explode,

        max
    }

    private Vector3 mStartPos;
    private Vector3 mEndPos;
    private float mSpeed;
    private State mState;
    private float mHoldingCD;
    
    public override bool InitMagic(params object[] extParams)
    {
        if (base.InitMagic(extParams) == false)
        {
            return false;
        }

        mStartPos = (Vector3)extParams[0];
        mEndPos = (Vector3)extParams[1];
        mSpeed = (float)extParams[2];
        mUpdateSecPerFrame = 0.03f;

        return true;
    }

    public override BattleMagicType GetMagicType()
    {
        return BattleMagicType.prize;
    }

    public override void EnterBattle(Battle battle)
    {
        base.EnterBattle(battle);
        Trans.DOKill();
        Trans.position = mStartPos;
        Go.SetActive(true);

        if (battle.Started == true)
        {
            OnStartBattle();

            SetState(State.enter);
        }
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        if (mState == State.enter)
        {

        }
        else if (mState == State.hold)
        {
            mHoldingCD -= delta;
            if (mHoldingCD <= 0)
            {
                SetState(State.fly);
            }
        }
        else if (mState == State.fly)
        {

        }
        else if (mState == State.targetHold)
        {
            mHoldingCD -= delta;
            if (mHoldingCD <= 0)
            {
                SetState(State.explode);
            }
        }
    }

    private void SetState(State newState)
    {
        if (newState == State.enter)
        {
            Trans.localScale = Vector3.zero;
            Trans.DOKill();
            Trans.DOScale(0.6f, 0.5f).onComplete += ()=> {
                SetState(State.hold);
            };
        }
        else if (newState == State.hold)
        {
            mHoldingCD = 0.5f;
        }
        else if (newState == State.fly)
        {
            Trans.DOKill();
            float distance = Vector3.Distance(mEndPos, Trans.position);
            float time = distance / mSpeed;
            Trans.DOMove(mEndPos, time).onComplete += () => {
                SetState(State.targetHold);
            };
        }
        else if (newState == State.targetHold)
        {
            mHoldingCD = 0.3f;
        }
        else if (newState == State.explode)
        {
            Trans.DOKill();
            Trans.DOScale(0.1f, 0.3f).onComplete = () => {
                Trans.DOScale(0.1f, 0.3f).onComplete = null;

                DispatchFinishCallback();

                // try cache
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
            };
        }

        mState = newState;
    }
}
