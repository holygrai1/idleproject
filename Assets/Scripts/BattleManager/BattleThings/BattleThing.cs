using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 参与战斗的实体, 角色和怪物的基类
/// </summary>
public class BattleThing : MonoBehaviour
{
    #region member variables
    private GameObject mGo;
    private Transform mTrans;
    private List<Collider2D> mAllColliders;
    private int mUID;
    private ThingType mThingType;
    private Battle mBattle;
    private bool mPaused;
    private bool mDestroyed;
    // 多久更新一次
    protected float mUpdateSecPerFrame = 0.05f;
    #endregion

    #region getter
    public int UID => mUID;
    public ThingType ThingType => mThingType;
    public Battle Battle => mBattle;
    public bool Paused => mPaused;
    public bool Destroyed => mDestroyed;
    public GameObject Go => mGo;
    public Transform Trans => mTrans;
    public List<Collider2D> AllColliders => mAllColliders;
    #endregion

    // 初始化
    public virtual bool Init(ThingType thingType)
    {
        mGo = gameObject;
        mTrans = transform;
        mAllColliders = new List<Collider2D>();
        mAllColliders.AddRange(mTrans.GetComponentsInChildren<Collider2D>());
        mUID = mGo.GetInstanceID();
        mThingType = thingType;
        mPaused = false;
        return true;
    }

    // 进入战场
    public virtual void EnterBattle(Battle battle)
    {
        mBattle = battle;
        mBattle.OnThingEnterBattle(this);

        ScheduleManager.Instance.On(mUpdateSecPerFrame, OnUpdate);

        EventManager.Instance.DispatchThingEnterBattleEvent(battle, this);
    }
    // 离开战场
    public virtual void LeaveBattle()
    {
        if (mBattle == null)
        {
            return;
        }

        ScheduleManager.Instance.Off(OnUpdate);

        mBattle.OnThingLeaveBattle(this);

        var tempBattle = mBattle;
        mBattle = null;

        EventManager.Instance.DispatchThingLeaveBattleEvent(tempBattle, this);
    }

    // 开始战斗回调
    public virtual void OnStartBattle()
    {
    }

    // 暂停
    public virtual void Pause()
    {
        mPaused = true;
    }
    // 暂停恢复
    public virtual void Resume()
    {
        mPaused = false;
    }

    // 固定间隔更新
    public virtual void OnUpdate(float delta, float unscaleDelta)
    {

    }

    // 删除
    public virtual void Destroy()
    {
        if (mDestroyed == true)
        {
            return;
        }

        EventManager.Instance.DispatchThingDestroyEvent(this);

        mDestroyed = true;

        LeaveBattle();

        var tempGo = mGo;
        mGo = null;
        mTrans = null;
        mAllColliders = null;
        GameObject.Destroy(tempGo);
    }

    // 销毁时unity系统回调
    public void OnDestroy()
    {
        if (mDestroyed == false)
        {
            //LogManager.Error("必须通过Destroy()删除thing");
            return;
        }
    }
}
