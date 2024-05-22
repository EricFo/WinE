using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SlotGame.Core;
using SlotGame.Symbol;
using UnityEngine;
using UniversalModule.DelaySystem;
using UniversalModule.SpawnSystem;

public class JackpotSymbol : SpawnItem
{
    public SpriteRenderer mainTex;
    public SpriteRenderer stopTex;
    public Animator mainAnim;
    public Animator stopAnim;
    
    public JackpotType _jackpotType;
    public virtual void Install(CommonSymbol parent)
    {
        parent._jackpotSymbol = this;
        //
        transform.SetParent(parent.transform);
        
        /*transform.localScale =
            Vector3.one * (GlobalObserver.FreeGamePlay.Contains(1) ? 0.5f : 1);*/
        transform.position = parent.transform.position;
        _jackpotType = parent.JackpotType;
        parent.SetJackpotSymbol(this);
        mainAnim.Play(_jackpotType + "Loop", 0, 0);
        //设置层级,超过100直接大于最大的layer
    }

    public void PlayStopAnim()
    {
        stopAnim.Play(_jackpotType+"Stop");
        mainTex.sortingOrder += 100;
        stopTex.sortingOrder += 100;
    }

    public void SetDisplay(bool isDisplay)
    {
        mainAnim.enabled = true;
        if (mainAnim.isActiveAndEnabled)
        {
            mainAnim.Play(isDisplay ? "Idle" : "Hide");
        }
    }


    public override void Recycle()
    {
        transform.localScale=Vector3.one;
        base.Recycle();
    }
}
