using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 奖励的飞行魔法
/// </summary>
public class BattlePrizeBallMagic : BattleMagic
{
    private enum State
    {
        // 进场, 原地放大
        enter,
        // 原地停留
        hold,
        // 飞行
        fly,
        // 命中后爆炸
        explode,

        max
    }

    private Vector3 mStartPos;
    private Vector3 mEndPos;
    private float mSpeed;
    private State mState;
    private float mHoldingCD;
    private float mExplodeCD;
    
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
        else if (mState == State.explode)
        {
            mExplodeCD -= delta;
            if (mExplodeCD <= 0)
            {
                Trans.DOKill();

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
            }
        }
    }

    private void SetState(State newState)
    {
        if (newState == State.enter)
        {
            Trans.localScale = Vector3.zero;
            Trans.DOKill();
            Trans.DOScale(0.4f, 0.5f).onComplete += ()=> {
                SetState(State.hold);
            };
        }
        else if (newState == State.hold)
        {
            mHoldingCD = 1.0f;
        }
        else if (newState == State.fly)
        {
            Trans.DOKill();
            float distance = Vector3.Distance(mEndPos, Trans.position);
            float time = distance / mSpeed;
            Trans.DOMove(mEndPos, time).onComplete += () => {
                SetState(State.explode); ;
            };
            Trans.DOScale(0.8f, time);
        }
        else if (newState == State.explode)
        {
            DispatchFinishCallback();
            mExplodeCD = 0.0f;
        }

        mState = newState;
    }
}
