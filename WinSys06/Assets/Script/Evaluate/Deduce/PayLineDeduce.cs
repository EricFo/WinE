using SlotGame.Symbol;
using UniversalModule.AudioSystem;

namespace SlotGame.Deduce {
    using UnityEngine;
    using SlotGame.Core;
    using SlotGame.Result;
    using System.Collections;
    using System.Collections.Generic;
    using UniversalModule.Initialize;
    using UniversalModule.DelaySystem;
    using UniversalModule.SpawnSystem;

    public class PayLineDeduce {
        private static WaitForSeconds Wait;
        private static Dictionary<int,List<PayLine>> Lines;
        private static Dictionary<int,Dictionary<string, Coroutine>>  coroutines;
        [AutoLoad]
        private static void Initialize()
        {
            Lines = new Dictionary<int, List<PayLine>>();
            for (int i = 0; i < 4; i++)
            {
                Lines.Add(i,new List<PayLine>());
            }
            Wait = new WaitForSeconds(3f);
            coroutines = new Dictionary<int, Dictionary<string, Coroutine>>();
            GlobalObserver.RegisterDeduce(PayMode.Line, Deduce);
            GlobalObserver.RegisterAbortDeduce(PayMode.Line, Abort);
        }
        /// <summary>
        /// 开始推演赔付流程
        /// </summary>
        private static void Deduce(int index, ResultNode node) {
            PayLineResult result = node as PayLineResult;
            if (result.DrawLines.Count > 0) {
                var coroutine = DelayCallback.BeginCoroutine(CycleDeduce(index,result));
                if (coroutines.ContainsKey(index))
                {
                    coroutines[index].Add(node.ID, coroutine);
                }
                else
                {
                    coroutines.Add(index,new Dictionary<string, Coroutine>());
                    coroutines[index].Add(node.ID, coroutine);
                }
            }
        }
        /// <summary>
        /// 终止推演
        /// </summary>
        private static void Abort(int index, ResultNode node) {
                if (Lines[index].Count>0)
                {
                    PayLineResult result = node as PayLineResult;
                    for (int i = 0; i < Lines[index].Count; i++) {
                        Lines[index][i].Hide();
                        for (int j = 0; j < result.WinSymbols[i].Length; j++) {
                            result.WinSymbols[i][j].PlayIdleAnim();
                        }
                        Lines[index][i].Recycle();
                    }
                    Lines[index].Clear();
                    Coroutine coroutine = null;
                    foreach (var corout in coroutines.Values)
                    {
                        if (corout.TryGetValue(node.ID, out coroutine)) {
                            DelayCallback.AbortCoroutine(coroutine);
                            corout.Remove(node.ID);
                        }
                    }
                }

        }
        /// <summary>
        /// 循环推演
        /// </summary>
        /// <returns></returns>
        private static IEnumerator CycleDeduce(int index,PayLineResult result)
        {
            List<CommonSymbol> winSymbols = new List<CommonSymbol>();
            foreach (var symbols in result.WinSymbols)
            {
                foreach (var symbol in symbols)
                {
                    if (symbol.ItemName==SymbolNames.WILD&& !winSymbols.Contains(symbol))
                    {
                        winSymbols.Add(symbol);
                    }
                }
            }
            foreach (var item in winSymbols)
            {
                GlobalObserver.TotalWinWildCount++;
                (item as WildSymbol).PlayMultiplierIntroAnim();
            }

            if (winSymbols.Count > 0 && GlobalObserver.CurrGameState == GameState.Free&& GlobalObserver.FreeGamePlay.Contains(3))
            {
                yield return new WaitForSeconds(18 / 25f);
            }

            EventClass.EventAwardRollUp EventAwardRollUp = new EventClass.EventAwardRollUp();
            //发送事件
            EventManager.Send(EventAwardRollUp);
            Lines[index].Clear();
            for (int i = 0; i < result.DrawLines.Count; i++) {
                PayLine line = SpawnFactory.GetObject<PayLine>(SpawnItemNames.PayLine);
                if (GlobalObserver.FreeGamePlay.Contains(1)&& GlobalObserver.CurrGameState==GameState.Free)
                {
                    line.SetLineWidth(0.135f);
                }
                if (Lines.ContainsKey(index))
                {
                    Lines[index].Add(line);
                }
                else
                {
                    Lines.Add(index,new List<PayLine>());
                    Lines[index].Add(line);
                }
                line.DrawLine(result.DrawLines[i]);
                line.Show();
                for (int j = 0; j < result.WinSymbols[i].Length; j++) {
                    result.WinSymbols[i][j].PlayAwardAnim();
                }
            }
            yield return Wait;
            if (Lines[index].Count > 1) {
                for (int i = 0; i < Lines[index].Count; i++) {
                    Lines[index][i].Hide();
                    for (int j = 0; j < result.WinSymbols[i].Length; j++) {
                        result.WinSymbols[i][j].PlayIdleAnim();
                    }
                }
                while (true) {
                    for (int i = 0; i < Lines[index].Count; i++) {
                        Lines[index][i].Show();
                        for (int j = 0; j < result.WinSymbols[i].Length; j++) {
                            result.WinSymbols[i][j].PlayAwardAnim();
                        }
                        yield return Wait;
                        Lines[index][i].Hide();
                        for (int j = 0; j < result.WinSymbols[i].Length; j++) {
                            result.WinSymbols[i][j].PlayIdleAnim();
                        }
                    }
                }
            }
        }
    }
}
