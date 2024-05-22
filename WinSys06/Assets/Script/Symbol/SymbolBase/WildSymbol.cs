using System.Collections;
using SlotGame.Config;
using UnityEngine;
using SlotGame.Symbol;
using SlotGame.Core.Reel;
using UniversalModule.AudioSystem;

public class WildSymbol : CommonSymbol
{
    private int multiplier;
    public SpriteRenderer multiplierTex;
    public ArtText multiplierTxt;
    public Sprite[] multiplierSprites;
    public Animator multiplierAnim;
    //记录当前wild初始的层级
    public int MainTexOrder;
    public int Multiplier
    {
        get { return multiplier; }
        set
        {
            multiplier = value;
            //multiplierTxt.SetContent(value.ToString());
            /*for (int i = 0; i < ConfigManager.bonusConfig.WildMultiplier.Length; i++)
            {
                if (value==ConfigManager.bonusConfig.WildMultiplier[i])
                {
                    multiplierTex.sprite = multiplierSprites[i];
                    break;
                }
            }*/

        }
    }

    public void PlayMultiplierIntroAnim()
    {
        if (multiplier>1)
        {
            multiplierTex.enabled = true;
            multiplierAnim.Play(multiplier + "_Intro");
            AudioManager.PlayOneShot("FE_WildMultiplier");
        }
        else
        {
            multiplierTex.enabled = false;
        }
    }
    
    public override void PlayAwardAnim()
    {
        SetAniOrder();
        base.PlayAwardAnim();
        if (multiplierAnim.isActiveAndEnabled&& multiplier>1)
        {
            multiplierAnim.Play(multiplier+"_Loop",0,0);
        }
    }
    public override void PlayIdleAnim()
    {
        RecoverMainTexOrder();
        base.PlayIdleAnim();
        if (multiplierAnim.isActiveAndEnabled&& multiplier>1)
        {
            multiplierAnim.Play("Idle"+multiplier,0,0);
        }
    }
    public override void Install(Transform parent, Vector3 localPos, int reelID, int sortingOrder = 101)
    {
        base.Install(parent, localPos, reelID, sortingOrder + (reelID == 4 ? 2 : 21));
    }

    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        multiplierTex.sortingOrder = order + 1;
        MainTexOrder = MainTex.sortingOrder;
    }

    public override void SetMaskMode(SpriteMaskInteraction interaction)
    {
        base.SetMaskMode(interaction);
        multiplierTex.maskInteraction = interaction;
    }

    public override void Recycle()
    {
        Multiplier = 1;
        multiplierTex.enabled = false;
        base.Recycle();
    }
    //对于播放动画时设置的层级
    public void SetAniOrder()
    {
        //根据reelid，添加层级
        //因为scatter只存在base，所以获取base
        int id;
        if(transform.parent.GetComponent<ReelFree>() == null)
        {
            id = transform.parent.GetComponent<ReelBase>().Setting.reelID;
        }
        else
        {
            id = transform.parent.GetComponent<ReelFree>().Setting.reelID;
        }
        if (id < 3)
        {
            MainTex.sortingOrder = MainTexOrder + 21;
            multiplierTex.sortingOrder = MainTex.sortingOrder + 1;
        }
        else
        {
            MainTex.sortingOrder = MainTexOrder + 2;
            multiplierTex.sortingOrder = MainTex.sortingOrder + 1;
        }
    }
    //部分symbol需要的恢复层级
    public void RecoverMainTexOrder()
    {
        MainTex.sortingOrder = MainTexOrder;
        multiplierTex.sortingOrder = MainTex.sortingOrder + 1;
    }
}
