using SlotGame.Core;
using SlotGame.Result;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;

public class BonusGameController : StateProcessBase
{
    public MeterUI meterUI;
    public SurplusUI surplusUI;
    public BaseReelContainer baseReelContainer;
    private PayoutInfo result = null;
    private PayoutInfo cacheResult = null;
    private int cacheWinWildSymbolCount = 0;
    bool isWinloopAudio = false;

    public override GameState GameState
    {
        get { return GameState.Free; }
    }

    public override bool OnStateEnter()
    {
        var machine = GetStateMachine(GameState);
        cacheResult = null;
        cacheWinWildSymbolCount = 0;
        GlobalObserver.TotalWinWildCount = 0;
        EventManager.Register<EventClass.EventAwardRollUp>(StartRollUp);
        if (surplusUI.CheckRemaining() > 0)
        {
            surplusUI.EditSurplus(-1);
            UIController.BottomPanel.UpdatePressSpinBtnState();
            GlobalObserver.OnAbortDeduce(machine.PayMode);
             GlobalObserver.ClearResult();
             baseReelContainer.potsUI.OnFreeSpin();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void StartRollUp(EventClass.EventAwardRollUp obj)
    {
        if (GlobalObserver.FreeGamePlay.Contains((3)) && GlobalObserver.TotalWinWildCount>0)
        {
            WinMoneyAndDeduce(cacheResult);
        }
    }

    public override void OnStateExit(PayoutInfo result)
    {
        result = OnDeduceState();
        cacheResult = result;
        if (!GlobalObserver.FreeGamePlay.Contains((3)) || cacheResult.WinMoney<=0 || GlobalObserver.TotalWinWildCount<=0)
        {
            WinMoneyAndDeduce(result);
        }
    }

    /// <summary>
    /// 结算
    /// </summary>
    public void WinMoneyAndDeduce(PayoutInfo result)
    {
        if (cacheResult==null)
        {
            return;
        }
        if (cacheResult.WinMoney > 0)
        {
            AudioManager.LoopPlayback("AwardLoop");
            isWinloopAudio = true;
            UIController.BottomPanel.OnAwardComplateEvent += AwardCoroutineComplete;
            UIController.BottomPanel.StopBtn.interactable = true;
            UIController.BottomPanel.AwardRollUp(cacheResult.WinMoney);

        }
        else
        {
            AwardCoroutineComplete();
        }
    }

    /// <summary>
    /// 处理奖励结束时的逻辑
    /// </summary>
    public void AwardCoroutineComplete()
    {
        EventManager.UnRegister<EventClass.EventAwardRollUp>(StartRollUp);
        if (isWinloopAudio)
        {
            AudioManager.Stop("AwardLoop");
            isWinloopAudio = false;
            AudioManager.PlayOneShot("AwardEnd");
        }
        //取消事件注册，防止在不需要的情况下执行
        UIController.BottomPanel.OnAwardComplateEvent -= AwardCoroutineComplete;
        //这里有一个Bonus模式是否结束的判断
        GlobalObserver.IsBonusGameIsOver = CheckProcess.CheckBonusGameIsOver(surplusUI.CheckRemaining());
        if (GlobalObserver.IsBonusGameIsOver)
        {
            UIController.BottomPanel.DisableAllButtons();
            StartCoroutine(BonusGameIsOver());
        }
        else
        {
            /*if (Cheat.isAutoPlay)
            {*/
            if (cacheResult.WinMoney>0)
            {
                UIController.BottomPanel.OnSpinBtnClick();
            }
            else
            {
                DelayCallback.Delay(this, 0.5f, UIController.BottomPanel.OnSpinBtnClick);
            }
            /*}
            else
            {*/
                //UIController.BottomPanel.DisplaySpinBtn();
            /*}*/
        }
    }

    /// <summary>
    /// 处理Bonus游戏结束相关的状态
    /// </summary>
    IEnumerator BonusGameIsOver()
    {
        GlobalObserver.UpdateWin(cacheResult);
        if (cacheResult.WinMoney<=0)
        {
            yield return new WaitForSeconds(0.5f);
        }
        TotalWinAction();
    }

    public void TotalWinAction()
    {
        AudioManager.Stop("WinSys_Game6_Free" + GlobalObserver.FreeGamePlay.Count + "_Music");
        float winAudioTime = 0;
        /*if (GlobalObserver.TotalWin > GlobalObserver.BetValue * 32.5f)
        {
            //AudioManager.PlayOneShot("");
            //winAudioTime = AudioManager.GetAudioTime("");
        }
        else
        {
            //AudioManager.PlayOneShot("");
            //winAudioTime = AudioManager.GetAudioTime("");
        }*/
        AudioManager.PlayOneShot("WinSys_Game6_Free_Totalwin");
        winAudioTime = AudioManager.GetAudioTime("WinSys_Game6_Free_Totalwin");
        UIController.BottomPanel.DisableAllButtons();
        UIController.BottomPanel.SetAward(GlobalObserver.TotalWin);
        TotalWin totalWin=GetPopupBox("TotalWin") as TotalWin;
        totalWin.SetTotalWin(/*result.TotalWin*/GlobalObserver.TotalWin);
        totalWin.OnClickEvent += BonusGameToBaseGame;
        if (/*result.TotalWin*/GlobalObserver.TotalWin > GlobalObserver.BetValue * 50)
        {
            totalWin.Show(winAudioTime,winAudioTime);
        }
        else
        {
            totalWin.Show(3f,winAudioTime);
        }

        baseReelContainer.potsUI.OnFreeGameTotalWin();
    }

    /// <summary>
    /// BonusGame切换到BaseGame
    /// </summary>
    private void BonusGameToBaseGame()
    {
        //清除最后一次的赔付列表
        GlobalObserver.OnAbortDeduce(GetStateMachine(GameState).PayMode);
        
        AudioManager.Stop("WinSys_Game6_Free_Totalwin");
        UIController.BottomPanel.SetAward(GlobalObserver.TotalWin);
        GlobalObserver.UpdateCredit(GlobalObserver.TotalWin);
        //更新游戏状态
        GlobalObserver.UpdateGameState(GameState.Base);
        //控制UI转场
        ViewTransition.TransitionTo(GameState.Free, GameState.Base);
        meterUI.SetMeterLayout();
        baseReelContainer.DisplayTheInvisibleSymbols(false);
        baseReelContainer.potsUI.ExitFreeResetTriggerPotLevel();
        //恢复按钮状态
        UIController.BottomPanel.RestoreButtonDefultState();
        UIController.CheatPanel.UpdateFeatureBtnState(true);

        if (Cheat.isAutoPlay)
        {
            GlobalObserver.SetAutoPlay(true);
            UIController.BottomPanel.OnSpinBtnClick();
        }
    }
    
    /// <summary>
    /// 赔付
    /// </summary>
    protected PayoutInfo OnDeduceState()
    {
        IStateMachine machine = GetStateMachine(GameState.Free);
        PayoutInfo result = new PayoutInfo(GameState.Free);
        if (GlobalObserver.FreeGamePlay.Contains(1))
        {
            PayoutInfo[] results = (machine as BonusGameStateMachine).ReportTheResultMulti();
            foreach (var node in results)
            {
                GlobalObserver.UpdateResult(node);
            }
            GlobalObserver.OnEvaluate(machine.PayMode);

            GlobalObserver.OnDeduce(machine.PayMode);
            foreach (var node in results)
            {
                result.WinMoney += node.WinMoney;
            }
            return result;
        }
        else
        {
            result = machine.ReportTheResult();
            GlobalObserver.UpdateResult(result);
            GlobalObserver.OnEvaluate(machine.PayMode);
            GlobalObserver.OnDeduce(machine.PayMode);
            return result;
        }
    }

}
