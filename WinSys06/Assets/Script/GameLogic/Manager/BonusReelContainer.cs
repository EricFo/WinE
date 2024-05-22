using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using SlotGame.Core.Reel;
using System.Buffers.Text;
using System;
using System.Linq;
using SlotGame.Config;
using SlotGame.Reel.Args;
using SlotGame.Core;
using SlotGame.Symbol;
using UniversalModule.AudioSystem;
using UniversalModule.DelaySystem;
using UniversalModule.SpawnSystem;

public class BonusReelContainer : ReelContainer
{
    public override event Action<IReelState> OnSpinReelListener;
    public override event Action<IReelState> OnStopReelListener;
    public override event Action<ReelContainer> OnSpinAllListener;
    public override event Action<ReelContainer> OnStopAllListener;

    public event Action<int> OnSpecialSymbolListener;
    public event Func<int> OnGetCurrentSpinCount;
    public event Action OnJackpotSymbolListener; 
    
    public int containerIndex;

    public List<JackpotType> jackpotPool;
    public Dictionary<int, List<JackpotType>> jackpotOfSpin;
    /// <summary>
    /// 本次SPIN参与旋转的所有Reel，键是ReelID
    /// </summary>
    public Dictionary<int, ReelSpinArgs> reelsPredict = new Dictionary<int, ReelSpinArgs>();
    
    public string reelStripe;

    private bool isSpecialSymbol;


    public override void Initialize()
    {
        jackpotOfSpin = new Dictionary<int, List<JackpotType>>();
        reelStripe = "Free3";
        base.Initialize();
    }

    /// <summary>
    /// 每次进入Bonus的初始化
    /// </summary>
    public void ToBonusInit()
    {
        //surplusUI.Init();
        InitParameter();
        InitLayout();
        InitReplaceSymbolForCoin();
    }

