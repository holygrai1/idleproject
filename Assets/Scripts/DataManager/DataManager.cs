using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

#region 文字
public class StringResAllInfo
{
    public Dictionary<string, string> stringReses;
}

#endregion

#region 其他
public class MiscInfo
{
}
#endregion

#region 声音相关
public class SoundInfo
{
    public string id;
    public string[] address;
    public float volume = 1;
    public bool loop = false;
    public bool persist = false;
    public float fadeInSeconds = 0;
    public float fadeOutSeconds = 0;
    public float cd = 0;
}
public class SoundAllInfo
{
    public Dictionary<string, SoundInfo> sounds;
}
#endregion

#region 表现相关
/// <summary>
/// 表现信息
/// </summary>
public class ViewInfo
{
    public static class TypeConst
    {
        // 普通类型,
        public readonly static int Normal = 0;
        public readonly static int Image = 2;
        public readonly static int CircleImage = 3;
    }

    public string id;
    public int type;
    public string desc;
    public Vector3 offset;
    public Vector3 rotation;
    public Vector3 scale;
    public string[] address;
    public bool encrypt;
    public int cacheNum;
    public bool needRectTransform;

    public ViewInfo()
    {
        type = TypeConst.Normal;
        scale = Vector3.one;
    }
}

/// <summary>
/// Image表现信息
/// </summary>
public class ViewImageInfo : ViewInfo
{
    public ViewImageInfo() : base()
    {
        type = TypeConst.Image;
    }
}

public class ViewCircleImageInfo : ViewInfo
{
    public ViewCircleImageInfo() : base()
    {
        type = TypeConst.CircleImage;
    }
}

public class ViewAllInfo
{
    public Dictionary<string, ViewInfo> views;
}

#endregion

#region 图集相关
public class AtlasAllInfo
{
    public Dictionary<string, string> atlass;
}

#endregion

#region 预加载相关
public class PreloadAllInfo
{
    public List<string> preloadBundleName;
    public List<string> preloadBundleLoadAllType;
}
#endregion

#region 战斗相关

/// <summary>
/// 战斗配置信息
/// </summary>
public class BattleInfo
{
    public string battleID;
    public BattleType battleType;
    public string assetAddress;
    public List<int> actorActiveExpByRank;
    public int actorNoactiveExp;
    public int bossActiveExp;
}

public class BattleAllInfo
{
    public Dictionary<string, BattleInfo> battles;
}

/// <summary>
/// 生物配置信息
/// </summary>
public class CreatureInfo
{
    public string id;
    public string assetAddress;
    public string normalAttackSkillID;

    // 评论后使用的技能id
    public string commentSkillID;
    // 点赞后使用的技能id
    public string likeSkillID;
    // 送礼后使用的技能id
    public string prizeSkillID;

    // 高分区缩放
    public float scaleBig;
    // 其他区缩放
    public float scaleSmall;
}

/// <summary>
/// 怪物配置信息
/// </summary>
public class MonsterInfo : CreatureInfo
{
    public MonsterType type;
}

public class MonsterAllInfo
{
    public Dictionary<string, MonsterInfo> monsters;
}

/// <summary>
/// 角色配置信息
/// </summary>
public class ActorInfo : CreatureInfo
{
    public ActorType type;
}
public class ActorAllInfo
{
    public Dictionary<string, ActorInfo> actors;
}

// 技能配置信息
public class SkillInfo
{
    public string id;
    public SkillType type;
    public float cd;
    // 魔法资源地址
    public string magicAssetAddress;
    // 根据等级变化的伤害
    public int damage;
    // 速度
    public float speed;

