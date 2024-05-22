using SlotGame.Config;
using SlotGame.Core;
using SlotGame.Core.Reel;
using SlotGame.Reel.Args;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;
using UniversalModule.SpawnSystem;

public class BaseReelContainer : ReelContainer
{
    public PotsUI potsUI;

    public override event Action<IReelState> OnSpinReelListener;
    public override event Action<IReelState> OnStopReelListener;
    public override event Action<ReelContainer> OnSpinAllListener;
    public override event Action<ReelContainer> OnStopAllListener;

    public Dictionary<int, ReelSpinArgs> reelsPredict = new Dictionary<int, ReelSpinArgs>();

    private Dictionary<int, List<int>> IndexOfFreeGamePlay = new Dictionary<int, List<int>>()
    {
        { 1, new List<int>() { 1 } },
        { 2, new List<int>() { 2 } },
        { 3, new List<int>() { 3 } },
        { 4, new List<int>() { 1, 2 } },
        { 5, new List<int>() { 1, 3 } },
        { 6, new List<int>() { 2, 3 } },
        { 7, new List<int>() { 1, 2, 3 } }
    };

    private int hitPotCounter = 0;

    public override void Initialize()
    {
        base.Initialize();
        potsUI.Init();
    }

    public override void OnReset()
    {
        MaxScatterCount = 0;
        ScatterIsValid = false;
        reelsPredict.Clear();
        base.OnReset();
    }

    public override void Spin()
    {
        foreach (var reel in reels)
        {
            foreach (var symbol in reel.GetVisibleSymbols())
            {
                symbol.SetMaskMode(SpriteMaskInteraction.VisibleInsideMask);
            }
        }

        if (isRolling == false)
        {
            OnReset();
            isRolling = true;
            OnSpinAllListener?.Invoke(this);
            int scatterCount = 0;
            for (int i = 0; i < reels.Length; i++)
            {
                ReelSpinArgs reelSpinArg = Predict(i);
                reelsPredict.Add(i, reelSpinArg);
                foreach (var resultSymbol in reelSpinArg.resultSymbols)
                {
                    if (resultSymbol == SymbolNames.SCATTER)
                    {
                        scatterCount++;
                    }
                }

                OnSpinReelListener?.Invoke(reels[i]);
            }

            if (scatterCount > 0)
            {
                int scatterAssembly = ConfigManager.baseConfig.ScatterAssembly[scatterCount][
                    GlobalObserver.GetRandomWeightedIndex(ConfigManager.baseConfig.ScatterAssemblyProb[scatterCount])];
                if (Cheat.isTriggerBonusGame)
                {
                    scatterAssembly = 123;
                }

                List<int> scatterAssemblyList = new List<int>();
                int scatterAss = scatterAssembly;
                while (scatterAss > 0)
                {
                    scatterAssemblyList.Add(scatterAss % 10);
                    scatterAss /= 10;
                }

                foreach (var item in reelsPredict.Values)
                {
                    for (int i = 0; i < item.resultSymbols.Length; i++)
                    {
                        if (item.resultSymbols[i] == SymbolNames.SCATTER)
                        {
                            int type = scatterAssemblyList.GetRandomItem();
                            item.scatterType[i] = type;
                            scatterAssemblyList.Remove(type);
                        }
                        else
                        {
                            item.scatterType[i] = 0;
                        }
                    }
                }

                int triggerIndex =
                    GlobalObserver.GetRandomWeightedIndex(
                        ConfigManager.baseConfig.AssemblyTriggerProb[scatterAssembly]);
                if (triggerIndex > 0)
                {
                    GlobalObserver.IsTriggerBonusGame = true;
                    GlobalObserver.FreeGamePlay = IndexOfFreeGamePlay[triggerIndex];
                }

                if (Cheat.isTriggerBonusGame)
                {
                    GlobalObserver.IsTriggerBonusGame = true;
                    GlobalObserver.FreeGamePlay = IndexOfFreeGamePlay[Cheat.FreeGamePlayIndex];
                }
            }

            foreach (var item in reelsPredict.Keys)
            {
                reels[item].Spin(reelsPredict[item]);
            }
        }
    }

