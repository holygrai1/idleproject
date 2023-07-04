using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// 战斗中角色信息窗口
public class BattleActorInfoWnd : WndBase
{
    public RectTransform BgRectTrans;
    public Text Name;
    public Text Lv;
    public Image HeadImg;

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
    }

    public override void OnHide(bool isNeedFade = true)
    {
        base.OnHide(isNeedFade);
        HeadImg.sprite = null;
    }

    public override void OnMsg(WndMsgType msgType, params object[] msgParams)
    {
        base.OnMsg(msgType, msgParams);

        if (WndMsgType.initContent == msgType)
        {
            SetActor(msgParams[0] as BattleActor);
        }
    }

    public void SetActor(BattleActor actor)
    {
        string userID = actor.UserID;
        if (string.IsNullOrEmpty(userID) == true)
        {
            Helpers.LoadSpriteAtlas("ActorIcon", actor.FakeID.ToString(), (Sprite sp) =>
            {
                HeadImg.sprite = sp;
            });
            Name.text = actor.FakeName;
        }
        else
        {
            var userData = ClientManager.Instance.GetUserData(userID);
            Helpers.SetImageFromURL(userData.headPic, HeadImg);
            Name.text = userData.name;
        }
        Lv.text = "Lv: " + (actor.CurLevel + 1).ToString();

        var anchorPos = Helpers.WorldPositionUIAnchorPos(actor.MsgBubblePos.position);
        var halfWidth = BgRectTrans.sizeDelta.x * 0.5f;
        float gap = 5.0f;
        if (anchorPos.x - halfWidth <= gap - 270.0f)
        {
            anchorPos.x = gap + halfWidth - 270.0f;
        }
        else if (anchorPos.x + halfWidth >= 270 - gap)
        {
            anchorPos.x = 270 - gap - halfWidth;
        }

        BgRectTrans.anchoredPosition = anchorPos;
    }
}
