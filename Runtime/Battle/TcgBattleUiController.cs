using System.Collections;
using System.Collections.Generic;
using GGemCo2DCore;
using R3;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 세션과 전투 관련 UI 윈도우들을 연결/갱신/해제하는 역할을 하는 코디네이터.
    /// </summary>
    public sealed class TcgBattleUiController
    {
        private UIWindowTcgFieldEnemy  _fieldEnemy;
        private UIWindowTcgFieldPlayer _fieldPlayer;
        private UIWindowTcgHandPlayer  _handPlayer;
        private UIWindowTcgHandEnemy   _handEnemy;
        private UIWindowTcgBattleHud   _battleHud;
        private readonly CompositeDisposable _disposables = new();

        private TcgBattleSession _session;
        private Coroutine _presentationCoroutine;
        
        public bool IsReady =>
            _fieldEnemy  != null &&
            _fieldPlayer != null &&
            _handPlayer  != null &&
            _handEnemy   != null &&
            _battleHud   != null;

        /// <summary>
        /// SceneGame 의 UIWindowManager 에서 전투 관련 윈도우를 찾아와 보관합니다.
        /// </summary>
        public bool TrySetupWindows()
        {
            var windowManager = SceneGame.Instance?.uIWindowManager;
            if (windowManager == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleUiController)}] {nameof(UIWindowManager)} 를 찾을 수 없습니다.");
                return false;
            }

            _fieldEnemy  = GetWindow<UIWindowTcgFieldEnemy>(windowManager, UIWindowConstants.WindowUid.TcgFieldEnemy);
            _fieldPlayer = GetWindow<UIWindowTcgFieldPlayer>(windowManager, UIWindowConstants.WindowUid.TcgFieldPlayer);
            _handPlayer  = GetWindow<UIWindowTcgHandPlayer>(windowManager, UIWindowConstants.WindowUid.TcgHandPlayer);
            _handEnemy   = GetWindow<UIWindowTcgHandEnemy>(windowManager, UIWindowConstants.WindowUid.TcgHandEnemy);
            _battleHud   = GetWindow<UIWindowTcgBattleHud>(windowManager, UIWindowConstants.WindowUid.TcgBattleHud);

            if (IsReady) return true;
            
            GcLogger.LogError($"[{nameof(TcgBattleUiController)}] 전투 UI 윈도우 중 일부를 찾을 수 없습니다.");
            return false;

        }

        private static TWindow GetWindow<TWindow>(UIWindowManager windowManager, UIWindowConstants.WindowUid uid)
            where TWindow : UIWindow
        {
            return windowManager.GetUIWindowByUid<TWindow>(uid);
        }

        /// <summary>
        /// 윈도우 활성/비활성.
        /// </summary>
        public void ShowAll(bool isShow)
        {
            if (!IsReady)
                return;

            _fieldEnemy.Show(isShow);
            _fieldPlayer.Show(isShow);
            _handPlayer.Show(isShow);
            _handEnemy.Show(isShow);
            _battleHud.Show(isShow);
        }

        /// <summary>
        /// BattleManager 와 연동하여, 윈도우에 BattleManager/Side 정보를 바인딩합니다.
        /// </summary>
        public void BindBattleManager(TcgBattleManager manager, TcgBattleSession session)
        {
            if (!IsReady || manager == null || session == null)
                return;

            _session = session;
            // BindMana(session);

            _handPlayer.SetBattleManager(manager);
        }

        private void BeginPresentation()
        {
        }

        private void EndPresentation(TcgBattleDataMain context)
        {
            RefreshAll(context);
        }

        /// <summary>
        /// CommandTrace 목록을 기반으로 연출을 순차 재생한 뒤,
        /// 모든 UI를 최종 상태로 갱신합니다.
        /// </summary>
        public void PlayPresentationAndRefresh(TcgBattleDataMain context, IReadOnlyList<TcgBattleCommandTrace> traces)
        {
            if (!IsReady || context == null)
                return;

            // 코루틴 호스트는 MonoBehaviour여야 함 (UIWindow는 MonoBehaviour)
            if (_battleHud == null)
            {
                RefreshAll(context);
                return;
            }

            BeginPresentation();
            if (_presentationCoroutine != null)
            {
                _battleHud.StopCoroutine(_presentationCoroutine);
            }
            _presentationCoroutine = _battleHud.StartCoroutine(CoPlayPresentation(context, traces));
        }

        private IEnumerator CoPlayPresentation(TcgBattleDataMain context, IReadOnlyList<TcgBattleCommandTrace> traces)
        {
            if (traces != null)
            {
                foreach (var trace in traces)
                {
                    var result = trace.Result;
                    if (result == null || !result.Success) continue;
                    if (!result.HasPresentation) continue;

                    foreach (var step in result.PresentationSteps)
                    {
                        yield return PlayStep(step);
                        
                        // 연출 하나가 종료될 때마다, 게임 종료 체크
                        _session.TryCheckBattleEnd();
                        if (_session.IsBattleEnded)
                        {
                            _battleHud.StopCoroutine(_presentationCoroutine);
                            _presentationCoroutine = null;
                        }
                    }
                }
            }

            EndPresentation(context);
        }

        private IEnumerator PlayStep(TcgPresentationStep step)
        {
            switch (step.Type)
            {
                case TcgPresentationStepType.MoveCardHandToBoard:
                    // todo. 정리 필요. 파라미터를 step 넘기기
                    yield return CoSummonFromHandToBoard(step.Side, step.FromIndex, step.ToIndex, step.ValueA);
                    break;

                case TcgPresentationStepType.AttackUnit:
                    yield return CoAttackUnit(step);
                    break;

                case TcgPresentationStepType.AttackHero:
                    yield return CoAttackHero(step.Side, step.FromIndex);
                    break;

                case TcgPresentationStepType.DamagePopup:
                    // TODO: 피해 텍스트/이펙트 시스템이 준비되면 연결
                    yield return new WaitForSecondsRealtime(0.08f);
                    break;

                case TcgPresentationStepType.DeathFadeOut:
                    yield return CoDeathFadeOut(step.Side, step.ToIndex);
                    break;

                default:
                    yield return null;
                    break;
            }
        }

        private IEnumerator CoSummonFromHandToBoard(ConfigCommonTcg.TcgPlayerSide side, int handIndex, int boardIndex, int childCount)
        {
            var handWindow = GetHandWindow(side);
            var fieldWindow = GetFieldWindow(side);
            if (handWindow == null || fieldWindow == null)
                yield break;

            // Hand UI: 0번은 영웅, 실제 손패는 1번부터
            var icon = handWindow.GetIconByIndex(handIndex + 1);
            var destSlot = fieldWindow.GetSlotByIndex(boardIndex);
            var grid = fieldWindow.containerIcon;
            if (GridLayoutPositionUtility.TryGetCellTransformPosition(grid, boardIndex, childCount, out var pos))
            {
                // 카드/이펙트/프리뷰 등의 목표 위치로 사용
                // (같은 부모 RectTransform 기준으로 사용할 때 가장 정확합니다) 
            }

            if (icon == null || destSlot == null)
            {
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            // 아이콘 이동
            // GcLogger.Log($"move icon: {icon.transform.position.x}, {icon.transform.position.y} =>" +
            //              $"to {pos.x}, {pos.y}");
            yield return TcgUiTween.MoveTo(icon.transform, pos, fieldWindow.timeToMove);
            yield return new WaitForSecondsRealtime(0.05f);
        }

        private IEnumerator CoAttackUnit(TcgPresentationStep step)
        {
            ConfigCommonTcg.TcgPlayerSide attackerSide = step.Side;
            int attackerBoardIndex = step.FromIndex;
            int defenderBoardIndex = step.ToIndex;
            
            int attackerHp = step.ValueA;
            int targetHp = step.ValueB;
            // 공격자가 받은 데미지
            int attackerDamage = step.ValueC;
            // 타겟이 받은 데미지
            int targetDamage = step.ValueD;
            
            var attackerField = GetFieldWindow(attackerSide);
            var defenderField = GetFieldWindow(attackerSide == ConfigCommonTcg.TcgPlayerSide.Player
                ? ConfigCommonTcg.TcgPlayerSide.Enemy
                : ConfigCommonTcg.TcgPlayerSide.Player);

            if (attackerField == null || defenderField == null)
                yield break;

            var attackerSlot = attackerField.GetSlotByIndex(attackerBoardIndex);
            var attackerIcon = attackerField.GetIconByIndex(attackerBoardIndex);
            var defenderIcon = defenderField.GetIconByIndex(defenderBoardIndex);

            if (attackerIcon == null || defenderIcon == null)
            {
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }
            var defenderGrid = defenderField.containerIcon;
            int defenderChildCount = defenderField.GetActiveIconCount();
            if (GridLayoutPositionUtility.TryGetCellTransformPosition(defenderGrid, defenderBoardIndex, defenderChildCount, out var target))
            {
                // 카드/이펙트/프리뷰 등의 목표 위치로 사용
                // (같은 부모 RectTransform 기준으로 사용할 때 가장 정확합니다) 
            }

            // 이동 중 최상위에 보이게 하기위한 처리 
            attackerIcon.transform.SetParent(SceneGame.Instance.canvasUI.gameObject.transform);
            
            // 공격 하는 카드가 타겟 카드로 이동하는 처리. todo. 정리 필요. 좌표 값
            yield return TcgUiTween.MoveTo(attackerIcon.transform, target - new Vector3(20, 20), attackerField.timeToMove);
            
            // 공격 후 원래 슬롯 자리로 되돌리기
            attackerIcon.transform.SetParent(attackerSlot.transform);
            // 아이콘 위치도 초기화
            attackerIcon.transform.localPosition = Vector3.zero;
            
            // hp 업데이트, 데미지 표시. 원래 위치에서 데미지 텍스트를 표시
            UIIconCard uiIconFieldPlayer = attackerIcon as UIIconCard;
            if (uiIconFieldPlayer != null)
            {
                uiIconFieldPlayer.UpdateHealth(attackerHp, attackerDamage);
            }
            
            // 사망 처리 fade out
            if (attackerHp <= 0)
            {
                var cg = attackerIcon.GetComponent<CanvasGroup>();
                if (cg != null)
                    yield return TcgUiTween.FadeTo(cg, 0f, _fieldPlayer.timeToFadeOut);
            }
            
            // hp 업데이트, 데미지 표시
            UIIconCard uiIconFieldEnemy = defenderIcon as UIIconCard;
            if (uiIconFieldEnemy != null)
            {
                uiIconFieldEnemy.UpdateHealth(targetHp, targetDamage);
            }
            // 사망 처리 fade out
            if (targetHp <= 0)
            {
                var cg = defenderIcon.GetComponent<CanvasGroup>();
                if (cg != null)
                    yield return TcgUiTween.FadeTo(cg, 0f, _fieldEnemy.timeToFadeOut);
            }
        }

        private IEnumerator CoAttackHero(ConfigCommonTcg.TcgPlayerSide attackerSide, int attackerBoardIndex)
        {
            var attackerField = GetFieldWindow(attackerSide);
            var defenderHand = GetHandWindow(attackerSide == ConfigCommonTcg.TcgPlayerSide.Player
                ? ConfigCommonTcg.TcgPlayerSide.Enemy
                : ConfigCommonTcg.TcgPlayerSide.Player);

            if (attackerField == null || defenderHand == null)
                yield break;

            var attackerIcon = attackerField.GetIconByIndex(attackerBoardIndex);
            // Hero card는 Hand 0번 슬롯
            var heroIcon = defenderHand.GetIconByIndex(0);

            if (attackerIcon == null || heroIcon == null)
            {
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            var origin = attackerIcon.transform.position;
            var target = heroIcon.transform.position;

            yield return TcgUiTween.MoveTo(attackerIcon.transform, target, 0.12f);
            yield return new WaitForSecondsRealtime(0.03f);
            yield return TcgUiTween.MoveTo(attackerIcon.transform, origin, 0.12f);
        }

        private IEnumerator CoDeathFadeOut(ConfigCommonTcg.TcgPlayerSide side, int boardIndex)
        {
            var field = GetFieldWindow(side);
            if (field == null)
                yield break;

            var icon = field.GetIconByIndex(boardIndex);
            if (icon == null)
            {
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            // CanvasGroup이 있으면 알파 페이드, 없으면 비활성화
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                icon.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            yield return TcgUiTween.FadeTo(cg, 0f, 5.12f);
        }

        private UIWindowTcgHandBase GetHandWindow(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? _handPlayer : _handEnemy;
        }

        private UIWindowTcgFieldBase GetFieldWindow(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? _fieldPlayer : _fieldEnemy;
        }

        /// <summary>
        /// 현재 전투 데이터를 기준으로 모든 윈도우를 갱신합니다.
        /// </summary>
        public void RefreshAll(TcgBattleDataMain context)
        {
            if (!IsReady || context == null)
                return;

            var player = context.Player;
            var enemy  = context.Enemy;

            _handPlayer.RefreshHand(player);
            _handEnemy.RefreshHand(enemy);
            _fieldPlayer.RefreshBoard(player);
            _fieldEnemy.RefreshBoard(enemy);
            // _battleHud.Refresh(context);
            
            _handPlayer.SetMana(player.Mana.Current, player.Mana.Max);
            _handEnemy.SetMana(enemy.Mana.Current, enemy.Mana.Max);
        }

        /// <summary>
        /// 참조를 해제합니다. (씬 전환 등)
        /// </summary>
        public void Release()
        {
            _disposables.Clear(); // 또는 _disposables.Dispose();

            _fieldEnemy.Release();
            _fieldPlayer.Release();
            _handPlayer.Release();
            _handEnemy.Release();
            _battleHud.Release();
            
            _fieldEnemy  = null;
            _fieldPlayer = null;
            _handPlayer  = null;
            _handEnemy   = null;
            _battleHud   = null;
        }
    }
}
