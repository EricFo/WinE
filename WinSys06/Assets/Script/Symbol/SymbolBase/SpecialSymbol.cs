using System.Collections;
using System.Collections.Generic;
using SlotGame.Symbol;
using UnityEngine;
using UniversalModule.SpawnSystem;

public class SpecialSymbol : CommonSymbol
{
    #region 动画Hash
    protected int WIN = Animator.StringToHash("Win");
    #endregion
    //记录播放动画前的层级
    public int MainTexOrder;
    //回到正常层级
    public void RecoverAniOrder()
    {
        MainTex.sortingOrder = MainTexOrder;
    }
    //对于播放动画时设置的层级
    public void SetAniOrder()
    {
        //根据reelid，添加层级
        //因为SpecialSymbol只存在奖励关卡，所以获取free
        int id = transform.parent.GetComponent<ReelFree>().Setting.reelID;
        if (id < 3)
        {
            MainTex.sortingOrder = MainTexOrder + 21;
        }
        else
        {
            MainTex.sortingOrder = MainTexOrder + 2;
        }
    }
    public override void PlayIdleAnim()
    {
        RecoverAniOrder();
        base.PlayIdleAnim();
    }
    public void PlayWinAnim()
    {
        SetAniOrder();
        PlayAnimation(WIN);
    }
    public override void Install(Transform parent, Vector3 localPos, int reelID, int sortingOrder = 101)
    {
        base.Install(parent, localPos, reelID, sortingOrder + (reelID == 4 ? 2 : 21));
    }
    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        MainTexOrder = MainTex.sortingOrder;
    }
    
    public void EndAnim()
    {
        PlayIdleAnim();
    }



}
