using System.Collections;
using System.Collections.Generic;
using SlotGame.Core;
using UnityEngine;

public class FeatureIntro : FeaturePicked
{
    public Animator introAnim;
    public SpriteRenderer intro;
    public Sprite[] introSprites;

    public void SetIntroSprite()
    {
        introAnim.Play(GlobalObserver.FreeGamePlay.ListIntToString()+"_Intro");
        /*intro.sprite =
            introSprites[GlobalObserver.FreeGamePlay.ListIntSum() - (GlobalObserver.FreeGamePlay.Count > 1 ? 0 : 1)];*/
    }

    public override void Show(float freeTime)
    {
        base.Show(freeTime);
        SetIntroSprite();
    }

    public override void Show(float freeTime, float duration)
    {
        base.Show(freeTime, duration);
        SetIntroSprite();
    }

    public override void Show(float duration, bool isAutoHide = false)
    {
        base.Show(duration, isAutoHide);
        SetIntroSprite();
    }
}
