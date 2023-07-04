using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// 格子类型
public enum BattleGridType
{
    highLevel,
    highDmg,
    crowd,

    max
}

// 战斗格子
public class BattleGrid
{
    // 所在位置
    public Vector2 pos;
    // 是否在左侧区
    public bool leftSide;
    // 类型
    public BattleGridType type;
    // 优先顺序
    public int priority;
    // 格子大小
    public float width;
    public float height;

    // 当前站着的角色
    public HashSet<BattleActor> actors = new HashSet<BattleActor>();

    public BattleGrid(BattleGridType type)
    {
        this.type = type;
    }
    public virtual void OnActorEnter(BattleActor actor)
    {
        actors.Add(actor);
    }
    public virtual void OnActorLeave(BattleActor actor)
    {
        actors.Remove(actor);
    }
    public bool HasActor(BattleActor actor)
    {
        return actors.Contains(actor);
    }
    public void ClearAllActor()
    {
        actors.Clear();
    }
}

// 战斗基类
public class Battle : MonoBehaviour
{
    public Transform HighLevelArea;
    public Transform HighDamageLeftArea;
    public Transform HighDamageRightArea;
    public Transform CrowdAreaStart;
    public Transform CrowdAreaEnd;
    public Transform MonsterArea;

    protected GameObject mGo;
    protected Transform mTrans;
    protected BattleInfo mInfo;
    protected BattleState mState;
    protected bool mPaused;
    protected bool mDestroyed;
    protected bool mStarted;
    protected float mAccTime;
    protected float mDuration;

    // 参与者索引, key: uid
    protected Dictionary<int, BattleThing> mAllThings = new Dictionary<int, BattleThing>();
    protected Dictionary<int, BattleActor> mAllActors = new Dictionary<int, BattleActor>();
    protected Dictionary<int, BattleMonster> mAllMonsters = new Dictionary<int, BattleMonster>();
    protected Dictionary<int, BattleMagic> mAllMagics = new Dictionary<int, BattleMagic>();

    // 正在创建的角色
    protected class WaitAction
    {
        public UserActionType type;
        public string extParam;
    }
    protected Dictionary<string, WaitAction> mCreatingActorUsers = new Dictionary<string, WaitAction>();
   
    // 格子相关
    // 高等级区网格站位
    protected List<BattleGrid> mHighLevelGrids = new List<BattleGrid>();
    protected List<BattleGrid> mHighLevelGridsPriority = new List<BattleGrid>();
    // 高伤害区左侧网格站位
    protected List<BattleGrid> mHighDmgLeftGrids = new List<BattleGrid>();
    protected List<BattleGrid> mHighDmgLeftGridsPriority = new List<BattleGrid>();
    // 高伤害区右侧网格站位
    protected List<BattleGrid> mHighDmgRightGrids = new List<BattleGrid>();
    protected List<BattleGrid> mHighDmgRightGridsPriority = new List<BattleGrid>();
    protected List<BattleGrid> mHighDmgGrids = new List<BattleGrid>();
    // 群众网格站位
    protected List<BattleGrid> mCrowdGrids = new List<BattleGrid>();
    protected List<BattleGrid> mCrowdLeftGrids = new List<BattleGrid>();
    protected List<BattleGrid> mCrowdRightGrids = new List<BattleGrid>();

    #region getter
    public GameObject Go => mGo;
    public Transform Trans => mTrans;
    public BattleInfo Info => mInfo;
    public BattleState State => mState;
    public bool Paused => mPaused;
    public bool Destroyed => mDestroyed;
    public bool Started => mStarted;
    public Dictionary<int, BattleMonster> AllMonster => mAllMonsters;
    public Dictionary<int, BattleActor> AllActors => mAllActors;
    public Dictionary<int, BattleMagic> AllMagic => mAllMagics;
    public Dictionary<int, BattleThing> AllThing => mAllThings;
    public float Duration => mDuration;
    public float RemainTime => Mathf.Max(mDuration - mAccTime, 0);
    #endregion

    public async virtual Task<bool> Init(BattleInfo info)
    {
        mInfo = info;
        mGo = gameObject;
        mTrans = transform;
        mState = BattleState.max;
        mPaused = false;
        mDestroyed = false;
        mStarted = false;
        mCreatingActorUsers.Clear();

        CreateGrids();

        EventManager.Instance.RegisterUserEnterEvent(OnUserAddHandle);
        EventManager.Instance.RegisterUserCommentEvent(OnUserCommentHandle);
        EventManager.Instance.RegisterUserLikeEvent(OnUserLikeHandle);
        EventManager.Instance.RegisterUserPrizeEvent(OnUserPrizeHandle);

        return true;
    }

