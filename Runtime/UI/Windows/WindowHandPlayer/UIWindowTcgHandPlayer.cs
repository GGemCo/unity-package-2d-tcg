using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어가 현재 가지고 있는 카드 윈도우
    /// </summary>
    public class UIWindowTcgHandPlayer : UIWindowTcgHandBase
    {
        [Tooltip("턴 종료 버튼")]
        public Button buttonTurnOff;
        [Tooltip("마나 토글 컴포넌트를 넣을 Transform")]
        public Transform containerToggleMana;
        [Tooltip("마나 토글 프리팹")]
        public GameObject prefabToggleMana;
        
        private TcgBattleControllerPlayer _battleControllerPlayer;
        private readonly List<Toggle> _toggleManaList = new List<Toggle>();
        
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            if (iconPrefab == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgHandPlayer)}: iconPrefab 가 지정되지 않았습니다.");
                return;
            }
            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgHandPlayer)}: containerIcon 이 null 입니다.");
                return;
            }
            
            uid = UIWindowConstants.WindowUid.TcgHandPlayer;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerHandPlayer());
            DragDropHandler.SetStrategy(new DragDropStrategyHandPlayer());
            buttonTurnOff?.onClick.AddListener(OnClickTurnOff);

            CreateToggleMana();
        }

        private void CreateToggleMana()
        {
            int countMaxManaInBattle = AddressableLoaderSettingsTcg.Instance.tcgSettings.countMaxManaInBattle;
            _toggleManaList.Clear();
            for (int i = 0; i < countMaxManaInBattle; i++)
            {
                var toggleMana = Instantiate(prefabToggleMana, containerToggleMana);
                toggleMana.SetActive(false);
                var toggle = toggleMana.GetComponent<Toggle>();
                if (toggle == null)
                {
                    GcLogger.LogError($"마나 토글 프리팹에 {nameof(Toggle)} 클래스가 없습니다.");
                    continue;
                }

                toggleMana.SetActive(false);
                toggle.isOn = false;
                _toggleManaList.Add(toggle);
            }
        }

        protected void OnDestroy()
        {
            buttonTurnOff?.onClick.RemoveAllListeners();
            if (battleManager != null)
            {
                battleManager.onExecuteCommand -= UpdateMana;    
            }
        }

        public override void SetInteractable(bool b)
        {
            // 버튼/슬롯에 RaycastTarget, 버튼 활성화 등 적용
        }

        public override void SetBattleManager(TcgBattleManager tcgBattleManager, TcgBattleControllerBase tcgBattleController)
        {
            base.SetBattleManager(tcgBattleManager, tcgBattleController);
            _battleControllerPlayer = tcgBattleController as TcgBattleControllerPlayer;
        }
        /// <summary>
        /// 현재 마나 표시 정보 업데이트
        /// </summary>
        protected override void UpdateMana()
        {
            base.UpdateMana();
            var maxMana = battleController.GetCurrentMaxMana();
            var currentMana = battleController.GetCurrentMana();
            for (int i = 0; i < maxMana; i++)
            {
                var toggle = _toggleManaList[i];
                if (toggle == null) continue;
                toggle.gameObject.SetActive(true);
                toggle.isOn = i < currentMana;
            }
        }

        private void OnClickTurnOff()
        {
            if (_battleControllerPlayer == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleControllerPlayer)} 클래스가 없습니다.");
                return;
            }
            _battleControllerPlayer.OnUiRequestEndTurn();
        }
        public override void RefreshHand()
        {
            base.RefreshHand();
            var hand = _battleControllerPlayer.GetHandCards();
            int i = 0;
            foreach (var tcgBattleDataCard in hand)
            {
                var uiIcon = SetIconCount(i, tcgBattleDataCard.Uid, 1);
                if (!uiIcon) continue;
                var uiIconHandPlayer = uiIcon as UIIconHandPlayer;
                if (!uiIconHandPlayer) continue;
                uiIconHandPlayer.SetBattleDataCard(tcgBattleDataCard);
                i++;
            }
        }
    }
}