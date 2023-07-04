using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 战斗管理器
/// </summary>
public class BattleManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static BattleManager sInstance;
    public static BattleManager Instance => sInstance;
    private void Awake()
    {
        if (sInstance != null && sInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sInstance = this;
            DontDestroyOnLoad(gameObject);

            mGo = gameObject;
            mTrans = transform;
        }
    }
    #endregion

    private GameObject mGo;
    private Transform mTrans;
    // 当前战斗, 默认同一时刻只有一个战斗
    private Battle mCurBattle;

    #region getter
    public Battle curBattle => mCurBattle;
    public GameObject Go => mGo;
    public Transform Trans => mTrans;
    #endregion

    // 创建战斗
    public async Task<Battle> CreateBattle(BattleInfo info)
    {
        var go = await AssetManager.Instantiate(info.assetAddress, mTrans);
        var battle = go.GetComponent<Battle>();
        await battle.Init(info);

        if (info.battleType == BattleType.killDragon)
        {

        }
        else if (info.battleType == BattleType.challenge)
        {
        }

        mCurBattle = battle;

        return battle;
    }

    // 删除战斗
    public void DestroyCurBattle()
    {
        if (mCurBattle != null)
        {
            mCurBattle.Destroy();
            mCurBattle = null;
        }
    }
}
