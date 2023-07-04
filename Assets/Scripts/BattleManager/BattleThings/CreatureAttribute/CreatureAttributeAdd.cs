using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 计算公式为BaseValue + x
/// x是该位置的属性值累加结果
/// </summary>
public class CreatureAttributeAdd : CreatureAttribute
{
    public CreatureAttributeAdd(CreatureAttributeType type, float baseValue) : base(type, baseValue)
    { }

    public override float Update()
    {
        if (mDirty == false)
        {
            return mValue;
        }

        float addFirstPosValueSum = 0;
        foreach (var operand in mAllOperands)
        {
            if (operand.Pos == CreatureAttributeOperandPos.x)
            {
                addFirstPosValueSum += operand.Value;
            }
        }

        mValue = mBaseValue + addFirstPosValueSum;
        return base.Update();
    }
}