    public virtual void BattleStart()
    {
        foreach (var kv in mAllThings)
        {
            kv.Value.OnStartBattle();
        }

        mStarted = true;
    }

    public virtual void OnThingEnterBattle(BattleThing thing)
    {
        var uid = thing.UID;
        var thingType = thing.ThingType;
        if (mAllThings.ContainsKey(thing.UID) == true)
        {
            LogManager.Error("Thing重复进入战斗. uid: " + uid + " thingType: " + thingType);
            return;
        }

        mAllThings.Add(uid, thing);
        if (ThingType.actor == thingType)
        {
            mAllActors.Add(uid, (BattleActor)thing);
        }
        else if (ThingType.monster == thingType)
        {
            mAllMonsters.Add(uid, (BattleMonster)thing);
        }
        else if (ThingType.magic == thingType)
        {
            mAllMagics.Add(uid, (BattleMagic)thing);
        }

        thing.Trans.SetParent(mTrans);
    }

    public virtual void OnThingLeaveBattle(BattleThing thing)
    {
        var uid = thing.UID;
        var thingType = thing.ThingType;
        mAllThings.Remove(thing.UID);

        if (ThingType.actor == thingType)
        {
            mAllActors.Remove(uid);
        }
        else if (ThingType.monster == thingType)
        {
            mAllMonsters.Remove(uid);
        }
        else if (ThingType.magic == thingType)
        {
            mAllMagics.Remove(uid);
        }

        thing.Trans.SetParent(null);
    }

    public virtual void SetState(BattleState state)
    {
        var preState = mState;
        mState = state;

        EventManager.Instance.DispatchBattleStateChangeEvent(this, preState, state);
    }

    public virtual void Pause()
    {
        if (mPaused == true)
        {
            return;
        }

        mPaused = true;

        foreach (var thing in mAllThings)
        {
            thing.Value.Pause();
        }
    }

    public virtual void Resume()
    {
        if (mPaused == false)
        {
            return;
        }

        mPaused = false;

        foreach (var kv in mAllThings)
        {
            kv.Value.Resume();
        }
    }

    public virtual void Destroy()
    {
        if (mDestroyed == true)
        {
            return;
        }

        mDestroyed = true;

        EventManager.Instance.UnregisterUserEnterEvent(OnUserAddHandle);
        EventManager.Instance.UnregisterUserCommentEvent(OnUserCommentHandle);
        EventManager.Instance.UnregisterUserLikeEvent(OnUserLikeHandle);
        EventManager.Instance.UnregisterUserPrizeEvent(OnUserPrizeHandle);

        var tempAllThings = new List<BattleThing>();
        foreach (var kv in mAllThings)
        {
            tempAllThings.Add(kv.Value);
        }
        foreach (var thing in tempAllThings)
        {
            thing.Destroy();
        }
        mAllThings.Clear();
        mAllThings = null;
        mAllActors.Clear();
        mAllActors = null;
        mAllMonsters.Clear();
        mAllMonsters = null;
        mAllMagics.Clear();
        mAllMagics = null;

        var tempGo = mGo;
        mGo = null;
        mTrans = null;
        mInfo = null;
        GameObject.Destroy(tempGo);
    }

    public void OnDestroy()
    {
        if (mDestroyed == false)
        {
            LogManager.Error("必须通过Destroy()删除Battle");
        }
    }

