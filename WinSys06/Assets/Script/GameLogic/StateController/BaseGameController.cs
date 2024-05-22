using System.Collections;
using System.Collections.Generic;
using SlotGame.Core;
using SlotGame.Result;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;

public class BaseGameController : StateProcessBase
{
    public MeterUI meterUI;
    public BaseReelContainer baseReelContainer;
    public Animator transitionAnim;
    
    PayoutInfo result = null;
    bool isWinloopAudio = false;

    public override GameState GameState {
        get {
            return GameState.Base;
        }
    }

    public override bool OnStateEnter() {
        var machine = GetStateMachine(GameState);
        if (GlobalObserver.CurrCredit >= GlobalObserver.BetValue) {
            GlobalObserver.Consume();
            GlobalObserver.UpdateMeters();
            GlobalObserver.ResetTotalWin();
            GlobalObserver.OnAbortDeduce(machine.PayMode);
            GlobalObserver.ClearResult();
            UIController.BottomPanel.ResetWinPanel();
            UIController.BottomPanel.UpdatePressSpinBtnState();
            UIController.MeterPanel.UpdateMeters(GlobalObserver.Meters);
            UIController.BottomPanel.UpdateCredit(GlobalObserver.CurrCredit);
            UIController.CheatPanel.UpdateFeatureBtnState(false);
            UIController.CheatPanel.HideCheatBtnList();
            GlobalObserver.SetSpinStarted();
            if (result!=null)
            {
                result.Clear();
            }
            return true;
        } else {
            UIController.BottomPanel.DisableAllButtons();
            return false;
        }
    }

    public override void OnStateExit(PayoutInfo result) {
        GlobalObserver.IsTriggerJackpot = CheckProcess.CheckJackPotIsTrigger(result);
        GlobalObserver.IsTriggerFreeGame = CheckProcess.CheckFreeGameIsTrigger(result);

        StartCoroutine(PayGemCredit());
    }

