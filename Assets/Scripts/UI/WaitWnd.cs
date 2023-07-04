using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitWnd : WndBase
{
    public GameObject contentPart;
    public Image waitImage;
    public float fillTime;
    public float waitToShowGreyBoardTime;
    private float accWaitToShowTime;
    private float accTime;
    private bool inverse;

    public override async Task<bool> Init(sWndAssetRef assetRef)
    {
        bool result = await base.Init(assetRef);
        if (result == false)
        {
            return false;
        }

        mCurLayer = CanvasLayer.wait;
        mShowTransitionType = EnWndShowHideTransition.max;
        mHideTransitionType = EnWndShowHideTransition.fade;

        mInited = true;

        return true;
    }

    // Update is called once per frame
    protected void Update()
    {
        var deltaTime = Time.deltaTime;
        accWaitToShowTime += deltaTime;
        accTime += deltaTime;

        if (accWaitToShowTime >= waitToShowGreyBoardTime && contentPart.activeInHierarchy == false)
        {
            contentPart.SetActive(true);
            mCanvasGroup.DOKill();
            mCanvasGroup.alpha = 0.0f;
            mCanvasGroup.DOFade(1.0f, 0.15f);
        }

        if (accTime >= fillTime)
        {
            waitImage.fillAmount = inverse == true ? 0.0f : 1.0f;
            accTime = accTime - fillTime;

            inverse = !inverse;
        }
        else
        {
            if (accTime <= float.MinValue)
            {
                waitImage.fillAmount = inverse == true ? 1.0f : 0.0f;
            }
            else
            {
                if (inverse == false)
                {
                    waitImage.fillAmount = accTime / fillTime;
                }
                else
                {
                    waitImage.fillAmount = 1 - accTime / fillTime;
                }
            }
        }
    }

    public override void OnShow(bool isNeedFade = true)
    {
        base.OnShow(isNeedFade);

        accWaitToShowTime = 0;
        accTime = 0;
        waitImage.fillAmount = 0;
        inverse = false;

        contentPart.SetActive(false);
    }
}
