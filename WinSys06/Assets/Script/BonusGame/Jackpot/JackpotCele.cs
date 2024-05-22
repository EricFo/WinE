using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackpotCele : FeaturePicked
{
    public ArtText meterTxt;


    public void SetPrizeValue(float value)
    {
        meterTxt.SetContent(value.ToString());
    }

    public void Show(float prize,float freeTime, float duration)
    {
        SetPrizeValue(prize);
        base.Show(freeTime, duration);
    }
}
