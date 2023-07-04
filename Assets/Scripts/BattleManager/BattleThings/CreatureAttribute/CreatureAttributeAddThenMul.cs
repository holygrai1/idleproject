using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 计算公式为(BaseValue + x) * (1 + y) / 10000
/// x, y是该位置的属性值累加结果
/// </summary>
public class CreatureAttributeAddThenMul : CreatureAttribute
{
    public CreatureAttributeAddThenMul(CreatureAttributeType type, float baseValue) : base(type, baseValue)
    { }

    public override float Update()
    {
        if (mDirty == false)
        {
            return mValue;
        }

        float xSum = 0;
        float ySum = 0;
        foreach (var operand in mAllOperands)
        {
            var pos = operand.Pos;
            if (pos == CreatureAttributeOperandPos.x)
            {
                xSum += operand.Value;
            }
            else if (pos == CreatureAttributeOperandPos.y)
            {
                ySum += operand.Value;
            }
        }

        mValue = (mBaseValue + xSum) * (10000 + ySum) * 0.0001f;
        return base.Update();
    }
}

