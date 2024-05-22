using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SlotGame.Config;
using UnityEngine;
using SlotGame.Core;
using SlotGame.Core.Reel;
using SlotGame.Result;
using SlotGame.Symbol;
using UnityEditor;
using UniversalModule.AudioSystem;
using UniversalModule.SpawnSystem;

public class BonusGameStateMachine : StateMachine
{
    public BonusReelContainer[] container;
    public SurplusUI surplusUI;
    public SpriteRenderer freeBackground;
    public Sprite[] BackgroundSprites;
    public SpriteRenderer freeReel;
    public Sprite[] ReelSprites;
    public SpriteRenderer freeFrame;
    public Sprite[] frameSprites;

    public GameObject[] messages;
    private int reelContainerStopCount;

    private List<JackpotType> totalJackpotPool;
    public JackPot jackPot;

    private bool haveJackpotSymbol = false;

    public PayoutInfo[] Payouts;
    
    public override void Initialize()
    {
        base.Initialize();
        totalJackpotPool = new List<JackpotType>();
        Payouts = new PayoutInfo[container.Length];
        for (int i = 0; i < container.Length; i++)
        {
            Payouts[i] = new PayoutInfo(State);
        }
        foreach (var item in container)
        {
            item.Initialize();
            item.OnStopAllListener += OnStopAllComplete;
            item.OnSpecialSymbolListener += surplusUI.EditSurplus;
            item.OnGetCurrentSpinCount += surplusUI.GetCurrentSpinCount;
            item.OnJackpotSymbolListener+= delegate { haveJackpotSymbol = true; };
        }
        GlobalObserver.UpdateResult(Payout);
    }
    public override void OnTrigger()
    {
        Payout.Clear();
        Payout.ResetTotalWin();
        jackPot.InitFree();
        InitParameter();
        ToFreeLayout();
        string stripe = "Free" + GlobalObserver.FreeGamePlay.ListIntToString();
        int rand = Random.Range(0, container.Length);
        for (int i = 0; i < container.Length; i++)
        {
            string reelStripe = stripe;
            if (GlobalObserver.FreeGamePlay.Contains(1))
            {
                reelStripe = stripe+ (i == rand ? "_1" : "_2");
            }
            container[i].InitReelStripe(reelStripe);
            container[i].ToBonusInit();
        }
        surplusUI.Init();
        
        UIController.BottomPanel.OnSpinBtnClick();
    }
    
    /// <summary>
    /// 初始化参数
    /// </summary>
    private void InitParameter()
    {
        
        totalJackpotPool.Clear();

        for (int i = 0; i < 4; i++)
        {
            int jackpotCount = 0;
            if (Random.Range(0, 1f) < ConfigManager.bonusConfig.JackpotProb[i]*GlobalObserver.GetMultiplyer())
            {
                jackpotCount = 3;
            }
            else
            {
                jackpotCount = GlobalObserver.GetRandomWeightedIndex(ConfigManager.bonusConfig.JackpotBallCountProb[i]);
            }
            if (i == 0 && Cheat.isGrand)
            {
                jackpotCount = 3;
                Cheat.isGrand = false;
            }

            if (i == 1 && Cheat.isMajor)
            {
                jackpotCount = 3;
                Cheat.isMajor = false;
            }

            if (i == 2 && Cheat.isMinor)
            {
                jackpotCount = 3;
                Cheat.isMinor = false;
            }

            if (i == 3 && Cheat.isMini)
            {
                jackpotCount = 3;
                Cheat.isMini = false;
            }

            for (int j = 0; j < jackpotCount; j++)
            {
                totalJackpotPool.Add((JackpotType)i);
            }
        }

        totalJackpotPool.ListSortRandom();
        if (GlobalObserver.FreeGamePlay.Contains(1))
        {
            List<List<JackpotType>> splitPools = totalJackpotPool.SplitList(container.Length);
            for (int i = 0; i < container.Length; i++)
            {
                container[i].jackpotPool = splitPools[i];
            }
        }
        else
        {
            container[0].jackpotPool = totalJackpotPool;
        }

        foreach (var VARIABLE in totalJackpotPool)
        {
            Debug.Log(VARIABLE);
        }
        Debug.Log("-----------------------");
        GlobalObserver.NewResult(GlobalObserver.FreeGamePlay.ListIntToString());
    }

