using System.Collections;
using System.Collections.Generic;
using SlotGame.Core.Reel;
using SlotGame.Symbol;
using UnityEngine;

public class VarSymbol : CommonSymbol
{
    public SpriteRenderer AnimTex;
    
    #region 动画Hash
    protected int OPENA = Animator.StringToHash("OpenA");
    protected int OPENK = Animator.StringToHash("OpenK");
    protected int OPENQ = Animator.StringToHash("OpenQ");
    protected int OPENJ = Animator.StringToHash("OpenJ");
    protected int OPENT = Animator.StringToHash("OpenT");
    protected int OPENN = Animator.StringToHash("OpenN");
    protected int OPENPIC01 = Animator.StringToHash("OpenPIC01");
    protected int OPENPIC02 = Animator.StringToHash("OpenPIC02");
    protected int OPENPIC03 = Animator.StringToHash("OpenPIC03");
    protected int OPENPIC04 = Animator.StringToHash("OpenPIC04");
    protected int OPENPIC05 = Animator.StringToHash("OpenPIC05");
    #endregion
    //记录播放动画前的层级
    public int MainTexOrder;
    //回到正常层级
    public void RecoverAniOrder()
    {
        MainTex.sortingOrder = MainTexOrder;
        AnimTex.sortingOrder = MainTex.sortingOrder - 1;
    }
    //对于播放动画时设置的层级
    public void SetAniOrder()
    {
        //根据reelid，添加层级
        //因为VarSymbol只存在free，所以获取free
        int id = transform.parent.GetComponent<ReelFree>().Setting.reelID;
        if (id < 3)
        {
            AnimTex.sortingOrder = MainTexOrder + 21;
        }
        else
        {
            AnimTex.sortingOrder = MainTexOrder + 2;
        }
    }
    public override void PlayIdleAnim()
    {
        RecoverAniOrder();
        base.PlayIdleAnim();
    }
    public void PlayDoorOpenAnim(string symbolName)
    {
        SetAniOrder();
        MainTex.enabled = false;
        MainAnim.Play("Open"+symbolName);
    }

    public override void SetMaskMode(SpriteMaskInteraction interaction)
    {
        base.SetMaskMode(interaction);
        AnimTex.maskInteraction = interaction;
    }
    public override void Install(Transform parent, Vector3 localPos, int reelID, int sortingOrder = 101)
    {
        base.Install(parent, localPos, reelID, sortingOrder + (reelID == 4 ? 2 : 21));
    }
    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        AnimTex.sortingOrder = order - 1;
        MainTexOrder = MainTex.sortingOrder;
    }

    public override void Recycle()
    {
        MainTex.enabled = true;
        base.Recycle();
    }
}
