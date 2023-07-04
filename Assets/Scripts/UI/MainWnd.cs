using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 主页界面
/// </summary>
public class MainWnd : WndBase
{
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
        public GameObject LvGo;
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
            HeadIcon = Trans.Find("HeadBg/HeadIcon").GetComponent<Image>();
            LvGo = Trans.Find("Lv").gameObject;
            Level = Trans.Find("Level").GetComponent<Text>();

            this.IsBig = isBig;
        }

        public void SetData(string name, string url, int index, int level, bool levelRank)
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

            if (levelRank == true)
            {
                Level.gameObject.SetActive(true);
                Level.text = (level + 1).ToString();
                LvGo.SetActive(true);
            }
            else
            {
                Level.gameObject.SetActive(false);
                LvGo.SetActive(false);
            }
            LoadHeadIcon(url);
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
    private Dictionary<int, RankItem> mLevelRankItems = new Dictionary<int, RankItem>();
    private Dictionary<int, RankItem> mDmgRankItems = new Dictionary<int, RankItem>();
    private int mBigItemAheadNum = 3;
    private float mRankItemBigHeight;
    private float mRankItemSmallHeight;

    public GameObject RankItemBigTemp;
    public GameObject RankItemSmallTemp;
    public Transform DmgRankPartTrans;
    public Transform LevelRankPartTrans;
    public ScrollRect DmgRankScrollRect;
    public ScrollRect LevelRankScrollRect;
    public RectTransform DmgRankScrollTrans;
    public RectTransform LevelRankScrollTrans;
    public RectTransform DmgRankContent;
    public RectTransform LevelRankContent;

    public Button ChallengeModeButton;
    public Button KillDragonModeButton;
    public Button LimitTimeActButton;
    public Button SettingButton;

    public Text ConnectionMsg;

    private List<UserData> mDmgUserList;
    private List<UserData> mLevelUserList;

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

        for (int i = 0; i < 30; ++i)
        {
            var go = GameObject.Instantiate(RankItemSmallTemp, Trans);
            var item = new RankItem(go, false);
            item.Go.SetActive(false);
            mFreeRankItemSmall.Enqueue(item);
            go.name = "small " + i.ToString();
        }
        for (int i = 0; i < 6; ++i)
        {
            var go = GameObject.Instantiate(RankItemBigTemp, Trans);
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

        return true;
    }

    private void SetupDmgRankList(List<UserData> rankUserList)
    {
        mDmgUserList = rankUserList;

        float totalHeight = 0;
        for (int i = 0, max = rankUserList.Count; i < max; ++i)
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
        DmgRankContent.sizeDelta = new Vector2(DmgRankContent.sizeDelta.x, totalHeight);
        DmgRankScrollRect.onValueChanged.AddListener(OnDmgRankScrollValueChange);

        OnDmgRankScrollValueChange(Vector2.zero);
    }
    private void OnDmgRankScrollValueChange(Vector2 delta)
    {
        // 下拖拉y值越大, 顶部时0
        float height = DmgRankScrollTrans.sizeDelta.y;
        float curPosY = DmgRankContent.anchoredPosition.y;
        float heightAcc = 0;
        float curPosIndexHeight = 0;
        bool foundStartPosIndex = false;
        int curPosIndex = 0;
        int endPosIndex = mDmgUserList.Count - 1;
        for (int i = 0, max = mDmgUserList.Count; i < max; ++i)
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

        var rankItemList = mDmgRankItems.ToList();
        foreach (var kv in rankItemList)
        {
            var item = kv.Value;
            if (item.Index < curPosIndex || item.Index > endPosIndex)
            {
                mDmgRankItems.Remove(kv.Key);
                item.Trans.SetParent(DmgRankPartTrans, false);
                item.Go.SetActive(false);
                if (item.IsBig == true)
                {
                    mFreeRankItemBig.Enqueue(item);
                }
                else
                {
                    mFreeRankItemSmall.Enqueue(item);
                }

                item.SetData("", "", -1, 0, false);
            }
        }

        for (int i = curPosIndex; i <= endPosIndex; ++i)
        {
            if (mDmgRankItems.ContainsKey(i) == false)
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

                item.Trans.SetParent(DmgRankContent.transform, false);
                item.Go.SetActive(true);
                var userData = mDmgUserList[i];
                item.SetData(userData.name, userData.headPic, i, DataManager.Instance.GetActorLevelByExp(userData.exp), false);
                item.RectTrans.anchoredPosition = new Vector2(0, -curPosIndexHeight);
                mDmgRankItems.Add(i, item);
            }

            curPosIndexHeight += (i < mBigItemAheadNum ? mRankItemBigHeight : mRankItemSmallHeight);
        }
    }

    private void SetupLevelRankList(List<UserData> rankUserList)
    {
        mLevelUserList = rankUserList;

        float totalHeight = 0;
        for (int i = 0, max = rankUserList.Count; i < max; ++i)
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
        LevelRankContent.sizeDelta = new Vector2(LevelRankContent.sizeDelta.x, totalHeight);
        LevelRankScrollRect.onValueChanged.AddListener(OnLevelRankScrollValueChange);

        OnLevelRankScrollValueChange(Vector2.zero);
    }
    private void OnLevelRankScrollValueChange(Vector2 delta)
    {
        // 下拖拉y值越大, 顶部时0
        float height = LevelRankScrollTrans.sizeDelta.y;
        float curPosY = LevelRankContent.anchoredPosition.y;
        float heightAcc = 0;
        float curPosIndexHeight = 0;
        bool foundStartPosIndex = false;
        int curPosIndex = 0;
        int endPosIndex = mLevelUserList.Count - 1;
        for (int i = 0, max = mLevelUserList.Count; i < max; ++i)
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

        var rankItemList = mLevelRankItems.ToList();
        foreach (var kv in rankItemList)
        {
            var item = kv.Value;
            if (item.Index < curPosIndex || item.Index > endPosIndex)
            {
                mLevelRankItems.Remove(kv.Key);
                item.Trans.SetParent(LevelRankPartTrans, false);
                item.Go.SetActive(false);
                if (item.IsBig == true)
                {
                    mFreeRankItemBig.Enqueue(item);
                }
                else
                {
                    mFreeRankItemSmall.Enqueue(item);
                }

                item.SetData("", "", -1, 0, true);
            }
        }

        for (int i = curPosIndex; i <= endPosIndex; ++i)
        {
            if (mLevelRankItems.ContainsKey(i) == false)
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

                item.Trans.SetParent(LevelRankContent.transform, false);
                item.Go.SetActive(true);
                var userData = mLevelUserList[i];
                item.SetData(userData.name, userData.headPic, i, DataManager.Instance.GetActorLevelByExp(userData.exp), true);
                item.RectTrans.anchoredPosition = new Vector2(0, -curPosIndexHeight);
                mLevelRankItems.Add(i, item);
            }

            curPosIndexHeight += (i < mBigItemAheadNum ? mRankItemBigHeight : mRankItemSmallHeight);
        }
    }

    public async override void OnShow(bool isNeedFade = true)
    {
        base.OnShow(isNeedFade);

        ChallengeModeButton.onClick.AddListener(OnChallengeModeButtonClick);
        KillDragonModeButton.onClick.AddListener(OnKillDragonModeButtonClick);
        LimitTimeActButton.onClick.AddListener(OnLimitActButtonClick);
        SettingButton.onClick.AddListener(OnSettingButtonClick);

        var selfUserData = ClientManager.Instance.SelfUserData;
        if (selfUserData != null && string.IsNullOrEmpty(selfUserData.ID) == false)
        {
            var levelRankData = await ClientManager.Instance.GetRank(ClientManager.Instance.SelfUserData.ID, 1);
            var dmgRankData = await ClientManager.Instance.GetRank(ClientManager.Instance.SelfUserData.ID, 2);
            OnMsg(WndMsgType.showRank, levelRankData, dmgRankData);
        }
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);

        ChallengeModeButton.onClick.RemoveAllListeners();
        KillDragonModeButton.onClick.RemoveAllListeners();
        LimitTimeActButton.onClick.RemoveAllListeners();
        SettingButton.onClick.RemoveAllListeners();
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
        }
        else if (WndMsgType.showRank == msgType)
        {
            var rankItemList = mDmgRankItems.ToList();
            foreach (var kv in rankItemList)
            {
                var item = kv.Value;
                mDmgRankItems.Remove(kv.Key);
                item.Trans.SetParent(DmgRankPartTrans, false);
                item.Go.SetActive(false);
                if (item.IsBig == true)
                {
                    mFreeRankItemBig.Enqueue(item);
                }
                else
                {
                    mFreeRankItemSmall.Enqueue(item);
                }
                item.SetData("", "", -1, 0, false);
            }

            rankItemList = mLevelRankItems.ToList();
            foreach (var kv in rankItemList)
            {
                var item = kv.Value;
                mLevelRankItems.Remove(kv.Key);
                item.Trans.SetParent(LevelRankPartTrans, false);
                item.Go.SetActive(false);
                if (item.IsBig == true)
                {
                    mFreeRankItemBig.Enqueue(item);
                }
                else
                {
                    mFreeRankItemSmall.Enqueue(item);
                }
                item.SetData("", "", -1, 0, true);
            }

            if (msgParams[0] != null)
            {
                SetupLevelRankList(msgParams[0] as List<UserData>);
            }
            if (msgParams[1] != null)
            {
                SetupDmgRankList(msgParams[1] as List<UserData>);
            }
        }
    }

    private void OnChallengeModeButtonClick()
    {

    }
    private async void OnKillDragonModeButtonClick()
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

        var battleInfo = DataManager.Instance.GetBattleInfo("killDragon1");
        await BattleManager.Instance.CreateBattle(battleInfo);

        UIManager.Instance.ShowWnd(WndType.killDragonBattleWnd);
        UIManager.Instance.SendMsg(WndType.killDragonBattleWnd, WndMsgType.initContent);

        HideSelf();
    }
    private void OnLimitActButtonClick()
    {

    }
    private void OnSettingButtonClick()
    {
        UIManager.Instance.ShowWnd(WndType.settingWnd);
    }

    private int loginStep = 0;
    private void Update()
    {
        if (ClientManager.Instance.HasLogined == true)
        {
            if (loginStep != 1)
            {
                loginStep = 1;
                ConnectionMsg.gameObject.SetActive(true);
                ConnectionMsg.text = "已连接服务器";
                ConnectionMsg.color = Color.green;
            }
        }
        else if (ClientManager.Instance.Logining == true)
        {
            if (loginStep != 2)
            {
                loginStep = 2;
                ConnectionMsg.gameObject.SetActive(true);
                ConnectionMsg.text = "正在连接中...";
                ConnectionMsg.color = Color.yellow;
            }
        }
        else
        {
            if (loginStep != 3)
            {
                loginStep = 3;
                ConnectionMsg.gameObject.SetActive(true);
                ConnectionMsg.text = "未连接服务器...";
                ConnectionMsg.color = Color.red;
            }
        }
    }
}
