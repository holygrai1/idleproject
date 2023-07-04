using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// image类型
/// </summary>
public class ViewImage : ViewNormal
{
    protected ViewImageInfo mSubInfo;
    protected Image mImage;

    /// <summary>
    /// 初始化， 只会被初始化一次
    /// </summary>
    /// <param name="viewInfo"></param>
    /// <returns></returns>
    public override async Task<bool> Init(ViewInfo viewInfo, Action<ViewNormal> completeHandler, object extParam)
    {
        mSubInfo = viewInfo as ViewImageInfo;
        mInfo = viewInfo;
        mGo = gameObject;
        mImage = mGo.AddComponent<Image>();
        mTrans = gameObject.transform;
        mRectTrans = gameObject.GetComponent<RectTransform>();
        mVisualGo = gameObject;
        mVisualTrans = mTrans;

        mIsLoaded = false;
        mDestroyed = false;
        mCompleteHandler = completeHandler;

        if (mInfo.scale != null)
        {
            mRectTrans.localScale = mInfo.scale;
        }
        if (mInfo.rotation != null)
        {
            mRectTrans.localRotation = Quaternion.Euler(mInfo.rotation);
        }
        if (mInfo.offset != null)
        {
            mRectTrans.localPosition = mInfo.offset;
        }

        // load visual part
        using (var op = AssetManager.LoadAssetAsync<SpriteAtlas>(viewInfo.address[0]))
        {
            await op;

            if (mDestroyed == true)
            {
                return false;
            }

            mIsLoaded = true;
            mImage.sprite = op.Result.GetSprite(viewInfo.address[1]);
            if (mCompleteHandler != null)
            {
                mCompleteHandler.Invoke(this);
                mCompleteHandler = null;
            }
            return true;
        }
    }

    /// <summary>
    /// 彻底销毁， 主动调用或系统删除此GAMEOBJECT时(如删除场景时)，被动调用
    /// </summary>
    public override void OnDestroy()
    {
        if (mDestroyed == true)
        {
            return;
        }
        mDestroyed = true;

        mVisualGo = null;
        mVisualTrans = null;
        mTrans = null;
        mIsLoaded = false;
        mInfo = null;
        mExtParam = null;
        mCompleteHandler = null;
        Destroy(mGo);
        mGo = null;
    }
}
