using System.Collections;
using System.Collections.Generic;
using SlotGame.Config;
using SlotGame.Core;
using SlotGame.Core.Reel;
using SlotGame.Symbol;
using UnityEngine;
using UniversalModule.SpawnSystem;

public class ReelFree : ReelBase
{
    
    protected override CommonSymbol GetNextSymbol()
    {
        int id = stripeID++;
        stripeID %= setting.reelStripes.Length;
        string symbolName;
        CommonSymbol symbol;
        if (currMoveStep < maxMoveStep - (setting.symbolCount - 2) || currMoveStep > maxMoveStep - 2) {
            if(id>=setting.reelStripes.Length)
            {
                id = stripeID++;
            }
            symbolName = setting.reelStripes[id];
            if (symbolName==SymbolNames.VAR&& GlobalObserver.FreeGamePlay.ListIntToString()=="3")
            {
                symbolName = GlobalObserver.FreeVarSymbol;
            }
            symbol= SpawnFactory.GetObject<CommonSymbol>(symbolName);
            if (Random.Range(0, 1f) < 0.05f)
            {
                symbol.JackpotType = (JackpotType)Random.Range(0, 4);
            }
        } else {
            id = maxMoveStep - currMoveStep - 2;
            symbolName = reelSpinArgs.resultSymbols[id];
            //symbolName = SymbolNames.SPECIAL;
            if (symbolName==SymbolNames.VAR&& GlobalObserver.FreeGamePlay.ListIntToString()=="3")
            {
                symbolName = GlobalObserver.FreeVarSymbol;
            }
            symbol = SpawnFactory.GetObject<CommonSymbol>(symbolName);
            symbol.JackpotType = reelSpinArgs.jackpotType[id];
        }

        /*if (symbol.ItemName==SymbolNames.WILD&& GlobalObserver.FreeGamePlay.Contains(3))
        {
            (symbol as WildSymbol).Multiplier =
                ConfigManager.bonusConfig.WildMultiplier[
                    GlobalObserver.GetRandomWeightedIndex(
                        ConfigManager.bonusConfig.WildMultiplierProb[GlobalObserver.FreeGamePlay.ListIntToString()])];
        }*/
        if (symbol._jackpotSymbol!=null)
        {
            symbol._jackpotSymbol.transform.localScale = Vector3.one;
        }
        symbol.transform.localScale = Vector3.one * (GlobalObserver.FreeGamePlay.Contains(1) ? 0.5f : 1);
        return symbol;
    }

    public override CommonSymbol ReplaceSymbol(string symbolName, CommonSymbol oldSymbol)
    {
        /*JackpotSymbol jSymbol = null;
        JackpotType jackpotType = JackpotType.Null;
        if (oldSymbol.JackpotType!=JackpotType.Null)
        {
            jSymbol = oldSymbol._jackpotSymbol;
            oldSymbol._jackpotSymbol = null;
            jackpotType = oldSymbol.JackpotType;
        }*/
        CommonSymbol symbol = base.ReplaceSymbol(symbolName, oldSymbol);
        /*if (jSymbol!=null)
        {
            jSymbol.Install(symbol);
            symbol.JackpotType = jackpotType;
        }*/
        symbol.transform.localScale = Vector3.one;
        return symbol;
    }
}
