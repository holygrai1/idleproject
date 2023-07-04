using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 通用信息窗口有描述内容， 标题， 确认按钮窗口
/// </summary>
public class MsgBoxYesWnd : WndBase
{
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI contentLabel;
    public Button yesButton;
    public Action callbackFunc;

    public override async Task<bool> Init(sWndAssetRef assetRef)
    {
        bool result = await base.Init(assetRef);
        if (result == false)
        {
            return false;
        }

        mCurLayer = CanvasLayer.msgBox;
        mShowTransitionType = EnWndShowHideTransition.pop;
        mHideTransitionType = EnWndShowHideTransition.pop;

        mInited = true;

        return true;
    }

    public override void OnShow(bool isNeedFade = true)
    {
        base.OnShow(isNeedFade);

        yesButton.onClick.AddListener(() => {
            callbackFunc.Invoke();
        });
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);

        yesButton.onClick.RemoveAllListeners();
        callbackFunc = null;
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
            titleLabel.text = msgParams[0] as string;
            contentLabel.text = msgParams[1] as string;
            callbackFunc = msgParams[2] as Action;
        }
    }

}