    void ToFreeLayout()
    {
        for (int i = 0; i < messages.Length; i++)
        {
            if (i == GlobalObserver.FreeGamePlay.ListIntSum() - (GlobalObserver.FreeGamePlay.Count > 1 ? 0 : 1))
            {
                messages[i].SetActive(true);
            }
            else
            {
                messages[i].SetActive(false);
            }
        }
        freeBackground.sprite = BackgroundSprites[GlobalObserver.FreeGamePlay.Count - 1];
        freeReel.sprite =
            ReelSprites[GlobalObserver.FreeGamePlay.Count - 1 + (GlobalObserver.FreeGamePlay.Contains(1) ? 3 : 0)];
        freeFrame.sprite = frameSprites[GlobalObserver.FreeGamePlay.Contains(1) ? 1 : 0];
        if (GlobalObserver.FreeGamePlay.Contains(1))
        {
            foreach (var item in container)
            {
                item.gameObject.SetActive(true);
            }

            container[0].transform.localPosition = new Vector3(-8.29f, 0);
            container[0].transform.localScale = Vector3.one * 0.5f;
        }
        else
        {
            for (int i = 0; i < container.Length; i++)
            {
                container[i].gameObject.SetActive(i < 1);
            }
            container[0].transform.localPosition = new Vector3(-7.51f, 0f);
            container[0].transform.localScale = Vector3.one;
        }
    }
    
    public override void Enter()
    {
        OnReset();
        if (GlobalObserver.FreeGamePlay.Contains(1))
        {
            foreach (var item in container)
            {
                item.Spin();
            }
        }
        else
        {
            container[0].Spin();
        }
    }

    public override void OnReset()
    {
        GlobalObserver.isFreeShutDown = false;
        haveJackpotSymbol = false;
        if (GlobalObserver.FreeGamePlay.ListIntToString()=="3")
        {
            GlobalObserver.FreeVarSymbol = ConfigManager.bonusConfig.VarySymbolName[
                GlobalObserver.GetRandomWeightedIndex(ConfigManager.bonusConfig.VarySymbolNameProb["3"])];
        }
        base.OnReset();
        reelContainerStopCount = 0;
        foreach (var VARIABLE in Payouts)
        {
            VARIABLE.Clear();
        }
    }

    public override PayoutInfo ReportTheResult()
    {
        StatisticalResult.UpdateResult(PayMode, Payout, container[0].Reels);
        return Payout;
    }

    public PayoutInfo[] ReportTheResultMulti()
    {
        for (int i = 0; i < (GlobalObserver.FreeGamePlay.Contains(1)?this.container.Length:1); i++)
        {
            StatisticalResult.UpdateResult(PayMode,Payouts[i],container[i].Reels);
        }
        return Payouts;
    }

    public override bool ShutDown()
    {
        bool doneShutDown = false;
        foreach (var VARIABLE in container)
        {
            doneShutDown |= VARIABLE.ShutDown();
        }

        GlobalObserver.isFreeShutDown = doneShutDown;
        return doneShutDown;
    }

    /// <summary>
    /// 所有转轮都停止时调用
    /// </summary>
    /// <param name="container"></param>
    private void OnStopAllComplete(ReelContainer container)
    {
        reelContainerStopCount++;
        if (reelContainerStopCount==(GlobalObserver.FreeGamePlay.Contains(1)?this.container.Length:1))
        {
            StartCoroutine(_OnStopAllComplete());
        }
    }

