using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.U2D;

/// <summary>
/// ?????
/// </summary>
public class AssetManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static AssetManager sInstance;
    public static AssetManager Instance => sInstance;
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

    public bool logEnable;

    /// <summary>
    /// ??????
    /// </summary>
    public enum AssetUpdateStep
    {
        initialize,
        checkForCatalogUpdate,
        UpdateCatalogs,
        StartDownload,
        Download,
        finish,

        max
    }


    private static Transform sGoPoolParentTrans;
    private static Dictionary<AsyncOperationHandle, int> sCurOperations;
    private Transform mTrans;
    private bool mInited = false;
    private AssetUpdateStep mAssetUpdateStep = AssetUpdateStep.max;

    public bool inited => mInited;
    public AssetUpdateStep assetUpdateStep => mAssetUpdateStep;

    /// <summary>
    /// ???
    /// </summary>
    /// <returns></returns>
    public Task<bool> Init()
    {
        if (mInited == true)
        {
            return Task.FromResult(false);
        }

        mTrans = transform;
        sGoPoolParentTrans = mTrans.Find("AssetDefaultParent");
        sGoPoolParentTrans.gameObject.SetActive(false);
        sCurOperations = new Dictionary<AsyncOperationHandle, int>();

        Addressables.InternalIdTransformFunc = InternalIdTransformFunc;

        if (logEnable)
        {
            LogManager.Log("Application.streamingAssetsPath: " + Application.streamingAssetsPath);
            LogManager.Log("Application.persistentDataPath: " + Application.persistentDataPath);
            LogManager.Log("Application.temporaryCachePath: " + Application.temporaryCachePath);
            LogManager.Log("Addressables.RuntimePath: " + Addressables.RuntimePath);
            LogManager.Log("Addressables.BuildPath: " + Addressables.BuildPath);
            LogManager.Log("Addressables.LibraryPath: " + Addressables.LibraryPath);
            LogManager.Log("Caching.currentCacheForWriting.path: " + Caching.currentCacheForWriting.path);

        }

        mInited = true;

        return Task.FromResult(true);
    }

    /// <summary>
    /// ????
    /// </summary>
    /// <param name="stepCallback"></param>
    /// <returns></returns>
    public async Task UpdateAssets(Action<AssetUpdateStep, int> stepCallback)
    {
        stepCallback?.Invoke(AssetUpdateStep.initialize, 0);
        var start = DateTime.Now;
        await Addressables.InitializeAsync().Task;

        if (logEnable) LogManager.Log("InitializeAsync use " + (DateTime.Now - start).Milliseconds + " ms");

        stepCallback?.Invoke(AssetUpdateStep.checkForCatalogUpdate,0);
        start = DateTime.Now;

        if (logEnable) LogManager.Log("CheckForCatalogUpdates use " + (DateTime.Now - start).Milliseconds + " ms");
        var handle = Addressables.CheckForCatalogUpdates(false);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (logEnable) LogManager.Log("CheckForCatalogUpdates succeed");

            List<string> catalogs = handle.Result;
            float downloadTotalSize = 0;
            if (logEnable) LogManager.Log("handle.Result: " + handle.Result);

            if (catalogs != null && catalogs.Count > 0)
            {
                if (logEnable) LogManager.Log("Addressables.UpdateCatalogs start");

                stepCallback?.Invoke(AssetUpdateStep.UpdateCatalogs,0);
                start = DateTime.Now;

                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                await updateHandle.Task;
                if (logEnable) LogManager.Log("UpdateCatalog use " + (DateTime.Now - start).Milliseconds + " ms");
                
                var locators = updateHandle.Result;
                List<object> downloadKeys = new List<object>();                
                foreach (var locator in locators)
                {
                    int i = 0;
                    stepCallback?.Invoke(AssetUpdateStep.StartDownload, locator.Keys.Count());
                    //  Debug.Log("LocatorId " + locator.LocatorId);
                    
                    foreach (var key in locator.Keys)
                    {
                        i++;
                        stepCallback?.Invoke(AssetUpdateStep.Download, i);

                        // Debug.Log("key " + key);
                        AsyncOperationHandle<long> downloadSize = Addressables.GetDownloadSizeAsync(key);
                        await downloadSize.Task;
                        if (downloadSize.Result > 0)
                        {
                            if (logEnable) LogManager.Log("Update [" + key + "]");
                            downloadKeys.Add(key);
                            downloadTotalSize += downloadSize.Result / Mathf.Pow(1024, 2);


                            var downloadHandle = Addressables.DownloadDependenciesAsync(key, false);
                            while (downloadHandle.IsDone == false)
                            {
                                float percent = downloadHandle.PercentComplete;
                                if (logEnable) LogManager.Log($"Downloaded [{(int)(downloadTotalSize * percent)}/{downloadTotalSize}]");

                                await downloadHandle.Task;
                            }                            
                            Addressables.Release(downloadHandle);
                        }
                    }
                }

                //if (downloadKeys.Count > 0)
                //{
                //    var downloadHandle = Addressables.DownloadDependenciesAsync(downloadKeys, false);
                //    while (downloadHandle.IsDone == false)
                //    {
                //        float percent = downloadHandle.PercentComplete;
                //        if (logEnable) LogManager.Log($"Downloaded [{(int)(downloadTotalSize * percent)}/{downloadTotalSize}]");
                //    }

                //    Addressables.Release(downloadHandle);
                //}

                Addressables.Release(updateHandle);
            }
        }

        Addressables.Release(handle);

        stepCallback?.Invoke(AssetUpdateStep.finish, 0);
    }

    /// <summary>
    /// ????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="cancleable">?????</param>
    /// <returns></returns>
    public static async Task<T> LoadAssetAsync<T>(string address, bool cancleable = true)
    {
        if (IsAssetExists(address) == false)
        {
            LogManager.Error("LoadAssetAsync????????: " + address);
            return default(T);
        }

        var op = Addressables.LoadAssetAsync<T>(address);

        if (cancleable == true)
        {
            int count = 0;
            if (sCurOperations.TryGetValue(op, out count) == false)
            {
                sCurOperations.Add(op, 1);
            }
            else
            {
                sCurOperations[op] = count + 1;
            }
        }

        var asset = await op.Task;

        if (cancleable == false)
        {
            return asset;
        }
        else
        {
            int count = 0;
            if (sCurOperations.TryGetValue(op, out count) == true)
            {
                --count;
                if (count <= 0)
                {
                    sCurOperations.Remove(op);
                }
                else
                {
                    sCurOperations[op] = count;
                }

                return asset;
            }
            else
            {
                ReleaseAsset(asset);
                return default(T);
            }
        }
    }

    public static async Task<TMP_FontAsset> LoadFontAsync(string address, bool cancleable = true)
    {
        return await LoadAssetAsync<TMP_FontAsset>(address, cancleable);
    }

    public static async Task<SpriteAtlas> LoadAtlasAsync(string address, bool cancleable = true)
    {
        return await LoadAssetAsync<SpriteAtlas>(address, cancleable);
    }

    public static async Task<TextAsset> LoadTextAsync(string address, bool cancleable = true)
    {
        return await LoadAssetAsync<TextAsset>(address, cancleable);
    }
    public static async Task<GameObject> LoadGameObjectAsync(string address, bool cancleable = true)
    {
        return await LoadAssetAsync<GameObject>(address, cancleable);
    }

    public static async Task<Sprite> LoadSpriteAsync(string atlasAddress, string spriteName, bool cancleable = true)
    {
        return await LoadAssetAsync<Sprite>(atlasAddress + "[" + spriteName + "]", cancleable);
    }

    public static async void LoadSpriteCallback(string atlasAddress, string spriteName, Action<Sprite> callback, bool cancleable = true)
    {
        var sprite = await LoadAssetAsync<Sprite>(atlasAddress + "[" + spriteName + "]", cancleable);
        callback(sprite);
    }

    public static async Task<AudioClip> LoadSoundsAsync(string address, bool cancleable = true)
    {
        return await LoadAssetAsync<AudioClip>(address, cancleable);
    }

    public static void ReleaseAsset<T>(T obj)
    {
        Addressables.Release(obj);
    }

    public static void ReleaseSprite(UnityEngine.Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        ReleaseAsset<UnityEngine.Sprite>(sprite);
    }

    /// <summary>
    /// ??label????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="label"></param>
    /// <param name="cancleable"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, T>> LoadAssetsAsyncByLabel<T>(string label, bool cancleable = true)
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(label);
        if (cancleable == true)
        {
            int count = 0;
            if (sCurOperations.TryGetValue(locationHandle, out count) == false)
            {
                sCurOperations.Add(locationHandle, 1);
            }
            else
            {
                sCurOperations[locationHandle] = count + 1;
            }
        }

        await locationHandle.Task;

        var waitTasks = new Dictionary<string, Task<T>>();
        foreach (var loc in locationHandle.Result)
        {
            waitTasks.Add(loc.PrimaryKey, Addressables.LoadAssetAsync<T>(loc).Task);
        }

        var result = new Dictionary<string, T>();
        foreach (var kv in waitTasks)
        {
            var task = kv.Value;
            var primaryKey = kv.Key;
            var asset = await task;
            result.Add(primaryKey, asset);
        }

        Addressables.Release(locationHandle);

        if (cancleable == false)
        {
            return result;
        }
        else
        {
            int count = 0;
            if (sCurOperations.TryGetValue(locationHandle, out count) == false)
            {
                foreach (var kv in result)
                {
                    Addressables.Release(kv.Value);
                }
                result.Clear();
                return result;
            }
            else
            {
                --count;
                if (count <= 0)
                {
                    sCurOperations.Remove(locationHandle);
                }
                else
                {
                    sCurOperations[locationHandle] = count;
                }

                return result;
            }
        }
    }

    /// <summary>
    /// ??????
    /// </summary>
    /// <param name="address"></param>
    /// <param name="parentTransOrPool">null???? ????????Go????????????, inactive????</param>
    /// <param name="instantiateInWorldSpace"></param>
    /// <param name="trackHandle"></param>
    /// <param name="cancleable">?????</param>
    /// <returns></returns>
    public static async Task<GameObject> Instantiate(string address, Transform parentTransOrPool, bool instantiateInWorldSpace = false, bool trackHandle = true, bool cancleable = true)
    {
        var parent = parentTransOrPool == null ? sGoPoolParentTrans : parentTransOrPool;
        
        var op = Addressables.InstantiateAsync(address, parent, instantiateInWorldSpace, trackHandle);
        if (cancleable == true)
        {
            int count = 0;
            if (sCurOperations.TryGetValue(op, out count) == false)
            {
                sCurOperations.Add(op, 1);
            }
            else
            {
                sCurOperations[op] = count + 1;
            }
        }

        var instance = await op.Task;

        if (cancleable == false)
        {
            return instance;
        }
        else
        {
            int count = 0;
            if (sCurOperations.TryGetValue(op, out count) == false)
            {
                ReleaseInstance(instance);
                return null;
            }
            else
            {
                --count;
                if (count <= 0)
                {
                    sCurOperations.Remove(op);
                }
                else
                {
                    sCurOperations[op] = count;
                }

                return instance;
            }
        }
    }
    public static void ReleaseInstance(GameObject go)
    {
        Addressables.ReleaseInstance(go);
    }

    /// <summary>
    /// ????????
    /// </summary>
    public static void CancelAllOperations()
    {
        sCurOperations.Clear();
    }
    public static void CancelAnOperation(AsyncOperationHandle op)
    {
        int count = 0;
        if (sCurOperations.TryGetValue(op, out count) == true)
        {
            --count;
            if (count <= 0)
            {
                sCurOperations.Remove(op);
            }
            else
            {
                sCurOperations[op] = count;
            }
        }
    }

    /// <summary>
    /// ????????
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool IsAssetExists(object address)
    {
        foreach (var loc in Addressables.ResourceLocators)
        {
            if (loc.Locate(address, null, out IList<IResourceLocation> locs) == true)
            {
                return true;
            }
        }
        return false;
    }

    private string InternalIdTransformFunc(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation location)
    {
        if (location.Data is AssetBundleRequestOptions)
        {
            var ab = location.Data as AssetBundleRequestOptions;
            if (logEnable) LogManager.Log(">>>>:: " + ab.BundleName + ", " + ab.BundleSize +","+ab.Hash);
            var path = Path.Combine(Addressables.RuntimePath, location.PrimaryKey);
            if (File.Exists(path))
            {
                if (logEnable) LogManager.Log("From: " + Addressables.ResolveInternalId(location.InternalId)
                                            + "\nTo streaming: " + path);
                return path;
            }
            else
            {
                path = Path.Combine(Application.persistentDataPath, ab.BundleName, ab.Hash, "__data");
                if (File.Exists(path))
                {
                    if (logEnable) LogManager.Log("From: " + Addressables.ResolveInternalId(location.InternalId) 
                                                    + "\nTo presisten: " + path + ", file: " + location.PrimaryKey);
                    return path;
                }
            }
        }
        if (logEnable) LogManager.Log("From: " + Addressables.ResolveInternalId(location.InternalId) 
                                        + "\nTo same: " + location.InternalId);

        return location.InternalId;
    }
}
