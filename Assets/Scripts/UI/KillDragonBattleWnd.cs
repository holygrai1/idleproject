using System;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屠龙战斗主界面
/// </summary>
public class KillDragonBattleWnd : WndBase
{
    public Button ReturnBtn;
    public Button StartBattleButton;
    public Button SettingButton;
    public Button RankButton;

    public Button TestFastLightBallButton;
    public Button TestHeartBallButton;
    public Button TestAddUserButton;
    public Button TestPrizeButton;
    public Button TestPrizeBossButton;

    public override async Task<bool> Init(sWndAssetRef assetRef)
    {
        bool result = await base.Init(assetRef);
        if (result == false)
        {
            return false;
        }

        mCurLayer = CanvasLayer.main;
        mShowTransitionType = EnWndShowHideTransition.max;
        mHideTransitionType = EnWndShowHideTransition.max;

        mInited = true;

        return true;
    }

    public async override void OnShow(bool isNeedFade = true)
    {
        
        base.OnShow(isNeedFade);
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);

        ReturnBtn.onClick.RemoveAllListeners();
        StartBattleButton.onClick.RemoveAllListeners();
        TestFastLightBallButton.onClick.RemoveAllListeners();
        TestHeartBallButton.onClick.RemoveAllListeners();
        TestAddUserButton.onClick.RemoveAllListeners();
        TestPrizeButton.onClick.RemoveAllListeners();
        TestPrizeBossButton.onClick.RemoveAllListeners();

        SettingButton.onClick.RemoveAllListeners();
        RankButton.onClick.RemoveAllListeners();
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
            StartBattleButton.gameObject.SetActive(true);

            ReturnBtn.onClick.AddListener(OnReturnButtonClick);
            StartBattleButton.onClick.AddListener(OnStartBattleButtonClick);
            SettingButton.onClick.AddListener(OnSettingButtonClick);
            RankButton.onClick.AddListener(OnRankButtonClick);

            TestFastLightBallButton.onClick.AddListener(() => {
                var userDatas = ClientManager.Instance.AllUserDatas;
                var userData = userDatas[UnityEngine.Random.Range(0, userDatas.Count)];
                EventManager.Instance.DispatchUserCommentEvent(userData, "comment 1231231231231");
            });

            TestHeartBallButton.onClick.AddListener(() => {
                var userDatas = ClientManager.Instance.AllUserDatas;
                var userData = userDatas[UnityEngine.Random.Range(0, userDatas.Count)];
                EventManager.Instance.DispatchUserLikeEvent(userData);

                var curBattle = BattleManager.Instance.curBattle as KillDragonBattle;
                curBattle.CurBoss.OnUserActionTypeHandle(UserActionType.like);
            });

            TestAddUserButton.onClick.AddListener(() => {
                UserData userData = new UserData();
                userData.Init(null);
                ClientManager.Instance.AllUserDatas.Add(userData);
                EventManager.Instance.DispatchUserEnterEvent(userData);
            });

            TestPrizeButton.onClick.AddListener(() => {
                var userDatas = ClientManager.Instance.AllUserDatas;
                if (userDatas.Count > 1)
                {
                    var userData = userDatas[1];
                    userData.TestPrize();
                    EventManager.Instance.DispatchUserPrizeEvent(userData);
                }
            });

            TestPrizeBossButton.onClick.AddListener(() => {
                var battle = BattleManager.Instance.curBattle as KillDragonBattle;
                battle.CurBoss.OnUserActionTypeHandle(UserActionType.prize);
            });
        }
    }

    private void OnReturnButtonClick()
    {
        HideSelf();
        UIManager.Instance.ShowWnd(WndType.mainWnd);

        BattleManager.Instance.DestroyCurBattle();
    }

    private async void OnStartBattleButtonClick()
    {
        UIManager.Instance.ShowWait();
        //if (await ClientManager.Instance.StartKillDragonBattle() == false)
        //{
        //    UIManager.Instance.HideWait();
        //    UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
        //    Action callback = () =>
        //    {
        //        UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
        //    };
        //    UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "请求boss数据失败, 请链接网络后再尝试", callback);
        //    return;
        //}

        UIManager.Instance.HideWait();

        StartBattleButton.gameObject.SetActive(false);
        BattleManager.Instance.curBattle.BattleStart();

      
    }

    private async void OnSettingButtonClick()
    {
        UIManager.Instance.ShowWnd(WndType.settingWnd);
    }

    private async void OnRankButtonClick()
    {
        
    }
}