    IEnumerator _OnStopAllComplete()
    {
        if (haveJackpotSymbol)
        {
            yield return new WaitForSeconds(20 / 25f);
        }
        for (int i = 0; i < (GlobalObserver.FreeGamePlay.Contains(1)?this.container.Length:1); i++)
        {
            foreach (var reel in container[i].Reels)
            {
                foreach (var symbol in reel.GetVisibleSymbols())
                {
                    if (symbol.JackpotType != JackpotType.Null)
                    {
                        Vector3 startPos = symbol._jackpotSymbol.transform.position;
                        symbol._jackpotSymbol.Recycle();
                        symbol._jackpotSymbol = null;
                        //symbol.FadeDisplay();
                        FlySymbol flySymbol =SpawnFactory.GetObject<FlySymbol>("Fly");
                        flySymbol.PlayJackpotBallFlyAnim(symbol.JackpotType,startPos,jackPot.GetFlyTargetPos(symbol.JackpotType));
                        yield return new WaitForSeconds(12 / 25f);
                        if (jackPot.ballCount[symbol.JackpotType]>1)
                        {
                            AudioManager.Playback("FE_JPBallStop-Fly-Hit02");
                        }
                        else
                        {
                            AudioManager.Playback("FE_JPBallStop-Fly-Hit01");
                        }
                        yield return new WaitForSeconds(8 / 25f);
                        jackPot.AddBallCount(symbol.JackpotType);
                        if (jackPot.CheckJackpotTrigger())
                        {
                            yield return jackPot.PlayJackpotCele();
                        }
                    }
                }
            }
        }
        if (haveJackpotSymbol)
        {
            yield return new WaitForSeconds(10 / 25f);
        }
        
        bool isSpecialSymbol = false;
        for (int i = 0; i < (GlobalObserver.FreeGamePlay.Contains(1)?this.container.Length:1); i++)
        {
            foreach (var reel in container[i].Reels)
            {
                foreach (var symbol in reel.GetVisibleSymbols())
                {
                    if (symbol.ItemName==SymbolNames.SPECIAL)
                    {
                        isSpecialSymbol = true;
                        (symbol as SpecialSymbol).PlayWinAnim();
                        StartCoroutine(AddSpinFly(symbol.transform.position));
                    }
                }
            }
        }
        if (isSpecialSymbol)
        {
            AudioManager.Playback("FE_SpinsADD");
            yield return new WaitForSeconds(65 / 25f);
        }
        
        bool haveVarSymbol = false;
        List<VarSymbol> varSymbols = new List<VarSymbol>();
        for (int i = 0; i < (GlobalObserver.FreeGamePlay.Contains(1)?this.container.Length:1); i++)
        {
            foreach (var reel in container[i].Reels)
            {
                foreach (var symbol in reel.GetVisibleSymbols())
                {
                    if (symbol.ItemName==SymbolNames.VAR)
                    {
                        haveVarSymbol = true;
                        varSymbols.Add((VarSymbol)symbol);
                    }
                }
            }
        }

        if (haveVarSymbol)
        {
            string varySymbolName =
                ConfigManager.bonusConfig.VarySymbolName[
                    GlobalObserver.GetRandomWeightedIndex(
                        ConfigManager.bonusConfig.VarySymbolNameProb[GlobalObserver.FreeGamePlay.ListIntToString()])];
            AudioManager.Playback("FE_DoorOpen");
            foreach (var varSymbol in varSymbols)
            {
                varSymbol.PlayDoorOpenAnim(varySymbolName);
            }
            yield return new WaitForSeconds(2f);
            for (int i = 0; i < (GlobalObserver.FreeGamePlay.Contains(1)?this.container.Length:1); i++)
            {
                foreach (var reel in container[i].Reels)
                {
                    foreach (var symbol in reel.GetVisibleSymbols())
                    {
                        if (symbol.ItemName==SymbolNames.VAR)
                        {
                            CommonSymbol newSymbol=reel.ReplaceSymbol(varySymbolName, symbol);
                            newSymbol.PlayIdleAnim();
                        }
                    }
                }
            }
            
        }
        
        
        
        
        
        
        OnComplate();
        yield return null;
    }
    
    IEnumerator AddSpinFly(Vector3 startPos)
    {
        yield return new WaitForSeconds(43 / 25f);
        FlySymbol flySymbol = SpawnFactory.GetObject<FlySymbol>("Fly");
        flySymbol.PlayAddSpinFlyAnim(startPos, new Vector3(8.215f, -10.37f));
        yield return new WaitForSeconds(20 / 25f);
        surplusUI.PlayAddSpinHit();
        surplusUI.EditSurplus(1);
    }
    
    

}
