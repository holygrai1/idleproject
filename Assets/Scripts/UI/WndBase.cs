using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// 窗口显示隐藏的动作类型
/// </summary>
public enum EnWndShowHideTransition
{
    custom,
    pop,
    fade,

    max
}

/// <summary>
/// 窗口基类
/// </summary>
public class WndBase : MonoBehaviour
{
    /// <summary>
    /// 是否显示完成(播放完缩小放大的显示效果后才设置为true)
    /// </summary>
    protected bool mIsShowTransitionCompleted;
    protected Transform mTrans;
    protected RectTransform mRectTrans;
    protected GameObject mGO;
    protected CanvasGroup mCanvasGroup;
    protected CanvasLayer mCurLayer;
    protected WndType mWndType;
    protected Transform mTransiztionPart;
    protected EnWndShowHideTransition mShowTransitionType;
    protected EnWndShowHideTransition mHideTransitionType;
    protected bool mInited;
    private bool mIsDestroyed;

    public bool IsShowTransitionCompleted { get => mIsShowTransitionCompleted; }
    public Transform Trans { get => mTrans; }
    public GameObject GO { get => mGO; }
    public WndType WndType { get => mWndType; }
    public CanvasLayer CurLayer { get => mCurLayer; set => mCurLayer = value; }
    public bool Inited { get => mInited; }
    public bool IsDestroyed { get => mIsDestroyed; }

    protected sWndAssetRef mAssetRef;

    public virtual void Awake()
    {

    }

    /// <summary>
    /// called after Awake()
    /// </summary>
    /// <param name="assetRef"></param>
    public virtual async Task<bool> Init(sWndAssetRef assetRef)
    {
        mAssetRef = assetRef;
        mWndType = mAssetRef.wndType;

        mIsShowTransitionCompleted = false;
        mGO = gameObject;
        mTrans = transform;
        mRectTrans = mGO.GetComponent<RectTransform>();
        mTransiztionPart = mRectTrans.transform;
        mShowTransitionType = EnWndShowHideTransition.pop;
        mHideTransitionType = EnWndShowHideTransition.pop;
        mCurLayer = CanvasLayer.normal;

        if (mGO.GetComponent<CanvasGroup>() == null)
        {
            mCanvasGroup = mGO.AddComponent<CanvasGroup>();
            mCanvasGroup.alpha = 1.0f;
            mCanvasGroup.blocksRaycasts = true;
            mCanvasGroup.interactable = true;
        }

        if (Helpers.IsFringeFit() == true)
        {
            mRectTrans.offsetMin = new Vector2(mRectTrans.offsetMin.x, mRectTrans.offsetMin.y - Helpers.FringeFitBottom);
            mRectTrans.offsetMax = new Vector2(mRectTrans.offsetMax.x, mRectTrans.offsetMax.y - Helpers.FringeFitTop);
        }

        if (LocaleManager.Instance.curLocale != LocaleManager.defaultLocal)
        {
            OnLocaleUpdate(LocaleManager.defaultLocal, LocaleManager.Instance.curLocale);
        }

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = false;
        }

        return await Task.FromResult(true);
    }

    public virtual void HideSelf(bool isNeedTransition = true)
    {
        UIManager.Instance.HideWnd(WndType, isNeedTransition);
    }
    public virtual void ShowSelf(bool isNeedTransition = true)
    {
        UIManager.Instance.ShowWnd(WndType, isNeedTransition);
    }

    /// <summary>
    /// 本地化更新
    /// </summary>
    public virtual void OnLocaleUpdate(string preLocale, string curLocale)
    {
        var curFontAsset = LocaleManager.Instance.GetMainFontAsset(curLocale);
        if (curFontAsset == null)
        {
            return;
        }
        var allTextMeshProText = mGO.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in allTextMeshProText)
        {
            text.font = curFontAsset;
            text.ForceMeshUpdate();
        }
    }

    public virtual void OnShow(bool isNeedFade = true)
    {
        if (isNeedFade == true && mShowTransitionType != EnWndShowHideTransition.max)
        {
            OnShowStart();
        }
        else
        {
            mGO.SetActive(true);
            OnShowCompleted();
        }

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = false;
        }
    }

    public virtual void OnHide(bool isNeedFade = true)
    {
        if (isNeedFade == true && mHideTransitionType != EnWndShowHideTransition.max)
        {
            OnHideStart();
        }
        else
        {
            OnHideCompleted();
        }
    }

    public virtual void OnEnable()
    {

    }
    public virtual void OnDisable()
    {

    }

    public virtual void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        LogManager.Log("Receive Msg: " + msgType + " at " + WndType.ToString());
    }

    protected virtual void OnShowStart()
    {
        mGO.SetActive(true);

        if (mShowTransitionType == EnWndShowHideTransition.pop)
        {
            mTransiztionPart.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            mTransiztionPart.DOKill();
            mTransiztionPart.DOScale(new Vector3(1.1f, 1.1f, 1.0f), 0.05f).OnComplete(() => {
                mTransiztionPart.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.1f).OnComplete(() => {
                    OnShowCompleted();
                });
            });
        }
        else if (mShowTransitionType == EnWndShowHideTransition.fade)
        {
            mCanvasGroup.DOKill();
            mCanvasGroup.DOFade(1.0f, 0.5f).OnComplete(() => {
                OnShowCompleted();
            });
        }
        else if (mShowTransitionType == EnWndShowHideTransition.custom)
        {

        }
    }

    protected virtual void OnShowCompleted()
    {
        mIsShowTransitionCompleted = true;
    }

    protected virtual void OnHideStart()
    {
        if (mHideTransitionType == EnWndShowHideTransition.pop)
        {
            mTransiztionPart.DOKill();
            mTransiztionPart.DOScale(new Vector3(1.1f, 1.1f, 1.0f), 0.05f).OnComplete(() => {
                mTransiztionPart.DOScale(new Vector3(0.9f, 0.9f, 1.0f), 0.1f).OnComplete(() => {
                    OnHideCompleted();
                });
            });
        }
        else if (mHideTransitionType == EnWndShowHideTransition.fade)
        {
            mCanvasGroup.DOKill();
            mCanvasGroup.DOFade(0.3f, 0.15f).OnComplete(() => {
                OnHideCompleted();
            });
        }
        else if (mHideTransitionType == EnWndShowHideTransition.custom)
        {

        }
    }

    protected virtual void OnHideCompleted()
    {
        mGO.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        OnHideCompleted();
        mIsDestroyed = true;
    }

    public virtual void DestroySelf()
    {
        UIManager.Instance.DestroyWnd(WndType);
    }

    protected virtual void Start()
    {

    }
}
