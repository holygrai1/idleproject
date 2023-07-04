using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 事件管理器
/// </summary>
public class EventManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static EventManager sInstance;
    public static EventManager Instance => sInstance;
    private void Awake()
    {
        if (sInstance != null && sInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sInstance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    public async Task<bool> Init()
    {
        return await Task.FromResult(true);
    }

    #region 玩家事件
    // 玩家数据改变事件
    private event Action<UserData> mUserDataChangeEvent;
    // 主播数据改变事件
    private event Action<SelfUserData> mSelfUserDataChangeEvent;
    // 新加入的玩家事件
    private event Action<UserData> mUserDataEnterEvent;

    // 用户点评事件
    private event Action<UserData, string> mUserCommentEvent;
    // 用户点赞事件
    private event Action<UserData> mUserLikeEvent;
    // 用户送礼事件
    private event Action<UserData> mUserPrizeEvent;
    // 用户离开事件
    private event Action<string> mUserLeaveEvent;

    public void RegisterUserDataChangeEvent(Action<UserData> handler)
    {
        mUserDataChangeEvent += handler;
    }
    public void UnregisterUserDataChangeEvent(Action<UserData> handler)
    {
        mUserDataChangeEvent -= handler;
    }
    public void DispatchUserDataChangeEvent(UserData data)
    {
        mUserDataChangeEvent?.Invoke(data);
    }

    public void RegisterSelfUserDataChangeEvent(Action<SelfUserData> handler)
    {
        mSelfUserDataChangeEvent += handler;
    }
    public void UnregisterSelfUserDataChangeEvent(Action<SelfUserData> handler)
    {
        mSelfUserDataChangeEvent -= handler;
    }
    public void DispatchSelfUserDataChangeEvent(SelfUserData data)
    {
        mSelfUserDataChangeEvent?.Invoke(data);
    }

    public void RegisterUserEnterEvent(Action<UserData> handler)
    {
        mUserDataEnterEvent += handler;
    }
    public void UnregisterUserEnterEvent(Action<UserData> handler)
    {
        mUserDataEnterEvent -= handler;
    }
    public void DispatchUserEnterEvent(UserData data)
    {
        mUserDataEnterEvent?.Invoke(data);
    }

    public void RegisterUserCommentEvent(Action<UserData, string> handler)
    {
        mUserCommentEvent += handler;
    }
    public void UnregisterUserCommentEvent(Action<UserData, string> handler)
    {
        mUserCommentEvent -= handler;
    }
    public void DispatchUserCommentEvent(UserData data, string comment)
    {
        mUserCommentEvent?.Invoke(data, comment);
    }

    public void RegisterUserLikeEvent(Action<UserData> handler)
    {
        mUserLikeEvent += handler;
    }
    public void UnregisterUserLikeEvent(Action<UserData> handler)
    {
        mUserLikeEvent -= handler;
    }
    public void DispatchUserLikeEvent(UserData data)
    {
        mUserLikeEvent?.Invoke(data);
    }

    public void RegisterUserPrizeEvent(Action<UserData> handler)
    {
        mUserPrizeEvent += handler;
    }
    public void UnregisterUserPrizeEvent(Action<UserData> handler)
    {
        mUserPrizeEvent -= handler;
    }
    public void DispatchUserPrizeEvent(UserData data)
    {
        mUserPrizeEvent?.Invoke(data);
    }

    public void RegisterUserLeaveEvent(Action<string> handler)
    {
        mUserLeaveEvent += handler;
    }
    public void UnregisterUserLeaveEvent(Action<string> handler)
    {
        mUserLeaveEvent -= handler;
    }
    public void DispatchUserLeaveEvent(string userID)
    {
        mUserLeaveEvent?.Invoke(userID);
    }

    #endregion

    #region 战斗事件
    // 战斗实体(thing)已进入战斗事件
    private event Action<Battle, BattleThing> mThingEnterBattleEvent;
    // 战斗实体(thing)已离开战斗事件
    private event Action<Battle, BattleThing> mThingLeaveBattleEvent;
    // 战斗实体(thing)删除事件(删除开始前)
    private event Action<BattleThing> mThingDestroyEvent;
    // 战斗状态改变事件
    private event Action<Battle, BattleState, BattleState> mBattleStateChangeEvent;
    // 生物死亡(creature)事件
    private event Action<BattleCreature, BattleCreature, string> mCreatureDieEvent;
    // 生物状态改变事件
    private event Action<BattleCreature, BattleCreatureState, BattleCreatureState> mCreatureStateChangeEvent;

    public void RegisterThingEnterBattleEvent(Action<Battle, BattleThing> handler)
    {
        mThingEnterBattleEvent += handler;
    }
    public void UnregisterThingEnterBattleEvent(Action<Battle, BattleThing> handler)
    {
        mThingEnterBattleEvent -= handler;
    }
    public void DispatchThingEnterBattleEvent(Battle battle, BattleThing thing)
    {
        mThingEnterBattleEvent?.Invoke(battle, thing);
    }

    public void RegisterThingLeaveBattleEvent(Action<Battle, BattleThing> handler)
    {
        mThingLeaveBattleEvent += handler;
    }
    public void UnregisterThingLeaveBattleEvent(Action<Battle, BattleThing> handler)
    {
        mThingLeaveBattleEvent -= handler;
    }
    public void DispatchThingLeaveBattleEvent(Battle battle, BattleThing thing)
    {
        mThingLeaveBattleEvent?.Invoke(battle, thing);
    }

    public void RegisterThingDestroyEvent(Action<BattleThing> handler)
    {
        mThingDestroyEvent += handler;
    }
    public void UnregisterThingDestroyEvent(Action<BattleThing> handler)
    {
        mThingDestroyEvent -= handler;
    }
    public void DispatchThingDestroyEvent(BattleThing thing)
    {
        mThingDestroyEvent?.Invoke(thing);
    }

    public void RegisterBattleStateChangeEvent(Action<Battle, BattleState, BattleState> handler)
    {
        mBattleStateChangeEvent += handler;
    }
    public void UnregisterBattleStateChangeEvent(Action<Battle, BattleState, BattleState> handler)
    {
        mBattleStateChangeEvent -= handler;
    }
    public void DispatchBattleStateChangeEvent(Battle battle, BattleState preState, BattleState newState)
    {
        mBattleStateChangeEvent?.Invoke(battle, preState, newState);
    }

    public void RegisterCreatureDieEvent(Action<BattleCreature, BattleCreature, string> handler)
    {
        mCreatureDieEvent += handler;
    }
    public void UnregisterCreatureDieEvent(Action<BattleCreature, BattleCreature, string> handler)
    {
        mCreatureDieEvent -= handler;
    }
    public void DispatchCreatureDieEvent(BattleCreature dieCreature, BattleCreature killCreature, string killCreatureName)
    {
        mCreatureDieEvent?.Invoke(dieCreature, killCreature, killCreatureName);
    }

    public void RegisterCreatureStateChangeEvent(Action<BattleCreature, BattleCreatureState, BattleCreatureState> handler)
    {
        mCreatureStateChangeEvent += handler;
    }
    public void UnregisterCreatureStateChangeEvent(Action<BattleCreature, BattleCreatureState, BattleCreatureState> handler)
    {
        mCreatureStateChangeEvent -= handler;
    }
    public void DispatchCreatureStateChangeEvent(BattleCreature creature, BattleCreatureState preState, BattleCreatureState newState)
    {
        mCreatureStateChangeEvent?.Invoke(creature, preState, newState);
    }
    #endregion

    #region 生物属性事件
    // 生物属性数据改变事件
    private event Action<CreatureAttribute> mCreatureAttributeDirtyEvent;
    public void RegisterCreatureAttributeDirtyEvent(Action<CreatureAttribute> handler)
    {
        mCreatureAttributeDirtyEvent += handler;
    }
    public void UnregisterCreatureAttributeDirtyEvent(Action<CreatureAttribute> handler)
    {
        mCreatureAttributeDirtyEvent -= handler;
    }
    public void DispatchCreatureAttributeDirtyEvent(CreatureAttribute att)
    {
        mCreatureAttributeDirtyEvent?.Invoke(att);
    }
    #endregion
}
