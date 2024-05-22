using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SlotGame.Config;
using SlotGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;
using Random = UnityEngine.Random;

public enum PotType
{
    free1,
    free2,
    free3
}

public class PotUI : MonoBehaviour, IPointerClickHandler
{
    public PotType potType;
    [SerializeField] private Animator mainAnim;
    [SerializeField] private BoxCollider collider;
    private Vector3[] colliderPos;
    private Vector3[] colliderSize;

    private Vector3[] hitPoses;
    public Vector3 hitPos;

    [SerializeField] private SpriteRenderer mainTex;
    [SerializeField] private SpriteRenderer gameNameTex;
    private int level;

    public int Level
    {
        get { return level; }
        set
        {
            level = value;
            for (int i = 0; i <= topLevel; i++)
            {
                if (i != level)
                {
                    mainAnim.SetLayerWeight(i, 0);
                }
                else
                {
                    mainAnim.SetLayerWeight(i, 1);
                }
            }

            collider.center = colliderPos[level];
            collider.size = colliderSize[level];
            hitPos = hitPoses[level];
        }
    }

    private int topLevel;

    private string loopName;

    private int currentCoinCount;

    private float upgradeTime;
    private float upgradeDeltaTime;
    private float upgradeFailTime;
    private float triggerTime;
    private float triggerLoopTime;
    private float hitTime;

    public bool isUpgrading;

    public bool isUpgradingStage;
    public bool isUpgradingFailStage;

    private float bonusPosY;

    #region Conditions

    private const string To02Loop = "To02Loop";
    private const string Hit = "Hit";
    private const string UpgradeSuccessful = "UpgradeSuccessful";
    private const string UpgradeFail = "UpgradeFail";
    private const string TriggerFree = "TriggerFree";
    private const string Click = "Click";
    private const string ToLoop = "ToLoop";

    #endregion

    private string AudioChangeAName;
    private string AudioChangeBName;

    public void Init()
    {
        topLevel = 4;
        upgradeTime = 2f;
        upgradeDeltaTime = 0.01f;
        triggerTime = 1.2f;
        triggerLoopTime = 1.52f;
        upgradeFailTime = 1.04f;
        hitTime = 1f;
        bonusPosY = -2.47f;
        colliderPos = new[]
        {
            new Vector3(0, -1.7f, 0), new Vector3(0, -1.6f, 0), new Vector3(0, -1.3f, 0), new Vector3(0, -1f, 0),
            new Vector3(0, -0.7f, 0)
        };
        colliderSize = new[]
        {
            new Vector3(4, 3.5f, 0), new Vector3(4.3f, 3.5f, 0), new Vector3(4.7f, 4.1f, 0), new Vector3(5.2f, 4.6f, 0),
            new Vector3(5.5f, 5.1f, 0)
        };
        hitPoses = new[]
        {
            transform.localPosition - Vector3.up * 0.342f,
            transform.localPosition - Vector3.up * 0.15f,
            transform.localPosition + Vector3.up * 0.258f,
            transform.localPosition + Vector3.up * 0.61f,
            transform.localPosition + Vector3.up * 0.985f
        };
        Level = 0;

        switch (potType)
        {
            case PotType.free1:
                AudioChangeAName = "FE_PotChange0{0}A-h";
                AudioChangeBName = "FE_PotChange0{0}B-h";
                break;
            case PotType.free2:
                AudioChangeAName = "FE_PotChange0{0}A-h";
                AudioChangeBName = "FE_PotChange0{0}B-h";
                break;
            case PotType.free3:
                AudioChangeAName = "FE_PotChange0{0}A-h";
                AudioChangeBName = "FE_PotChange0{0}B-h";
                break;
        }
    }

    private void SetLoopID()
    {
        float pro = Random.Range(0f, 1f);
        if (pro < 0.05f)
        {
            mainAnim.SetTrigger(To02Loop);
        }
    }

    public void PlayHitAnim()
    {
        isUpgrading = true;
        AudioManager.Stop("FE_PotHit");
        AudioManager.PlayOneShot("FE_PotHit");
        mainAnim.SetTrigger(Hit);
        currentCoinCount++;
    }

    public void Upgrade()
    {
        float animTime = 0;
        if (Level < topLevel)
        {
            if (IsUpgrade())
            {
                isUpgradingStage = true;
                DelayCallback.Delay(this, hitTime, PotUpgradeSuccessful);
                animTime = upgradeTime + hitTime;
            }
            else if (Random.Range(0, 1f) <= ConfigManager.coinConfig.PlayFlickerProbability[potType.ToString()][level])
            {
                isUpgradingFailStage = true;
                DelayCallback.Delay(this, hitTime, () =>
                {
                    mainAnim.SetTrigger(UpgradeFail);
                    AudioManager.Stop(string.Format(AudioChangeBName, level + 1));
                    AudioManager.PlayOneShot(string.Format(AudioChangeBName, level + 1));
                });
                animTime = upgradeFailTime + hitTime;
            }
        }
        else
        {
            animTime = hitTime;
        }

        DelayCallback.Delay(this, animTime, () =>
        {
            isUpgrading = false;
            isUpgradingStage = false;
            isUpgradingFailStage = false;
        });
    }

