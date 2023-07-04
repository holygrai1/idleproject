using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 角色头像对话框
public class MsgBubble : MonoBehaviour
{
    public GameObject Go;
    public RectTransform Bg;
    public RectTransform TextRect;
    public Text Text;
    public RectTransform ArrowRect;
    public float MaxWidth;
    public int MaxLine;
    public float HeightPerLine;

    private string mMsg;
    private float mOffsetY = 0;

    public void SetMsg(string msg)
    {
        if (mMsg == msg)
        {
            return;
        }

        Text.text = msg;
        float preferredWidth = Text.preferredWidth;

        if (preferredWidth > MaxWidth)
        {
            int lineNum = Mathf.CeilToInt(preferredWidth / MaxWidth);
            lineNum = Mathf.Min(MaxLine, lineNum);
            float height = HeightPerLine * lineNum;
            TextRect.sizeDelta = new Vector2(MaxWidth, height);
            float gap = (lineNum - 1) * Text.lineSpacing;
            Bg.sizeDelta = new Vector2(MaxWidth + 10, height + gap + 10);
            ArrowRect.anchoredPosition = new Vector2(ArrowRect.anchoredPosition.x, -(height + gap + 10) * 0.5f);

            mOffsetY = (height + gap + 10 - HeightPerLine) * 0.5f;

            Text.SetTextWithEllipsis(msg);
        }
        else
        {
            TextRect.sizeDelta = new Vector2(preferredWidth, HeightPerLine);
            Bg.sizeDelta = new Vector2(preferredWidth + 10, HeightPerLine + 10);

            ArrowRect.anchoredPosition = new Vector2(ArrowRect.anchoredPosition.x, -(HeightPerLine + 10) * 0.5f);

            mOffsetY = 0.0f;
        }

        Go.SetActive(true);
    }

    public void UpdatePos(Transform target)
    {
        var anchorPos = Helpers.WorldPositionUIAnchorPos(target.position);
        Bg.anchoredPosition = anchorPos + new Vector2(0, mOffsetY);
    }

    public void Release()
    {
        UIManager.Instance.ReleaseMsgBubble(this);
    }
}
