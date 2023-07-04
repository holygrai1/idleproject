using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// 屠龙战斗
public class KillDragonBattle : Battle
{
    // 当前boss
    protected BattleMonster mCurBoss;

    protected float mNextTimeAcc;
    // 高等级区玩家超过多久不操作则被认为离线
    protected float mHighLevelAreaMaxAllowNoActionTime = 10 * 60;
    // 每隔多久更新高等级区玩家调整
    protected float mHighLevelAreaActorAdjustTime = 1.0f;
    // 剩余多久时间更新高等级区玩家调整
    protected float mHighLevelAreaActorAdjustCD;

    // 每隔多久更新高伤害区玩家调整
    protected float mHighDmgAreaActorAdjustTime = 10f;
    // 剩余多久时间更新高伤害区玩家调整
    protected float mHighDmgAreaActorAdjustCD;
    // 每次更新前x最高伤害的角色
    protected int mHighDmgActorUpdateNum = 12;

    // 结束后多久
    protected float mEndAccTime;
    // 结束后多久弹结束窗口
    protected float mEndTimeToShowResult = 2.0f;

    // 结束多久后新一批玩家上来/下去时间
    protected float mEndTimeWhenNewActorsUp = 2.5f;
    // 本次战斗伤害最高玩家列表
    protected List<BattleActor> mRankActors = new List<BattleActor>();

    // 当前boss名字和头像
    protected string mBossName;
    protected string mBossUrl;

    // 是否正在创建ai角色中
    protected bool mCreatingAIUser = false;

    // 结束步骤
    protected enum EndStep
    {
        waitToShowResult,
        oldActorMoveDown,
        newActorMoveUp,
        done,
        max
    }
    protected EndStep mCurEndStep;

    // 准备步骤
    protected enum NextStep
    {
        oldActorMoveDown,
        newActorMoveUp,
        done,

        max
    }
    protected NextStep mCurNextStep;

    public string BossName => mBossName;
    public string BossUrl => mBossUrl;
    public BattleMonster CurBoss => mCurBoss;

    public async override Task<bool> Init(BattleInfo info)
    {
        if (await base.Init(info) == false)
        {
            return false;
        }

        await CreateBoss();
        await CreateTopLevelAreaActors();
        await CreateTopDmgAreaActors();
        await CreateCrowdAreaActors();
        return true;
    }

    public override void BattleStart()
    {
        mCurBoss.gameObject.SetActive(true);

        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        mDuration = killDragonBattleData.duration;
        var actorNum = mAllActors.Count;
        float rand = UnityEngine.Random.Range(1.0f - killDragonBattleData.rand, 1.0f + killDragonBattleData.rand);
        float hp = (actorNum + killDragonBattleData.extFillUserNum + 1) * killDragonBattleData.ratio * rand;
        mCurBoss.GetAttributeByType(CreatureAttributeType.hp).SetBaseValue(hp);
        SetState(BattleState.prepare);

        ScheduleManager.Instance.On(0.1f, OnUpdate);
    }

    public async virtual void StartNextBoss()
    {
        // 清空上一次战斗留下的伤害值
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            actor.ClearDmg();
        }

        // 升级能升级的玩家
        await UpdateActorsLevel();

        await CreateBoss();
        mCurBoss.gameObject.SetActive(false);

        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        mDuration = killDragonBattleData.duration;
        var actorNum = mAllActors.Count;
        float rand = UnityEngine.Random.Range(1.0f - killDragonBattleData.rand, 1.0f + killDragonBattleData.rand);
        float hp = (actorNum + killDragonBattleData.extFillUserNum) * killDragonBattleData.ratio * rand;
        mCurBoss.GetAttributeByType(CreatureAttributeType.hp).SetBaseValue(hp);
        mCurBoss.PrepareHeadTop();

        SetState(BattleState.next);

