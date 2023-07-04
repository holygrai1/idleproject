using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 生物属性类
/// </summary>
public class CreatureAttribute
{
    protected CreatureAttributeType mType = CreatureAttributeType.max;
    protected float mValue = 0;
    protected float mBaseValue = 0;
    protected bool mDirty = true;

    protected List<CreatureAttributeOperand> mAllOperands = new List<CreatureAttributeOperand>();
    protected float mMinValue = float.MaxValue;
    protected float mMaxValue = float.MinValue;

    #region getter
    public CreatureAttributeType Type => mType;
    public bool Dirty => mDirty;
    public float Value => Update();
    public float ValueNoUpdate => mValue;
    public float BaseValue => mBaseValue;
    #endregion

    public CreatureAttribute(CreatureAttributeType type, float baseValue)
    {
        mType = type;
        mBaseValue = baseValue;
    }

    public virtual float Update()
    {
        if (mMinValue != float.MaxValue && mValue < mMinValue)
        {
            mValue = mMinValue;
        }

        if (mMaxValue != float.MinValue && mValue > mMaxValue)
        {
            mValue = mMaxValue;
        }
        mDirty = false;

        return mValue;
    }

    public void SetMinMaxValue(float min, float max)
    {
        if (mMinValue != min || mMaxValue != max)
        {
            mMinValue = min;
            mMaxValue = max;

            SetDirtyAndBroadcast();
        }
    }

    public float SetBaseValue(float value)
    {
        if (mBaseValue == value)
        {
            return mBaseValue;
        }

        var oldBaseValue = mBaseValue;
        mBaseValue = value;
        SetDirtyAndBroadcast();
        return oldBaseValue;
    }

    public void SetDirtyAndBroadcast()
    {
        if (mDirty == true)
        {
            return;
        }

        mDirty = true;
        EventManager.Instance.DispatchCreatureAttributeDirtyEvent(this);
    }

    public void AddOperand(CreatureAttributeOperand operand)
    {
        if (operand.Type != mType)
        {
            return;
        }

        mAllOperands.Add(operand);
        SetDirtyAndBroadcast();
    }

    public void AddOperands(List<CreatureAttributeOperand> operands)
    {
        foreach (var oper in operands)
        {
            AddOperand(oper);
        }
    }

    public CreatureAttributeOperand RemoveOperand(CreatureAttributeOperand operand)
    {
        for (int i = 0, max = mAllOperands.Count; i < max; ++i)
        {
            if (mAllOperands[i] == operand)
            {
                mAllOperands.RemoveAt(i);
                SetDirtyAndBroadcast();
                return operand;
            }
        }

        return null;
    }

    public void RemoveOperands(List<CreatureAttributeOperand> operands)
    {
        foreach (var opr in operands)
        {
            RemoveOperand(opr);
        }
    }

    public void RemoveAllOperands()
    {
        mAllOperands.Clear();
        SetDirtyAndBroadcast();
    }

    public bool HasOperand(CreatureAttributeOperand operand)
    {
        for (int i = 0, max = mAllOperands.Count; i < max; ++i)
        {
            if (mAllOperands[i] == operand)
            {
                return true;
            }
        }
        return false;
    }
}