    public class NormalAttackInfo
    {
        // 普攻动作的播放时长
        public float animationTime;
        // 攻击帧延迟时间
        public float attackFrameDeldy;
    }
    public class FastLightBallInfo
    {
        // 总发出时间
        public float duration;
        // 技能动作的播放时长
        public float animationTime;
        // 攻击帧延迟时间
        public float attackFrameDeldy;
    }
    public class HeartBallInfo
    {
        // 总发出时间
        public float duration;
        // 技能动作的播放时长
        public float animationTime;
        // 攻击帧延迟时间
        public float attackFrameDeldy;
        // 爆炸魔法资源名
        public string explodeMagicAssetAddress;
    }
    public class HealInfo
    {
        // 加血比, 万分比
        public List<int> healPers; 
    }
    public class PrizeBallInfo
    {
        // 每一发礼物的总发出时间
        public float duration;
        // 技能动作的播放时长
        public float animationTime;
        // 攻击帧延迟时间
        public float attackFrameDeldy;
        // 开方次数
        public float pow;
        // 最大血量比例
        public int hpPer;
        // 下一个礼物间隔时间
        public float nextCD;
        // 身上火持续时间
        public float bodyMagicFireDuration;
        // 身上火魔法资源名
        public string bodyMagicFireAssetAddress;
        // 爆炸魔法资源名
        public string explodeMagicAssetAddress;
    }
    public class PrizeHealInfo
    {
        // 每一发礼物的总发出时间
        public float duration;
        // 技能动作的播放时长
        public float animationTime;
        // 攻击帧延迟时间
        public float attackFrameDeldy;
        // 开方次数
        public float pow;
        // 最大血量比例
        public int hpPer;
        // 下一个礼物间隔时间
        public float nextCD;
    }

    public NormalAttackInfo normalAttack;
    public FastLightBallInfo fastLightBall;
    public HeartBallInfo heartBall;
    public HealInfo heal;
    public PrizeBallInfo prizeBall;
    public PrizeHealInfo prizeHeal;
}
public class SkillAllInfo
{
    public Dictionary<string, SkillInfo> skills;
}

// 角色等级信息
public class ActorLevelInfo
{
    public float exp;
    public string actorID;
    public string headIcon;
}
public class ActorLevelAllInfo
{
    public List<ActorLevelInfo> actorLevels;
}

#endregion

/// <summary>
/// 数据管理器
/// </summary>

