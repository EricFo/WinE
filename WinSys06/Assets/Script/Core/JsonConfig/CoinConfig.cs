using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinConfig
{
    public Dictionary<string,int[]> CoinsLevelCount = new Dictionary<string, int[]>();
    public Dictionary<string,float[]> CoinsLevelCountProbability = new Dictionary<string, float[]>();
    public Dictionary<string,float[]> PlayFlickerProbability = new Dictionary<string, float[]>();
}