    public void PotUpgradeSuccessful()
    {
        AudioManager.Stop(string.Format(AudioChangeAName, level + 1));
        AudioManager.PlayOneShot(string.Format(AudioChangeAName, level + 1));
        mainAnim.SetTrigger(UpgradeSuccessful);
        DelayCallback.Delay(this, upgradeTime, () => { Level++; });

        currentCoinCount = currentCoinCount - ConfigManager.coinConfig.CoinsLevelCount[potType.ToString()][level];
        if (currentCoinCount < 0)
        {
            currentCoinCount = 0;
        }
    }

    private bool IsUpgrade()
    {
        return Random.Range(0, 1f) <=
               ConfigManager.coinConfig.CoinsLevelCountProbability[potType.ToString()][level] ||
               currentCoinCount >=
               ConfigManager.coinConfig.CoinsLevelCount[potType.ToString()][level];
    }

    public float GetTriggerFreeGameUpgradeTime()
    {
        if (level < topLevel)
        {
            int ugradeTimes = topLevel - level;
            return ugradeTimes * (upgradeTime + upgradeDeltaTime);
        }

        return 0;
    }

    public void TriggerFreeGameUpgrade()
    {
        DelayCallback.Delay(this, GetTriggerFreeGameUpgradeTime(), () => PlayTriggerAnimation());
        //如果等级小于最高等级，连续升到最高级
        if (level < topLevel)
        {
            int ugradeTimes = topLevel - level;

            for (int i = 0; i < ugradeTimes; i++)
            {
                DelayCallback.Delay(this, i * (upgradeTime + upgradeDeltaTime), PotUpgradeSuccessful);
            }
        }
    }

    public float GetTriggerIntroTime()
    {
        return triggerTime;
    }

    public float GetTriggerLoopTime()
    {
        return triggerLoopTime;
    }

    public void PlayTriggerAnimation()
    {
        mainAnim.SetBool(ToLoop, false);
        mainAnim.SetTrigger(TriggerFree);
        AudioManager.PlayOneShot("FE_PotTrigger_Intro");
        DelayCallback.Delay(this, triggerTime, () =>
        {
            AudioPlayer audioPlayer = AudioManager.AdditionalLoopPlayback("FE_PotTrigger_Loop");
            DelayCallback.Delay(this, triggerLoopTime,
                () => { audioPlayer.FadeOut(audioPlayer.Source.volume * 0.1f, triggerLoopTime * 2, false); });
        });
    }

    public void OnlyPlayTriggerAnimation()
    {
        mainAnim.SetBool(ToLoop, false);
        mainAnim.SetTrigger(TriggerFree);
    }

    public void ResetLevel()
    {
        Level = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AnimatorStateInfo animatorStateInfo = mainAnim.GetCurrentAnimatorStateInfo(level);
        if (!animatorStateInfo.IsName("Select") &&
            (animatorStateInfo.IsName("Loop01") || animatorStateInfo.IsName("Loop02")))
        {
            mainAnim.SetTrigger(Click);
            AudioManager.Playback("FE_PotTouch");
        }
    }

    public void Show()
    {
        mainTex.color = new Color(mainTex.color.r, mainTex.color.g, mainTex.color.b, 1);
        gameNameTex.color = new Color(gameNameTex.color.r, gameNameTex.color.g, gameNameTex.color.b, 1);
        collider.enabled = true;
    }

    public void Fade()
    {
        mainTex.color = new Color(mainTex.color.r, mainTex.color.g, mainTex.color.b, 0);
        gameNameTex.color = new Color(gameNameTex.color.r, gameNameTex.color.g, gameNameTex.color.b, 0);
        collider.enabled = false;
    }

    public void BeginRandomToLoop2()
    {
        InvokeRepeating("SetLoopID", 0f, 2f);
    }

    public void StopRandomToLoop2()
    {
        CancelInvoke("SetLoopID");
    }

    public void ToLoopAnimation()
    {
        AudioManager.StopAllAdditionalLoop("FE_PotTrigger_Loop");
        mainAnim.SetBool(ToLoop, true);
    }

    public void PlayHitAnimation()
    {
        mainAnim.SetTrigger(Hit);
    }

    public float GetUpgradeLeftAnimationTime()
    {
        AnimatorStateInfo animatorStateInfo = mainAnim.GetCurrentAnimatorStateInfo(level);
        float animTime = 0;
        if (animatorStateInfo.IsName("Hit"))
        {
            float hitLeftTime = hitTime - animatorStateInfo.normalizedTime * hitTime;
            animTime += hitLeftTime;
            if (isUpgradingStage)
            {
                animTime += upgradeTime;
            }
            else if (isUpgradingFailStage)
            {
                animTime += upgradeFailTime;
            }
        }
        else if (animatorStateInfo.IsName("UpgradeSuccessful"))
        {
            float UpgradeSuccessfulLeftTime = upgradeTime - animatorStateInfo.normalizedTime * upgradeTime;
            animTime += UpgradeSuccessfulLeftTime;
        }
        else if (animatorStateInfo.IsName("UpgradeFail"))
        {
            float UpgradeFailLeftTime = upgradeFailTime - animatorStateInfo.normalizedTime * upgradeFailTime;
            animTime += UpgradeFailLeftTime;
        }

        return animTime;
    }

    public void SetBonusGamePosY()
    {
        mainTex.transform.localPosition += Vector3.up * bonusPosY;
    }

    public void SetBaseGamePosY()
    {
        mainTex.transform.localPosition = Vector3.zero;
    }
}