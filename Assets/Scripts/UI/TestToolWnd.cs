using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestToolWnd : WndBase
{
    public override async Task<bool> Init(sWndAssetRef assetRef)
    {
        bool result = await base.Init(assetRef);
        if (result == false)
        {
            return false;
        }

        mCurLayer = CanvasLayer.main;
        mShowTransitionType = EnWndShowHideTransition.pop;
        mHideTransitionType = EnWndShowHideTransition.pop;

        mInited = true;

        return true;
    }
}
