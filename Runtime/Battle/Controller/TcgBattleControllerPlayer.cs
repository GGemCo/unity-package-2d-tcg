
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 실제 유저(사람)를 위한 컨트롤러.
    /// - UI에서 발생한 입력을 큐에 쌓아두고,
    ///   턴 종료 시점에 BattleManager가 한꺼번에 실행합니다.
    /// </summary>
    public sealed class TcgBattleControllerPlayer : TcgBattleControllerBase, ITcgPlayerController
    {
        public ConfigCommonTcg.TcgPlayerSide Side { get; }
        public ConfigCommonTcg.TcgPlayerKind Kind => ConfigCommonTcg.TcgPlayerKind.Human;
        
        private UIWindowTcgHandPlayer _uiWindowTcgHandPlayer;

        public TcgBattleControllerPlayer(ConfigCommonTcg.TcgPlayerSide side)
        {
            Side = side;
        }

        public override TcgBattleDataSide Initialize(TcgBattleManager tcgBattleManager)
        {
            base.Initialize(tcgBattleManager);
            
            _uiWindowTcgHandPlayer = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgHandPlayer>(UIWindowConstants.WindowUid.TcgHandPlayer);
            
            // 1. 기본 의존성 및 저장 데이터 검증
            if (!ValidateCoreDependencies())
                return null;

            var myDeckData = saveDataManagerTcg.MyDeck;
            if (!ValidateMyDeckData(myDeckData))
                return null;

            var playerDataTcg = saveDataManagerTcg.PlayerTcg;
            if (!ValidatePlayerData(playerDataTcg))
                return null;

            var deck = myDeckData.GetDeckInfoByIndex(playerDataTcg.defaultDeckIndex);
            if (!ValidateDeck(deck, playerDataTcg.defaultDeckIndex))
                return null;

            // 2. 셔플 컨텍스트 생성
            var shuffleContext = BuildShuffleContext();

            // 3. 런타임 덱 생성 및 셔플
            List<TcgBattleDataCard> runtimeCardList = TcgBattleDataDeckBuilder.BuildRuntimeDeck(deck.cardList);
            var deckRuntime = new TcgBattleDataDeck<TcgBattleDataCard>(shuffleContext);
            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();

            InitializeSideState(Side, deckRuntime);

#if UNITY_EDITOR
            // 에디터에서만 디버그 로그 출력
            LogShuffledDeckForDebug(deckRuntime);
#endif
            
            // 대결 시작 시 처음 드로우하는 카드
            _uiWindowTcgHandPlayer.SetController(this);
            _uiWindowTcgHandPlayer.RefreshHand(battleDataSide.Hand);
            return battleDataSide;
        }


        #region Shuffle

        /// <summary>
        /// 설정된 테스트 시드가 있으면 고정 시드를 사용
        /// </summary>
        private void UpdateSeedManager()
        {
            int serverSeed = tcgSettings.testSeed;

            seedManager.SetFixedSeed(null);
            if (serverSeed > 0)
            {
                seedManager.SetFixedSeed(serverSeed);
            }
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
        
        #region Validation
        /// <summary>
        /// 필수 필드(패키지 매니저, 세이브 매니저)가 모두 유효한지 검사합니다.
        /// </summary>
        private bool ValidateCoreDependencies()
        {
            if (packageManager == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleManager)} 가 초기화되지 않았습니다. {nameof(TcgPackageManager)} 가 null 입니다.");
                return false;
            }

            if (saveDataManagerTcg == null)
            {
                GcLogger.LogError($"{nameof(SaveDataManagerTcg)} 가 null 입니다. 저장 데이터를 사용할 수 없습니다.");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// MyDeckData가 존재하고, 최소 1개 이상의 덱이 있는지 검사합니다.
        /// </summary>
        private bool ValidateMyDeckData(MyDeckData myDeckData)
        {
            if (myDeckData == null)
            {
                GcLogger.LogError($"{nameof(MyDeckData)} 클래스가 없습니다.");
                return false;
            }

            if (myDeckData.GetCurrentCount() <= 0)
            {
                GcLogger.LogError("저장된 덱 데이터가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 플레이어 TCG 데이터가 유효한지 검사합니다.
        /// </summary>
        private bool ValidatePlayerData(PlayerDataTcg playerDataTcg)
        {
            if (playerDataTcg == null)
            {
                GcLogger.LogError($"{nameof(PlayerDataTcg)} 클래스가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 기본 덱 정보와 카드 리스트가 유효한지 검사합니다.
        /// </summary>
        private bool ValidateDeck(MyDeckSaveData deck, int deckIndex)
        {
            if (deck == null)
            {
                GcLogger.LogError($"저장된 덱 정보가 없습니다. index: {deckIndex}");
            }
            else if (deck.cardList == null || deck.cardList.Count == 0)
            {
                GcLogger.LogError($"덱 안에 카드 정보가 없습니다. index: {deckIndex}");
            }
            else
            {
                return true;
            }

            return false;
        }
        
        #endregion
        
        /// <summary>
        /// 이번 턴에 수행할 명령들을 턴 종료 시점에 BattleManager가 가져갈 때 사용.
        /// - 내부 큐를 모두 비우고 outCommands 로 옮깁니다.
        /// </summary>
        public void DecideTurnActions()
        {
            
        }
        
        /// <summary>
        /// UI에서 "카드 사용" 요청이 들어왔을 때 호출.
        /// </summary>
        public void OnUiRequestPlayCard(TcgBattleDataCard tcgBattleDataCard)
        {
            var cmd = TcgBattleCommand.PlayCard(Side, tcgBattleDataCard);
            battleManager.ExecuteCommand(cmd);
            // 명령 실행 후 UI 리프레시
            _uiWindowTcgHandPlayer.RefreshHand(battleDataSide.Hand);
        }

        /// <summary>
        /// UI에서 "유닛 공격" 요청이 들어왔을 때 호출.
        /// </summary>
        public void OnUiRequestAttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker,
            TcgBattleDataFieldCard target)
        {
            var cmd = TcgBattleCommand.AttackUnit(side, attacker, target);
            battleManager.ExecuteCommand(cmd);
        }

        public void OnUiRequestAttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker)
        {
            var cmd = TcgBattleCommand.AttackHero(side, attacker);
            battleManager.ExecuteCommand(cmd);
        }

        /// <summary>
        /// 턴 종료 버튼이 눌렸을 때 호출.
        /// - HumanController 큐를 비워 명령을 가져온 후,
        ///   ExecuteCommands 로 실제 처리.
        /// </summary>
        public void OnUiRequestEndTurn()
        {
            battleManager.ExecuteCommand(TcgBattleCommand.EndTurn(Side));
            
            // UI 토글 (내 턴에만 조작 가능하도록)
            _uiWindowTcgHandPlayer.SetInteractable(false);
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 에디터 환경에서 셔플된 덱 구성을 로그로 출력합니다.
        /// </summary>
        private void LogShuffledDeckForDebug(TcgBattleDataDeck<TcgBattleDataCard> tcgBattleDataDeck)
        {
            if (tcgBattleDataDeck == null)
                return;

            tcgBattleDataDeck.DebugCard();
        }
#endif
    }
}