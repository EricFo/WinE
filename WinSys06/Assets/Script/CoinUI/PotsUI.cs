using System;
using System.Collections;
using System.Collections.Generic;
using SlotGame.Config;
using SlotGame.Core;
using UnityEngine;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;

public class PotsUI : MonoBehaviour
{
    //0红 1绿 2蓝
    public PotUI[] freePots;

    private List<PotUI> triggerPots = new List<PotUI>();

    private Vector3[] threePotPos;
    private Vector3[] twoPotPos;


    public void Init()
    {
        foreach (var freePot in freePots)
        {
            freePot.Init();
        }

        BeginAllPotsRandomToLoop2();

        threePotPos = new[] { new Vector3(0, 4.36f, 0), new Vector3(-6.43f, 4.36f, 0), new Vector3(6.19f, 4.36f, 0) };
        twoPotPos = new[] { new Vector3(-3.53f, 4.36f, 0), new Vector3(3.53f, 4.36f, 0) };
    }

    public void Hit(int scatterType, Action<float> onHitComplete, bool isTrigger)
    {
        float animTime = 0;
        if (freePots[scatterType - 1].isUpgrading == false)
        {
            freePots[scatterType - 1].PlayHitAnim();

            if (isTrigger == false)
            {
                freePots[scatterType - 1].Upgrade();
            }
        }


        animTime += freePots[scatterType - 1].GetUpgradeLeftAnimationTime();
        if (animTime == 0&&freePots[scatterType - 1].isUpgrading)
        {
            animTime = 1;
        }

        onHitComplete.Invoke(animTime);
    }

    public float TriggerFreeGameUpgrade(List<int> freeType)
    {
        foreach (var freePot in freePots)
        {
            freePot.isUpgrading = false;
        }

        float animTime = 0;
        for (int i = 0; i < freeType.Count; i++)
        {
            PotUI pot = freePots[freeType[i] - 1];
            DelayCallback.Delay(this, animTime, () => { pot.TriggerFreeGameUpgrade(); });
            animTime += pot.GetTriggerFreeGameUpgradeTime();
            animTime += pot.GetTriggerIntroTime();
            animTime += pot.GetTriggerLoopTime();
            triggerPots.Add(pot);
        }

        DelayCallback.Delay(this, animTime - freePots[^1].GetTriggerLoopTime()- freePots[^1].GetTriggerIntroTime(), () =>
        {
            AudioManager.PlayOneShot("Bell");
            DelayCallback.Delay(this, 2f, () =>
            {
                AudioManager.Stop("Bell");
                AudioManager.PlayOneShot("Bell");
            });
        });
        
        return animTime;
    }

    public void OnEnterFree()
    {
        AudioManager.Stop("Bell");
        foreach (var freePot in freePots)
        {
            if (!triggerPots.Contains(freePot))
            {
                freePot.Fade();
            }
            else
            {
                freePot.ToLoopAnimation();
                freePot.SetBonusGamePosY();
            }
        }

        if (triggerPots.Count == 2)
        {
            for (int i = 0; i < triggerPots.Count; i++)
            {
                triggerPots[i].transform.localPosition = twoPotPos[i];
            }
        }

        if (triggerPots.Count == 1)
        {
            triggerPots[0].transform.localPosition = threePotPos[0];
        }

        StopAllPotsRandomToLoop2();
    }

    public void OnFreeGameTotalWin()
    {
        foreach (var pot in triggerPots)
        {
            pot.OnlyPlayTriggerAnimation();
        }
    }


    public void ExitFreeResetTriggerPotLevel()
    {
        foreach (var pot in triggerPots)
        {
            pot.ResetLevel();
            pot.ToLoopAnimation();
            pot.SetBaseGamePosY();
        }

        for (int i = 0; i < freePots.Length; i++)
        {
            freePots[i].Show();
            freePots[i].transform.localPosition = threePotPos[i];
        }

        triggerPots.Clear();
        BeginAllPotsRandomToLoop2();
    }


    public void BeginAllPotsRandomToLoop2()
    {
        foreach (var freePot in freePots)
        {
            freePot.BeginRandomToLoop2();
        }
    }

    public void StopAllPotsRandomToLoop2()
    {
        foreach (var freePot in freePots)
        {
            freePot.StopRandomToLoop2();
        }
    }

    public void OnFreeSpin()
    {
        foreach (var pot in triggerPots)
        {
            pot.PlayHitAnimation();
        }
    }
}