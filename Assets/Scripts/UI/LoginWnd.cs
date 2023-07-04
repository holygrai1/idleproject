using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 登陆界面
/// </summary>
public class LoginWnd : WndBase
{
    public InputField IDInput;
    public Button IDConfirmButton;

    public override async Task<bool> Init(sWndAssetRef assetRef)
    {
        bool result = await base.Init(assetRef);
        if (result == false)
        {
            return false;
        }

        mCurLayer = CanvasLayer.above;
        mShowTransitionType = EnWndShowHideTransition.max;
        mHideTransitionType = EnWndShowHideTransition.max;

        mInited = true;

        return true;
    }

    public override void OnShow(bool isNeedFade = true)
    {
        base.OnShow(isNeedFade);

        var id = PlayerPrefs.GetString("id");
        if (string.IsNullOrEmpty(id) == false)
        {
            IDInput.text = id;
        }

        IDConfirmButton.onClick.AddListener(OnIDConfirmButtonClick);
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);

        IDConfirmButton.onClick.RemoveAllListeners();
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
        }
    }

    private void OnIDConfirmButtonClick()
    {
        if (string.IsNullOrEmpty(IDInput.text) == false)
        {
            //UIManager.Instance.ShowWait();
            //var ret = await ClientManager.Instance.Login(IDInput.text);
            //UIManager.Instance.HideWait();

            //if (ret == null)
            //{
            //    UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
            //    Action callback = () => {
            //        UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
            //    };
            //    UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "登陆失败", callback);

            //    return;
            //}

            PlayerPrefs.SetString("id", IDInput.text);
            PlayerPrefs.SetInt("platform", 1);
            
            HideSelf();

            UIManager.Instance.ShowWnd(WndType.mainWnd);
        }
        else
        {
            UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
            Action callback = () => {
                UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
            };
            UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "请输入抖音账号id", callback);
        }
    }
}
