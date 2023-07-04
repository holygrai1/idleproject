using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生物属性运算因子
/// </summary>
public class CreatureAttributeOperand
{
    private static long sUID = 0;

    protected float mValue;
    protected long mUID = ++sUID;
    protected CreatureAttributeSourceType mSourceType = CreatureAttributeSourceType.max;
    protected object mSource;
    protected CreatureAttributeOperandPos mPos;
    protected CreatureAttributeType mType;
    protected CreatureAttributeChangeType mChangeType;

    #region getter
    public long UID => mUID;
    public object Source => mSource;
    public CreatureAttributeSourceType SourceType => mSourceType;
    public CreatureAttributeOperandPos Pos => mPos;
    public CreatureAttributeType Type => mType;
    public CreatureAttributeChangeType ChangeType => mChangeType;
    public float Value => mValue;
    #endregion

    public CreatureAttributeOperand(float value, CreatureAttributeChangeType changeType, CreatureAttributeSourceType sourceType, object source)
    {
        mValue = value;
        mChangeType = changeType;
        mSourceType = sourceType;
        mSource = source;
        mPos = GetPosTypeByChangeType(changeType);
        mType = GetTypeByChangeType(changeType);
    }

    public float SetValue(float newValue)
    {
        var oldValue = mValue;
        mValue = newValue;
        return oldValue;
    }

    public static CreatureAttributeType GetTypeByChangeType(CreatureAttributeChangeType changeType)
    {
        switch (changeType)
        {
            case CreatureAttributeChangeType.attackPowerAdd:
            case CreatureAttributeChangeType.attackPowerPer:
                return CreatureAttributeType.attackPower;
            case CreatureAttributeChangeType.attackTimeAdd:
            case CreatureAttributeChangeType.attackTimePer:
                return CreatureAttributeType.attackSpeed;
            case CreatureAttributeChangeType.HPAdd:
            case CreatureAttributeChangeType.HPPer:
                return CreatureAttributeType.hp;
            default:
                return CreatureAttributeType.max;
        }
    }

    public static CreatureAttributeOperandPos GetPosTypeByChangeType(CreatureAttributeChangeType changeType)
    {
        switch (changeType)
        {
            case CreatureAttributeChangeType.attackPowerAdd:
            case CreatureAttributeChangeType.attackTimeAdd:
            case CreatureAttributeChangeType.HPAdd:
                return CreatureAttributeOperandPos.y;

            case CreatureAttributeChangeType.attackPowerPer:
            case CreatureAttributeChangeType.attackTimePer:
            case CreatureAttributeChangeType.HPPer:
                return CreatureAttributeOperandPos.x;

            default:
                return CreatureAttributeOperandPos.max;
        }
    }
}
