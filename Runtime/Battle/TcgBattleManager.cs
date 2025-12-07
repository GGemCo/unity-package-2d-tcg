using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투 시작을 관리하는 메인 엔트리 클래스.
    /// - 저장된 덱/플레이어 데이터 검증
    /// - 셔플 컨텍스트 생성
    /// - 런타임 덱 생성 및 셔플
    /// - UI 윈도우 초기화
    /// </summary>
    public class TcgBattleManager
    {
        public Action onExecuteCommand;
        
        private UIWindowTcgFieldEnemy _uiWindowTcgFieldEnemy;
        private UIWindowTcgFieldPlayer _uiWindowTcgFieldPlayer;
        private UIWindowTcgBattleHud _uiWindowTcgBattleHud;

        // === 컨트롤러 추가 ===
        private TcgBattleControllerPlayer _battleControllerPlayer;
        private TcgBattleControllerEnemy _battleControllerEnemy;

        // 현재 턴 진행 여부
        private bool _isPlayerTurn;
        private int _turnNumber;
        
        /// <summary>
        /// TCG 패키지 매니저로부터 필수 의존성을 주입합니다.
        /// </summary>
        /// <param name="packageManager">TCG 패키지 매니저 인스턴스.</param>
        public void Initialize(TcgPackageManager packageManager)
        {
        }

        /// <summary>
        /// 저장된 정보(기본 덱)를 기반으로 전투를 시작합니다.
        /// </summary>
        public void StartBattle()
        {
            // todo. 대결 시작 단계 적용하기. 로딩(대결 준비) 효과가 필요할 수 있음
            
            // 1. 필드 윈도우 준비
            if (!EnsureBattleWindows())
                return;
            
            // 2. 컨트롤러 초기화
            SetupBattleRuntime();
            
            // 3. 필드 윈도우 표시
            ShowWindows(true);
        }

        #region UI Windows

        /// <summary>
        /// 전투에 필요한 UI 윈도우(필드, 내 패)를 확보합니다.
        /// 없으면 로그를 남기고 false 를 반환합니다.
        /// </summary>
        private bool EnsureBattleWindows()
        {
            var windowManager = SceneGame.Instance?.uIWindowManager;
            if (windowManager == null)
            {
                GcLogger.LogError("SceneGame 또는 UIWindowManager 가 없습니다. 전투 UI를 열 수 없습니다.");
                return false;
            }

            if (_uiWindowTcgFieldEnemy == null)
            {
                _uiWindowTcgFieldEnemy = windowManager.GetUIWindowByUid<UIWindowTcgFieldEnemy>(UIWindowConstants.WindowUid.TcgFieldEnemy);
            }

            if (_uiWindowTcgFieldEnemy == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgFieldEnemy)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            if (_uiWindowTcgFieldPlayer == null)
            {
                _uiWindowTcgFieldPlayer = windowManager.GetUIWindowByUid<UIWindowTcgFieldPlayer>(UIWindowConstants.WindowUid.TcgFieldPlayer);
            }

            if (_uiWindowTcgFieldPlayer == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgFieldPlayer)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            if (_uiWindowTcgBattleHud == null)
            {
                _uiWindowTcgBattleHud = windowManager.GetUIWindowByUid<UIWindowTcgBattleHud>(UIWindowConstants.WindowUid.TcgBattleHud);
            }

            if (_uiWindowTcgBattleHud == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgBattleHud)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// UI 윈도우를 표시합니다.
        /// </summary>
        private void ShowWindows(bool show)
        {
            _uiWindowTcgFieldEnemy?.Show(show);
            _uiWindowTcgFieldPlayer?.Show(show);
            _uiWindowTcgBattleHud?.Show(show);
        }
        #endregion
        
        /// <summary>
        /// 플레이어/적 측 런타임 상태와 전투 컨텍스트를 생성하고,
        /// 사람/AI 컨트롤러를 초기화합니다.
        /// </summary>
        private void SetupBattleRuntime()
        {
            // 컨트롤러 생성
            var human = new TcgBattleControllerPlayer(ConfigCommonTcg.TcgPlayerSide.Player);
            var ai    = new TcgBattleControllerEnemy(ConfigCommonTcg.TcgPlayerSide.Enemy, ConfigCommonTcg.TcgPlayerKind.AiEasy);

            _battleControllerPlayer = human;
            _battleControllerEnemy  = ai;

            _battleControllerPlayer.Initialize(this);
            _battleControllerEnemy.Initialize(this);
            
            // 플레이어 턴 시작
            _isPlayerTurn = true;
            _uiWindowTcgFieldPlayer.SetBattleManager(this, _battleControllerPlayer);
            _uiWindowTcgFieldEnemy.SetBattleManager(this, _battleControllerEnemy);
        }

        // /// <summary>
        // /// 한 턴에 대한 명령 목록을 순서대로 실행합니다.
        // /// </summary>
        // private void ExecuteCommands(List<TcgBattleCommand> commands)
        // {
        //     if (commands == null || commands.Count == 0)
        //         return;
        //
        //     foreach (var cmd in commands)
        //     {
        //         ExecuteCommand(cmd);
        //
        //         // TODO: 연출을 위해 코루틴으로 바꾸고,
        //         //       명령당 대기/애니메이션을 넣고 싶다면 여기서 처리.
        //     }
        // }

        private TcgBattleDataSide GetSideState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? _battleControllerPlayer.GetBattleDataSide() : _battleControllerEnemy.GetBattleDataSide();
        }

        private TcgBattleDataSide GetOpponentState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? _battleControllerEnemy.GetBattleDataSide() : _battleControllerPlayer.GetBattleDataSide();
        }
        /// <summary>
        /// 단일 명령을 실제 전투 상태/체력/마나/보드/UI 에 반영합니다.
        /// </summary>
        public void ExecuteCommand(TcgBattleCommand cmd)
        {
            if (cmd == null)
                return;

            var actor    = GetSideState(cmd.Side);
            var opponent = GetOpponentState(cmd.Side);
            
            switch (cmd.CommandType)
            {
                case ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand:
                    ExecutePlayCard(actor, opponent, cmd);
                    break;

                case ConfigCommonTcg.TcgBattleCommandType.AttackUnit:
                    ExecuteAttackUnit(actor, opponent, cmd);
                    break;

                case ConfigCommonTcg.TcgBattleCommandType.AttackHero:
                    ExecuteAttackHero(actor, opponent, cmd);
                    break;

                case ConfigCommonTcg.TcgBattleCommandType.EndTurn:
                    ExecuteEndTurn(cmd.Side);
                    break;
            }
            onExecuteCommand?.Invoke();
            
            _uiWindowTcgFieldEnemy.RefreshBoard();
            _uiWindowTcgFieldPlayer.RefreshBoard();
        }
        private void ExecutePlayCard(
            TcgBattleDataSide actor,
            TcgBattleDataSide opponent,
            TcgBattleCommand cmd)
        {
            var card = cmd.tcgBattleDataCard;
            if (card == null)
                return;

            // 마나 차감
            if (!actor.TryConsumeMana(card.Cost)) return;
            // 손에서 제거
            if (!actor.RemoveCardFromHand(card)) return;

            // 카드 타입에 따라 분기 (예시)
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    // 1) 유닛 런타임 생성
                    var unit = CreateUnitFromCard(actor.Side, card);
                    if (unit != null)
                    {
                        actor.AddUnitToBoard(unit);
                    }

                    // 2) "소환 시 발동" 이펙트가 있다면 실행
                    if (card.SummonEffects != null && card.SummonEffects.Count > 0)
                    {
                        EffectRunner.RunEffects(
                            actor,
                            opponent,
                            card,
                            card.SummonEffects,
                            explicitTargetBattleData: null /* 필요 시 타겟 전달 */);
                    }
                    break;
                }

                case CardConstants.Type.Spell:
                {
                    // 스펠은 필드에 남지 않고, 이펙트만 실행
                    if (card.SpellEffects != null && card.SpellEffects.Count > 0)
                    {
                        // TODO: TargetType 에 따라 타겟 선택 로직 추가
                        EffectRunner.RunEffects(
                            actor,
                            opponent,
                            card,
                            card.SpellEffects,
                            explicitTargetBattleData: null);
                    }
                    break;
                }

                default:
                {
                    // 기타 타입(장비/영속물 등)은 추후 확장
                    break;
                }
            }
        }

        private void ExecuteAttackUnit(
            TcgBattleDataSide actor,
            TcgBattleDataSide opponent,
            TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            var target   = cmd.targetBattleData;

            if (attacker == null || target == null)
                return;

            if (!actor.ContainsOnBoard(attacker))
                return;

            if (!opponent.ContainsOnBoard(target))
                return;

            if (!attacker.CanAttack)
                return;

            // 양쪽에 데미지 적용
            target.ModifyAttack(-attacker.Attack);
            attacker.ModifyAttack(-target.Attack);

            attacker.CanAttack = false;

            // 사망 처리
            if (target.Hp <= 0)
                opponent.RemoveUnitFromBoard(target);

            if (attacker.Hp <= 0)
                actor.RemoveUnitFromBoard(attacker);
        }
        private void ExecuteAttackHero(
            TcgBattleDataSide actor,
            TcgBattleDataSide opponent,
            TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            if (attacker == null)
                return;

            if (!actor.ContainsOnBoard(attacker))
                return;

            if (!attacker.CanAttack)
                return;

            opponent.TakeHeroDamage(attacker.Attack);
            attacker.CanAttack = false;

            // TODO: 영웅 HP 0 이하이면 전투 종료 처리
            if (opponent.HeroHp <= 0)
            {
                OnBattleEnd(actor.Side);
            }
        }

        private void OnBattleEnd(ConfigCommonTcg.TcgPlayerSide actorSide)
        {
        }

        private void ExecuteEndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            // 턴 수 증가/액티브 변경
            if (side == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                _isPlayerTurn = false;
            }
            else
            {
                _isPlayerTurn = true;
                _turnNumber++;
            }

            // foreach (var unit in newActor.Board)
            // {
            //     unit.CanAttack = true;
            // }
            
            // 새 턴 시작 시 마나/공격 가능 여부 리셋
            if (_isPlayerTurn) return;
            
            _battleControllerPlayer.IncreaseMaxMana(1);
            _battleControllerEnemy.IncreaseMaxMana(1);
            // AI 턴이면 바로 AI 명령 결정/실행
            RunAiTurn();
        }
        private void RunAiTurn()
        {
            _battleControllerEnemy.DecideTurnActions();
        }
        /// <summary>
        /// Creature 타입 카드를 기반으로 필드에 소환할 유닛 런타임을 생성합니다.
        /// - 실제 스탯/키워드는 카드 테이블/런타임에서 가져와야 합니다.
        /// </summary>
        private TcgBattleDataFieldCard CreateUnitFromCard(
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            TcgBattleDataCard tcgBattleDataCard)
        {
            if (tcgBattleDataCard == null)
            {
                GcLogger.LogError("[Battle] CreateUnitFromCard: cardRuntime is null.");
                return null;
            }

            // 1) CardRuntime 에서 스탯/키워드 정보 가져오기
            //    (아래는 예시. 실제 필드 이름에 맞게 수정 필요)
            int attack = tcgBattleDataCard.Attack; // 예: CardRuntime.Attack
            int hp     = tcgBattleDataCard.Health; // 예: CardRuntime.Health

            // 키워드 예시: CardRuntime.Keywords 또는 테이블에서 변환
            List<ConfigCommonTcg.TcgKeyword> keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
            foreach (var kw in tcgBattleDataCard.Keywords) // 예: IEnumerable<TcgKeyword>
            {
                keywords.Add(kw);
            }

            // 2) 유닛 런타임 생성
            var unit = new TcgBattleDataFieldCard(
                tcgBattleDataCard.Uid,
                ownerSide,
                tcgBattleDataCard,
                attack,
                hp,
                keywords);

            // 소환 시점에는 공격 불가 (돌진 키워드가 있으면 예외)
            if (unit.HasKeyword(ConfigCommonTcg.TcgKeyword.Rush))
                unit.CanAttack = true;
            else
                unit.CanAttack = false;

            return unit;
        }
        /// <summary>
        /// 대결 강제 종료
        /// </summary>
        public void EndBattleForce()
        {
            ShowWindows(false);
        }
    }
}
