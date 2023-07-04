using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// 本地化类型
/// </summary>
public static class LocaleTypeConst
{
    // 简体中文
    public readonly static string zh_CN= "zh_CN";
    // 繁体中文
    public readonly static string zh_TW = "zh_TW";
    // 英文
    public readonly static string en_US = "en_US";
    // 法文
    public readonly static string fr_FR = "fr_FR";
    // 西班牙文
    public readonly static string es_ES = "es_ES";
    // 德语
    public readonly static string de_DE = "de_DE";
}

/// <summary>
/// 本地化管理器
/// </summary>
public class LocaleManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static LocaleManager sInstance;
    public static LocaleManager Instance => sInstance;
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

    private Func<string, string, Task<bool>> mPreWaitChangeAction;
    private Func<string, string, bool> mChangeAction;
    private string mPreLocale;
    private string mCurLocale;
    private readonly static string PrefarbKey = "locale";
    private Dictionary<string, List<TMP_FontAsset>> mDicFontAssets;
    private readonly static string sDefaultLocale = LocaleTypeConst.en_US;
    public static string defaultLocal { get=>sDefaultLocale;}

    /// <summary>
    /// 获取当前语言
    /// </summary>
    public string curLocale { get => mCurLocale; }
    /// <summary>
    /// 获取上一个语言
    /// </summary>
    public string preLocale { get => mPreLocale; }


    public async Task<bool> Init()
    {
        mDicFontAssets = new Dictionary<string, List<TMP_FontAsset>>();

        if (PlayerPrefs.HasKey(PrefarbKey) == false)
        {
            SetLocaleBySystemLanguage();
        }
        else
        {
            var strLocale = PlayerPrefs.GetString(PrefarbKey);

            if (string.IsNullOrEmpty(strLocale) == true)
            {
                SetLocaleBySystemLanguage();
            }
            else
            {
                if (strLocale == LocaleTypeConst.en_US)
                {
                    mCurLocale = LocaleTypeConst.en_US;
                }
                else if (strLocale == LocaleTypeConst.zh_CN)
                {
                    mCurLocale = LocaleTypeConst.zh_CN;
                }
                else if (strLocale == LocaleTypeConst.zh_TW)
                {
                    mCurLocale = LocaleTypeConst.zh_TW;
                }
                else if (strLocale == LocaleTypeConst.fr_FR)
                {
                    mCurLocale = LocaleTypeConst.fr_FR;
                }
                else if (strLocale == LocaleTypeConst.es_ES)
                {
                    mCurLocale = LocaleTypeConst.es_ES;
                }
                else if (strLocale == LocaleTypeConst.de_DE)
                {
                    mCurLocale = LocaleTypeConst.de_DE;
                }
                else
                {
                    mCurLocale = LocaleTypeConst.en_US;
                }
                
                if (string.IsNullOrEmpty(mCurLocale) == true)
                {
                    SetLocaleBySystemLanguage();
                }
            }
        }
        
        //if (mCurLocale != defaultLocal)
        {
            await LoadFontAssets(mCurLocale);
        }

        return await Task.FromResult(true);
    }

    // 寻找Font包内的字体资源， 该资源名的命名方式固定， "Font_" + locale + 序号, 其中序号1为该locale的默认字体， 其余的为fallback字体
    public async Task LoadFontAssets(string locale)
    {
        if (mDicFontAssets.ContainsKey(locale) == true)
        {
            return;
        }

        var found = await LoadFont(locale);
        if (found == false)
        {
            mCurLocale = LocaleTypeConst.en_US;
            PlayerPrefs.SetString(PrefarbKey, LocaleTypeConst.en_US);
            await LoadFont(LocaleTypeConst.en_US);
        }
    }

    private async Task<bool> LoadFont(string locale)
    {
        int i = 1;
        bool foundFont = false;
        do
        {
            string fileNamePre = "Font_";
            string fileNameFull = fileNamePre + locale + i;
            bool fileFound = false;
            if (AssetManager.IsAssetExists(fileNameFull) == true)
            {
                // example : Font_cn_ZH1
                fileFound = true;
                foundFont = true;
            }
            else
            {
                string[] strLocalInfo = locale.Split('_');
                if (strLocalInfo.Length > 0)
                {
                    fileNameFull = fileNamePre + strLocalInfo[0] + i;
                    if (AssetManager.IsAssetExists(fileNameFull) == true)
                    {
                        // example : Font_cn1
                        fileFound = true;
                        foundFont = true;
                    }
                }
            }

            ++i;

            if (fileFound == true)
            {
                var fontAsset = await AssetManager.LoadAssetAsync<TMP_FontAsset>(fileNameFull);
                List<TMP_FontAsset> fontAssets = null;
                if (mDicFontAssets.TryGetValue(locale, out fontAssets) == false)
                {
                    fontAssets = new List<TMP_FontAsset>();
                    mDicFontAssets.Add(locale, fontAssets);
                }
                fontAssets.Add(fontAsset);

                if (fontAssets.Count > 1)
                {
                    fontAssets[0].fallbackFontAssetTable.Add(fontAsset);
                }
            }
            else
            {
                break;
            }
        } while (true);

        return foundFont;
    }

    public async void ChangeLocale(string newLocale)
    {
        if (newLocale == mCurLocale)
        {
            return;
        }

        string preLocals = mCurLocale;

        if (newLocale == LocaleTypeConst.en_US)
        {
            mCurLocale = LocaleTypeConst.en_US;
        }
        else if (newLocale == LocaleTypeConst.zh_CN)
        {
            mCurLocale = LocaleTypeConst.zh_CN;
        }
        else if (newLocale == LocaleTypeConst.zh_TW)
        {
            mCurLocale = LocaleTypeConst.zh_TW;
        }
        else if (newLocale == LocaleTypeConst.fr_FR)
        {
            mCurLocale = LocaleTypeConst.fr_FR;
        }
        else if (newLocale == LocaleTypeConst.es_ES)
        {
            mCurLocale = LocaleTypeConst.es_ES;
        }
        else if (newLocale == LocaleTypeConst.de_DE)
        {
            mCurLocale = LocaleTypeConst.de_DE;
        }

        if (preLocals == mCurLocale)
        {
            Debug.LogError("没定义的本地化语言类型");
            return;
        }

        mPreLocale = preLocals;

        PlayerPrefs.SetString(PrefarbKey, mCurLocale);

        if (mDicFontAssets.ContainsKey(mCurLocale) == false)
        {
            await LoadFontAssets(mCurLocale);
        }

        if (mPreWaitChangeAction != null)
        {
            await mPreWaitChangeAction(preLocale, mCurLocale);
        }

        if (mChangeAction != null)
        {
            mChangeAction(preLocals, mCurLocale);
        }
    }

    public void RegisterLocaleChangeEvent(Func<string, string, bool> handler, bool sendEventNow =  true)
    {
        mChangeAction += handler;

        if (sendEventNow == true)
        {
            handler(mPreLocale, mCurLocale);
        }
    }
    public void UnregisterLocaleChangeEvent(Func<string, string, bool> handler)
    {
        mChangeAction -= handler;
    }

    public void RegisterPreWaitLocaleChangeEvent(Func<string, string, Task<bool>> handler, bool sendEventNow = true)
    {
        mPreWaitChangeAction += handler;

        if (sendEventNow == true)
        {
            handler(mPreLocale, mCurLocale);
        }
    }
    public void UnregisterPreWaitLocaleChangeEvent(Func<string, string, Task<bool>> handler)
    {
        mPreWaitChangeAction -= handler;
    }

    public TMP_FontAsset GetMainFontAsset(string locale)
    {
        List<TMP_FontAsset> fonts;
        if (mDicFontAssets.TryGetValue(locale, out fonts) == true)
        {
            return fonts[0];
        }
        else
        {
            return null;
        }
    }

    public string GetExistingLocaleAddress(string address)
    {
        string fileNamePre = address;
        string fileNameFull = fileNamePre + curLocale;
        if (AssetManager.IsAssetExists(fileNameFull) == true)
        {
            return fileNameFull;
        }
        else
        {
            string[] strLocalInfo = curLocale.Split('_');
            if (strLocalInfo.Length > 0)
            {
                fileNameFull = fileNamePre + strLocalInfo[0];
                if (AssetManager.IsAssetExists(fileNameFull) == true)
                {
                    // example : StringResConfig_cn
                    return fileNameFull;
                }
            }
        }

        if (AssetManager.IsAssetExists(address) == true)
        {
            return address;
        }

        return "";
    }

    private void SetLocaleBySystemLanguage()
    {
        if (Application.systemLanguage == SystemLanguage.English)
        {
            mCurLocale = LocaleTypeConst.en_US;
        }
        else if (Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.Chinese)
        {
            mCurLocale = LocaleTypeConst.zh_CN;
        }
        else if (Application.systemLanguage == SystemLanguage.ChineseTraditional)
        {
            mCurLocale = LocaleTypeConst.zh_TW;
        }
        else if (Application.systemLanguage == SystemLanguage.Spanish)
        {
            mCurLocale = LocaleTypeConst.es_ES;
        }
        else if (Application.systemLanguage == SystemLanguage.German)
        {
            mCurLocale = LocaleTypeConst.de_DE;
        }
        else
        {
            mCurLocale = LocaleTypeConst.en_US;
        }

        PlayerPrefs.SetString(PrefarbKey, mCurLocale);
    }
}
