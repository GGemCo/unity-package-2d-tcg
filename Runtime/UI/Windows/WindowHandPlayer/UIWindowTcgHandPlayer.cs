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
    public class UIWindowTcgHandPlayer : UIWindowTcgHandBase
    {
        [Tooltip("턴 종료 버튼")]
        public Button buttonTurnOff;
        [Tooltip("마나 토글 컴포넌트를 넣을 Transform")]
        public Transform containerToggleMana;
        [Tooltip("마나 토글 프리팹")]
        public GameObject prefabToggleMana;

        private readonly List<Toggle> _toggleManaList = new List<Toggle>();
        private TcgBattleManager _battleManager;

        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgHandPlayer;

        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerHandPlayer();

        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyHandPlayer();

        protected override void Awake()
        {
            base.Awake();

            // Player 전용 처리
            buttonTurnOff?.onClick.AddListener(OnClickTurnOff);
            CreateToggleMana();
        }

        private void CreateToggleMana()
        {
            if (!AddressableLoaderSettingsTcg.Instance) return;
            int countMaxManaInBattle = AddressableLoaderSettingsTcg.Instance
                .tcgSettings.countMaxManaInBattle;

            _toggleManaList.Clear();

            for (int i = 0; i < countMaxManaInBattle; i++)
            {
                var go = Instantiate(prefabToggleMana, containerToggleMana);
                go.SetActive(false);

                var toggle = go.GetComponent<Toggle>();
                if (toggle == null)
                {
                    GcLogger.LogError($"마나 토글 프리팹에 {nameof(Toggle)} 컴포넌트가 없습니다.");
                    continue;
                }

                toggle.isOn = false;
                _toggleManaList.Add(toggle);
            }
        }

        public void SetBattleManager(TcgBattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        protected void OnDestroy()
        {
            buttonTurnOff?.onClick.RemoveAllListeners();
        }

        protected override void BindCardIcon(UIIcon uiIcon, TcgBattleDataCard card, bool isHero)
        {
            var iconPlayer = uiIcon as UIIconHandPlayer;
            if (!iconPlayer) return;

            iconPlayer.SetBattleDataCard(card);
        }

        public override void SetMana(int currentMana, int maxMana)
        {
            // 1) 기본 텍스트 처리
            base.SetMana(currentMana, maxMana);

            // 2) Player 전용 토글 처리
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
