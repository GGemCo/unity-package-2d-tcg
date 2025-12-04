using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어가 현재 가지고 있는 카드 윈도우
    /// </summary>
    public class UIWindowTcgMyHand : UIWindow
    {
        private int? _startingHandCardCount;
        private TcgBattleManager _battleManager;
        private ConfigCommonTcg.TcgPlayerSide _side;
        
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            if (iconPrefab == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyHand)}: iconPrefab 가 지정되지 않았습니다.");
                return;
            }
            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyHand)}: containerIcon 이 null 입니다.");
                return;
            }
            
            uid = UIWindowConstants.WindowUid.TcgMyHand;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerMyHand());
            DragDropHandler.SetStrategy(new DragDropStrategyMyHand());
        }
        /// <summary>
        /// 대결 시작 시, 카드 받기
        /// </summary>
        /// <param name="deckRuntime"></param>
        public void SetFirstCard(DeckRuntime<CardRuntime> deckRuntime)
        {
            _startingHandCardCount ??= AddressableLoaderSettingsTcg.Instance.tcgSettings.startingHandCardCount;

            if (_startingHandCardCount <= 0)
            {
                GcLogger.LogError($"{nameof(GGemCoTcgSettings)} 스크립터블 오브젝트에서 startingHandCardCount 값을 셋팅해주세요.");
                return;
            }
            DetachAllIcons();

            for (int i = 0; i < _startingHandCardCount; i++)
            {
                if (deckRuntime.TryDraw(out var firstCard))
                {
                    GcLogger.Log($"드로우 {i+1}: {firstCard.Uid}");
                }

                SetIconCount(firstCard.Uid, 1);
            }
        }
        public void SetBattleManager(TcgBattleManager battleManager, ConfigCommonTcg.TcgPlayerSide side)
        {
            _battleManager = battleManager;
            _side = side;
        }

        public void RefreshHand(IReadOnlyList<CardRuntime> hand)
        {
            // 기존 SetFirstCard 기반 구현을 확장하여,
            // hand 리스트를 기준으로 슬롯/아이콘 다시 배치
        }

        public void SetInteractable(bool interactable)
        {
            // 버튼/슬롯에 RaycastTarget, 버튼 활성화 등 적용
        }

        // 카드 클릭 시 (예: 아이콘에 연결)
        public void OnClickCard(CardRuntime card)
        {
            if (_battleManager == null)
                return;

            _battleManager.OnUiRequestPlayCard(_side, card);
        }
    }
}