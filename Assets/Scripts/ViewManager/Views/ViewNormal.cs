using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// View基类, 普通View
/// </summary>
public class ViewNormal : MonoBehaviour
{
    protected Transform mTrans;
    protected RectTransform mRectTrans;
    protected ViewInfo mInfo;
    protected GameObject mGo;
    protected GameObject mVisualGo;
    protected Transform mVisualTrans;
    // 相关资源是否已经被载入完成
    protected bool mIsLoaded;
    // 完成回调
    protected Action<ViewNormal> mCompleteHandler;
    // 额外参数
    protected object mExtParam;
    // 是否已彻底删除
    protected bool mDestroyed;
    // 当前跟随的目标
    protected Vector3 mFollowOffset;

    #region getter
    public Transform trans { get => mTrans; }
    public ViewInfo info { get => mInfo; }
    public GameObject go { get => mGo; }
    public Transform visualTrans { get => mVisualTrans; }
    public GameObject visualGO { get => mVisualGo; }
    public bool isLoaded { get => mIsLoaded; }
    #endregion

    #region setter
    public Action<ViewNormal> completeHandler { set => mCompleteHandler = value;}
    public object extParam { set => mExtParam = value; }
    #endregion

    /// <summary>
    /// 初始化， 只会被初始化一次
    /// </summary>
    /// <param name="viewInfo"></param>
    /// <returns></returns>
    public virtual async Task<bool> Init(ViewInfo viewInfo, Action<ViewNormal> completeHandler, object extParam)
    {
        mInfo = viewInfo;
        mGo = gameObject;
        mTrans = gameObject.transform;
        mIsLoaded = false;
        mDestroyed = false;
        mCompleteHandler = completeHandler;
        ResetPosScaleRotToConfig();

        if (viewInfo.needRectTransform == true)
        {
            mRectTrans = mGo.GetComponent<RectTransform>();
        }

#if UNITY_EDITOR
        mGo.name = viewInfo.id;
#endif

        // load visual part
        using (var op = AssetManager.LoadAssetAsync<GameObject>(viewInfo.address[0]))
        {
            await op;

            if (mDestroyed == true || op.Result == null)
            {
                return false;
            }

            OnAssetLoaded(op.Result);

            return true;
        }
    }

    /// <summary>
    /// 重置， 使其还原为开始状态
    /// </summary>
    public virtual void Restart()
    {
        if (mIsLoaded == false || mVisualGo == null)
        {
            return;
        }

        var restart = mVisualGo.GetComponent<ViewRestarter>();
        restart?.Restart(this);
    }

    /// <summary>
    /// 等待visual加载完成
    /// </summary>
    /// <param name="wnd"></param>
    /// <returns></returns>
    public async Task WaitCompleted()
    {
        while (mIsLoaded == false)
        {
            await new WaitForUpdate();
        }
    }

    /// <summary>
    /// 复原配置状态
    /// </summary>
    public virtual void OnGetFromCache()
    {
        if (mDestroyed == true)
        {
            return;
        }

        mGo.SetActive(true);
        ResetPosScaleRotToConfig();
    }

    /// <summary>
    /// 释放本对象, 根据参数和脚本配置删除或缓存本view
    /// </summary>
    /// <param name="force">true时， 不管脚本配置，强制释放到缓存</param>
    public virtual void Release(bool force)
    {
        if (mDestroyed == true)
        {
            return;
        }

        mCompleteHandler = null;

        ViewManager.Instance.ReleaseView(this, force);
    }

    /// <summary>
    /// 彻底销毁， 主动调用或系统删除此GAMEOBJECT时(如删除场景时)，被动调用
    /// </summary>
    public virtual void OnDestroy()
    {
        if (mDestroyed == true)
        {
            return;
        }
        mDestroyed = true;

        if (mVisualGo != null)
        {
            Destroy(mVisualGo);
            mVisualGo = null;
            mVisualTrans = null;
        }

        mTrans = null;
        mIsLoaded = false;
        mInfo = null;
        mExtParam = null;
        mCompleteHandler = null;

        Destroy(mGo);
        mGo = null;
    }

    /// <summary>
    /// 回复配置属性
    /// </summary>
    public virtual void ResetPosScaleRotToConfig()
    {
        if (mDestroyed == true)
        {
            return;
        }

        if (mInfo.scale != null)
        {
            trans.localScale = mInfo.scale;
        }
        if (mInfo.rotation != null)
        {
            trans.localRotation = Quaternion.Euler(mInfo.rotation);
        }
        if (mInfo.offset != null)
        {
            trans.localPosition = mInfo.offset;
        }
    }

    protected virtual void OnAssetLoaded(GameObject asset)
    {
        mIsLoaded = true;
        mVisualGo = Instantiate(asset, mTrans);
        mVisualTrans = mVisualGo.transform;

        if (mCompleteHandler != null)
        {
            mCompleteHandler.Invoke(this);
            mCompleteHandler = null;
        }
    }
    
    public void ReleaseLater(float sec)
    {
        StartCoroutine(CoReleaseLater(sec));
    }

    private IEnumerator CoReleaseLater(float sec)
    {
        yield return new WaitForSeconds(sec);
        ViewManager.Instance.ReleaseView(this, false);
    }
}
