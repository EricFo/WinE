using System;
using System.Collections;
using System.Collections.Generic;

public class BonusConfig
{
    public int[] WildMultiplier = new int[0];
    public Dictionary<string, float[]> WildMultiplierProb = new Dictionary<string, float[]>();
    public string[] VarySymbolName = new string[0];
    public Dictionary<string, float[]> VarySymbolNameProb = new Dictionary<string, float[]>();
    public double[] JackpotProb = new Double[0];
    public Dictionary<int, float[]> JackpotBallCountProb = new Dictionary<int, float[]>();
}
