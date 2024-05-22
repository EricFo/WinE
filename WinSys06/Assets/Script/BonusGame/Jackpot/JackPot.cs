using SlotGame.Config;
using SlotGame.Core;
using SlotGame.Symbol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SlotGame.Result;
using UnityEngine;
using UnityEngine.UI;
using UniversalModule.AudioSystem;
using UniversalModule.SpawnSystem;

public enum JackpotType
{
    Grand,
    Major,
    Minor,
    Mini,
    Null
}

public class JackPot : MonoBehaviour {

    public ArtText[] texts;
    public GameObject[] celeAnims;
    public GameObject[] meterCeleAnims;
    public Animator meterWinAnim;

    public float[] CeleTime;
    private const float GRANDTIME = 25f;
    private const float MAJORTIME = 19f;
    private const float MINORTIME = 14f;
    private const float MINITIME = 9.5f;

    public Dictionary<JackpotType, int> ballCount;
    public Dictionary<JackpotType, int> ballPrizeCount;

    public MeterUI meter;
    
    
    

    public void Awake() {
        /*texts[0].text = "$10.00";
        texts[1].text = "$25.00";
        texts[2].text = "$400.00";
        texts[3].text = "$5,000.00";*/
        ballCount = new Dictionary<JackpotType, int>()
        {
            { JackpotType.Grand, 0 },
            { JackpotType.Major, 0 },
            { JackpotType.Minor, 0 },
            { JackpotType.Mini, 0 }
        };
        ballPrizeCount = new Dictionary<JackpotType, int>()
        {
            { JackpotType.Grand, 3 },
            { JackpotType.Major, 3 },
            { JackpotType.Minor, 3 },
            { JackpotType.Mini, 3 }
        };
    }
    
    

    /// <summary>
    /// 播放JackPot弹窗动画
    /// </summary>
    /// <param name="i"></param>
    public float PlayJackPot(List<int> JPList) {

        JPList.Sort();

        StartCoroutine(PlayCeleIntro(JPList));

        float time = 0;
        for (int i = 0; i < JPList.Count; i++) {
            switch (JPList[i]) {
                case 0:
                    time += MINITIME;
                    break;
                case 1:
                    time += MINORTIME;
                    break;
                case 2:
                    time += MAJORTIME;
                    break;
                case 3:
                    time += GRANDTIME;
                    break;
            }
        }
        return time;
    }

    private IEnumerator PlayCeleIntro(List<int> JPList) {
        for (int i = 0; i < JPList.Count; i++) {
            celeAnims[JPList[i]].SetActive(true);
            meterCeleAnims[JPList[i]].SetActive(true);
            UpdateTexts(JPList[i]);
            float time = 0;
            switch (JPList[i]) {
                case 0:
                    time = MINITIME;
                    AudioManager.PlayOneShot("KGI_Game32_Cele1");
                    break;
                case 1:
                    time = MINORTIME;
                    AudioManager.PlayOneShot("KGI_Game32_Cele2");
                    break;
                case 2:
                    time = MAJORTIME;
                    AudioManager.PlayOneShot("KGI_Game32_Cele3");
                    break;
                case 3:
                    time = GRANDTIME;
                    AudioManager.PlayOneShot("KGI_Game32_Cele4");
                    break;
            }
            AudioManager.Pause("KGI_Game32_Bonus_Music");
            yield return new WaitForSeconds(time);
            AudioManager.Continue("KGI_Game32_Bonus_Music");
            RestJackPot(JPList[i]);
            celeAnims[JPList[i]].SetActive(false);
            meterCeleAnims[JPList[i]].SetActive(false);
        }
    }
    

    public void InitFree()
    {
        ballCount = new Dictionary<JackpotType, int>()
        {
            { JackpotType.Grand, 0 },
            { JackpotType.Major, 0 },
            { JackpotType.Minor, 0 },
            { JackpotType.Mini, 0 }
        };
        UpdateMeterBall(JackpotType.Grand);
        UpdateMeterBall(JackpotType.Major);
        UpdateMeterBall(JackpotType.Minor);
        UpdateMeterBall(JackpotType.Mini);
    }
    
