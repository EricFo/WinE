using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SlotGame.Core;
using SlotGame.Symbol;
using UnityEngine;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;

public class FlySymbol : CommonSymbol
{
    #region 动画Hash
    protected int REDFLY = Animator.StringToHash("RedFly");
    protected int GREENFLY = Animator.StringToHash("GreenFly");
    protected int BLUEFLY = Animator.StringToHash("BlueFly");
    protected int ADDSPINFLY = Animator.StringToHash("AddSpinFly");
    #endregion

    public float PlayGemFlyAnim(int scatterType,Vector3 startPos,Vector3 targetPos)
    {
        transform.position = startPos;
        startPos.z = 0;
        targetPos.z = 0;
        Vector3 dir = (targetPos - startPos).normalized;
        transform.up = dir;
        switch (scatterType)
        {
            case 1:
                PlayAnimation(REDFLY);
                break;
            case 2:
                PlayAnimation(GREENFLY);
                break;
            case 3:
                PlayAnimation(BLUEFLY);
                break;
        }

        transform.DOMove(targetPos, 20 / 25f);
        DelayCallback.Delay(this, 21 / 25f, Recycle);
        return 21 / 25f;
    }

    public void PlayJackpotBallFlyAnim(JackpotType jackpotType,Vector3 startPos,Vector3 targetPos)
    {
        if (GlobalObserver.FreeGamePlay.Contains(1))
        {
            transform.localScale = Vector3.one * 0.5f;
        }
        AudioManager.Playback("FE_JPBallFly");
        MainTex.sortingOrder = 9000;
        transform.position = startPos;
        MainAnim.Play(jackpotType+"Fly");
        transform.DOMove(targetPos, 12 / 25f);
        DelayCallback.Delay(this, 21 / 25f, Recycle);
    }

    public void PlayAddSpinFlyAnim(Vector3 startPos,Vector3 targetPos)
    {
        if (GlobalObserver.FreeGamePlay.Contains(1))
        {
            transform.localScale = Vector3.one * 0.8f;
        }
        MainTex.sortingOrder = 12000;
        transform.position = startPos;
        startPos.z = 0;
        targetPos.z = 0;
        Vector3 dir = (targetPos - startPos).normalized;
        transform.up = dir;
        PlayAnimation(ADDSPINFLY);
        transform.DOMove(targetPos, 20 / 25f);
        DelayCallback.Delay(this, 21 / 25f, Recycle);
    }


    public override void Recycle()
    {
        transform.localScale = Vector3.one;
        MainTex.sortingOrder = 1500;
        base.Recycle();
    }
}
