using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗中怪物类
/// </summary>
public class BattleMonster : BattleCreature
{
    // 怪物配置信息
    protected MonsterInfo mMonsterInfo;
    public Action OnEnterDone;

    // 头顶部分
    [HideInInspector]
    public RectTransform BossHeadTop;
    [HideInInspector]
    public Image HeadIcon;
    [HideInInspector]
    public Slider HP;
    [HideInInspector]
    public Text Name;
    [HideInInspector]
    public TextMeshProUGUI Time;
    [HideInInspector]
    public RectTransform HPAndTimePartTrans;
    [HideInInspector]
    public Slider TimeBar;
    [HideInInspector]
    public TextMeshProUGUI HPNum;
    [HideInInspector]
    public Slider HPGreen;

    private float mHeadTopInitPosY;

    #region getter
    public MonsterInfo MonsterInfo => mMonsterInfo;
    #endregion

    // 初始化
    public virtual bool InitMonster(MonsterInfo monsterInfo, string userID)
    {
        if (InitCreature(ThingType.monster, monsterInfo, userID) == false)
        {
            return false;
        }

        mMonsterInfo = monsterInfo;

        var allUserData = ClientManager.Instance.AllUserDatas;
        var killDragonBattleData = ClientManager.Instance.KillDragonBattleData;
        float baseHP = (allUserData.Count + killDragonBattleData.extFillUserNum) * killDragonBattleData.ratio * killDragonBattleData.rand;
        var hpAtt = GetAttributeByType(CreatureAttributeType.hp);
        hpAtt.SetBaseValue(baseHP);
        mCurHP = hpAtt.Value;

        var render = Go.GetComponentInChildren<Renderer>();
        render.sortingOrder = 100;

        BossHeadTop = Trans.Find("Canvas/BossHeadTop").GetComponent<RectTransform>();
        HeadIcon = BossHeadTop.Find("HeadMask/HeadIcon").GetComponent<Image>();
        Name = BossHeadTop.Find("Name").GetComponent<Text>();
        HPAndTimePartTrans = BossHeadTop.Find("HPTimePart").GetComponent<RectTransform>();
        HP = HPAndTimePartTrans.Find("HP").GetComponent<Slider>();
        Time = HPAndTimePartTrans.Find("Time").GetComponent<TextMeshProUGUI>();
        TimeBar = HPAndTimePartTrans.Find("TimeBar").GetComponent<Slider>();
        HPNum = HPAndTimePartTrans.Find("HPNum").GetComponent<TextMeshProUGUI>();
        HPGreen = HPAndTimePartTrans.Find("HPGreen").GetComponent<Slider>();
        HPGreen.gameObject.SetActive(false);
        BossHeadTop.gameObject.SetActive(false);

        mHeadTopInitPosY = BossHeadTop.anchoredPosition.y;

        PrepareHeadTop();

        return true;
    }
    
