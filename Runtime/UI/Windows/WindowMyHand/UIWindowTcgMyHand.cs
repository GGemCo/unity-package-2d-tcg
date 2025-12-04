using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어가 현재 가지고 있는 카드 윈도우
    /// </summary>
    public class UIWindowTcgMyHand : UIWindow
    {
        private int? _startingHandCardCount;
        
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
    }
}