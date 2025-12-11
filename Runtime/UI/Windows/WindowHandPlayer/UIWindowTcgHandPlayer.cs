using System.Collections.Generic;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어가 현재 가지고 있는 카드 윈도우
    /// </summary>
    public class UIWindowTcgHandPlayer : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public TMP_Text textCurrentMana;
        [Tooltip("턴 종료 버튼")]
        public Button buttonTurnOff;
        [Tooltip("마나 토글 컴포넌트를 넣을 Transform")]
        public Transform containerToggleMana;
        [Tooltip("마나 토글 프리팹")]
        public GameObject prefabToggleMana;
        
        private readonly List<Toggle> _toggleManaList = new List<Toggle>();

        private TcgBattleManager _battleManager;

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
            // if (battleManager != null)
            // {
            //     battleManager.onExecuteCommand -= UpdateMana;    
            // }
        }

        public void SetBattleManager(TcgBattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        public void RefreshHand(IReadOnlyList<TcgBattleDataCard> hand)
        {
            // 기존 SetFirstCard 기반 구현을 확장하여,
            // hand 리스트를 기준으로 슬롯/아이콘 다시 배치
            DetachAllIcons();
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
        public void SetInteractable(bool interactable)
        {
            // 버튼/슬롯에 RaycastTarget, 버튼 활성화 등 적용
        }
        
        /// <summary>
        /// 현재 마나 표시 정보 업데이트
        /// </summary>
        /// <param name="currentMana">현재 마나</param>
        /// <param name="maxMana">현재 최대 마나</param>
        public void SetMana(int currentMana, int maxMana)
        {
            if (!textCurrentMana) return;
            textCurrentMana.text = $"{currentMana}/{maxMana}";
            var count = Mathf.Min(maxMana, _toggleManaList.Count);
            for (int i = 0; i < count; i++)
            {
                var toggle = _toggleManaList[i];
                if (toggle == null) continue;
                toggle.gameObject.SetActive(true);
                toggle.isOn = i < currentMana;
            }
        }

        private void OnClickTurnOff()
        {
            _battleManager?.OnUiRequestEndTurn();
        }
    }
}