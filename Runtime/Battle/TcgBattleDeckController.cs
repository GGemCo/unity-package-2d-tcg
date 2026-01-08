using System;
using System.Collections.Generic;
using GGemCo2DCore;
using Random = UnityEngine.Random;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 시작 시 사용할 덱 런타임과 셔플 컨텍스트를 생성하는 서비스입니다.
    /// </summary>
    /// <remarks>
    /// SaveData(플레이어 덱), 카드 테이블(<c>TableTcgCard</c>), 설정(<see cref="GGemCoTcgSettings"/>),
    /// 그리고 시드(Seed)를 조합하여 <c>TcgBattleDataDeck</c>을 구성합니다.
    /// </remarks>
    public sealed class TcgBattleDeckController
    {
        private readonly SaveDataManagerTcg _saveDataManagerTcg;
        private readonly GGemCoTcgSettings _tcgSettings;
        private readonly TableTcgCard _tableTcgCard;
        private readonly SeedManager _seedManager;

        /// <summary>
        /// 덱 런타임 생성을 위한 의존성을 주입하여 컨트롤러를 초기화합니다.
        /// </summary>
        /// <param name="saveDataManagerTcg">플레이어 덱 저장 데이터를 제공하는 매니저입니다.</param>
        /// <param name="settings">TCG 전반 설정(셔플 정책/시작 마나/드로우 규칙 등)입니다.</param>
        /// <param name="tableTcgCard">카드 메타(UID, 코스트 등)를 제공하는 테이블입니다.</param>
        /// <exception cref="ArgumentNullException">필수 의존성이 null이면 발생합니다.</exception>
        public TcgBattleDeckController(
            SaveDataManagerTcg saveDataManagerTcg,
            GGemCoTcgSettings settings,
            TableTcgCard tableTcgCard)
        {
            _saveDataManagerTcg = saveDataManagerTcg ?? throw new ArgumentNullException(nameof(saveDataManagerTcg));
            _tcgSettings        = settings ?? throw new ArgumentNullException(nameof(settings));
            _tableTcgCard       = tableTcgCard ?? throw new ArgumentNullException(nameof(tableTcgCard));
            _seedManager        = new SeedManager();
        }

        /// <summary>
        /// 플레이어 덱 런타임을 생성하고 셔플한 뒤 반환합니다.
        /// </summary>
        /// <param name="playerDeckIndex">저장된 덱 슬롯 인덱스입니다.</param>
        /// <param name="seed">셔플에 사용할 시드입니다(0 이하이면 고정 시드를 사용하지 않습니다).</param>
        /// <returns>생성된 플레이어 덱 런타임이며, 실패 시 null입니다.</returns>
        /// <remarks>
        /// - SaveData에서 덱 정보를 조회해 런타임 카드 리스트를 구성합니다.
        /// - 설정 기반 셔플 정책(<see cref="BuildShuffleContext"/>)으로 셔플 컨텍스트를 만들고 셔플을 수행합니다.
        /// - 영웅 카드는 별도로 생성하여 덱에 설정합니다.
        /// </remarks>
        public TcgBattleDataDeck<TcgBattleDataCardInHand> BuildPlayerDeckRuntime(int playerDeckIndex, int seed)
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

            // 런타임 덱 생성 및 셔플
            List<TcgBattleDataCardInHand> runtimeCardList =
                TcgBattleDataDeckBuilder.BuildRuntimeDeckCardList(deckInfo.cardList);

            var shuffleContext = BuildShuffleContext(ConfigCommonTcg.TcgPlayerSide.Player, seed, runtimeCardList.Count);
            var deckRuntime    = new TcgBattleDataDeck<TcgBattleDataCardInHand>(shuffleContext);

            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();

            LogShuffledDeckForDebug(deckRuntime, "Player", seed);

            var heroCard = TcgBattleDataDeckBuilder.BuildRuntimeHeroCard(deckInfo.heroCardUid);
            deckRuntime.SetHeroCard(heroCard);

            return deckRuntime;
        }

        /// <summary>
        /// 적(Enemy) 덱 런타임을 생성하고 셔플한 뒤 반환합니다.
        /// </summary>
        /// <param name="enemyDeckPresetId">적 덱 프리셋 ID입니다(현재 구현에서는 직접 사용되지 않을 수 있습니다).</param>
        /// <param name="seed">셔플에 사용할 시드입니다(0 이하이면 고정 시드를 사용하지 않습니다).</param>
        /// <returns>생성된 적 덱 런타임이며, 실패 시 null입니다.</returns>
        /// <remarks>
        /// - 카드 테이블에서 전체 카드 풀을 가져온 뒤, 설정의 적 덱 프리셋을 사용해 덱 UID 목록을 구성합니다.
        /// - 영웅 카드는 프리셋의 단일 UID 또는 후보 리스트 중 랜덤 선택으로 결정합니다.
        /// </remarks>
        public TcgBattleDataDeck<TcgBattleDataCardInHand> BuildEnemyDeckRuntime(int enemyDeckPresetId, int seed)
        {
            // TableTcgCard에서 전체 카드 리스트 가져오기
            var tableTcgCards = _tableTcgCard.GetAll();

            // AI 덱 구성 (현재는 테스트 프리셋 사용)
            List<int> cardUids = new List<int>();
            cardUids.Clear();
            cardUids = _tcgSettings.enemyDeckPreset.BuildDeckUids(tableTcgCards);

            Dictionary<int, int> cardList = new Dictionary<int, int>();
            cardList.Clear();
            foreach (int uid in cardUids)
            {
                cardList.TryAdd(uid, 1);
            }

            List<TcgBattleDataCardInHand> runtimeCardList =
                TcgBattleDataDeckBuilder.BuildRuntimeDeckCardList(cardList);

            var shuffleContext = BuildShuffleContext(ConfigCommonTcg.TcgPlayerSide.Enemy, seed, runtimeCardList.Count);
            var deckRuntime = new TcgBattleDataDeck<TcgBattleDataCardInHand>(shuffleContext);

            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();

            LogShuffledDeckForDebug(deckRuntime, "Enemy", seed);

            int heroCardUid = 0;
            if (_tcgSettings.enemyDeckPreset.heroCardUid > 0)
            {
                heroCardUid = _tcgSettings.enemyDeckPreset.heroCardUid;
            }
            else if (_tcgSettings.enemyDeckPreset.heroCardUids.Count > 0)
            {
                heroCardUid = _tcgSettings.enemyDeckPreset.heroCardUids[
                    Random.Range(0, _tcgSettings.enemyDeckPreset.heroCardUids.Count)];
            }

            var heroCard = TcgBattleDataDeckBuilder.BuildRuntimeHeroCard(heroCardUid);
            deckRuntime.SetHeroCard(heroCard);

            return deckRuntime;
        }

        /// <summary>
        /// 셔플에 사용할 <see cref="SeedManager"/>의 시드 상태를 갱신하고 적용합니다.
        /// </summary>
        /// <param name="seed">고정 시드 값입니다(0 이하이면 고정 시드를 사용하지 않습니다).</param>
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
        /// 셔플에 사용할 컨텍스트(Seed, 정책, 시작/드로우 규칙 등)를 생성합니다.
        /// </summary>
        /// <param name="side">셔플 정책을 조회할 플레이어 사이드입니다.</param>
        /// <param name="seed">셔플에 사용할 시드입니다(0 이하이면 고정 시드를 사용하지 않습니다).</param>
        /// <param name="deckSize">덱 카드 수입니다(셔플 정책 설정 구성에 사용).</param>
        /// <returns>셔플 모드와 설정이 포함된 <see cref="ShuffleMetaData"/>입니다.</returns>
        /// <remarks>
        /// 우선순위:
        /// 1) 설정에서 모드별 에셋이 제공되고 <c>ITcgShuffleSettingsAsset</c>을 구현한 경우: 에셋이 구성한 설정 사용
        /// 2) 모드가 None인 경우: 기본 <see cref="ShuffleConfig"/> 사용
        /// 3) 그 외: 설정 에셋이 없으면 의미가 약하므로 PureRandom으로 안전 폴백
        /// </remarks>
        private ShuffleMetaData BuildShuffleContext(ConfigCommonTcg.TcgPlayerSide side, int seed, int deckSize)
        {
            UpdateSeedManager(seed);

            // Settings(SO) 기반 정책 우선
            var (mode, settingsAsset) = _tcgSettings.GetShufflePolicy(side);

            // 1) 모드별 설정 에셋이 제공되었고, ITcgShuffleSettingsAsset을 구현한 경우
            if (settingsAsset is ITcgShuffleSettingsAsset builder)
            {
                var config = builder.BuildShuffleConfig(
                    deckSize: deckSize,
                    startMana: _tcgSettings.countManaBattleStart,
                    maxMana: _tcgSettings.countMaxManaInBattle,
                    manaPerTurn: _tcgSettings.countManaAfterTurn,
                    initialDrawCount: _tcgSettings.startingHandCardCount,
                    drawPerTurn: _tcgSettings.cardsDrawnPerTurn);

                return new ShuffleMetaData(mode, _seedManager, config);
            }

            if (mode == ConfigCommonTcg.ShuffleMode.None)
                return new ShuffleMetaData(mode, _seedManager, new ShuffleConfig());

            // 설정 에셋 없이는 의미가 약하므로 PureRandom으로 안전 폴백
            return new ShuffleMetaData(ConfigCommonTcg.ShuffleMode.PureRandom, _seedManager, new ShuffleConfig());
        }

        /// <summary>
        /// 디버그(에디터) 환경에서 셔플된 덱 정보를 로그로 출력합니다.
        /// </summary>
        /// <param name="deckRuntime">셔플이 완료된 덱 런타임입니다.</param>
        /// <param name="label">로그 구분용 라벨(예: Player/Enemy)입니다.</param>
        /// <param name="seed">요청된 시드 값입니다(로그 표시용).</param>
        private void LogShuffledDeckForDebug(TcgBattleDataDeck<TcgBattleDataCardInHand> deckRuntime, string label, int seed)
        {
#if UNITY_EDITOR
            if (_tcgSettings.showShuffleInfo)
            {
                // 실제 사용된 시드(고정 시드/랜덤 시드 포함)를 기록합니다.
                GcLogger.Log($"seed: {_seedManager.LastUsedSeed}");
            }

            if (!_tcgSettings.showDeckInfo) return;
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
