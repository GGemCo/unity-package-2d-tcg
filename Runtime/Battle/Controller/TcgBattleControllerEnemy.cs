using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 기본 규칙 기반 AI 컨트롤러 예시.
    /// - 실제 로직은 추후 점수 평가/프로파일과 함께 확장 가능합니다.
    /// </summary>
    public sealed class TcgBattleControllerEnemy : TcgBattleControllerBase, ITcgPlayerController
    {
        public ConfigCommonTcg.TcgPlayerSide Side { get; }
        public ConfigCommonTcg.TcgPlayerKind Kind { get; }

        private UIWindowTcgHandEnemy _uiWindowTcgHandEnemy;
        
        private IReadOnlyDictionary<int, StruckTableTcgCard> _tableTcgCards;
        private List<int> _cardUids = new List<int>();
        private readonly Dictionary<int, int> _cardList = new Dictionary<int, int>();

        public TcgBattleControllerEnemy(ConfigCommonTcg.TcgPlayerSide side, ConfigCommonTcg.TcgPlayerKind kind = ConfigCommonTcg.TcgPlayerKind.AiEasy)
        {
            Side = side;
            Kind = kind;
        }
        public override TcgBattleDataSide Initialize(TcgBattleManager tcgBattleManager)
        {
            base.Initialize(tcgBattleManager);
            
            _uiWindowTcgHandEnemy = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgHandEnemy>(UIWindowConstants.WindowUid.TcgHandEnemy);
            
            TcgBattleDataDeck<TcgBattleDataCard> deckRuntime = BuildEnemyDeckRuntime();

            InitializeSideState(Side, deckRuntime);
            
            // 대결 시작 시 처음 드로우하는 카드
            _uiWindowTcgHandEnemy.SetController(this);
            _uiWindowTcgHandEnemy.RefreshHand(battleDataSide.Hand);
            return battleDataSide;
        }

        /// <summary>
        /// AI용 덱을 구성하는 부분.
        /// 현재 구조에서는 임시로 플레이어 덱을 복사하거나,
        /// 별도의 세이브/테이블을 통해 생성하도록 확장할 수 있습니다.
        /// </summary>
        private TcgBattleDataDeck<TcgBattleDataCard> BuildEnemyDeckRuntime()
        {
            var shuffleContext = BuildShuffleContext();
            
            // TableTcgCard에서 전체 카드 리스트 가져오기
            _tableTcgCards ??= TableLoaderManagerTcg.Instance.TableTcgCard.GetAll();

            // AI 덱 구성 (Random은 외부 주입 or 내부 생성)
            _cardUids.Clear();
            _cardUids = AddressableLoaderSettingsTcg.Instance.tcgSettings.testDeckPreset.BuildDeckUids(_tableTcgCards);

            _cardList.Clear();
            foreach (int uid in _cardUids)
            {
                _cardList.TryAdd(uid, 1);
            }
            List<TcgBattleDataCard> runtimeCardList = TcgBattleDataDeckBuilder.BuildRuntimeDeck(_cardList);

            var enemyDeckRuntime = new TcgBattleDataDeck<TcgBattleDataCard>(shuffleContext);
            enemyDeckRuntime.SetCards(runtimeCardList);
            enemyDeckRuntime.Shuffle();
            return enemyDeckRuntime;
        }
        #region Shuffle

        private void UpdateSeedManager()
        {
            seedManager.SetFixedSeed(null);
            seedManager.ApplySeed();
        }
        /// <summary>
        /// 설정값과 테스트 시드를 기반으로 셔플 메타데이터를 생성합니다.
        /// - 테스트 시드가 0보다 크면 고정 시드 사용
        /// - costWeights 설정이 있으면 Weighted 모드, 없으면 PureRandom 모드
        /// </summary>
        private ShuffleMetaData BuildShuffleContext()
        {
            // 1) 시드 설정
            UpdateSeedManager();
            
            // 2) 셔플 모드 및 설정
            var shuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;
            var shuffleConfig = new ShuffleConfig();

            int[] costWeights = tcgSettings.costWeights;

            // costWeights 에 값이 정의되어 있으면 Weighted 모드로 전환
            if (costWeights == null || costWeights.Length <= 0)
                return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
            
            shuffleMode = ConfigCommonTcg.ShuffleMode.Weighted;

            shuffleConfig.FrontLoadedCount = 10;

            if (shuffleConfig.CostWeights == null || shuffleConfig.CostWeights.Count <= 0)
                return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
            int length = Mathf.Min(costWeights.Length, shuffleConfig.CostWeights.Count);
            for (int i = 0; i < length; i++)
            {
                if (costWeights[i] <= 0) continue;
                shuffleConfig.CostWeights[i] = costWeights[i];
            }

            return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
        }

        #endregion
        
        public void DecideTurnActions()
        {
            // 방어 코드
            // if (outCommands == null)
            //     return;
            //
            // outCommands.Clear();
            
            // 1) 낼 수 있는 카드 중 코스트가 맞는 카드 찾아서 1장 사용(예시)
            foreach (var card in battleDataSide.Hand)
            {
                if (card.Cost <= battleDataSide.CurrentMana)
                {
                    // outCommands.Add(TcgBattleCommand.PlayCard(Side, card));
                    var cmd = TcgBattleCommand.PlayCard(Side, card);
                    battleManager.ExecuteCommand(cmd);
                    // 명령 실행 후 UI 리프레시
                    _uiWindowTcgHandEnemy.RefreshHand(battleDataSide.Hand);
                    break;
                }
            }
            
            // 2) 필드에 유닛이 있으면, 적 유닛/영웅 공격 명령 추가 (아주 단순한 예시)
            // foreach (var myUnit in _me.Board)
            // {
            //     if (!myUnit.CanAttack)
            //         continue;
            //
            //     if (_opponent.Board.Count > 0)
            //     {
            //         // 가장 체력이 낮은 유닛을 대상으로 공격
            //         TcgUnitRuntime lowHpTarget = null;
            //         int minHp = int.MaxValue;
            //         foreach (var enemy in _opponent.Board)
            //         {
            //             if (enemy.Hp < minHp)
            //             {
            //                 minHp = enemy.Hp;
            //                 lowHpTarget = enemy;
            //             }
            //         }
            //
            //         if (lowHpTarget != null)
            //         {
            //             outCommands.Add(
            //                 TcgBattleCommand.AttackUnit(Side, myUnit, lowHpTarget));
            //         }
            //     }
            //     else
            //     {
            //         outCommands.Add(
            //             TcgBattleCommand.AttackHero(Side, myUnit));
            //     }
            // }
            //
            // // 3) 마지막에 턴 종료 명령 추가
            // outCommands.Add(TcgBattleCommand.EndTurn(Side));
        }

        /// <summary>
        /// todo. 정리 필요. enemy쪽도 마우스 클릭으로 처리하는 테스트용
        /// </summary>
        /// <param name="tcgBattleDataCard"></param>
        public void OnUiRequestPlayCard(TcgBattleDataCard tcgBattleDataCard)
        {
            var cmd = TcgBattleCommand.PlayCard(Side, tcgBattleDataCard);
            battleManager.ExecuteCommand(cmd);
            // 명령 실행 후 UI 리프레시
            _uiWindowTcgHandEnemy.RefreshHand(battleDataSide.Hand);
        }
    }
}