    public Vector3 GetFlyTargetPos(JackpotType jackpotType)
    {
        Vector3 targetPos = Vector3.one;
        switch (jackpotType)
        {
            case JackpotType.Grand:
                targetPos = meter.grandBalls[ballCount[jackpotType]].transform.position;
                break;
            case JackpotType.Major:
                targetPos = meter.majorBalls[ballCount[jackpotType]].transform.position;
                break;
            case JackpotType.Minor:
                targetPos = meter.minorBalls[ballCount[jackpotType]].transform.position;
                break;
            case JackpotType.Mini:
                targetPos = meter.miniBalls[ballCount[jackpotType]].transform.position;
                break;
        }

        return targetPos;
    }

    public bool CheckJackpotTrigger()
    {
        bool isTrigger = false;
        for (int i = 0; i < ballCount.Count; i++)
        {
            if (ballCount[(JackpotType)i]==ballPrizeCount[(JackpotType)i])
            {
                isTrigger= true;
            }
        }

        return isTrigger;
    }

    private void UpdateMeterBall(JackpotType jackpotType)
    {
        switch (jackpotType)
        {
            case JackpotType.Grand:
                for (int i = 0; i < meter.grandBalls.Length; i++)
                {
                    meter.grandBalls[i].SetActive(i < ballCount[jackpotType]);
                }
                break;
            case JackpotType.Major:
                for (int i = 0; i < meter.majorBalls.Length; i++)
                {
                    meter.majorBalls[i].SetActive(i<ballCount[jackpotType]);
                }
                break;
            case JackpotType.Minor:
                for (int i = 0; i < meter.minorBalls.Length; i++)
                {
                    meter.minorBalls[i].SetActive(i<ballCount[jackpotType]);
                }
                break;
            case JackpotType.Mini:
                for (int i = 0; i < meter.miniBalls.Length; i++)
                {
                    meter.miniBalls[i].SetActive(i<ballCount[jackpotType]);
                }
                break;
        }
    }

    public void AddBallCount(JackpotType jackpotType)
    {
        ballCount[jackpotType]++;
        UpdateMeterBall(jackpotType);
    }

    public IEnumerator PlayJackpotCele()
    {
        for (int i = 0; i < ballCount.Count; i++)
        {
            if (ballCount[(JackpotType)i]==ballPrizeCount[(JackpotType)i])
            {
                meterWinAnim.Play((JackpotType)i+"Win");
                AudioManager.PlayOneShot("FeatureSelect");
                yield return new WaitForSeconds(3f);
                AudioManager.Pause("WinSys_Game6_Free" + GlobalObserver.FreeGamePlay.Count + "_Music");
                AudioManager.PlayOneShot("WinSys_Game6_Music_Cele"+(4-i));
                float time = AudioManager.GetAudioTime("WinSys_Game6_Music_Cele"+(4-i));
                UpdateTexts(i);
                celeAnims[i].SetActive(true);
                yield return new WaitForSeconds(time);
                celeAnims[i].SetActive(false);
                AudioManager.Continue("WinSys_Game6_Free" + GlobalObserver.FreeGamePlay.Count + "_Music");
                RestJackPot(i);
                meterWinAnim.Play("Idle");
                ballCount[(JackpotType)i] = 0;
            }
        }
        yield return null;
    }

    public IEnumerator PlayJackpotCeleBase()
    {
        yield return null;
    }
    

    public void DisableAllMeterAnim()
    {
        foreach(var item in meterCeleAnims)
        {
            item.SetActive(false);
        }
    }

    /// <summary>
    /// 更新弹窗的JP显示
    /// </summary>
    public void UpdateTexts(int id) {
        string value = GlobalObserver.Meters[id].ToString("N3");
        value = value.Remove(value.Length - 1);
        texts[id].text = string.Format("${0}", value);
    }

    /// <summary>
    /// 重新Mater上的奖池，以及将奖池的钱添加到ToTalWin中
    /// </summary>
    public void RestJackPot(int id) {
        //var result = GlobalObserver.GetResult(GlobalObserver.CurrGameState);
        PayoutInfo result = new PayoutInfo(GameState.Free);
        result.WinMoney = (int)(GlobalObserver.Meters[id] * 100);
        GlobalObserver.UpdateWin(result);
        UIController.BottomPanel.SetAward(GlobalObserver.TotalWin);
        GlobalObserver.ResetMeter(id);
        UIController.MeterPanel.ResetMeter(id, GlobalObserver.Meters[id]);
    }
}