    IEnumerator PayGemCredit()
    {
        IStateMachine machine = GetStateMachine(GameState.Base);
        result  = machine.ReportTheResult();
        if (GlobalObserver.IsTriggerBonusGame)
        {
            yield return new WaitForSeconds(1f);
            foreach (var reel in baseReelContainer.Reels)
            {
                foreach (var symbol in reel.GetVisibleSymbols())
                {
                    if (symbol.ItemName==SymbolNames.SCATTER)
                    {
                        ScatterSymbol sSymbol = symbol as ScatterSymbol;
                        if (GlobalObserver.FreeGamePlay.Contains(sSymbol.ScatterType))
                        {
                            sSymbol.PlayWinAnim();
                            result.WinMoney += sSymbol.gemCredit;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(1f);
            UIController.BottomPanel.OnAwardComplateEvent += WinMoneyAndDeduce;
            GlobalObserver.UpdateWin(result);
            AudioManager.LoopPlayback("AwardLoop");
            isWinloopAudio = true;
            UIController.BottomPanel.AwardRollUp(result.WinMoney);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            WinMoneyAndDeduce();
        }
        
    }

    /// <summary>
    /// 结算
    /// </summary>
    public void WinMoneyAndDeduce()
    {
        if (isWinloopAudio)
        {
            AudioManager.Stop("AwardLoop");
            isWinloopAudio = false;
            AudioManager.PlayOneShot("AwardEnd");
        }
        UIController.BottomPanel.OnAwardComplateEvent -= WinMoneyAndDeduce;
        result.WinMoney = 0;
        StartCoroutine(_WinMoneyAndDeduce());
    }

    IEnumerator _WinMoneyAndDeduce()
    {
        yield return null;
        result = OnDeduceState();
        if (result.WinMoney > 0)
        {
            AudioManager.LoopPlayback("AwardLoop");
            isWinloopAudio = true;
            UIController.BottomPanel.OnAwardComplateEvent += OnBaseGameAwardComplate;
            UIController.BottomPanel.StopBtn.interactable = true;
            UIController.BottomPanel.AwardRollUp(result.WinMoney);

        }
        else
        {
            OnBaseGameAwardComplate();
        }
    }

    /// <summary>
    /// 处理奖励结束时的逻辑
    /// </summary>
    private void OnBaseGameAwardComplate() {
        if (isWinloopAudio)
        {
            AudioManager.Stop("AwardLoop");
            isWinloopAudio = false;
            AudioManager.PlayOneShot("AwardEnd");
        }
        //取消事件注册，防止在不需要的情况下执行
        UIController.BottomPanel.OnAwardComplateEvent -= OnBaseGameAwardComplate;
        List<PayoutInfo> result = GlobalObserver.GetResult(GameState.Base);
        if (!GlobalObserver.IsTriggerJackpot) {
            //没触发JackPot要检查是否触发Free，触发JackPot的情况下先进JackPot
            if (GlobalObserver.IsTriggerBonusGame) {
                //处理触发Free的情况
                StartCoroutine(BeforeBaseGameToFreeGame());
            } else { //没触发Free或JP就恢复按钮状态
                GlobalObserver.UpdateCredit(result[0].TotalWin);
                UIController.BottomPanel.RestoreButtonDefultState();
                UIController.CheatPanel.UpdateFeatureBtnState(true);
                //UIController.BottomPanel.UpdateCredit(GlobalObserver.CurrCredit);
                GlobalObserver.SetFinalizeState();
                //如果启用AutoSpin就直接继续
                if (GlobalObserver.AutoSpinEnabled == true) {
                    UIController.BottomPanel.OnSpinBtnClick();
                }
            }
        } else {
            //触发任何特殊玩法都要禁用掉Auto Play
            GlobalObserver.SetAutoPlay(false);
        }
    }

    IEnumerator BeforeBaseGameToFreeGame()
    {
        //To Do 此处应该处理FreeGame弹框逻辑，弹框结束后应该是转场
        var popup = GetPopupBox("BonusGame");
        //此处事件已经做了防止重复注册的处理，不需要再手动注销
        popup.OnClickEvent += BaseGameToFreeGame;
        popup.Show( /*result[0], */1f,Cheat.isAutoPlay);
        AudioManager.Playback("WinSys_Game6_Free_Intro");

        //触发任何特殊玩法都要禁用掉Auto Play
        GlobalObserver.SetAutoPlay(false);
        yield return new WaitForSeconds(1f);
        if (GlobalObserver.CurrGameState == GameState.Base)
        {
            AudioManager.LoopPlayback("WinSys_Game6_Free_Waiting");
        }
    }

    private void BaseGameToFreeGame()
    {
        StartCoroutine(_BaseGameToFreeGame());
    }

    IEnumerator _BaseGameToFreeGame()
    {
        
        baseReelContainer.potsUI.OnEnterFree();
        AudioManager.Playback("FE_Cutscenes");
        transitionAnim.Play("Transition"+GlobalObserver.FreeGamePlay.ListIntToString());
        yield return new WaitForSeconds(10 / 25f);
        AudioManager.Stop("WinSys_Game6_Free_Waiting");
        AudioManager.LoopPlayback("WinSys_Game6_Free" + GlobalObserver.FreeGamePlay.Count + "_Music");
        //每次转场之前最好先停止当前模式下的赔付流程!!!
        var machine = GetStateMachine(GameState);
        GlobalObserver.OnAbortDeduce(machine.PayMode);
        Cheat.isTriggerBonusGame = false;
        GlobalObserver.IsTriggerBonusGame = false;
        GlobalObserver.ToBonusState();
        //更新游戏状态到Free
        GlobalObserver.UpdateGameState(GameState.Free);
        ViewTransition.TransitionTo(GameState.Base, GameState.Free);
        meterUI.SetMeterLayout();
        UIController.BottomPanel.DisplaySpinBtn();
        GetStateMachine(GameState.Free).OnTrigger();
    }

    /// <summary>
    /// 赔付
    /// </summary>
    protected PayoutInfo OnDeduceState()
    {
        IStateMachine machine = GetStateMachine(GameState.Base);
        /*PayoutInfo result = machine.ReportTheResult();*/
        GlobalObserver.UpdateResult(result);
        GlobalObserver.OnEvaluate(machine.PayMode);
        GlobalObserver.OnDeduce(machine.PayMode);
        return result;
    }

}
