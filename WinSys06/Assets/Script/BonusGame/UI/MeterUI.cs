using System;
using System.Collections;
using System.Collections.Generic;
using SlotGame.Core;
using UnityEngine;

public class MeterUI : MonoBehaviour
{
    public GameObject baseMeter;
    public GameObject freeMeter;

    public Transform[] meterTxt;
    public Transform[] baseMeterPos;
    public Transform[] freeMeterPos;

    public GameObject[] grandBalls;
    public GameObject[] majorBalls;
    public GameObject[] minorBalls;
    public GameObject[] miniBalls;

    public void SetMeterLayout()
    {
        if (GlobalObserver.CurrGameState==GameState.Base)
        {
            baseMeter.SetActive(true);
            freeMeter.SetActive(false);
            for (int i = 0; i < meterTxt.Length; i++)
            {
                meterTxt[i].position = baseMeterPos[i].position;
            }

            meterTxt[0].localScale = Vector3.one * 0.45f;
            meterTxt[1].localScale = Vector3.one * 0.27f;
            meterTxt[2].localScale = Vector3.one * 0.27f;
            meterTxt[3].localScale = Vector3.one * 0.27f;
        }
        else
        {
            baseMeter.SetActive(false);
            freeMeter.SetActive(true);
            for (int i = 0; i < meterTxt.Length; i++)
            {
                meterTxt[i].position = freeMeterPos[i].position;
            }
            
            meterTxt[0].localScale = Vector3.one * 0.58f;
            meterTxt[1].localScale = Vector3.one * 0.34f;
            meterTxt[2].localScale = Vector3.one * 0.34f;
            meterTxt[3].localScale = Vector3.one * 0.34f;
        }
    }
    
    


}
