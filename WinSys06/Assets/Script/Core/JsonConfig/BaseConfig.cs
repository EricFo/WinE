using System.Collections;
using System.Collections.Generic;

public class BaseConfig
{
    public int[] GemCredit = new int[0];
    public float[] GemCreditProb = new float[0];
    public Dictionary<int, int[]> ScatterAssembly = new Dictionary<int, int[]>();
    public Dictionary<int, float[]> ScatterAssemblyProb = new Dictionary<int, float[]>();
    public Dictionary<int, float[]> AssemblyTriggerProb = new Dictionary<int, float[]>();
}
