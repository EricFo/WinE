using DG.Tweening;
using UnityEngine;
using UniversalModule.SpawnSystem;

namespace SlotGame.Symbol {
    public class CommonSymbol : SpawnItem {
        [SerializeField] protected Animator MainAnim;
        [SerializeField] protected SpriteRenderer MainTex;

        protected int originSortingOrder = -1;

        public JackpotSymbol _jackpotSymbol;
        protected JackpotType _jackpotType;

        #region 属性
        /// <summary>
        /// Symbol在Reel中的位置信息
        /// </summary>
        public int IndexOnReel { get; protected set; }
        protected int DefaultSortingOrder { get; set; }

        public JackpotType JackpotType
        {
            get { return _jackpotType; }
            set
            {
                _jackpotType = value;
                if (_jackpotType!=JackpotType.Null)
                {
                    if (_jackpotSymbol==null)
                    {
                        JackpotSymbol jSymbol= SpawnFactory.GetObject<JackpotSymbol>("Jackpot");
                        jSymbol.Install(this);
                    }
                    //PlayAnimation(HIDE);
                }
            } 
        }
        #endregion

        #region 动画Hash
        protected int IDLE = Animator.StringToHash("Idle");
        protected int HIDE = Animator.StringToHash("Hide");
        protected int AWARD = Animator.StringToHash("Award");
        protected int FADE = Animator.StringToHash("Fade");
        #endregion

        #region 动画状态更新及播放动画相关虚函数接口
        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="animHashCode">动画哈希值</param>
        protected void PlayAnimation(int animHashCode) {
            if (MainAnim != null && MainAnim.isActiveAndEnabled) {
                MainAnim.Play(animHashCode, 0, 0);
            }
        }
        /// <summary>
        /// 播放奖励动画
        /// </summary>
        public virtual void PlayAwardAnim() {
            PlayAnimation(AWARD);
        }
        /// <summary>
        /// 播放Idle动画
        /// </summary>
        public virtual void PlayIdleAnim() {
            PlayAnimation(IDLE);
            //ResetLayer();
        }
        #endregion

        #region 设置相关的虚函数接口
        /// <summary>
        /// 更新Symbol信息
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="localPos"></param>
        public virtual void Install(Transform parent, Vector3 localPos, int reelID, int sortingOrder = 101) {
            base.transform.SetParent(parent);
            base.transform.SetAsFirstSibling();  //新出现的Symbol在最上面
            base.transform.localPosition = localPos;
            originSortingOrder = sortingOrder + reelID * 20;
            SetMaskMode(SpriteMaskInteraction.VisibleInsideMask);
            DefaultSortingOrder = originSortingOrder;
            SetSortingOrder(DefaultSortingOrder);
        }
        /// <summary>
        /// 设置遮罩交互模式
        /// </summary>
        /// <param name="interaction"></param>
        public virtual void SetMaskMode(SpriteMaskInteraction interaction) {
            MainTex.maskInteraction = interaction;
            if (_jackpotSymbol!=null)
            {
                _jackpotSymbol.mainTex.maskInteraction = interaction;
                _jackpotSymbol.stopTex.maskInteraction = interaction;
            }
        }
        /// <summary>
        /// 设置层级
        /// </summary>
        /// <param name="order"></param>
        public virtual void SetSortingOrder(int order) {
            MainTex.sortingOrder = order;
            if (_jackpotSymbol!=null)
            {
                _jackpotSymbol.mainTex.sortingOrder = order + 5;
                _jackpotSymbol.stopTex.sortingOrder = order + 6;
            }
        }
        /// <summary>
        /// 更新Symbol的索引值
        /// </summary>
        /// <param name="idx"></param>
        public virtual void UpdateIndexOnReel(int idx) {
            IndexOnReel = idx;
            DefaultSortingOrder = originSortingOrder + idx;
            SetSortingOrder(DefaultSortingOrder);
        }
        /// <summary>
        /// 设置symbol是否显示
        /// </summary>
        /// <param name="isDisplay"></param>
        public virtual void SetDisplay(bool isDisplay) {
            PlayAnimation(isDisplay ? IDLE : HIDE);
            if (_jackpotSymbol!=null)
            {
                _jackpotSymbol.SetDisplay(isDisplay);
            }
        }

        public virtual void FadeDisplay()
        {
            PlayAnimation(FADE);
        }

        public void SetJackpotSymbol(JackpotSymbol jackpotSymbol)
        {
            jackpotSymbol.mainTex.sortingOrder = MainTex.sortingOrder + 5;
            jackpotSymbol.mainTex.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            jackpotSymbol.stopTex.sortingOrder = MainTex.sortingOrder + 6;
            jackpotSymbol.stopTex.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        #endregion

        #region 辅助工具
        protected virtual void ResetLayer() {
            SetSortingOrder(DefaultSortingOrder);
            SetMaskMode(SpriteMaskInteraction.VisibleInsideMask);
        }

        public override void Recycle()
        {
            if (_jackpotSymbol!=null)
            {
                _jackpotSymbol.Recycle();
                _jackpotSymbol = null;
            }
            transform.localScale=Vector3.one;
            base.Recycle();
        }

        #endregion
    }
}
