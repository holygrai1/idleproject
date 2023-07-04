using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 战斗中角色
/// </summary>
public class BattleActor : BattleCreature, IPointerDownHandler, IPointerUpHandler
{
    protected ActorInfo mActorInfo;
    protected BattleGrid mGrid;
    protected BattleGrid mMoveToGrid;
    protected Vector3 mMoveToGridOffset;
    protected float mMoveSpeed = 2.0f;
    protected int mFakeID = -1;
    protected string mFakeName = "";
    protected int mCurLevel;

    #region getter
    public ActorInfo ActorInfo => mActorInfo;
    public BattleGrid Grid => mGrid;
    public int FakeID => mFakeID;
    public string FakeName => mFakeName;

    public int CurLevel => mCurLevel;
    public BattleGrid MovingToGrid => mMoveToGrid;
    #endregion

    // 初始化
    public virtual bool InitActor(ActorInfo actorInfo, string userID)
    {
        if (InitCreature(ThingType.actor, actorInfo, userID) == false)
        {
            return false;
        }

        mActorInfo = actorInfo;

        var render = Go.GetComponentInChildren<Renderer>();
        render.sortingOrder = 200;

        if (string.IsNullOrEmpty(userID) == true)
        {
            mFakeID = UnityEngine.Random.Range(1, 31);
            Helpers.LoadSpriteAtlas("ActorIcon", mFakeID.ToString(), (Sprite sp) =>
            {
                mUserHeadSprite.sprite = sp;
            });

            mFakeName = DataManager.Instance.GetStringRes("name" + UnityEngine.Random.Range(1, 301));

            mCurLevel = 0;
        }
        else
        {
            var userData = ClientManager.Instance.GetUserData(userID);
            //Helpers.SetSpriteRenderFromURL(userData.headPic, mUserHeadSprite);
            Helpers.SetImageFromURL(userData.headPic, mUserHeadSprite);
            mCurLevel = DataManager.Instance.GetActorLevelByExp(userData.exp);
        }

        var levelInfo = DataManager.Instance.GetActorLevelInfo(mCurLevel);
        Helpers.LoadSpriteAtlas("ActorIcon", levelInfo.headIcon, (Sprite sp) =>
        {
            mUserHeadFg.sprite = sp;
        });

        mNoActionTimeAcc = 0;

        return true;
    }

    // 设置所在格子
    public void SetGrid(BattleGrid grid)
    {
        if (mGrid == grid)
        {
            return;
        }

        if (mGrid != null)
        {
            mGrid.OnActorLeave(this);
        }

        mGrid = grid;
        if (grid != null)
        {
            grid.OnActorEnter(this);

            if (grid.leftSide == true)
            {
                FaceRight(true);
            }
            else
            {
                FaceRight(false);
            }

            if (grid.type == BattleGridType.crowd)
            {
                // 放慢攻速, 减慢一倍
                CreatureAttributeOperand operand = new CreatureAttributeOperand(-8333, CreatureAttributeChangeType.attackTimePer, CreatureAttributeSourceType.grid, grid);
                GetAttributeByType(CreatureAttributeType.attackSpeed).AddOperand(operand);
            }
            else
            {
                GetAttributeByType(CreatureAttributeType.attackSpeed).RemoveAllOperands();
            }
        }
    }

    public override void UpdateTarget()
    {
        var allMst = Battle.AllMonster;
        if (allMst.Count > 0)
        {
            var firstMst = allMst.First().Value;
            if (firstMst.Destroyed == false && firstMst.Dead == false)
            {
                SetTarget(firstMst);
            }
        }

        base.UpdateTarget();
    }

    public override void Destroy()
    {
        if (Destroyed == false)
        {
            SetGrid(null);

            base.Destroy();
        }
    }

    // 移动到目标格子并设置所在格子
    public void MoveToGrid(BattleGrid grid)
    {
        if (grid == null)
        {
            Debug.LogError("无法移动到空格子");
            return;
        }

        mMoveToGrid = grid;
        if (mMoveToGrid.type == BattleGridType.crowd)
        {
            float xOffset = UnityEngine.Random.Range(-mMoveToGrid.width * 0.5f, mMoveToGrid.width * 0.5f);
            float yOffset = UnityEngine.Random.Range(-mMoveToGrid.height * 0.5f, mMoveToGrid.height * 0.5f);
            mMoveToGridOffset = new Vector3(xOffset, yOffset, 0);
        }
        else
        {
            mMoveToGridOffset = Vector3.zero;
        }

        if (mMoveToGrid.type == BattleGridType.highLevel)
        {
            SetScale(true);
        }
        else
        {
            SetScale(false);
        }

        if (mGrid != null)
        {
            if (mMoveToGrid.pos.x > mGrid.pos.x)
            {
                FaceRight(true);
            }
            else
            {
                FaceRight(false);
            }

            SetGrid(null);
        }

        mCurAnimationCompleteHandlers = null;
        mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.walk, true);
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        if (Paused == false)
        {
            if (mMoveToGrid != null)
            {
                var targetPos = new Vector3(mMoveToGrid.pos.x, mMoveToGrid.pos.y, 0) + mMoveToGridOffset;
                var curPos = Trans.position;
                Trans.position = Vector3.MoveTowards(curPos, targetPos, delta * mMoveSpeed);
                if (Helpers.IsReachPos(curPos, targetPos, targetPos - curPos) == true)
                {
                    if (mMoveToGrid.type != BattleGridType.crowd)
                    {
                        if (mMoveToGrid.actors.Count > 0)
                        {
                            var existingActor = mMoveToGrid.actors.First();
                            var crowdGrid = Battle.GetLeastActorsGridAtCrowdGridOnlySide(mMoveToGrid.leftSide);
                            existingActor.MoveToGrid(crowdGrid);
                        }
                    }

                    SetGrid(mMoveToGrid);
                    mMoveToGrid = null;

                    if (mState != BattleCreatureState.wait)
                    {
                        SetState(BattleCreatureState.idle);
                    }
                    else
                    {
                        mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);
                    }
                }
                else
                {
                    var idleAni = mSkeletonAnimation.skeleton.Data.FindAnimation(CreatureAnimationName.idle);
                    if (mSkeletonAnimation.state.GetCurrent(0).Animation == idleAni)
                    {
                        mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.walk, true);
                    }
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UIManager.Instance.ShowWnd(WndType.battleActorInfoWnd);
        UIManager.Instance.SendMsg(WndType.battleActorInfoWnd, WndMsgType.initContent, this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UIManager.Instance.HideWnd(WndType.battleActorInfoWnd);
    }
}
