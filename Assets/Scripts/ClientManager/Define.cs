using System;
using UnityEngine;

#region Thing相关
/// <summary>
/// Thing类型
/// </summary>
public enum ThingType
{
    actor,

    monster,

    magic,

    max
}

/// <summary>
/// 魔法类型
/// </summary>
public enum BattleMagicType
{
    // 直线飞行魔法 
    fly,
    // 固定位置
    fix,
    // 奖励的飞行魔法
    prize,

    max
}
#endregion

#region 战斗状态相关
// 战斗状态类型
public enum BattleState
{
    prepare,
    run,
    win,
    lose,
    next,

    max
}

// 战斗类型
public enum BattleType
{
    // 屠龙
    killDragon,
    // 挑战
    challenge,

    max
}

#endregion

#region 生物相关
/// <summary>
/// 生物属性类型
/// </summary>
public enum CreatureAttributeType
{
    // 血量, 0
    hp,
    // 攻击力
    attackPower,
    // 攻击速度, 1
    attackSpeed,

    max
}

/// <summary>
/// 生物属性源类型
/// </summary>
public enum CreatureAttributeSourceType
{
    skill,

    grid,

    max
}

/// <summary>
/// 生物属性计算数(operand)的位置类型
/// </summary>
public enum CreatureAttributeOperandPos
{
    x,
    y,
    z,

    max,
}
/// <summary>
/// 生物属性改变类型
/// </summary>
public enum CreatureAttributeChangeType
{
    // 攻击力加百分比， baseValue * (1 + x / 10000)
    attackPowerPer,

    // 攻击力加固定值, baseValue + x
    attackPowerAdd,

    // 生命加百分比， baseValue * (1 + x / 10000)
    HPPer,

    // 生命加固定值, baseValue + x
    HPAdd,

    // 攻速加百分比, baseValue / (1 + x / 10000);
    attackTimePer,

    // 攻速加固定值, baseValue + x
    attackTimeAdd,

    max
}

/// <summary>
/// 状态类
/// </summary>
public enum StatusType
{
   
}

/// <summary>
/// 生物动画名
/// </summary>
public class CreatureAnimationName
{
    // 待机
    public static readonly string idle = "idle";
    // 攻击
    public static readonly string attack = "atk";
    // 移动
    public static readonly string walk = "walk";
    // 倒下
    public static readonly string fall = "fall";
    // 进场
    public static readonly string enterBattle = "in";
    // 被击
    public static readonly string beHit = "hit";
    // 死亡
    public static readonly string dead = "dead";
    // 治疗
    public static readonly string heal = "heal";
}

/// <summary>
/// 生物动画事件名
/// </summary>
public class CreatureAnimationEvent
{
    public static readonly string AttackFrame = "AttackFrame";
}

/// <summary>
/// 生物槽点
/// </summary>
public enum CreatureSlotType
{
    head,

    body,

    max
}

/// <summary>
/// 生物状态
/// </summary>
public enum BattleCreatureState
{
    // 待机
    idle,
    // 攻击
    attack,
    // 使用技能
    skill,
    // 死亡
    die,
    // 进场
    enter,
    // 等待
    wait,

    max
}

/// <summary>
/// 怪物类型, 于ai和能力相关
/// </summary>
public enum MonsterType
{
    normal,

    max
}

/// <summary>
/// 玩家类型
/// </summary>
public enum ActorType
{
    normal,

    max
}


/// <summary>
/// 技能类型
/// </summary>
public enum SkillType
{
    // 普攻
    normalAttack,
    // 快速光球攻击
    fastLightBallAttack,
    // 速爱心特效攻击
    heartBallAttack,
    // 加血
    heal,
    // 送礼伤害
    prizeBall,
    // 送礼加血
    prizeHeal,

    max
}

// 玩家行为类型
public enum UserActionType
{
    // 评论
    comment,
    // 点赞
    like,
    // 送礼
    prize,
    // 用户进入
    enter,
    // 用户离开
    leave,

    max
}

// 心跳事件类型
public enum BeatHeartEventType
{
    // 用户动作, 点评, 点赞, 送礼, 用户进入
    userAction,

    max
}

#endregion

public class Define 
{
    public static readonly int NPCLayer = LayerMask.NameToLayer("NPC");
    public static readonly int ActorLayer = LayerMask.NameToLayer("Actor");
    public static readonly int UILayer = LayerMask.NameToLayer("UI");
    public static readonly int MagicLayer = LayerMask.NameToLayer("Magic");
    public static readonly int HPBarLayer = LayerMask.NameToLayer("HPBar");

    public static readonly int NPCLayerMask = 1 << NPCLayer;
    public static readonly int ActorLayerMask = 1 << ActorLayer;
    public static readonly int UILayerMask = 1 << UILayer;
    public static readonly int MagicLayerMask = 1 << MagicLayer;
    public static readonly int HPBarLayerMask = 1 << HPBarLayer;

    public static readonly int AllThingMask = NPCLayerMask | ActorLayerMask | MagicLayerMask;
}

/// <summary>
/// 默认层的显示排序
/// </summary>
public class SortingLayer_Default
{
    public static readonly string name = "Default";

    public static readonly int MonsterHeadTop = 100;
    public static readonly int FriendRecruitIcon = 200;
    public static readonly int ActorHeadTop = 300;

    public static readonly int NPCBubble = 800;
    public static readonly int HeadTopTalk = 1000;
}
/// <summary>
/// UI层的显示排序
/// </summary>
public class SortingLayer_UI
{
    public static readonly string name = "UI";

}
