using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 全局调度管理器, 专门负责全局的统一管理， 比如按循序的执行任务
/// </summary>
public class ScheduleManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static ScheduleManager sInstance;
    public static ScheduleManager Instance => sInstance;
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

            mScheduleUpdateItemList = new LinkedList<sScheduleUpdateItem>();
            mScheduleUpdateItemDic = new Dictionary<ScheduleUpdateHandler, LinkedListNode<sScheduleUpdateItem>>();
            mFreeScheduleUpdateItemNodeQueue = new Queue<LinkedListNode<sScheduleUpdateItem>>();
            mOffItemList = new Queue<LinkedListNode<sScheduleUpdateItem>>();
        }
    }
    #endregion

    class sScheduleUpdateItem
    {
        public float deltaTime;
        public float accTime;
        public float accUnscaleTime;
        public int loopNum;
        public ScheduleUpdateHandler handler;
        public bool off;
    }

    public delegate void ScheduleUpdateHandler(float deltaTime, float unscaleDeltaTime);

    bool mInited = false;
    bool mPaused = false;
    // 实际游戏时长, 减去暂停部分
    float mTimePassedInGame = 0;
    int mMaxFreeNodeNum = 128;
    LinkedList<sScheduleUpdateItem> mScheduleUpdateItemList;
    Dictionary<ScheduleUpdateHandler, LinkedListNode<sScheduleUpdateItem>> mScheduleUpdateItemDic;
    Queue<LinkedListNode<sScheduleUpdateItem>> mFreeScheduleUpdateItemNodeQueue;
    Queue<LinkedListNode<sScheduleUpdateItem>> mOffItemList;
    public bool SpeedUp = false;
    float mSpeedUpScale = 1.8f;

    #region getter
    public bool inited { get => mInited; }
    public float timePassedInGame { get => mTimePassedInGame; }
    #endregion

    #region public method

    public async Task<bool> Init()
    {
        if (mInited)
        {
            return true;
        }

        mInited = true;

        return true;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        mPaused = true;
    }
    /// <summary>
    /// 暂停后回复
    /// </summary>
    public void Resume()
    {
        mPaused = false;
    }

    /// <summary>
    /// 开启加速
    /// </summary>
    public void StartSpeedUp()
    {
        LogManager.Log("StartSpeedUp");

        SpeedUp = true;

        Time.timeScale = mSpeedUpScale;
    }

    /// <summary>
    /// 结束加速
    /// </summary>
    public void StopSpeedUp()
    {
        SpeedUp = false;

        Time.timeScale = 1;
    }

    /// <summary>
    /// 是否已经加速
    /// </summary>
    /// <returns></returns>
    public bool IsSpeedUp()
    {
        return SpeedUp;
    }

    /// <summary>
    /// 获取加速比例
    /// </summary>
    /// <returns></returns>
    public float GetSpeedUpScale()
    {
        return mSpeedUpScale;
    }

    /// <summary>
    /// 设置加速的倍速
    /// </summary>
    /// <param name="speedScale"></param>
    public void SetSpeedUpScale(float speedScale = 1.0f)
    {
        mSpeedUpScale = speedScale;

        if (SpeedUp == true)
        {
            Time.timeScale = mSpeedUpScale;
        }
    }

    /// <summary>
    /// 订阅一次更新
    /// </summary>
    /// <param name="deltaTime">更新时间，秒</param>
    /// <param name="handler">时间到后回调函数</param>
    public void Once(float deltaTime, ScheduleUpdateHandler handler)
    {
        On(deltaTime, handler, 1);
    }

    /// <summary>
    /// 订阅多次更新
    /// </summary>
    /// <param name="deltaTime">更新时间，秒</param>
    /// <param name="handler">时间到后回调函数</param>
    /// <param name="loopNum">更新次数</param>
    public void On(float deltaTime, ScheduleUpdateHandler handler, int loopNum = int.MaxValue)
    {
        LinkedListNode<sScheduleUpdateItem> existNode;
        if (mScheduleUpdateItemDic.TryGetValue(handler, out existNode) == true)
        {
            var existItem = existNode.Value;
            if (existItem.off == false)
            {
                //Debug.LogError("ScheduleUpdate, 同一个函数不能同时注册多个更新器");
            }
            else
            {
                // 同一帧被删除后又添加
                existItem.deltaTime = deltaTime;
                existItem.accTime = 0;
                existItem.accUnscaleTime = 0;
                existItem.handler = handler;
                existItem.loopNum = loopNum;
                existItem.off = false;
            }

            return;
        }

        var node = GetFreeScheduleItemNode();
        var item = node.Value;
        item.deltaTime = deltaTime;
        item.accTime = 0;
        item.accUnscaleTime = 0;
        item.handler = handler;
        item.loopNum = loopNum;
        item.off = false;
        mScheduleUpdateItemList.AddLast(node);
        mScheduleUpdateItemDic.Add(handler, node);
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="handler"></param>
    public void Off(ScheduleUpdateHandler handler)
    {
        LinkedListNode<sScheduleUpdateItem> node;
        if (mScheduleUpdateItemDic.TryGetValue(handler, out node) == true)
        {
            node.Value.off = true;
            //node.Value.handler = null;
        }
    }

    #endregion

    #region implementation
    //public async void OnLogout()
    //{
    //    ClientManager.Instance.OnLogout();
    //    BattleManager.Instance.curBattleMain.DestroyBattle();
    //    Thing.DestroyAllThing();

    //    mLogoCanvas.SetActive(true);

    //    await new WaitForSeconds(2);

    //    StartBtnClick();
    //}

    void Update()
    {
        if (mInited == false || mPaused == true)
        {
            return;
        }

#if UNITY_EDITOR
        if (SpeedUp == true)
        {
            if (Time.timeScale != mSpeedUpScale)
            {
                Time.timeScale = mSpeedUpScale;
            }
        }
        else
        {
            if (Time.timeScale != 1)
            {
                Time.timeScale = 1;
            }
        }
#endif

        var fixedTime = Time.deltaTime;
        var unscaleTime = Time.unscaledDeltaTime;
        mTimePassedInGame += fixedTime;
        if (mScheduleUpdateItemList == null)
        {
            return;
        }

        var node = mScheduleUpdateItemList.First;
        LinkedListNode<sScheduleUpdateItem> nextNode = null;
        sScheduleUpdateItem item = null;
        while (node != null)
        {
            nextNode = node.Next;
            item = node.Value;

            if (item.off == true)
            {
                mOffItemList.Enqueue(node);
                node = nextNode;
                continue;
            }

            item.accTime += fixedTime;
            item.accUnscaleTime += unscaleTime;
            if (item.accTime >= item.deltaTime)
            {
                // fixed: real time rather than defined time
                item.handler(item.accTime, item.accUnscaleTime);
                item.accTime = 0;
                item.accUnscaleTime = 0;

                --item.loopNum;

                if (item.loopNum <= 0)
                {
                    Off(item.handler);
                }
            }
            node = nextNode;
        }

        // remove off item
        if (mOffItemList.Count > 0)
        {
            foreach (var removeNode in mOffItemList)
            {
                item = removeNode.Value;
                if (item.off == true)
                {
                    LinkedListNode<sScheduleUpdateItem> innerNode = null;
                    if (mScheduleUpdateItemDic.TryGetValue(item.handler, out innerNode) == true)
                    {
                        mScheduleUpdateItemList.Remove(innerNode);
                        ReleaseScheduleItemNode(innerNode);
                        mScheduleUpdateItemDic.Remove(item.handler);
                        item.handler = null;
                    }
                }
            }
            mOffItemList.Clear();
        }
    }
  
    LinkedListNode<sScheduleUpdateItem> GetFreeScheduleItemNode()
    {
        if (mFreeScheduleUpdateItemNodeQueue.Count > 0)
        {
            return mFreeScheduleUpdateItemNodeQueue.Dequeue();
        }

        return new LinkedListNode<sScheduleUpdateItem>(new sScheduleUpdateItem());
    }
    void ReleaseScheduleItemNode(LinkedListNode<sScheduleUpdateItem> node)
    {
        if (mFreeScheduleUpdateItemNodeQueue.Count < mMaxFreeNodeNum)
        {
            mFreeScheduleUpdateItemNodeQueue.Enqueue(node);
        }
    }
#endregion
}