        Resume();
    }

    public async virtual void TryAgain()
    { 
        // 清空上一次战斗留下的伤害值
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            actor.ClearDmg();
        }

        // 升级能升级的玩家
        await UpdateActorsLevel();

        await CreateBoss();
        mCurBoss.gameObject.SetActive(false);

        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        mDuration = killDragonBattleData.duration;
        var actorNum = mAllActors.Count;
        float rand = UnityEngine.Random.Range(1.0f - killDragonBattleData.rand, 1.0f + killDragonBattleData.rand);
        float hp = (actorNum + killDragonBattleData.extFillUserNum) * killDragonBattleData.ratio * rand;
        mCurBoss.GetAttributeByType(CreatureAttributeType.hp).SetBaseValue(hp);
        mCurBoss.PrepareHeadTop();

        SetState(BattleState.next);

        Resume();
    }

    protected async Task CreateBoss()
    {
        // kill last boss
        if (mCurBoss != null)
        {
            mCurBoss.Destroy();
            mCurBoss = null;
        }

        var selfUserData = ClientManager.Instance.SelfUserData;

        // 轮流替换boss
        string[] arrBossIDs = { "4", "5", "1", "2" };
      
        int bossIndex = 0;
        if (PlayerPrefs.HasKey("bossIndex") == false)
        {
            PlayerPrefs.SetInt("bossIndex", 0);
        }
        else
        {
            bossIndex = PlayerPrefs.GetInt("bossIndex");
            bossIndex = bossIndex % arrBossIDs.Length;
        }

        // 创建boss, 目前默认用id为1的怪物, 以后可根据内容扩展
        var bossInfo = DataManager.Instance.GetMonsterInfo(arrBossIDs[bossIndex]);
        var bossGO = await AssetManager.Instantiate(bossInfo.assetAddress, Trans);
        var boss = bossGO.GetComponent<BattleMonster>();

        if (string.IsNullOrEmpty(selfUserData.KillDragonUserID) == false)
        {
            boss.InitMonster(bossInfo, selfUserData.KillDragonUserID);
        }
        else
        {
            boss.InitMonster(bossInfo, selfUserData.ID);
        }

        boss.EnterBattle(this);
        boss.transform.position = MonsterArea.transform.position;
        mCurBoss = boss;
        mCurBoss.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(selfUserData.KillDragonUserName) == false)
        {
            mBossName = selfUserData.KillDragonUserName;
        }
        else
        {
            mBossName = selfUserData.Name;
        }

        if (string.IsNullOrEmpty(selfUserData.KillDragonUserHeadUrl) == false)
        {
            mBossUrl = selfUserData.KillDragonUserHeadUrl;
        }
        else
        {
            mBossUrl = selfUserData.HeadPicUrl;
        }
    }

    // 创建高等级区玩家
    protected async Task CreateTopLevelAreaActors()
    {
        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        var allUserData = ClientManager.Instance.AllUserDatas;
        var selfUserData = ClientManager.Instance.SelfUserData;
        // 创建角色
        // 高等级区
        List<UserData> highLevelAreaCandidate = new List<UserData>();
        foreach (var userData in allUserData)
        {
            if (userData.id == selfUserData.KillDragonUserID)
            {
                continue;
            }

            int level = DataManager.Instance.GetActorLevelByExp(userData.exp);
            if (level >= killDragonBattleData.highLevelAreaLevelNeed)
            {
                highLevelAreaCandidate.Add(userData);
            }
        }
        highLevelAreaCandidate.Sort((UserData user1, UserData user2) => {
            if (user1.exp == user2.exp)
            {
                return 0;
            }
            else if (user1.exp > user2.exp)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        });

        if (highLevelAreaCandidate.Count > killDragonBattleData.highLevelAreaMaxUserNum)
        {
            highLevelAreaCandidate.RemoveRange(killDragonBattleData.highLevelAreaMaxUserNum , highLevelAreaCandidate.Count - killDragonBattleData.highLevelAreaMaxUserNum);
        }

        int index = 0;
        foreach (var userData in highLevelAreaCandidate)
        {
            if (index >= mHighLevelGridsPriority.Count)
            {
                break;
            }

            var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
            var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
            var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
            var actorGO = await AssetManager.Instantiate(actorInfo.assetAddress, Trans);
            var actor = actorGO.GetComponent<BattleActor>();
            actor.InitActor(actorInfo, userData.id);
            actor.EnterBattle(this);

            var grid = mHighLevelGridsPriority[index];
            actor.transform.position = grid.pos;
            actor.FaceRight(grid.leftSide);
            actor.SetScale(true);
            actor.SetGrid(grid);


           // userData.AddJoinDragonNum(1);

            ++index;
        }
    }

    // 获取适合高等级区域的角色列表, 从高到低排列, 排除正在移动中的角色
    protected List<BattleActor> GetHighLevelCandidateActorsInOrder()
    {
        List<BattleActor> actors = new List<BattleActor>();
        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            if (string.IsNullOrEmpty(actor.UserID) == false && actor.MovingToGrid == null)
            {
                var userData = ClientManager.Instance.GetUserData(actor.UserID);
                int level = DataManager.Instance.GetActorLevelByExp(userData.exp);
                if (level >= killDragonBattleData.highLevelAreaLevelNeed)
                {
                    actors.Add(actor);
                }
            }
        }
        actors.Sort((BattleActor a1, BattleActor a2) =>
        {
            var user1 = ClientManager.Instance.GetUserData(a1.UserID);
            var user2 = ClientManager.Instance.GetUserData(a2.UserID);

            if (user1.exp == user2.exp)
            {
                return 0;
            }
            else if (user1.exp > user2.exp)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        });

        if (actors.Count > killDragonBattleData.highLevelAreaMaxUserNum)
        {
            actors.RemoveRange(killDragonBattleData.highLevelAreaMaxUserNum, actors.Count - killDragonBattleData.highLevelAreaMaxUserNum);
        }

        return actors;
    }

    // 创建高伤害区玩家
    protected async Task CreateTopDmgAreaActors()
    {
        var allUserData = ClientManager.Instance.AllUserDatas;
        var selfUserData = ClientManager.Instance.SelfUserData;
        // 高伤害区
        List<UserData> highDamageAreaCandidate = new List<UserData>();
        foreach (var userData in allUserData)
        {
            if (userData.id == selfUserData.KillDragonUserID)
            {
                continue;
            }

            var found = mHighLevelGrids.Find((BattleGrid grid) => {
                if (grid.actors.Count > 0)
                {
                    var actor = grid.actors.First();
                    if (actor.UserID == userData.id)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });

            if (found == null)
            {
                highDamageAreaCandidate.Add(userData);
            }
        }
        // 随机抽取
        highDamageAreaCandidate.Shuffle();

        if (highDamageAreaCandidate.Count > 34)
        {
            highDamageAreaCandidate.RemoveRange(34, highDamageAreaCandidate.Count - 34);
        }

        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        int actualFillNum = Mathf.Min(34 - highDamageAreaCandidate.Count, killDragonBattleData.extFillUserNum);
        if (actualFillNum > 0)
        {
            // 需填充的ai人数
            for (int i = 0; i < actualFillNum; ++i)
            {
                highDamageAreaCandidate.Add(new UserData());
            }
        }
    
        List<Task<GameObject>> createActorGoTask = new List<Task<GameObject>>();
        for (int i = 0, max = highDamageAreaCandidate.Count; i < 34 && i < max; ++i)
        {
            var level = DataManager.Instance.GetActorLevelByExp(highDamageAreaCandidate[i].exp);
            var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
            var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
            var task = AssetManager.Instantiate(actorInfo.assetAddress, Trans);
            createActorGoTask.Add(task);
        }
        GameObject[] actorGOs = await Task.WhenAll(createActorGoTask);

        for (int i = 0, max = highDamageAreaCandidate.Count; i < 17 && i < max; ++i)
        {
            var userData = highDamageAreaCandidate[i];
            var actorGO = actorGOs[i];
            var actor = actorGO.GetComponent<BattleActor>();
            var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
            var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
            var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
            actor.InitActor(actorInfo, userData.id);
            actor.EnterBattle(this);

            var grid = mHighDmgLeftGridsPriority[i];
            actor.transform.position = grid.pos;
            actor.SetScale(false);
            actor.SetGrid(grid);

           // userData.AddJoinDragonNum(1);
        }

        for (int i = 17, max = highDamageAreaCandidate.Count; i < 34 && i < max; ++i)
        {
            var userData = highDamageAreaCandidate[i];
            var actorGO = actorGOs[i];
            var actor = actorGO.GetComponent<BattleActor>();
            var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
            var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
            var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
            actor.InitActor(actorInfo, userData.id);
            actor.EnterBattle(this);

            var grid = mHighDmgRightGridsPriority[i - 17];
            actor.transform.position = grid.pos;
            actor.SetScale(false);
            actor.SetGrid(grid);

           // userData.AddJoinDragonNum(1);
        }
    }

    // 创建群众区玩家
    protected async Task CreateCrowdAreaActors()
    {
        // 群众区
        var allUserData = ClientManager.Instance.AllUserDatas;
        var selfUserData = ClientManager.Instance.SelfUserData;
        int counter = 0;

        List<Task<GameObject>> createActorGoTask = new List<Task<GameObject>>();
        List<UserData> createActorUserData = new List<UserData>();

        for (int i = 0, max = allUserData.Count; i < max; ++i)
        {
            var userData = allUserData[i];
            if (userData.id == selfUserData.KillDragonUserID)
            {
                continue;
            }

            var found = mHighDmgLeftGrids.Find((BattleGrid grid) => {
                if (grid.actors.Count > 0)
                {
                    var actor = grid.actors.First();
                    if (actor.UserID == userData.id)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });

            if (found == null)
            {
                found = mHighDmgRightGrids.Find((BattleGrid grid) => {
                    if (grid.actors.Count > 0)
                    {
                        var actor = grid.actors.First();
                        if (actor.UserID == userData.id)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                });
            }

            if (found == null)
            {
                found = mHighLevelGrids.Find((BattleGrid grid) => {
                    if (grid.actors.Count > 0)
                    {
                        var actor = grid.actors.First();
                        if (actor.UserID == userData.id)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                });
            }

            if (found == null && counter < 100)
            {
                var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
                var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
                var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
                var task = AssetManager.Instantiate(actorInfo.assetAddress, Trans);
                createActorGoTask.Add(task);
                createActorUserData.Add(userData);

              //  userData.AddJoinDragonNum(1);
                ++counter;
            }
        }

        GameObject[] actorGOs = await Task.WhenAll(createActorGoTask);

        for (int i = 0; i < actorGOs.Length; ++i)
        {
            var actor = actorGOs[i].GetComponent<BattleActor>();
            var userData = createActorUserData[i];
            var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
            var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
            var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
            actor.InitActor(actorInfo, userData.id);
            actor.EnterBattle(this);

            var grid = GetLeastActorsGridAtCrowdGrid();
            float offsetX = UnityEngine.Random.Range(-grid.width * 0.5f, grid.width * 0.5f);
            float offsetY = UnityEngine.Random.Range(-grid.height * 0.5f, grid.height * 0.5f);
            actor.transform.position = grid.pos + new Vector2(offsetX, offsetY);
            actor.SetScale(false);
            actor.SetGrid(grid);
        }
    }

    // 新用户处理
    protected async override void OnUserAddHandle(UserData userData)
    {
     //   var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
     //   var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
     //   var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
     //   var actorGO = await AssetManager.Instantiate(actorInfo.assetAddress, Trans);
     //   var actor = actorGO.GetComponent<BattleActor>();
     //   actor.InitActor(actorInfo, userData.id);
     //   actor.EnterBattle(this);
     //   var grid = GetLeastActorsGridAtCrowdGrid();
     //   float offsetX = UnityEngine.Random.Range(-grid.width * 0.5f, grid.width * 0.5f);
     //   float offsetY = UnityEngine.Random.Range(-grid.height * 0.5f, grid.height * 0.5f);
     //   actor.transform.position = grid.pos + new Vector2(offsetX, offsetY);
     //   actor.SetScale(false);
     //   actor.SetGrid(grid);

     //   if (string.IsNullOrEmpty(userData.Comment) == false)
     //   {
     //       actor.OnUserActionTypeHandle(UserActionType.comment, userData.Comment);
     //   }

     ////   userData.AddJoinDragonNum(1);

     //   if (State != BattleState.run)
     //   {
     //       actor.SetState(BattleCreatureState.wait);
     //   }
    }

    public override void SetState(BattleState state)
    {
        if (state == BattleState.lose)
        {
            mEndAccTime = 0;
            mCurEndStep = EndStep.waitToShowResult;

            GetActorRankList();

            foreach (var kv in mAllActors)
            {
                var actor = kv.Value;
                actor.SetState(BattleCreatureState.wait);
            }
        }
        else if (state == BattleState.win)
        {
            mEndAccTime = 0;
            mCurEndStep = EndStep.waitToShowResult;

            GetActorRankList();

            foreach (var kv in mAllActors)
            {
                var actor = kv.Value;
                actor.SetState(BattleCreatureState.wait);
            }
        }
        else if (state == BattleState.prepare)
        {
            mNextTimeAcc = 0;
            mCurBoss.gameObject.SetActive(true);
            mCurBoss.SetState(BattleCreatureState.enter);

            foreach (var kv in mAllActors)
            {
                var actor = kv.Value;
                actor.SetState(BattleCreatureState.wait);
            }

            mCurBoss.OnEnterDone = () => {

                mCurNextStep = NextStep.done;
                mCurBoss.OnEnterDone = null;
                base.BattleStart();
                SetState(BattleState.run);
            };
        }
        else if (state == BattleState.run)
        {
            mHighDmgAreaActorAdjustCD = mHighDmgAreaActorAdjustTime;
            mHighLevelAreaActorAdjustCD = mHighLevelAreaActorAdjustTime;
            mAccTime = 0;

            foreach (var kv in mAllActors)
            {
                var actor = kv.Value;
                actor.SetState(BattleCreatureState.idle);
            }
        }else if (state == BattleState.next)
        {
            mAccTime = 0;

            foreach (var kv in mAllActors)
            {
                var actor = kv.Value;
                actor.SetState(BattleCreatureState.wait);
            }

            mCurNextStep = NextStep.oldActorMoveDown;

            for (int i = 0; i < mHighDmgGrids.Count; ++i)
            {
                var grid = mHighDmgGrids[i];
                if (grid.actors.Count > 0)
                {
                    grid.actors.First().MoveToGrid(GetLeastActorsGridAtCrowdGrid());
                }
            }
        }

        base.SetState(state);
    }

    private void OnUpdate(float delta, float unscaleDelta)
    {
        if (Paused == true || Destroyed == true)
        {
            return;
        }

        if (State == BattleState.run)
        {
            mAccTime += delta;
            if (mAccTime >= mDuration)
            {
                // lose
                SetState(BattleState.lose);
                return;
            }
            else
            {
                if (mCurBoss.Dead == true)
                {
                    SetState(BattleState.win);
                    return;
                }
            }

            // 更新高等级区
            mHighLevelAreaActorAdjustCD -= delta;
            if (mHighLevelAreaActorAdjustCD <= 0)
            {
                mHighLevelAreaActorAdjustCD = mHighLevelAreaActorAdjustTime;

                // 移除断线玩家
                for (int i = 0; i < mHighLevelGrids.Count; ++i)
                {
                    var grid = mHighLevelGrids[i];
                    if (grid.actors.Count > 0)
                    {
                        var actor = grid.actors.First();
                        if (string.IsNullOrEmpty(actor.UserID) == false && actor.NoActionTimeAcc > mHighLevelAreaMaxAllowNoActionTime)
                        {
                            actor.Destroy();
                        }
                    }
                }

                var highLevelAreaCandidate = GetHighLevelCandidateActorsInOrder();
                for (int i = 0; i < mHighLevelGrids.Count; ++i)
                {
                    var grid = mHighLevelGrids[i];
                    if (grid.actors.Count > 0)
                    {
                        var actor = grid.actors.First();
                        if (highLevelAreaCandidate.Contains(actor) == true)
                        {
                            // 当前格子放的是前n名的高等级玩家
                            continue;
                        }
                    }

                    // 当前格子无玩家或者玩家不在前N名的高等级列表里
                    if (highLevelAreaCandidate.Count > 0)
                    {
                        for (int j = 0; j < highLevelAreaCandidate.Count; ++j)
                        {
                            BattleActor newActor = highLevelAreaCandidate[j];
                            if (newActor.Grid != null && newActor.Grid.type == BattleGridType.highLevel)
                            {
                                continue;
                            }

                            highLevelAreaCandidate.RemoveAt(0);

                            if (grid.actors.Count > 0)
                            {
                                // 当前格子放的玩家不在前N名的高等级列表里
                                var oldActor = grid.actors.First();
                                var crowGrid = GetLeastActorsGridAtCrowdGridOnlySide(grid.leftSide);
                                oldActor.MoveToGrid(crowGrid);
                            }

                            newActor.transform.position = grid.pos;
                            newActor.SetScale(true);
                            newActor.SetGrid(grid);

                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // 更新高伤害区
            mHighDmgAreaActorAdjustCD -= delta;
            if (mHighDmgAreaActorAdjustCD <= 0)
            {
                mHighDmgAreaActorAdjustCD = mHighDmgAreaActorAdjustTime;
                // 按伤害排序, 高到低排, ai排最后, 正在移动中的不考虑
                List<BattleActor> actorsInDmgOrder = new List<BattleActor>();
                for (int i = 0; i < mHighDmgGrids.Count; ++i)
                {
                    var grid = mHighDmgGrids[i];
                    if (grid.actors.Count > 0)
                    {
                        actorsInDmgOrder.Add(grid.actors.First());
                    }
                }
                for (int i = 0; i < mCrowdGrids.Count; ++i)
                {
                    var grid = mCrowdGrids[i];
                    if (grid.actors.Count > 0)
                    {
                        actorsInDmgOrder.AddRange(grid.actors);
                    }
                }

                actorsInDmgOrder.Sort((BattleActor a1, BattleActor a2) => {
                    bool isAI1 = string.IsNullOrWhiteSpace(a1.UserID);
                    bool isAI2 = string.IsNullOrWhiteSpace(a2.UserID);
                    if (isAI1 == true && isAI2 == true)
                    {
                        return 0;
                    }
                    else if (isAI1 == true && isAI2 == false)
                    {
                        return 1;
                    }
                    else if (isAI1 == false && isAI2 == true)
                    {
                        return -1;
                    }

                    var userData1 = ClientManager.Instance.GetUserData(a1.UserID);
                    var userData2 = ClientManager.Instance.GetUserData(a2.UserID);
                    if (userData1.dmg == userData2.dmg)
                    {
                        return 0;
                    }
                    else if (userData1.dmg > userData2.dmg)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });
                if (actorsInDmgOrder.Count > mHighDmgActorUpdateNum)
                {
                    actorsInDmgOrder.RemoveRange(mHighDmgActorUpdateNum, actorsInDmgOrder.Count - mHighDmgActorUpdateNum);
                }

                // 找出高伤害区格子中 可以替换角色的格子
                List<BattleGrid> allCanChangeActorGrids = new List<BattleGrid>();
                for (int i = 0, max = mHighDmgLeftGrids.Count; i < max; ++i)
                {
                    var checkingGrid = mHighDmgLeftGrids[i];
                    if (checkingGrid.actors.Count == 0)
                    {
                        // 无actor的格子
                        allCanChangeActorGrids.Add(checkingGrid);
                    }
                    else
                    {
                        // 判断此格子是否占了个高伤害的角色(前mHighDmgActorUpdateNum)
                        var oldActor = checkingGrid.actors.First();
                        if (actorsInDmgOrder.Contains(oldActor) == false)
                        {
                            allCanChangeActorGrids.Add(checkingGrid);
                        }
                    }
                }
                for (int i = 0, max = mHighDmgRightGrids.Count; i < max; ++i)
                {
                    var checkingGrid = mHighDmgRightGrids[i];
                    if (checkingGrid.actors.Count == 0)
                    {
                        // 无actor的格子
                        allCanChangeActorGrids.Add(checkingGrid);
                    }
                    else
                    {
                        // 判断此格子是否占了个高伤害的角色(前34)
                        var oldActor = checkingGrid.actors.First();
                        if (actorsInDmgOrder.Contains(oldActor) == false)
                        {
                            allCanChangeActorGrids.Add(checkingGrid);
                        }
                    }
                }

                // 优先填满没占角色的格子
                allCanChangeActorGrids.Sort((BattleGrid g1, BattleGrid g2) => {
                    var g1HasActor = g1.actors.Count > 0 ? true : false;
                    var g2HasActor = g2.actors.Count > 0 ? true : false;

                    if (g1HasActor == true && g2HasActor == false)
                    {
                        return -1;
                    }
                    else if (g1HasActor == false && g2HasActor == true)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                });

                for (int i = 0, max = allCanChangeActorGrids.Count; i < max; ++i)
                {
                    var canChangeActorGrid = allCanChangeActorGrids[i];

                    if (actorsInDmgOrder.Count <= 0)
                    {
                        break;
                    }

                    for (int j = 0, jMax = actorsInDmgOrder.Count; j < jMax; ++j)
                    {
                        var actorHighDmg = actorsInDmgOrder[j];
                        var gridActorHighDmg = actorHighDmg.Grid;
                        if (gridActorHighDmg != null && gridActorHighDmg.leftSide == canChangeActorGrid.leftSide && gridActorHighDmg.type == BattleGridType.crowd)
                        {
                            // 同一侧且在最下面的群众区的玩家移动到高伤害区
                            actorHighDmg.MoveToGrid(canChangeActorGrid);
                            actorsInDmgOrder.RemoveAt(j);
                            break;
                        }
                    }
                }
            }

            CheckNeedCreateAIActor();

            RemoveUnActionActor(delta);
        }
        else if (State == BattleState.next)
        {
            if (mCurNextStep == NextStep.oldActorMoveDown)
            {
                mNextTimeAcc += delta;
                if (mNextTimeAcc >= mEndTimeWhenNewActorsUp)
                {
                    mNextTimeAcc = 0;
                    mCurNextStep = NextStep.newActorMoveUp;
                    List<BattleActor> crowdActors = new List<BattleActor>();
                    for (int i = 0, max = mCrowdGrids.Count; i < max; ++i)
                    {
                        var grid = mCrowdGrids[i];
                        if (grid.actors.Count > 0)
                        {
                            crowdActors.AddRange(grid.actors);
                        }
                    }

                    crowdActors.Shuffle();

                    for (int i = 0, max = mHighDmgGrids.Count; i < max; ++i)
                    {
                        var grid = mHighDmgGrids[i];
                        if (grid.actors.Count == 0)
                        {
                            if (crowdActors.Count > 0)
                            {
                                var actor = crowdActors[0];
                                actor.MoveToGrid(grid);
                                crowdActors.RemoveAt(0);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else if (mCurNextStep == NextStep.newActorMoveUp)
            {
                mNextTimeAcc += delta;
                if (mNextTimeAcc >= mEndTimeWhenNewActorsUp)
                {
                    SetState(BattleState.prepare);
                }
            }
        }
        else if (State == BattleState.lose || State == BattleState.win)
        {
            if (mCurEndStep == EndStep.waitToShowResult)
            {
                mEndAccTime += delta;
                if (mEndAccTime >= mEndTimeToShowResult)
                {
                    mCurEndStep = EndStep.done;
                    OnBattleFinish();
                }
            }
        }
    }

    private void OnBattleFinish()
    {
        // 给boss经验
        if (mCurBoss.CommentNumAcc > 0 || mCurBoss.LikeNumAcc > 0 || mCurBoss.PrizeNumAcc > 0)
        {
            if (string.IsNullOrEmpty(mCurBoss.UserID) == false)
            {
                var userData = ClientManager.Instance.GetUserData(mCurBoss.UserID);
                if (userData != null)
                {
                    userData.AddExp(mInfo.bossActiveExp);
                }
            }
        }

        // 给角色经验
        int index = 0;
        bool nextDragonSet = false;
        var selfData = ClientManager.Instance.SelfUserData;
        foreach (var actor in mRankActors)
        {
            if (string.IsNullOrEmpty(actor.UserID) == false)
            {
                var userData = ClientManager.Instance.GetUserData(actor.UserID);
                if (actor.CommentNumAcc > 0 || actor.LikeNumAcc > 0 || actor.PrizeNumAcc > 0)
                {
                    var giveExp = Helpers.GetWithinArray(mInfo.actorActiveExpByRank, index);
                    userData.AddExp(giveExp);
                    ++index;
                }
                else
                {
                    userData.AddExp(mInfo.actorNoactiveExp);
                }

                if (State == BattleState.win)
                {
                    userData.AddKillDragonNum(1);

                    if (nextDragonSet == false)
                    {
                        nextDragonSet = true;

                        selfData.SetKillDragonUser(userData);
                    }
                }

                userData.AddJoinDragonNum(1);
            }
        }

        // 停止所有角色的技能
        BattleActor bossActor = null;
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            actor.SkillController.StopAllRunningSkill();

            if (actor.UserID == selfData.KillDragonUserID)
            {
                bossActor = actor;
            }
        }

        bossActor?.Destroy();

        SendBattleFinishResult();
    }

    private async void SendBattleFinishResult()
    {
        UIManager.Instance.ShowWnd(WndType.killDragonBattleResultWnd);
        UIManager.Instance.SendMsg(WndType.killDragonBattleResultWnd, WndMsgType.initContent, mRankActors);

        //if (mCurBoss.MonsterInfo.id == "1")
        //{
        //    PlayerPrefs.SetString("bossID", "2");
        //}
        //else
        //{
        //    PlayerPrefs.SetString("bossID", "1");
        //}

        int curBossIndex = PlayerPrefs.GetInt("bossIndex");
        PlayerPrefs.SetInt("bossIndex", curBossIndex + 1);

        await UpdateActorsLevel();

        var ret = await ClientManager.Instance.FinishBattle();
        if (ret == false)
        {
            // 失败后1秒再试
            ScheduleManager.Instance.Once(1, (float delta, float unscaleDelta) => {
                SendBattleFinishResult();
            });
        }
    }

    private async Task UpdateActorsLevel()
    {
        List<BattleActor> canUpgradeActors = new List<BattleActor>();
        List<UserData> canUpgradeUserData = new List<UserData>();
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            if (string.IsNullOrEmpty(actor.UserID) == false)
            {
                var userData = ClientManager.Instance.GetUserData(actor.UserID);
                if (userData != null)
                {
                    int newLevel = DataManager.Instance.GetActorLevelByExp(userData.exp);
                    if (newLevel > actor.CurLevel)
                    {
                        // upgrade
                        canUpgradeActors.Add(actor);
                        canUpgradeUserData.Add(userData);
                    }
                }
            }
        }

        for (int i = 0, max = canUpgradeActors.Count; i < max; ++i)
        {
            var actor = canUpgradeActors[i];
            var userData = canUpgradeUserData[i];
            var grid = actor.Grid;
            actor.Destroy();

            var level = DataManager.Instance.GetActorLevelByExp(userData.exp);
            var levelInfo = DataManager.Instance.GetActorLevelInfo(level);
            var actorInfo = DataManager.Instance.GetActorInfo(levelInfo.actorID);
            var actorGO = await AssetManager.Instantiate(actorInfo.assetAddress, Trans);
            var upgradeActor = actorGO.GetComponent<BattleActor>();
            upgradeActor.InitActor(actorInfo, userData.id);
            upgradeActor.EnterBattle(this);
            upgradeActor.transform.position = grid.pos;
            upgradeActor.FaceRight(grid.leftSide);
            upgradeActor.SetScale(true);
            upgradeActor.SetGrid(grid);

            if (State != BattleState.run)
            {
                upgradeActor.SetState(BattleCreatureState.wait);
            }
        }
    }

    // 获取累积伤害排名的角色列表
    private List<BattleActor> GetActorRankList()
    {
        mRankActors.Clear();
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            mRankActors.Add(actor);
        }

        mRankActors.Sort((BattleActor a1, BattleActor a2) => {

            bool a1AI = string.IsNullOrEmpty(a1.UserID);
            bool a2AI = string.IsNullOrEmpty(a2.UserID);

            if (a1AI == true && a2AI == false)
            {
                return 1;
            }
            else if (a1AI == false && a2AI == true)
            {
                return -1;
            }

            if (a1.DmgAcc == a2.DmgAcc)
            {
                return 0;
            }
            else if (a1.DmgAcc > a2.DmgAcc)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        });
        return mRankActors;
    }

    List<BattleActor> mTempActorAI = new List<BattleActor>();
    private List<BattleActor> GetCurAIActors()
    {
        mTempActorAI.Clear();
        foreach (var kv in mAllActors)
        {
            var actor = kv.Value;
            if (actor != null && actor.Destroyed == false && actor.Dead == false && string.IsNullOrEmpty(actor.UserID) == true)
            {
                mTempActorAI.Add(actor);
            }
        }
        return mTempActorAI;
    }

    private async void CheckNeedCreateAIActor()
    {
        if (mCreatingAIUser == true)
        {
            return;
        }

        mCreatingAIUser = true;

        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        var actorAIs = GetCurAIActors();
        int needNum = killDragonBattleData.extFillUserNum - actorAIs.Count;
        if (needNum > 0)
        {
            // 需填充的ai人数
            for (int i = 0; i < needNum; ++i)
            {
                var userData = new UserData();
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

           //     userData.AddJoinDragonNum(1);
                if (State != BattleState.run)
                {
                    actor.SetState(BattleCreatureState.wait);
                }
            }
        }
        else if (needNum < 0)
        {
            needNum = -needNum;

            // reduce num
            for (int i = 0; i < needNum && i < actorAIs.Count; ++i)
            {
                actorAIs[i].Destroy();

                await new WaitForEndOfFrame();
            }
            actorAIs.Clear();
        }

        mCreatingAIUser = false;
    }

    private float mCheckRemoveUnactionActorCD = 10.0f;
    // 移除10分钟不动的非ai角色
    private void RemoveUnActionActor(float delta)
    {
        mCheckRemoveUnactionActorCD -= delta;
        if (mCheckRemoveUnactionActorCD <= 0)
        {
            mCheckRemoveUnactionActorCD = 10.0f;

            List<BattleActor> actors = new List<BattleActor>();

            foreach (var kv in mAllActors)
            {
                var actor = kv.Value;
                if (string.IsNullOrEmpty(actor.UserID) == false)
                {
                    if (actor.NoActionTimeAcc >= 60 * 5)
                    {
                        actors.Add(actor);
                    }
                }
            }

            foreach (var actor in actors)
            {
                actor.Destroy();
            }
        }
    }
}
