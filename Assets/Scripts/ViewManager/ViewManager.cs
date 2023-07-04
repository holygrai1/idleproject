using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 表现管理器， 类型ParticleManager
/// </summary>
public class ViewManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static ViewManager sInstance;
    public static ViewManager Instance => sInstance;
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

    Dictionary<string, Queue<ViewNormal>> mViewCache;
    // pool parent transform
    Transform mTrans;

    public async Task<bool> Init()
    {
        mViewCache = new Dictionary<string, Queue<ViewNormal>>();
        mTrans = transform;

        return await Task.FromResult(true);
    }

    /// <summary>
    /// 创建后放入pool里， 等待被使用
    /// </summary>
    /// <param name="id"></param>
    /// <param name="completeHandler"></param>
    /// <param name="extParam"></param>
    /// <returns></returns>
    public ViewNormal CreateViewInPool(string id, Action<ViewNormal> completeHandler = null, object extParam = null)
    {
        var view = CreateView(id, completeHandler, extParam);
        view.trans.parent = mTrans;
        view.go.SetActive(false);

        return view;
    }

    /// <summary>
    /// 创建表现
    /// </summary>
    /// <param name="id"></param>
    /// <param name="completeHandler"></param>
    /// <param name="extParam"></param>
    /// <returns></returns>
    public ViewNormal CreateView(string id, Action<ViewNormal> completeHandler = null, object extParam = null)
    {
        var info = DataManager.Instance.GetViewInfo(id);
        if (info == null)
        {
            Debug.LogError("View有未配置信息, id: " + id);
            return null;
        }

        Queue<ViewNormal> viewCache;
        ViewNormal view = null;
        if (mViewCache.TryGetValue(id, out viewCache) == true && viewCache.Count > 0)
        {
            view = viewCache.Dequeue();
            view.trans.SetParent(null, false);
            view.extParam = extParam;
            if (view.isLoaded == true)
            {
                view.OnGetFromCache();
                if (completeHandler != null)
                {
                    completeHandler.Invoke(view);
                }
            }
            else
            {
                view.completeHandler = completeHandler;
            }
        }
        else
        {
            var go = new GameObject();
#if UNITY_EDITOR
            go.name = id;
#endif
            if (info.needRectTransform == true)
            {
                go.AddComponent<RectTransform>();
            }

            if (info.type == ViewInfo.TypeConst.Normal)
            {
                view = go.AddComponent<ViewNormal>();
            }
            else if (info.type == ViewInfo.TypeConst.Image)
            {
                view = go.AddComponent<ViewImage>();
            }
            else if (info.type == ViewInfo.TypeConst.CircleImage)
            {
                view = go.AddComponent<ViewCircleImage>();
            }

            if (view == null)
            {
                Debug.LogError("view创建失败， id : " + info.id);
                Destroy(go);
                if (completeHandler != null)
                {
                    completeHandler.Invoke(null);
                }
                return null;
            }

            view.Init(info, completeHandler, extParam);
        }

        return view;
    }

    /// <summary>
    /// 根据参数和配置选择删除或释放对象
    /// </summary>
    /// <param name="view"></param>
    /// <param name="forceCache"></param>
    public void ReleaseView(ViewNormal view, bool forceCache)
    {
        if (view == null)
        {
            return;
        }

        if (view.info.cacheNum <= 0 && forceCache == false)
        {
            view.OnDestroy();
            return;
        }

        Queue<ViewNormal> viewCache;
        if (mViewCache.TryGetValue(view.info.id, out viewCache) == false)
        { 
            viewCache = new Queue<ViewNormal>();
            mViewCache.Add(view.info.id, viewCache);
        }

        if (viewCache.Count >= view.info.cacheNum && forceCache == false)
        {
            view.OnDestroy();
            return;
        }

        view.trans.DOKill();
        view.trans.SetParent(mTrans, false);
        view.go.SetActive(false);
        viewCache.Enqueue(view);
    }

    public void DestroyFreeCaches()
    {
        foreach (var kv in mViewCache)
        {
            var viewQueue = kv.Value;
            while (viewQueue.Count > 0)
            {
                var view = viewQueue.Dequeue();
                view.OnDestroy();
            }
        }
        mViewCache.Clear();
    }
}