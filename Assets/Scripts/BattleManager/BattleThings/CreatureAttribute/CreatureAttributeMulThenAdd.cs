using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 计算公式为(BaseValue * (1 + x / 10000)  + y)
/// x, y是该位置的属性值累加结果
/// </summary>
public class CreatureAttributeMulThenAdd : CreatureAttribute
{
    public CreatureAttributeMulThenAdd(CreatureAttributeType type, float baseValue) : base(type, baseValue)
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

        mValue = mBaseValue * (1 + xSum * 0.0001f) + ySum;
        return base.Update();
    }
}

