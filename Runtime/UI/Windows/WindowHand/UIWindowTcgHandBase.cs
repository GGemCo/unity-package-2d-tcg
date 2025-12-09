using System.Collections.Generic;
using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class UIWindowTcgHandBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public TMP_Text textCurrentMana;
        
        protected int? startingHandCardCount;
        // protected TcgBattleControllerBase battleController;
        protected TcgBattleManager battleManager;
        
        protected override void Start()
        {
            base.Start();
        }
        public virtual void RefreshHand()
        {
            DetachAllIcons();
        }

        /// <summary>
        /// 현재 마나 표시 정보 업데이트
        /// </summary>
        protected virtual void UpdateMana()
        {
            if (!textCurrentMana) return;
            // var maxMana = battleController.GetCurrentMaxMana();
            // var currentMana = battleController.GetCurrentMana();
            // textCurrentMana.text = $"{currentMana}/{maxMana}";
        }
        // public virtual void SetBattleManager(TcgBattleManager tcgBattleManager, TcgBattleControllerBase tcgBattleController)
        // {
        //     battleManager = tcgBattleManager;
        //     if (battleManager != null)
        //     {
        //         battleManager.onExecuteCommand += UpdateMana;
        //     }
        //     battleController = tcgBattleController;
        //     SetInteractable(true);
        //     UpdateMana();
        // }

        public virtual void SetInteractable(bool set)
        {
        }
    }
}