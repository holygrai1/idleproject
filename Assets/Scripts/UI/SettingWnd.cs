using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设定界面
/// </summary>
public class SettingWnd : WndBase
{
    public Button LoginButton;
    public Button LogoutButton;
    public Button CloseButton;
    public InputField InputField;

    public override async Task<bool> Init(sWndAssetRef assetRef)
    {
        bool result = await base.Init(assetRef);
        if (result == false)
        {
            return false;
        }

        mCurLayer = CanvasLayer.above;
        mShowTransitionType = EnWndShowHideTransition.pop;
        mHideTransitionType = EnWndShowHideTransition.pop;

        mInited = true;

        return true;
    }

    public override void OnShow(bool isNeedFade = true)
    {
        base.OnShow(isNeedFade);
        LoginButton.onClick.AddListener(OnLoginButtonClick);
        LogoutButton.onClick.AddListener(OnLogoutButtonClick);
        CloseButton.onClick.AddListener(OnCloseButtonClick);
        InputField.text = PlayerPrefs.GetString("id");
        InputField.interactable = false;
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);
        LoginButton.onClick.RemoveAllListeners();
        LogoutButton.onClick.RemoveAllListeners();
        CloseButton.onClick.RemoveAllListeners();
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
        }
    }

    private async void OnLoginButtonClick()
    {
        var id = InputField.text;
        if (string.IsNullOrEmpty(id) == false)
        {
            UIManager.Instance.ShowWait();
            var ret = await ClientManager.Instance.Login(id);
            UIManager.Instance.HideWait();

            if (ret == false)
            {
                UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
                Action callback = () =>
                {
                    UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
                };
                var errorMsg = ClientManager.Instance.ErrorMsg;
                UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", errorMsg, callback);

                return;
            }
            else
            {
                PlayerPrefs.SetString("id", id);
                HideSelf();

                var levelRankData = await ClientManager.Instance.GetRank(ClientManager.Instance.SelfUserData.ID, 1);
                var dmgRankData = await ClientManager.Instance.GetRank(ClientManager.Instance.SelfUserData.ID, 2);
                UIManager.Instance.SendMsg(WndType.mainWnd, WndMsgType.showRank, levelRankData, dmgRankData);
            }
        }
        else
        {
            UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
            Action callback = () =>
            {
                UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
                UIManager.Instance.ShowWnd(WndType.loginWnd);
            };
            UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "此主播id无发获取, 请重新确认主播id", callback);
        }
    }

    private async void OnLogoutButtonClick()
    {
        var id = PlayerPrefs.GetString("id");
        if (string.IsNullOrEmpty(id) == false)
        {
            UIManager.Instance.ShowWait();
            var ret = await ClientManager.Instance.Logout();
            UIManager.Instance.HideWait();

            if (ret == false)
            {
                UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
                Action callback = () =>
                {
                    UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
                };
                UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "登出失败", callback);

                return;
            }

            Application.Quit();
        }
    }

    private void OnCloseButtonClick()
    {
        HideSelf();
    }
}
