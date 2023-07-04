using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 普攻攻击间隔 最终攻击间隔=基础攻击间隔/（1+攻击速度加成）
/// 计算公式为(BaseValue / (1 + x / 10000) + y
/// x是该位置的属性值累加结果
/// </summary>
public class CreatureAttributeDiv : CreatureAttribute
{
    public CreatureAttributeDiv(CreatureAttributeType type, float baseValue) : base(type, baseValue)
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

        float div = (1 + xSum * 0.0001f);
        if (div == 0)
        {
            mValue = ySum;
        }
        else
        {
            mValue = mBaseValue / div + ySum;
        }

        return base.Update();
    }
}

