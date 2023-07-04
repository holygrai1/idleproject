using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 屠龙战斗结算界面
/// </summary>
public class KillDragonBattleResultWnd : WndBase, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    public GameObject WinPart;
    public Text WinPartTime;
    public Text WinPartBossName;
    public Image WinPartBossIcon;
    public Transform WinPartBossNode;
    public Button WinPartJumpButton;

    public GameObject LosePart;
    public Text LosePartTime;
    public Text LosePartBossName;
    public Image LosePartBossIcon;
    public Transform LosePartBossNode;
    public Button LosePartJumpButton;

    public GameObject RankPart;
    public Transform RankPartTrans;
    public GameObject RankItemBigTemp;
    public GameObject RankItemSmallTemp;
    public ScrollRect RankScrollRect;
    public RectTransform RankScrollTrans;
    public RectTransform RankContent;

    public Button SettingButton;
    public Button RankButton;

    private float mDoubleClickCD;
    private float mAutoScrollCD;
    private float mAutoScrollTime = 5.0f;

    class RankItem
    {
        public GameObject Go;
        public Transform Trans;
        public RectTransform RectTrans;
        public Text Num;
        public Image NumImg;
        public Image NumImg2;
        public Image NumImg3;
        public Text Name;
        public Image HeadIcon;
        public Text Level;
        public int Index;
        public bool IsBig;
        // 正在载入的图片的url地址
        public string LoadingUrl = "";
        // 已载入的图片的url地址
        public string LoadedUrl = "";
        // 等待下次载入的图片的url地址
        public string WaitToLoadUrl = "";

        public RankItem(GameObject go, bool isBig)
        {
            Go = go;
            Trans = go.transform;
            RectTrans = go.GetComponent<RectTransform>();
            Num = Trans.Find("Num")?.GetComponent<Text>();
            NumImg = Trans.Find("NumImg")?.GetComponent<Image>();
            NumImg2 = Trans.Find("NumImg2")?.GetComponent<Image>();
            NumImg3 = Trans.Find("NumImg3")?.GetComponent<Image>();
            Name = Trans.Find("Name").GetComponent<Text>();

            var headicon = Trans.Find("HeadBg/HeadIcon");

            HeadIcon = Trans.Find("HeadBg/HeadIcon").GetComponent<Image>();
            Level = Trans.Find("Level").GetComponent<Text>();

            this.IsBig = isBig;
        }

        public void SetData(string name, string url, int index, int level)
        {
            Index = index;
            if (Num != null)
            {
                Num.text = (index + 1).ToString();
            }
            else
            {
                if (index == 0)
                {
                    NumImg.gameObject.SetActive(true);
                    NumImg2.gameObject.SetActive(false);
                    NumImg3.gameObject.SetActive(false);
                }
                else if (index == 1)
                {
                    NumImg.gameObject.SetActive(false);
                    NumImg2.gameObject.SetActive(true);
                    NumImg3.gameObject.SetActive(false);
                }
                else
                {
                    NumImg.gameObject.SetActive(false);
                    NumImg2.gameObject.SetActive(false);
                    NumImg3.gameObject.SetActive(true);
                }
            }

            Name.text = name;
            Level.text = (level + 1).ToString();
            LoadHeadIcon(url);
        }

        public void SetDataFake(int fakeID, string fakeName, int index, int level)
        {
            this.Index = index;
            if (Num != null)
            {
                Num.text = (index + 1).ToString();
            }
            else
            {
                if (index == 0)
                {
                    NumImg.gameObject.SetActive(true);
                    NumImg2.gameObject.SetActive(false);
                    NumImg3.gameObject.SetActive(false);
                }
                else if (index == 1)
                {
                    NumImg.gameObject.SetActive(false);
                    NumImg2.gameObject.SetActive(true);
                    NumImg3.gameObject.SetActive(false);
                }
                else
                {
                    NumImg.gameObject.SetActive(false);
                    NumImg2.gameObject.SetActive(false);
                    NumImg3.gameObject.SetActive(true);
                }
            }
            Name.text = DataManager.Instance.GetStringRes(fakeName);
            Level.text = (level + 1).ToString();
            LoadHeadIcon("");

            Helpers.LoadSpriteAtlas("ActorIcon", fakeID.ToString(), (Sprite sp) =>
            {
                HeadIcon.sprite = sp;
            });
        }

        private async void LoadHeadIcon(string url)
        {
            if (string.IsNullOrEmpty(LoadingUrl) == true)
            {
                if (string.IsNullOrEmpty(url) == true)
                {
                    return;
                }

                // 未载入中的情况
                LoadingUrl = url;

                UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
                using (www)
                {
                    await www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        var toLoadURL = LoadingUrl;
                        LoadingUrl = "";
                        if (string.IsNullOrEmpty(WaitToLoadUrl) == false)
                        {
                            toLoadURL = WaitToLoadUrl;
                        }
                        LoadHeadIcon(toLoadURL);
                        return;
                    }

                    if (string.IsNullOrEmpty(WaitToLoadUrl) == false && WaitToLoadUrl != LoadingUrl)
                    {
                        // load next
                        LoadingUrl = "";
                        LoadHeadIcon(WaitToLoadUrl);
                        return;
                    }

                    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    Rect rec = new Rect(0, 0, texture.width, texture.height);
                    Sprite sprite = Sprite.Create(texture, rec, new Vector2(0, 0), 1);
                    HeadIcon.sprite = sprite;
                    LoadedUrl = LoadingUrl;
                    LoadingUrl = "";
                    WaitToLoadUrl = "";
                }
            }
            else
            {
                // 在载入中的情况
                if (LoadingUrl == url)
                {
                    WaitToLoadUrl = "";
                    return;
                }

                if (LoadedUrl == url)
                {
                    WaitToLoadUrl = "";
                    return;
                }

                WaitToLoadUrl = url;
            }
        }
    }

    private Queue<RankItem> mFreeRankItemSmall = new Queue<RankItem>();
    private Queue<RankItem> mFreeRankItemBig = new Queue<RankItem>();
    private Dictionary<int, RankItem> mRankItems = new Dictionary<int, RankItem>();
    private int mBigItemAheadNum = 3;
    private float mRankItemBigHeight;
    private float mRankItemSmallHeight;
    private List<BattleActor> mRankActorList = new List<BattleActor>();

    private float mTimeLeft;
    private bool mContentInited = false;
    private bool mStopCountDown;

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

        for (int i = 0; i < 30; ++i)
        {
            var go = GameObject.Instantiate(RankItemSmallTemp, RankPartTrans);
            var item = new RankItem(go, false);
            item.Go.SetActive(false);
            mFreeRankItemSmall.Enqueue(item);
            go.name = "small " + i.ToString();
        }
        for (int i = 0; i < 6; ++i)
        {
            var go = GameObject.Instantiate(RankItemBigTemp, RankPartTrans);
            var item = new RankItem(go, true);
            item.Go.SetActive(false);
            mFreeRankItemBig.Enqueue(item);
            go.name = "big " + i.ToString();
        }

        mRankItemBigHeight = mFreeRankItemBig.Peek().RectTrans.sizeDelta.y;
        mRankItemSmallHeight = mFreeRankItemSmall.Peek().RectTrans.sizeDelta.y;

        RankItemBigTemp.SetActive(false);
        RankItemSmallTemp.SetActive(false);

        mInited = true;
        mContentInited = false;

        return true;
    }

    public override void OnShow(bool isNeedFade = true)
    {
        base.OnShow(isNeedFade);

        mContentInited = false;
        mStopCountDown = false;

        SettingButton.onClick.AddListener(OnSettingButtonClick);
        RankButton.onClick.AddListener(OnRankButtonClick);

        mAutoScrollCD = mAutoScrollTime;

        var curPos = RankScrollRect.content.anchoredPosition;
        RankScrollRect.content.anchoredPosition = new Vector2(curPos.x, 0);
    }

    private async void SetupWinPart()
    {
        WinPart.SetActive(true);
        LosePart.SetActive(false);
        WinPartJumpButton.onClick.AddListener(OnJumpNext);

        var curBattle = BattleManager.Instance.curBattle;
        var boss = curBattle.AllMonster.First().Value;
        var userID = boss.UserID;
        var selfUserData = ClientManager.Instance.SelfUserData;

        var killDragonBattle = ((KillDragonBattle)curBattle);
        WinPartBossName.text = killDragonBattle.BossName;

        Helpers.SetImageFromURL(killDragonBattle.BossUrl, WinPartBossIcon);
        var assetAddress = boss.MonsterInfo.assetAddress + "UI";
        var bossGO = await AssetManager.Instantiate(assetAddress, Trans);
        bossGO.transform.SetParent(WinPartBossNode, false);
        var track = bossGO.GetComponent<Spine.Unity.SkeletonGraphic>().AnimationState.SetAnimation(0, CreatureAnimationName.idle, true);
    }
    private async void SetupLosePart()
    {
        WinPart.SetActive(false);
        LosePart.SetActive(true);
        LosePartJumpButton.onClick.AddListener(OnJumpRetry);

        var curBattle = BattleManager.Instance.curBattle;
        var boss = curBattle.AllMonster.First().Value;
        var userID = boss.UserID;
        var selfUserData = ClientManager.Instance.SelfUserData;
        var killDragonBattle = ((KillDragonBattle)curBattle);
        if (selfUserData.ID == userID)
        {
            LosePartBossName.text = killDragonBattle.BossName;
            Helpers.SetImageFromURL(killDragonBattle.BossUrl, LosePartBossIcon);
            var assetAddress = boss.MonsterInfo.assetAddress + "UI";
            var bossGO = await AssetManager.Instantiate(assetAddress, Trans);
            bossGO.transform.SetParent(LosePartBossNode, false);
            bossGO.GetComponent<Spine.Unity.SkeletonGraphic>().AnimationState.SetAnimation(0, CreatureAnimationName.attack, true);
        }
        else
        {
            var userData = ClientManager.Instance.GetUserData(userID);
            LosePartBossName.text = killDragonBattle.BossName;
            Helpers.SetImageFromURL(killDragonBattle.BossUrl, LosePartBossIcon);
            var assetAddress = boss.MonsterInfo.assetAddress + "UI";
            var bossGO = await AssetManager.Instantiate(assetAddress, Trans);
            bossGO.transform.SetParent(LosePartBossNode, false);
            bossGO.transform.SetParent(LosePartBossNode, false);
            bossGO.GetComponent<Spine.Unity.SkeletonGraphic>().AnimationState.SetAnimation(0, CreatureAnimationName.attack, true);
        }
    }

    private void SetupRankList(List<BattleActor> rankActorList)
    {
        var rankContentTrans = RankContent.transform;
        var allUserDatas = ClientManager.Instance.AllUserDatas;
        var curBattle = BattleManager.Instance.curBattle;

        mRankActorList.Clear();
        for (int i = 0; i < 50 && i < rankActorList.Count; ++i)
        {
            mRankActorList.Add(rankActorList[i]);
        }

        float totalHeight = 0;
        for (int i = 0, max = mRankActorList.Count; i < max; ++i)
        {
            if (i < mBigItemAheadNum)
            {
                totalHeight += mRankItemBigHeight;
            }
            else
            {
                totalHeight += mRankItemSmallHeight;
            }
        }
        RankContent.sizeDelta = new Vector2(RankContent.sizeDelta.x, totalHeight);
        RankScrollRect.onValueChanged.AddListener(OnRankScrollValueChange);

        OnRankScrollValueChange(Vector2.zero);
    }

    private void OnRankScrollValueChange(Vector2 delta)
    {
        // 下拖拉y值越大, 顶部时0
        float height = RankScrollTrans.sizeDelta.y;
        float curPosY = RankContent.anchoredPosition.y;
        float heightAcc = 0;
        float curPosIndexHeight = 0;
        bool foundStartPosIndex = false;
        int curPosIndex = 0;
        int endPosIndex = mRankActorList.Count - 1;
        for (int i = 0, max = mRankActorList.Count; i < max; ++i)
        {
            if (i < mBigItemAheadNum)
            {
                // big
                heightAcc += mRankItemBigHeight;
            }
            else
            {
                // small
                heightAcc += mRankItemSmallHeight;
            }

            if (foundStartPosIndex == false)
            {
                if (heightAcc >= curPosY)
                {
                    curPosIndexHeight = heightAcc - (i < mBigItemAheadNum ? mRankItemBigHeight : mRankItemSmallHeight);
                    curPosIndex = i;
                    heightAcc = 0;
                    foundStartPosIndex = true;
                }
            }
            else
            {
                if (heightAcc >= height)
                {
                    endPosIndex = i;
                    break;
                }
            }
        }

        var rankItemList = mRankItems.ToList();
        foreach (var kv in rankItemList)
        {
            var item = kv.Value;
            if (item.Index < curPosIndex || item.Index > endPosIndex)
            {
                mRankItems.Remove(kv.Key);
                item.Trans.SetParent(RankPartTrans, false);
                item.Go.SetActive(false);
                if (item.IsBig == true)
                {
                    mFreeRankItemBig.Enqueue(item);
                }
                else
                {
                    mFreeRankItemSmall.Enqueue(item);
                }

                item.SetData("", "", -1, 0);
            }
        }

        for (int i = curPosIndex; i <= endPosIndex; ++i)
        {
            if (mRankItems.ContainsKey(i) == false)
            {
                RankItem item = null;
                if (i < mBigItemAheadNum)
                {
                    // big
                    item = mFreeRankItemBig.Dequeue();
                }
                else
                {
                    // small
                    item = mFreeRankItemSmall.Dequeue();
                }

                item.Trans.SetParent(RankContent.transform, false);
                item.Go.SetActive(true);
                var actor = mRankActorList[i];
                var userData = ClientManager.Instance.GetUserData(actor.UserID);
                if (userData != null)
                {
                    item.SetData(userData.name, userData.headPic, i, DataManager.Instance.GetActorLevelByExp(userData.exp));
                }
                else
                {
                    item.SetDataFake(actor.FakeID, actor.FakeName, i, 0);
                }

                item.RectTrans.anchoredPosition = new Vector2(0, -curPosIndexHeight);

                mRankItems.Add(i, item);
            }

            curPosIndexHeight += (i < mBigItemAheadNum ? mRankItemBigHeight : mRankItemSmallHeight);
        }
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);
        WinPartJumpButton.onClick.RemoveAllListeners();
        LosePartJumpButton.onClick.RemoveAllListeners();
        SettingButton.onClick.RemoveAllListeners();
        RankButton.onClick.RemoveAllListeners();

        if (WinPart.activeSelf == true)
        {
            WinPartBossIcon.sprite = null;
            if (WinPartBossNode.childCount > 0)
            {
                var boss = WinPartBossNode.GetChild(0);
                boss.SetParent(null);
                Destroy(boss.gameObject);
            }
        }
        else
        {
            LosePartBossIcon.sprite = null;
            if (LosePartBossNode.childCount > 0)
            {
                var boss = LosePartBossNode.GetChild(0);
                boss.SetParent(null);
                Destroy(boss.gameObject);
            }
        }

        var rankItemList = mRankItems.ToList();
        foreach (var kv in rankItemList)
        {
            var item = kv.Value;
            mRankItems.Remove(kv.Key);
            item.Trans.SetParent(RankPartTrans, false);
            item.Go.SetActive(false);
            if (item.IsBig == true)
            {
                mFreeRankItemBig.Enqueue(item);
            }
            else
            {
                mFreeRankItemSmall.Enqueue(item);
            }

            item.SetData("", "", -1, 0);
        }
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
            mTimeLeft = 20;
            var curBattle = BattleManager.Instance.curBattle;
            if (curBattle.State == BattleState.win)
            {
                SetupWinPart();
            }
            else
            {
                SetupLosePart();
            }
            var actorRankList = (List<BattleActor>)msgParams[0];
            SetupRankList(actorRankList);

            mContentInited = true;
        }
    }

    public void Update()
    {
        if (mContentInited == false)
        {
            return;
        }

        mAutoScrollCD -= Time.deltaTime;
        if (mAutoScrollCD <= 0)
        {
            var curPos = RankScrollRect.content.anchoredPosition;
            RankScrollRect.content.anchoredPosition = new Vector2(curPos.x, curPos.y + Time.deltaTime * 10);
        }

        float lastTimeLeft = mTimeLeft;

        if (mStopCountDown == false)
        {
            mTimeLeft -= Time.deltaTime;
        }
        else
        {
            mDoubleClickCD -= Time.deltaTime;
            return;
        }

        if (mTimeLeft <= 0 && lastTimeLeft > 0)
        {
            mTimeLeft = 0;
            HideSelf();

            var killDragonBattle = (KillDragonBattle)BattleManager.Instance.curBattle;
            if (killDragonBattle.State == BattleState.win)
            {
                killDragonBattle.StartNextBoss();
            }
            else
            {
                killDragonBattle.TryAgain();
            }
        }

        if (WinPart.activeSelf == true)
        {
            if (mStopCountDown == false)
            {
                WinPartTime.text = Math.Floor(mTimeLeft) + "秒后自动开始下一关";
            }
        }
        else
        {
            if (mStopCountDown == false)
            {
                LosePartTime.text = Math.Floor(mTimeLeft) + "秒后自动开始下一关";
            }
        }
    }

    private async void OnJumpNext()
    {
        if (mStopCountDown == false)
        {
            mStopCountDown = true;
            WinPartTime.text = "双击此处开始下一关";
            mDoubleClickCD = 0.8f;
            return;
        }

        if (mDoubleClickCD <= 0)
        {
            mDoubleClickCD = 0.8f;
            return;
        }

        HideSelf();
        UIManager.Instance.ShowWait();
        if (await ClientManager.Instance.StartKillDragonBattle() == false)
        {
            UIManager.Instance.HideWait();
            UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
            Action callback = () =>
            {
                UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
            };
            UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "未连接服务器，点击左上角设置中开始游戏", callback);
            return;
        }

        UIManager.Instance.HideWait();

        var killDragonBattle = (KillDragonBattle)BattleManager.Instance.curBattle;
        killDragonBattle.StartNextBoss();

    }
    private async void OnJumpRetry()
    {
        if (mStopCountDown == false)
        {
            mStopCountDown = true;
            LosePartTime.text = "双击此处开始下一关";
            mDoubleClickCD = 0.8f;
            return;
        }

        if (mDoubleClickCD <= 0)
        {
            mDoubleClickCD = 0.8f;
            return;
        }

        HideSelf();

        UIManager.Instance.ShowWait();
        if (await ClientManager.Instance.StartKillDragonBattle() == false)
        {
            UIManager.Instance.HideWait();
            UIManager.Instance.ShowWnd(WndType.msgBoxYesWnd);
            Action callback = () =>
            {
                UIManager.Instance.HideWnd(WndType.msgBoxYesWnd);
            };
            UIManager.Instance.SendMsg(WndType.msgBoxYesWnd, WndMsgType.initContent, "提示", "未连接服务器，点击左上角设置中开始游戏", callback);
            return;
        }

        UIManager.Instance.HideWait();

        var killDragonBattle = (KillDragonBattle)BattleManager.Instance.curBattle;
        killDragonBattle.TryAgain();
    }

    private void OnSettingButtonClick()
    {
        UIManager.Instance.ShowWnd(WndType.settingWnd);
    }
    private void OnRankButtonClick()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("click");
        mAutoScrollCD = float.MaxValue;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag == RankScrollRect.gameObject)
        {
            Debug.Log("Dragging Scroll");
            mAutoScrollCD = float.MaxValue;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag == RankScrollRect.gameObject)
        {
            Debug.Log("Dragging Scroll");
            mAutoScrollCD = float.MaxValue;
        }
    }
}