    public override void SetState(BattleCreatureState newState)
    {
        base.SetState(newState);

        if (State == BattleCreatureState.enter)
        {
            SetBossNameAndIcon();

            mCurAnimationCompleteHandlers = null;
            mCurAnimationEventHandlers = null;
            mSkeletonAnimation.AnimationState.ClearTrack(0);
            mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.enterBattle, false);

            mCurAnimationCompleteHandlers += ((string animationName) => {
                if (animationName == CreatureAnimationName.enterBattle)
                {
                    mSkeletonAnimation.AnimationState.ClearTrack(0);

                    mSkeletonAnimation.AnimationState.SetAnimation(0, CreatureAnimationName.attack, false);

                    mCurAnimationCompleteHandlers = null;
                    mCurAnimationCompleteHandlers += (string ani) => {
                        mCurAnimationCompleteHandlers = null;
                        OnEnterDone?.Invoke();

                        ShowHeadTop();
                    };
                }
            });

            StartMoveHeadTop();
        }
    }

    public void PrepareHeadTop()
    {
        BossHeadTop.anchoredPosition = new Vector2(BossHeadTop.anchoredPosition.x, 0);
        HPAndTimePartTrans.transform.localScale = new Vector3(0, 1, 1);
        BossHeadTop.gameObject.SetActive(true);
        HP.gameObject.SetActive(false);
        Time.gameObject.SetActive(false);
        TimeBar.gameObject.SetActive(false);
    }

    public void SetBossNameAndIcon()
    {
        var curBattle = (KillDragonBattle)Battle;
        Name.text = curBattle.BossName;
        Helpers.SetImageFromURL(curBattle.BossUrl, HeadIcon);
    }

    public void StartMoveHeadTop()
    {
        BossHeadTop.anchoredPosition = new Vector2(BossHeadTop.anchoredPosition.x, 0);
        HPAndTimePartTrans.transform.localScale = new Vector3(0, 1, 1);

        BossHeadTop.DOKill();
        BossHeadTop.DOAnchorPosY(mHeadTopInitPosY, 0.75f);
        HPAndTimePartTrans.DOKill();
        HPAndTimePartTrans.DOScaleX(1.0f, 0.3f).SetDelay(1.05f);
        HP.transform.localScale = new Vector3(0, 1, 1);
        HP.value = 0;
        var killDragonBattle = (KillDragonBattle)Battle;
        Time.text = killDragonBattle.RemainTime.ToString("f1");
        HPNum.text = "";
        TimeBar.value = 0;
    }

    public void ShowHeadTop()
    {
        HP.transform.localScale = new Vector3(0, 1, 1);
        HP.transform.DOKill();
        HP.transform.DOScaleX(1, 0.6f);
        TimeBar.transform.DOKill();
        TimeBar.transform.localScale = new Vector3(0, 1, 1);
        TimeBar.transform.DOScaleX(1, 0.6f);
        HP.gameObject.SetActive(true);
        Time.gameObject.SetActive(true);
        var killDragonBattle = (KillDragonBattle)Battle;
        Time.text = killDragonBattle.RemainTime.ToString("f1");
        TimeBar.gameObject.SetActive(true);
    }

    public override void OnUpdate(float delta, float unscaleDelta)
    {
        base.OnUpdate(delta, unscaleDelta);

        if (Battle != null && Battle.Started == true)
        {
            HP.gameObject.SetActive(true);
            Time.gameObject.SetActive(true);
            var maxHP = GetAttributeValueByType(CreatureAttributeType.hp);
            var curHP = CurHP;
            float per = curHP / maxHP;
            if (per > 1.0f)
            {
                per = 1.0f;
            }

            HP.value = per;

            HPNum.text = ((int)curHP).ToString() + "/" + ((int)maxHP).ToString();

            var killDragonBattle = (KillDragonBattle)Battle;
            Time.text = killDragonBattle.RemainTime.ToString("f1");
            TimeBar.value = killDragonBattle.RemainTime / killDragonBattle.Duration;
        }
    }

    public override void OnStartBattle()
    {
        base.OnStartBattle();

        BossHeadTop.DOAnchorPosY(mHeadTopInitPosY, 0.75f);
        HPAndTimePartTrans.DOScaleX(1.0f, 0.3f).SetDelay(1.05f);
    }

    public override void BeHeal(float heal, BattleCreature healer, object ext = null)
    {
        if (Dead == true || Destroyed == true)
        {
            return;
        }

        float maxHP = GetAttributeValueByType(CreatureAttributeType.hp);
        // 可治疗/已损失血量
        float canHealNum = maxHP - mCurHP;
        // 溢出值
        float healLeft = 0;
        if (canHealNum < heal)
        {
            healLeft = heal - canHealNum;
            heal = canHealNum;
        }

        // 实际加的值
        if (heal > 0)
        {
            mCurHP += heal;
            if (mCurHP > maxHP)
            {
                mCurHP = maxHP;
            }

            HPGreen.gameObject.SetActive(true);
            var curHP = CurHP;
            float per = curHP / maxHP;
            if (per > 1.0f)
            {
                per = 1.0f;
            }
            HPGreen.value = HP.value;
            HP.value = per;
            HP.gameObject.SetActive(false);
            HPGreen.DOKill();
            HPGreen.DOValue(per, 0.2f).onComplete = ()=> {
                HP.gameObject.SetActive(true);
                HPGreen.gameObject.SetActive(false);
            };
        }
    }
}