public class DataManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static DataManager sInstance;
    public static DataManager Instance => sInstance;
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

    StringResAllInfo mStringResAllInfo;
    MiscInfo mMiscInfo;
    SoundAllInfo mSoundAllInfo;
    ViewAllInfo mViewAllInfo;
    PreloadAllInfo mPreloadAllInfo;
    BattleAllInfo mBattleAllInfo;
    MonsterAllInfo mMonsterAllInfo;
    SkillAllInfo mSkillAllInfo;
    ActorAllInfo mActorAllInfo;
    ActorLevelAllInfo mActorLevelAllInfo;

    #region getter

    public string GetStringRes(string id)
    {
        string value;
        if (mStringResAllInfo.stringReses.TryGetValue(id, out value) == false)
        {
            return id;
        }
        else
        {
            return value;
        }
    }

    public MiscInfo GetMiscInfo()
    {
        return mMiscInfo;
    }

    public PreloadAllInfo GetPreloadInfo()
    {
        return mPreloadAllInfo;
    }

    public SoundInfo GetSoundInfo(string id)
    {
        SoundInfo info = null;
        mSoundAllInfo.sounds.TryGetValue(id, out info);
        return info;
    }

    public ViewInfo GetViewInfo(string id)
    {
        ViewInfo info = null;
        mViewAllInfo.views.TryGetValue(id, out info);
        return info;
    }

    public BattleInfo GetBattleInfo(string id)
    {
        BattleInfo info = null;
        mBattleAllInfo.battles.TryGetValue(id, out info);
        return info;
    }

    public MonsterInfo GetMonsterInfo(string id)
    {
        MonsterInfo info = null;
        mMonsterAllInfo.monsters.TryGetValue(id, out info);
        return info;
    }

    public ActorInfo GetActorInfo(string id)
    {
        ActorInfo info = null;
        mActorAllInfo.actors.TryGetValue(id, out info);
        return info;
    }

    public SkillInfo GetSkillInfo(string id)
    {
        SkillInfo skill = null;
        mSkillAllInfo.skills.TryGetValue(id, out skill);
        return skill;
    }

    public ActorLevelAllInfo ActorLevelAllInfo => mActorLevelAllInfo;

    public int GetActorLevelByExp(float exp)
    {
        for (int i = 0; i < mActorLevelAllInfo.actorLevels.Count; ++i)
        {
            var levelInfo = mActorLevelAllInfo.actorLevels[i];
            if (exp < levelInfo.exp)
            {
                return i - 1;
            }
        }
        return mActorLevelAllInfo.actorLevels.Count - 1;
    }

    public ActorLevelInfo GetActorLevelInfo(int level)
    {
        return Helpers.GetWithinArray(mActorLevelAllInfo.actorLevels, level);
    }

    #endregion

    public async Task<bool> Init()
    {
        if (false == await LoadStringResConfig())
        {
            return false;
        }

        List<Task<bool>> listResult = new List<Task<bool>>();
        listResult.Add(LoadBattleConfig());
        listResult.Add(LoadMonsterConfig());
        listResult.Add(LoadSkillConfig());
        listResult.Add(LoadActorConfig());
        listResult.Add(LoadActorLevelConfig());

        await Task.WhenAll(listResult);

        //    LocaleManager.Instance.RegisterPreWaitLocaleChangeEvent(OnLocaleChangeEventHandle, false);

        return true;
    }

    #region Localization Update
    private async Task<bool> OnLocaleChangeEventHandle(string preLocale, string curLocale)
    {
        LogManager.Log("data OnLocaleChangeEventHandle");

        mStringResAllInfo?.stringReses?.Clear();
        var task = LoadStringResConfig();
        await task;
        return true;
    }
    #endregion

    #region loader
    async Task<bool> LoadStringResConfig()
    {
        string local = LocaleManager.Instance.curLocale;
        string fileNamePre = "StringResConfig_";
        string fileNameFull = fileNamePre + local;
        bool fileFound = false;
        if (AssetManager.IsAssetExists(fileNameFull) == true)
        {
            // example : StringResConfig_cn_ZH
            fileFound = true;
        }
        else
        {
            string[] strLocalInfo = local.Split('_');
            if (strLocalInfo.Length > 0)
            {
                fileNameFull = fileNamePre + strLocalInfo[0];
                if (AssetManager.IsAssetExists(fileNameFull) == true)
                {
                    // example : StringResConfig_cn
                    fileFound = true;
                }
            }
        }

        if (fileFound == false)
        {
            Debug.LogError("缺少对应" + local + "的StringResConfig文件");
            return false;
        }
        else
        {
            using (var op = AssetManager.LoadAssetAsync<TextAsset>(fileNameFull))
            {
                await op;
                mStringResAllInfo = JsonConvert.DeserializeObject<StringResAllInfo>(op.Result.text);
                return true;
            }
        }
    }

    async Task<bool> LoadBattleConfig()
    {
        using (var op = AssetManager.LoadAssetAsync<TextAsset>("BattleConfig"))
        {
            await op;
            mBattleAllInfo = JsonConvert.DeserializeObject<BattleAllInfo>(op.Result.text);
            return true;
        }
    }

    async Task<bool> LoadMonsterConfig()
    {
        using (var op = AssetManager.LoadAssetAsync<TextAsset>("MonsterConfig"))
        {
            await op;
            mMonsterAllInfo = JsonConvert.DeserializeObject<MonsterAllInfo>(op.Result.text);

            return true;
        }
    }

    async Task<bool> LoadSkillConfig()
    {
        using (var op = AssetManager.LoadAssetAsync<TextAsset>("SkillConfig"))
        {
            await op;
            mSkillAllInfo = JsonConvert.DeserializeObject<SkillAllInfo>(op.Result.text);

            return true;
        }
    }

    async Task<bool> LoadActorConfig()
    {
        using (var op = AssetManager.LoadAssetAsync<TextAsset>("ActorConfig"))
        {
            await op;
            mActorAllInfo = JsonConvert.DeserializeObject<ActorAllInfo>(op.Result.text);

            return true;
        }
    }

    async Task<bool> LoadActorLevelConfig()
    {
        using (var op = AssetManager.LoadAssetAsync<TextAsset>("ActorLevelConfig"))
        {
            await op;
            mActorLevelAllInfo = JsonConvert.DeserializeObject<ActorLevelAllInfo>(op.Result.text);

            return true;
        }
    }

    #endregion
}
