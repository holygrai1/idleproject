using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔法类, 使用技能时创建相应的魔法表现
/// </summary>
public class BattleMagic : BattleThing
{
    protected string mAssetAddress;
    // 完成回调
    protected Action<BattleMagic> mFinishCallback;

    public string AssetAddress => mAssetAddress;

    // 注册完成回调
    public void RegisterFinishCallback(Action<BattleMagic> handler)
    {
        mFinishCallback += handler;
    }
    // 注销完成回调
    public void UnregisterFinishCallback(Action<BattleMagic> handler)
    {
        mFinishCallback -= handler;
    }
    public void DispatchFinishCallback()
    {
        mFinishCallback?.Invoke(this);
    }

    // 获取魔法类型
    public virtual BattleMagicType GetMagicType()
    {
        return BattleMagicType.max;
    }

    // 初始化
    public virtual bool InitMagic(params object[] extParams)
    {
        if (base.Init(ThingType.magic) == false)
        {
            return false;
        }

        return true;
    }

    public override void Destroy()
    {
        base.Destroy();

        mFinishCallback = null;
    }

    public void SetAssetAddress(string address)
    {
        mAssetAddress = address;
    }
}
