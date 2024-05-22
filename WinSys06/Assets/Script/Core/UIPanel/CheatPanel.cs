using SlotGame.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.UI {
    [Serializable]
    public class CheatUIPanel {
        public Button FeatureBtn;
        public Button AddCreditBtn;
        public Button AutoPlayBtn;
        public Button BonusGameBtn1;
        public Button BonusGameBtn2;
        public Button BonusGameBtn3;
        public Button BonusGameBtn12;
        public Button BonusGameBtn13;
        public Button BonusGameBtn23;
        public Button BonusGameBtn123;
        public Button GrandBtn;
        public Button MajorBtn;
        public Button MinorBtn;
        public Button MiniBtn;
        public GameObject CheatBtnList;

        #region 事件监听
        public event Action OnAddCreditEvent;
        public event Action OnCheatSpinEvent;
        #endregion

        public void Initialize() {
            CheatBtnList.SetActive(false);
            FeatureBtn.onClick.AddListener(UpdateCheatBtnListState);

            AddCreditBtn.onClick.AddListener(OnAddCreditBtnClick);
            AutoPlayBtn.onClick.AddListener(OnAutoPlayBtnClick);
            BonusGameBtn1.onClick.AddListener(OnBonusGameBtn1Click);
            BonusGameBtn2.onClick.AddListener(OnBonusGameBtn2Click);
            BonusGameBtn3.onClick.AddListener(OnBonusGameBtn3Click);
            BonusGameBtn12.onClick.AddListener(OnBonusGameBtn12Click);
            BonusGameBtn13.onClick.AddListener(OnBonusGameBtn13Click);
            BonusGameBtn23.onClick.AddListener(OnBonusGameBtn23Click);
            BonusGameBtn123.onClick.AddListener(OnBonusGameBtn123Click);
            GrandBtn.onClick.AddListener(OnGrandBtnClick);
            MajorBtn.onClick.AddListener(OnMajorBtnClick);
            MinorBtn.onClick.AddListener(OnMinorBtnClick);
            MiniBtn.onClick.AddListener(OnMiniBtnClick);
        }
        /// <summary>
        /// 隐藏作弊按钮列表
        /// </summary>
        public void HideCheatBtnList() {
            CheatBtnList.SetActive(false);
        }
        /// <summary>
        /// 更新Feature按钮状态
        /// </summary>
        /// <param name="isActive"></param>
        public void UpdateFeatureBtnState(bool isActive) {
            FeatureBtn.interactable = isActive;
        }
        /// <summary>
        /// 更新作弊按钮列表状态
        /// </summary>
        private void UpdateCheatBtnListState() {
            CheatBtnList.SetActive(!CheatBtnList.activeSelf);
        }

        #region 行为事件
        /// <summary>
        /// 增加余额按钮点击事件
        /// </summary>
        private void OnAddCreditBtnClick() {
            OnAddCreditEvent();
            UpdateCheatBtnListState();
        }

        /// <summary>
        /// 自动游戏按钮点击事件
        /// </summary>
        private void OnAutoPlayBtnClick()
        {
            GlobalObserver.SetAutoPlay(true);
            Cheat.isAutoPlay = true;
            OnSpin();
        }

        /// <summary>
        /// 触发Bonus按钮点击事件
        /// </summary>
        private void OnBonusGameBtn1Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 1;
            OnSpin();
        }
        private void OnBonusGameBtn2Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 2;
            OnSpin();
        }
        private void OnBonusGameBtn3Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 3;
            OnSpin();
        }
        private void OnBonusGameBtn12Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 4;
            OnSpin();
        }
        private void OnBonusGameBtn13Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 5;
            OnSpin();
        }
        private void OnBonusGameBtn23Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 6;
            OnSpin();
        }
        private void OnBonusGameBtn123Click()
        {
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 7;
            OnSpin();
        }

        public void OnGrandBtnClick()
        {
            Cheat.isGrand = true;
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 7;
            OnSpin();
        }
        
        public void OnMajorBtnClick()
        {
            Cheat.isMajor = true;
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 7;
            OnSpin();
        }
        
        public void OnMinorBtnClick()
        {
            Cheat.isMinor = true;
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 7;
            OnSpin();
        }
        
        public void OnMiniBtnClick()
        {
            Cheat.isMini = true;
            Cheat.isTriggerBonusGame = true;
            Cheat.FreeGamePlayIndex = 7;
            OnSpin();
        }

        private void OnSpin() {
            UpdateCheatBtnListState();
            OnCheatSpinEvent?.Invoke();
        }
        #endregion

    }
}
