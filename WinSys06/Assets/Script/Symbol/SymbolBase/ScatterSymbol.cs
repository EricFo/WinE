using UnityEngine;
using SlotGame.Symbol;
using UnityEngine.TextCore.LowLevel;
using SlotGame.Core.Reel;

public class ScatterSymbol : CommonSymbol
{
    public SpriteRenderer AnimTex;
    public SpriteRenderer CreditTex;
    public ArtText creditTxt;

    public int gemCredit;
    
    public Sprite[] gemSprites;
    
    private int scatterType;

    public int ScatterType
    {
        get { return scatterType; }
        set
        {
            scatterType = value;
            MainTex.sprite = gemSprites[scatterType-1];
        }
    }
    
    #region 动画Hash
    protected int REDHIT = Animator.StringToHash("RedHit");
    protected int GREENHIT = Animator.StringToHash("GreenHit");
    protected int BLUEHIT = Animator.StringToHash("BlueHit");
    protected int REDWIN = Animator.StringToHash("RedWin");
    protected int GREENWIN = Animator.StringToHash("GreenWin");
    protected int BLUEWIN = Animator.StringToHash("BlueWin");
    protected int REDWIN2 = Animator.StringToHash("RedWin2");
    protected int GREENWIN2 = Animator.StringToHash("GreenWin2");
    protected int BLUEWIN2 = Animator.StringToHash("BlueWin2");
    #endregion
    //记录播放动画前的层级
    public int MainTexOrder;
    //对于播放动画时设置的层级
    public void SetAniOrder()
    {
        //根据reelid，添加层级
        //因为scatter只存在base，所以获取base
        int id = transform.parent.GetComponent<ReelBase>().Setting.reelID;
        if (id < 3)
        {
            AnimTex.sortingOrder = MainTexOrder + 21;
            CreditTex.sortingOrder = AnimTex.sortingOrder + 1;
        }
        else
        {
            AnimTex.sortingOrder = MainTexOrder + 2;
            CreditTex.sortingOrder = AnimTex.sortingOrder + 1;
        }
        //Debug.Log("AnimTex.sortingOrder:    " + MainTexOrder);
    }
    //回到正常层级
    public void RecoverAniOrder()
    {
        AnimTex.sortingOrder = MainTexOrder + 1;
        CreditTex.sortingOrder = AnimTex.sortingOrder + 1;
    }
    //回到idle就恢复
    public override void PlayIdleAnim()
    {
        RecoverAniOrder();
        base.PlayIdleAnim();
    }
    //动画结束后关闭动画，
    public void PlayWinAnim()
    {
        //设置层级
        SetAniOrder();
        creditTxt.GetComponent<Animator>().Play(gemCredit>999? "Win2":"Win",0,0);
        switch (scatterType)
        {
            
            case 1:
                
                PlayAnimation(REDWIN);
                break;
            case 2:
                PlayAnimation(GREENWIN);
                break;
            case 3:
                PlayAnimation(BLUEWIN);
                break;
        }
    }

    public void PlayHitAnim()
    {
        //设置层级
        SetAniOrder();
        switch (scatterType)
        {
            case 1:
                PlayAnimation(REDHIT);
                break;
            case 2:
                PlayAnimation(GREENHIT);
                break;
            case 3:
                PlayAnimation(BLUEHIT);
                break;
        }
    }

    public void SetCredit(int value)
    {
        gemCredit = value;
        creditTxt.SetContent(value.ToString());
        creditTxt.GetComponent<Animator>().Play(gemCredit>999? "Idle2":"Idle");
    }

    public void ResetCreditScale()
    {
        creditTxt.GetComponent<Animator>().Play(gemCredit>999? "Idle2":"Idle");
    }
    
    public override void Install(Transform parent, Vector3 localPos, int reelID, int sortingOrder = 101) {
        base.Install(parent, localPos, reelID, sortingOrder + (reelID == 4 ? 2 : 21));
    }

    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        AnimTex.sortingOrder = order + 1;
        CreditTex.sortingOrder = order + 2;
        MainTexOrder = MainTex.sortingOrder;
    }

    public override void SetMaskMode(SpriteMaskInteraction interaction)
    {
        base.SetMaskMode(interaction);
        AnimTex.maskInteraction = interaction;
        CreditTex.maskInteraction = interaction;
    }
}
