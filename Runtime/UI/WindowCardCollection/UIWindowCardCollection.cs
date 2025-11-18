using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class UIWindowCardCollection : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public TableTcgCard tableTcgCard;
        // todo 정리 필요
        public readonly Dictionary<int, UIElementCard> uiElementTcgCards = new Dictionary<int, UIElementCard>();

        private UIWindowMyCardDeck _uiWindowTcgMyDeck;
        protected override void Awake()
        {
            uiElementTcgCards.Clear();
            uid = UIWindowConstants.WindowUid.TcgCardCollection;
            if (TableLoaderManager.Instance == null) return;
            tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            maxCountIcon = tableTcgCard.GetCount();
            
            // 순서 중요. IconPoolManager 에서 사용한다.
            SlotIconBuildStrategyRegistry.Register(
                UIWindowConstants.WindowUid.TcgCardCollection,
                window => new SlotIconBuildStrategyCollection()
            );
            base.Awake();
            
            IconPoolManager.SetSetIconHandler(new SetIconHandlerCardCollection());
            DragDropHandler.SetStrategy(new DragDropStrategyCardCollection());
        }

        protected override void Start()
        {
            base.Start();
            _uiWindowTcgMyDeck =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowMyCardDeck>(UIWindowConstants.WindowUid.TcgMyCardDeck);
        }

        public void SetPositionUiSlot(UISlot uiSlot, int index)
        {
        }
    }
}