    public void InitReelStripe(string stripe)
    {
        reelStripe = stripe;
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].Setting.reelStripes = GetReelStripe(i);
        }
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    private void InitParameter()
    {
        jackpotOfSpin.Clear();
        foreach (var item in jackpotPool)
        {
            int rand = Random.Range(1, 9);
            if (!jackpotOfSpin.ContainsKey(rand))
            {
                jackpotOfSpin[rand] = new List<JackpotType>();
            }
            jackpotOfSpin[rand].Add(item);
        }

        foreach (var VARIABLE in jackpotOfSpin.Values)
        {
            foreach (var item in VARIABLE)
            {
                Debug.Log(item);
            }
        }
    }

    void InitLayout()
    {

    }

    /// <summary>
    /// 初始化Symbol，继承Base的Coin
    /// </summary>
    void InitReplaceSymbolForCoin()
    {

    }

    public override void Spin()
    {
        foreach (var reel in reels) {
            foreach (var symbol in reel.GetVisibleSymbols()) {
                symbol.SetMaskMode(SpriteMaskInteraction.VisibleInsideMask);
            }
        }
        if (isRolling == false)
        {
            OnReset();
            isRolling = true;
            //OnSpinAllListener?.Invoke(this);
            for (int i = 0; i < reels.Length; i++)
            {
                ReelSpinArgs reelSpinArg = Predict(i);
                reelsPredict.Add(i, reelSpinArg);
                //OnSpinReelListener?.Invoke(reels[i]);
            }
            int spinCount = OnGetCurrentSpinCount.Invoke();
            if (jackpotOfSpin.ContainsKey(spinCount))
            {
                List<int> ableIndex = Enumerable.Range(0, 15).ToList();
                foreach (var item in jackpotOfSpin[spinCount])
                {
                    int index = ableIndex.GetRandomItem();
                    reelsPredict[index % 5].jackpotType[index / 5] = item;
                    ableIndex.Remove(index);
                }
            }
            foreach (var item in reelsPredict.Keys)
            {
                reels[item].Spin(reelsPredict[item]);
            }
        }
    }

    public override void OnReset()
    {
        base.OnReset();
        reelsPredict.Clear();
        isSpecialSymbol = false;
    }

    protected override void OnReelStop(ReelBase reel)
    {
        bool haveJackpotSymbol = false;
        var symbols = reel.GetVisibleSymbols();
        foreach (var symbol in symbols)
        {
            symbol.SetMaskMode(SpriteMaskInteraction.None);
            /*if (symbol.ItemName == SymbolNames.SPECIAL)
            {
                isSpecialSymbol = true;
                (symbol as SpecialSymbol).PlayWinAnim();
                StartCoroutine(AddSpinFly(symbol.transform.position));
            }*/

            if (symbol.ItemName==SymbolNames.WILD)
            {
                if (GlobalObserver.FreeGamePlay.Contains(3))
                {
                    (symbol as WildSymbol).Multiplier =
                        ConfigManager.bonusConfig.WildMultiplier[
                            GlobalObserver.GetRandomWeightedIndex(
                                ConfigManager.bonusConfig.WildMultiplierProb[GlobalObserver.FreeGamePlay.ListIntToString()])];
                }
            }

            if (symbol.JackpotType != JackpotType.Null)
            {
                haveJackpotSymbol = true;
                AudioManager.Stop("FE_JPBallStop");
                AudioManager.PlayOneShot("FE_JPBallStop");
                symbol._jackpotSymbol.PlayStopAnim();
                OnJackpotSymbolListener?.Invoke();
            }
        }

        /*if (haveJackpotSymbol)
        {
            if (GlobalObserver.isFreeShutDown)
            {
                AudioManager.PlayOneShot("FE_JPBallStop");
            }
            else
            {
                AudioManager.Playback("FE_JPBallStop");
            }
        }*/
        stopCount++;
        OnStopReelListener?.Invoke(reel);
        if (stopCount >= reels.Length)
        {
            StartCoroutine(OnAllReelStop());
        }
    }



    IEnumerator OnAllReelStop()
    {
        /*if (isSpecialSymbol)
        {
            yield return new WaitForSeconds(60 / 25f);
        }
        bool haveVarSymbol = false;
        foreach (var eReel in reels)
        {
            foreach (var symbol in eReel.GetVisibleSymbols())
            {
                if (symbol.ItemName==SymbolNames.VAR)
                {
                    haveVarSymbol = true;
                    //yield return new WaitForSeconds(0.5f);
                }
            }
        }
        yield return new WaitForSeconds(haveVarSymbol ? 0.5f : 0);
        
        if (haveVarSymbol)
        {
            string varySymbolName =
                ConfigManager.bonusConfig.VarySymbolName[
                    GlobalObserver.GetRandomWeightedIndex(
                        ConfigManager.bonusConfig.VarySymbolNameProb[GlobalObserver.FreeGamePlay.ListIntToString()])];
            foreach (var eReel in reels)
            {
                foreach (var symbol in eReel.GetVisibleSymbols())
                {
                    if (symbol.ItemName==SymbolNames.VAR)
                    {
                        (symbol as VarSymbol).PlayDoorOpenAnim(varySymbolName);
                    }
                }
            }

            yield return new WaitForSeconds(2f);
            foreach (var eReel in reels)
            {
                foreach (var symbol in eReel.GetVisibleSymbols())
                {
                    if (symbol.ItemName==SymbolNames.VAR)
                    {
                        eReel.ReplaceSymbol(varySymbolName, symbol);
                    }
                }
            }
        }

        yield return new WaitForSeconds(haveVarSymbol ? 0.5f : 0);*/
        isRolling = false;
        OnStopAllListener?.Invoke(this);
        yield return null;
    }
    

    protected override string[] GetReelStripe(int id)
    {
        return ConfigManager.reelStripConfig.GetStripe(reelStripe)[id];
    }
    protected override bool CheckScatter(int id, bool isScatter)
    {
        return false;
    }

}
