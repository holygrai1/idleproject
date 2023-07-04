using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 飞行魔法
/// </summary>
public class BattleFlyMagic : BattleMagic
{
    private Vector3 mStartPos;
    private Vector3 mEndPos;
    private float mSpeed;
    
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
        return BattleMagicType.fly;
    }

    public override void EnterBattle(Battle battle)
    {
        base.EnterBattle(battle);
        Trans.position = mStartPos;
        Go.SetActive(true);

        if (battle.Started == true)
        {
            OnStartBattle();
        }
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);
        var newPos = Vector3.MoveTowards(Trans.position, mEndPos, delta * mSpeed);
        if (Helpers.IsReachPos(newPos, mEndPos, mEndPos - mStartPos) == true)
        {
            Trans.position = mEndPos;
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
        }
        else
        {
            Trans.position = newPos;
        }
    }
}