    protected void CreateGrids()
    {
        // 创建高等级格子
        var highLevelAreaStartPos = HighLevelArea.position;
        float size = 0.4f;
        var gap = -highLevelAreaStartPos.x * 2 - 6 * size;
        int[] highLevelPriotiyList = { 4, 2, 0, 1, 3, 5 };
        for (int i = 0; i < 6; ++i)
        {
            Vector2 pos = new Vector2(highLevelAreaStartPos.x + (i + 0.5f) * size, highLevelAreaStartPos.y);
            bool leftSize = i <= 2 ? true : false;
            if (leftSize == false)
            {
                pos.x += gap;
            }
            var grid = new BattleGrid(BattleGridType.highLevel) { leftSide = leftSize, pos = pos, priority = highLevelPriotiyList[i] };
            mHighLevelGrids.Add(grid);
            mHighLevelGridsPriority.Add(grid);
        }
        mHighLevelGridsPriority.Sort((BattleGrid g1, BattleGrid g2) => {
            if (g1.priority == g2.priority)
            {
                return 0;
            }
            else if (g1.priority > g2.priority)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        // 创建高伤害格子
        var highDmgLeftAreaStartPos = HighDamageLeftArea.position;
        float width = 0.3f;
        float height = 0.63f;
        int[] highDmgLeftPriorityList = {0, 1, 2, -1, -1,
                                         3, 4, 5,  6, -1,
                                         7, 8, 9, 10, 11,
                                        12,13,14, 15, 16};
        for (int i = 0, max = highDmgLeftPriorityList.Length; i < max; ++i)
        {
            int row = Mathf.FloorToInt(i / 5);
            int col = i % 5;
            int prio = highDmgLeftPriorityList[i];
            if (prio < 0)
            {
                continue;
            }
            float posX = highDmgLeftAreaStartPos.x + (col + 0.5f) * width;
            float posY = highDmgLeftAreaStartPos.y - row * height;
            Vector2 pos = new Vector2(posX, posY);
            var highDmgGrid = new BattleGrid(BattleGridType.highDmg) { leftSide = true, priority = prio, pos = pos };
            mHighDmgLeftGrids.Add(highDmgGrid);
            mHighDmgLeftGridsPriority.Add(highDmgGrid);
            mHighDmgGrids.Add(highDmgGrid);
        }
        mHighDmgLeftGridsPriority.Sort((BattleGrid g1, BattleGrid g2) => {
            if (g1.priority == g2.priority)
            {
                return 0;
            }
            else if (g1.priority > g2.priority)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        var highDmgRightAreaStartPos = HighDamageRightArea.position;
        int[] highDmgRightPriorityList = { -1, -1, 0, 1, 2,
                                           -1,  3, 4, 5, 6,
                                            7,  8, 9,10,11,
                                           12, 13,14,15,16};
        for (int i = 0, max = highDmgRightPriorityList.Length; i < max; ++i)
        {
            int row = Mathf.FloorToInt(i / 5);
            int col = i % 5;
            int prio = highDmgRightPriorityList[i];
            if (prio < 0)
            {
                continue;
            }
            float posX = highDmgRightAreaStartPos.x + (col + 0.5f) * width;
            float posY = highDmgRightAreaStartPos.y - row * height;
            Vector2 pos = new Vector2(posX, posY);
            var highDmgGrid = new BattleGrid(BattleGridType.highDmg) { leftSide = false, priority = prio, pos = pos };
            mHighDmgRightGrids.Add(highDmgGrid);
            mHighDmgRightGridsPriority.Add(highDmgGrid);
            mHighDmgGrids.Add(highDmgGrid);
        }
        mHighDmgRightGridsPriority.Sort((BattleGrid g1, BattleGrid g2) => {
            if (g1.priority == g2.priority)
            {
                return 0;
            }
            else if (g1.priority > g2.priority)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        // 创建群众格子
        var crowdAreaStartPos = CrowdAreaStart.position;
        var crowdAreaEndPos = CrowdAreaEnd.position;
        int crowdColNum = 14;
        int crowdRowNum = 5;
        float crowdWidth = (crowdAreaEndPos.x - crowdAreaStartPos.x) / crowdColNum;
        float crowdHeight = (crowdAreaStartPos.y - crowdAreaEndPos.y) / crowdRowNum;
        for (int row = 0; row < crowdRowNum; ++row)
        {
            for (int col = 0; col < crowdColNum; ++col)
            {
                float posX = crowdAreaStartPos.x + (col + 0.5f) * crowdWidth;
                float posY = crowdAreaStartPos.y - (row + 0.5f) * crowdHeight;
                Vector2 pos = new Vector2(posX, posY);
                bool leftSize = col < (crowdColNum / 2) ? true : false;
                var crowGrid = new BattleGrid(BattleGridType.crowd) { width = crowdWidth, height = crowdHeight, leftSide = leftSize, pos = pos };
                mCrowdGrids.Add(crowGrid);

                if (leftSize == true)
                {
                    mCrowdLeftGrids.Add(crowGrid);
                }
                else
                {
                    mCrowdRightGrids.Add(crowGrid);
                }
            }
        }

        //for (int i = 0; i < mHighLevelGrids.Count; ++i)
        //{
        //    var s = 0.521f;
        //    var centerPos = mHighLevelGrids[i].pos;
        //    var topLeftPos = centerPos - new Vector2(s * 0.5f, -s);
        //    var bottomRightPos = centerPos + new Vector2(s * 0.5f, 0);
        //    var topRightPos = topLeftPos + new Vector2(s, 0);
        //    var bottomLeftPos = topLeftPos + new Vector2(0, -s);
        //    Debug.DrawLine(topLeftPos, topRightPos, Color.red, 1000000);
        //    Debug.DrawLine(topLeftPos, bottomLeftPos, Color.red, 1000000);
        //    Debug.DrawLine(bottomLeftPos, bottomRightPos, Color.red, 1000000);
        //    Debug.DrawLine(topRightPos, bottomRightPos, Color.red, 1000000);
        //}

        //for (int i = 0; i < mHighDmgLeftGrids.Count; ++i)
        //{
        //    var centerPos = mHighDmgLeftGrids[i].pos;
        //    var w = 0.28f;
        //    var h = 0.597f;
        //    var topLeftPos = centerPos - new Vector2(w * 0.5f, -h);
        //    var bottomRightPos = centerPos + new Vector2(w * 0.5f, 0);
        //    var topRightPos = topLeftPos + new Vector2(w, 0);
        //    var bottomLeftPos = topLeftPos + new Vector2(0, -h);
        //    Debug.DrawLine(topLeftPos, topRightPos, Color.red, 1000000);
        //    Debug.DrawLine(topLeftPos, bottomLeftPos, Color.red, 1000000);
        //    Debug.DrawLine(bottomLeftPos, bottomRightPos, Color.red, 1000000);
        //    Debug.DrawLine(topRightPos, bottomRightPos, Color.red, 1000000);
        //}
        //for (int i = 0; i < mHighDmgRightGrids.Count; ++i)
        //{
        //    var centerPos = mHighDmgRightGrids[i].pos;
        //    var w = 0.28f;
        //    var h = 0.597f;
        //    var topLeftPos = centerPos - new Vector2(w * 0.5f, -h);
        //    var bottomRightPos = centerPos + new Vector2(w * 0.5f, 0);
        //    var topRightPos = topLeftPos + new Vector2(w, 0);
        //    var bottomLeftPos = topLeftPos + new Vector2(0, -h);
        //    Debug.DrawLine(topLeftPos, topRightPos, Color.red, 1000000);
        //    Debug.DrawLine(topLeftPos, bottomLeftPos, Color.red, 1000000);
        //    Debug.DrawLine(bottomLeftPos, bottomRightPos, Color.red, 1000000);
        //    Debug.DrawLine(topRightPos, bottomRightPos, Color.red, 1000000);
        //}

        //for (int i = 0; i < mCrowdGrids.Count; ++i)
        //{
        //    var centerPos = mCrowdGrids[i].pos;
        //    var w = mCrowdGrids[i].width;
        //    var h = mCrowdGrids[i].height;
        //    var topLeftPos = centerPos - new Vector2(w * 0.5f, -h * 0.5f);
        //    var bottomRightPos = centerPos + new Vector2(w * 0.5f, -h * 0.5f);
        //    var topRightPos = topLeftPos + new Vector2(w, 0);
        //    var bottomLeftPos = topLeftPos + new Vector2(0, -h);
        //    Debug.DrawLine(topLeftPos, topRightPos, Color.green, 1000000);
        //    Debug.DrawLine(topLeftPos, bottomLeftPos, Color.green, 1000000);
        //    Debug.DrawLine(bottomLeftPos, bottomRightPos, Color.green, 1000000);
        //    Debug.DrawLine(topRightPos, bottomRightPos, Color.green, 1000000);
        //}
    }


    // 随机获取一个人群区里玩家密度最小的一个格子
    public BattleGrid GetLeastActorsGridAtCrowdGrid()
    {
        int leastActorNum = int.MaxValue;
        List<int> leastActorNumGridIndexs = new List<int>();
        for (int i = 0, max = mCrowdGrids.Count; i < max; ++i)
        {
            var grid = mCrowdGrids[i];
            var actorNum = grid.actors.Count;
            if (actorNum < leastActorNum)
            {
                leastActorNum = actorNum;
                leastActorNumGridIndexs.Clear();
                leastActorNumGridIndexs.Add(i);
            }
            else if (actorNum == leastActorNum)
            {
                leastActorNumGridIndexs.Add(i);
            }
        }

        int rand = UnityEngine.Random.Range(0, (int)leastActorNumGridIndexs.Count);
        return mCrowdGrids[leastActorNumGridIndexs[rand]];
    }

    // 随机获取一个人群区里某一侧玩家密度最小的一个格子
    public BattleGrid GetLeastActorsGridAtCrowdGridOnlySide(bool leftSideOnly)
    {
        int leastActorNum = int.MaxValue;
        List<int> leastActorNumGridIndexs = new List<int>();
        for (int i = 0, max = mCrowdGrids.Count; i < max; ++i)
        {
            var grid = mCrowdGrids[i];
            if (grid.leftSide == leftSideOnly)
            {
                var actorNum = grid.actors.Count;
                if (actorNum < leastActorNum)
                {
                    leastActorNum = actorNum;
                    leastActorNumGridIndexs.Clear();
                    leastActorNumGridIndexs.Add(i);
                }
                else if (actorNum == leastActorNum)
                {
                    leastActorNumGridIndexs.Add(i);
                }
            }
        }

        int rand = UnityEngine.Random.Range(0, (int)leastActorNumGridIndexs.Count);
        return mCrowdGrids[leastActorNumGridIndexs[rand]];
    }

    // 新用户处理
    protected virtual void OnUserAddHandle(UserData addUserData)
    {
      
    }

    protected async void CreateActorByUserData(UserData userData, UserActionType type, string extParam)
    {
        WaitAction existingAction = null;
        mCreatingActorUsers.TryGetValue(userData.id, out existingAction);
        if (existingAction != null)
        {
            existingAction.extParam = extParam;
            existingAction.type = type;
            return;
        }
        else
        {
            mCreatingActorUsers.Add(userData.id, new WaitAction() { type = type, extParam = extParam });
        }

        var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
        var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
        var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
        var actorGO = await AssetManager.Instantiate(actorInfo.assetAddress, Trans);
        var actor = actorGO.GetComponent<BattleActor>();
        actor.InitActor(actorInfo, userData.id);
        actor.EnterBattle(this);
        var grid = GetLeastActorsGridAtCrowdGrid();
        float offsetX = UnityEngine.Random.Range(-grid.width * 0.5f, grid.width * 0.5f);
        float offsetY = UnityEngine.Random.Range(-grid.height * 0.5f, grid.height * 0.5f);
        actor.transform.position = grid.pos + new Vector2(offsetX, offsetY);
        actor.SetScale(false);
        actor.SetGrid(grid);

        mCreatingActorUsers.TryGetValue(userData.id, out existingAction);
        if (existingAction != null)
        {
            mCreatingActorUsers.Remove(userData.id);

            if (existingAction.type == UserActionType.comment)
            {
                if (string.IsNullOrEmpty(existingAction.extParam) == false)
                {
                    actor.OnUserActionTypeHandle(UserActionType.comment, existingAction.extParam);
                }
                return;
            }
        }

        if (State != BattleState.run)
        {
            actor.SetState(BattleCreatureState.wait);
        }
        else
        {
            if (existingAction != null)
            {
                actor.OnUserActionTypeHandle(existingAction.type, existingAction.extParam);
            }
        }
    }

    // 用户点评处理
    private void OnUserCommentHandle(UserData userData, string comment)
    {
        var selfData = ClientManager.Instance.SelfUserData;
        if (selfData.KillDragonUserID != userData.id)
        {
            foreach (var kv in AllActors)
            {
                var actor = kv.Value;
                if (actor.UserID == userData.id)
                {
                    actor.OnUserActionTypeHandle(UserActionType.comment, comment);
                    return;
                }
            }

            CreateActorByUserData(userData, UserActionType.comment, comment);
        }
        else
        {
            foreach (var kv in AllMonster)
            {
                var mst = kv.Value;
                if (mst.UserID == userData.id)
                {
                    mst.OnUserActionTypeHandle(UserActionType.comment, comment);
                    return;
                }
            }
        }
    }

    // 用户点赞处理
    private void OnUserLikeHandle(UserData userData)
    {
        foreach (var kv in AllMonster)
        {
            var mst = kv.Value;
            if (mst.UserID == userData.id)
            {
                mst.OnUserActionTypeHandle(UserActionType.like);
                return;
            }
        }

        foreach (var kv in AllActors)
        {
            var actor = kv.Value;
            if (actor.UserID == userData.id)
            {
                actor.OnUserActionTypeHandle(UserActionType.like);
                return;
            }
        }

        var selfData = ClientManager.Instance.SelfUserData;
        if (selfData.KillDragonUserID != userData.id)
        {
            CreateActorByUserData(userData, UserActionType.like, null);
        }
    }
    // 用户送礼处理
    private void OnUserPrizeHandle(UserData userData)
    {
        foreach (var kv in AllMonster)
        {
            var mst = kv.Value;
            if (mst.UserID == userData.id)
            {
                mst.OnUserActionTypeHandle(UserActionType.prize);
                return;
            }
        }

        foreach (var kv in AllActors)
        {
            var actor = kv.Value;
            if (actor.UserID == userData.id)
            {
                actor.OnUserActionTypeHandle(UserActionType.prize);
                return;
            }
        }

        var selfData = ClientManager.Instance.SelfUserData;
        if (selfData.KillDragonUserID != userData.id)
        {
            // 没找到响应的角色, 则新建
            CreateActorByUserData(userData, UserActionType.prize, null);
        }
    }
}
