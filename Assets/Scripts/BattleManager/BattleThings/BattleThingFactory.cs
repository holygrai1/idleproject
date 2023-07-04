using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// thing制造工厂类
/// </summary>
public class BattleThingFactory : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static BattleThingFactory sInstance;
    public static BattleThingFactory Instance => sInstance;
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
        }
    }
    #endregion

    private Dictionary<string, Queue<BattleMagic>> mFreeMagics = new Dictionary<string, Queue<BattleMagic>>();

    // 获取魔法
    public async Task<BattleMagic> GetMagic(string assetAddress, Transform parent)
    {
        if (mFreeMagics.TryGetValue(assetAddress, out Queue<BattleMagic> freeMagicQueue) == true && freeMagicQueue.Count > 0)
        {
            return freeMagicQueue.Dequeue();
        }
        else
        {
            var go = await AssetManager.Instantiate(assetAddress, parent);
            var magic = go.GetComponent<BattleMagic>();
            magic.SetAssetAddress(assetAddress);
            return magic;
        }
    }

    // 缓存魔法
    public bool TryCacheMagic(string assetAddress, BattleMagic magic)
    {
        Queue<BattleMagic> freeMagicQueue = null;
        mFreeMagics.TryGetValue(assetAddress, out freeMagicQueue);
        if (freeMagicQueue == null)
        {
            freeMagicQueue = new Queue<BattleMagic>();
            mFreeMagics.Add(assetAddress, freeMagicQueue);
        }

        // 每个魔法资源暂时默认最多缓存10个
        if (freeMagicQueue.Count > 10)
        {
            return false;
        }

        freeMagicQueue.Enqueue(magic);

        return true;
    }

}