    protected override void OnReelStop(ReelBase reel)
    {
        stopCount++;
        //在第一列停下来的时候就要判断 要不要停下所有的罐子转换loop2的invoke
        if (stopCount == 1)
        {
            if (hitPotCounter > 0)
            {
                potsUI.StopAllPotsRandomToLoop2();
            }
        }

        var symbols = reel.GetVisibleSymbols();
        float flyTime = 0;
        foreach (var symbol in symbols)
        {
            symbol.SetMaskMode(SpriteMaskInteraction.None);
            if (symbol.ItemName == SymbolNames.SCATTER)
            {
                ScatterSymbol scatterSymbol = symbol as ScatterSymbol;
                scatterSymbol.PlayHitAnim();

                int scatterType = scatterSymbol.ScatterType;
                AudioManager.Playback("FE_GemStopFly");
                FlySymbol flySymbol = SpawnFactory.GetObject<FlySymbol>("Fly");
                flyTime = flySymbol.PlayGemFlyAnim(scatterType, scatterSymbol.transform.position,
                    potsUI.freePots[scatterType - 1].hitPos);
                DelayCallback.Delay(this, flyTime,
                    () =>
                    {
                        potsUI.Hit(scatterType, OnHitComplete,
                            Cheat.isTriggerBonusGame || GlobalObserver.IsTriggerBonusGame);
                    });
            }
        }

        OnStopReelListener?.Invoke(reel);

        if (stopCount >= reels.Length)
        {
            if (Cheat.isTriggerBonusGame || GlobalObserver.IsTriggerBonusGame)
            {
                return;
            }

            isRolling = false;
            OnStopAllListener?.Invoke(this);
        }
    }

    private void OnAllCoinHitComplete(float delayTime)
    {
        if (hitPotCounter <= 0)
        {
            if (Cheat.isTriggerBonusGame)
            {
                GlobalObserver.IsTriggerBonusGame = true;
            }

            if (GlobalObserver.IsTriggerBonusGame)
            {
                DelayCallback.Delay(this, delayTime, () =>
                {
                    float animTime = potsUI.TriggerFreeGameUpgrade(GlobalObserver.FreeGamePlay);
                    DelayCallback.Delay(this, animTime, () =>
                    {
                        isRolling = false;
                        OnStopAllListener?.Invoke(this);
                    });
                });
            }
            else
            {
                DelayCallback.Delay(this, delayTime, () =>
                {
                    potsUI.BeginAllPotsRandomToLoop2();
                });
            }
        }
    }

    protected override ReelSpinArgs Predict(int id)
    {
        var reelSpinArgs = base.Predict(id);
        //要作弊就直接替换下最终结果
        if (Cheat.isTriggerJackPot)
        {
            System.Array.Copy(ConfigManager.cheatConfig.GetCheats(Cheat.TriggerJackPot, id), 0,
                reelSpinArgs.resultSymbols, 1, 3);
        }

        if (Cheat.isTriggerFreeGame)
        {
            System.Array.Copy(ConfigManager.cheatConfig.GetCheats(Cheat.TriggerJackPot, id), 0,
                reelSpinArgs.resultSymbols, 1, 3);
        }

        if (Cheat.isFreeAndJackPot)
        {
            System.Array.Copy(ConfigManager.cheatConfig.GetCheats(Cheat.TriggerJackPot, id), 0,
                reelSpinArgs.resultSymbols, 1, 3);
        }

        if (Cheat.isFreeInJackPot)
        {
            System.Array.Copy(ConfigManager.cheatConfig.GetCheats(Cheat.TriggerJackPot, id), 0,
                reelSpinArgs.resultSymbols, 1, 3);
        }

        if (Cheat.isTriggerBonusGame)
        {
            System.Array.Copy(ConfigManager.cheatConfig.GetCheats(Cheat.TriggerBonusGame, id), 0,
                reelSpinArgs.resultSymbols, 0, 3);
        }

        for (int i = 0; i < reelSpinArgs.resultSymbols.Length; i++)
        {
            if (reelSpinArgs.resultSymbols[i].Equals(SymbolNames.SCATTER))
            {
                hitPotCounter++;
            }
        }

        //System.Array.Copy(ConfigManager.cheatConfig.GetCheats(Cheat.AppointPayout, id), 0, reelSpinArgs.resultSymbols, 0, 3);
        return reelSpinArgs;
    }

    private void OnHitComplete(float animTime)
    {
        hitPotCounter--;
        OnAllCoinHitComplete(animTime);
    }

    protected override string[] GetReelStripe(int id)
    {
        return ConfigManager.reelStripConfig.GetStripe("Base")[id];
    }
}