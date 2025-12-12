// TcgBattleDeckService.cs
using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 시작 시 사용할 덱 런타임과 셔플 컨텍스트를 생성하는 서비스.
    /// SaveData, 테이블, 설정, Seed 를 활용하여 TcgBattleDataDeck 을 구성합니다.
    /// </summary>
    public sealed class TcgBattleDeckController
    {
        private readonly SaveDataManagerTcg _saveDataManagerTcg;
        private readonly GGemCoTcgSettings _tcgSettings;
        private readonly TableTcgCard _tableTcgCard;
        private readonly SeedManager _seedManager;

        public TcgBattleDeckController(
            SaveDataManagerTcg saveDataManagerTcg,
            GGemCoTcgSettings settings,
            TableTcgCard tableTcgCard)
        {
            _saveDataManagerTcg = saveDataManagerTcg ?? throw new ArgumentNullException(nameof(saveDataManagerTcg));
            _tcgSettings           = settings ?? throw new ArgumentNullException(nameof(settings));
            _tableTcgCard       = tableTcgCard ?? throw new ArgumentNullException(nameof(tableTcgCard));
            _seedManager        = new SeedManager();
        }

        /// <summary>
        /// 플레이어 덱 런타임을 생성합니다.
        /// </summary>
        public TcgBattleDataDeck<TcgBattleDataCard> BuildPlayerDeckRuntime(int playerDeckIndex, int seed)
        {
            var myDeckData = _saveDataManagerTcg.MyDeck;
            if (myDeckData == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleDeckController)}] MyDeckData 가 없습니다.");
                return null;
            }

            var deckInfo = myDeckData.GetDeckInfoByIndex(playerDeckIndex);
            if (deckInfo == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleDeckController)}] 잘못된 플레이어 덱 인덱스: {playerDeckIndex}");
                return null;
            }

            var shuffleContext = BuildShuffleContext(seed);
            var deckRuntime    = new TcgBattleDataDeck<TcgBattleDataCard>(shuffleContext);

            // 3. 런타임 덱 생성 및 셔플
            List<TcgBattleDataCard> runtimeCardList = TcgBattleDataDeckBuilder.BuildRuntimeDeckCardList(deckInfo.cardList);
            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();
            LogShuffledDeckForDebug(deckRuntime, "Player");
            var heroCard = TcgBattleDataDeckBuilder.BuildRuntimeHeroCard(deckInfo.heroCardUid);
            deckRuntime.SetHeroCard(heroCard);

            return deckRuntime;
        }

        /// <summary>
        /// Enemy 덱 런타임을 생성합니다.
        /// </summary>
        public TcgBattleDataDeck<TcgBattleDataCard> BuildEnemyDeckRuntime(int enemyDeckPresetId, int seed)
        {
            var shuffleContext = BuildShuffleContext(seed);

            // TableTcgCard에서 전체 카드 리스트 가져오기
            var tableTcgCards = _tableTcgCard.GetAll();
            // AI 덱 구성 (Random은 외부 주입 or 내부 생성)
            List<int> cardUids = new List<int>();
            cardUids.Clear();
            cardUids = _tcgSettings.testDeckPreset.BuildDeckUids(tableTcgCards);

            Dictionary<int, int> cardList = new Dictionary<int, int>();
            cardList.Clear();
            foreach (int uid in cardUids)
            {
                cardList.TryAdd(uid, 1);
            }
            List<TcgBattleDataCard> runtimeCardList = TcgBattleDataDeckBuilder.BuildRuntimeDeckCardList(cardList);

            var deckRuntime = new TcgBattleDataDeck<TcgBattleDataCard>(shuffleContext);
            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();
            LogShuffledDeckForDebug(deckRuntime, "Enemy");
            
            int heroCardUid = 0;
            if (_tcgSettings.testDeckPreset.heroCardUid > 0)
            {
                heroCardUid = _tcgSettings.testDeckPreset.heroCardUid;
            }
            else if (_tcgSettings.testDeckPreset.heroCardUids.Count > 0)
            {
                heroCardUid = _tcgSettings.testDeckPreset.heroCardUids[Random.Range(0, _tcgSettings.testDeckPreset.heroCardUids.Count)];
            }
            
            var heroCard = TcgBattleDataDeckBuilder.BuildRuntimeHeroCard(heroCardUid);
            deckRuntime.SetHeroCard(heroCard);

            return deckRuntime;
        }

        private void UpdateSeedManager(int seed)
        {
            _seedManager.SetFixedSeed(null);
            if (seed > 0)
            {
                _seedManager.SetFixedSeed(seed);
            }
            _seedManager.ApplySeed();
        }
        /// <summary>
        /// 셔플에 사용할 컨텍스트(Seed, 정책 등)를 생성합니다.
        /// </summary>
        private ShuffleMetaData BuildShuffleContext(int seed)
        {
            // 1) 시드 설정
            UpdateSeedManager(seed);
            
            // 2) 셔플 모드 및 설정
            var shuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;
            var shuffleConfig = new ShuffleConfig();

            int[] costWeights = _tcgSettings.costWeights;

            // costWeights 에 값이 정의되어 있으면 Weighted 모드로 전환
            if (costWeights == null || costWeights.Length <= 0)
                return new ShuffleMetaData(shuffleMode, _seedManager, shuffleConfig);
            
            shuffleMode = ConfigCommonTcg.ShuffleMode.Weighted;

            shuffleConfig.FrontLoadedCount = 10;

            if (shuffleConfig.CostWeights == null || shuffleConfig.CostWeights.Count <= 0)
                return new ShuffleMetaData(shuffleMode, _seedManager, shuffleConfig);
            int length = Mathf.Min(costWeights.Length, shuffleConfig.CostWeights.Count);
            for (int i = 0; i < length; i++)
            {
                if (costWeights[i] <= 0) continue;
                shuffleConfig.CostWeights[i] = costWeights[i];
            }

            return new ShuffleMetaData(shuffleMode, _seedManager, shuffleConfig);
        }

        /// <summary>
        /// 디버그용으로 셔플된 덱 정보를 로그로 남깁니다.
        /// </summary>
        private void LogShuffledDeckForDebug(TcgBattleDataDeck<TcgBattleDataCard> deckRuntime, string label)
        {
#if UNITY_EDITOR
            if (deckRuntime == null) return;

            GcLogger.Log($"[TcgBattleDataDeck-{label}] 카드 개수: {deckRuntime.Count}");
            for (int i = 0; i < deckRuntime.Count; i++)
            {
                var card = deckRuntime.Cards[i];
                GcLogger.Log($"[{label}] {i:00}: Uid={card.Uid}, Cost={card.Cost}");
            }
#endif
        }

    }
}
