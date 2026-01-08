using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 버프(공격력/체력 변화) 연출을 처리하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 현재는 숫자 팝업과 간단한 이펙트만 표시한다.
    /// 추후 버프 아이콘, 프리팹 분기, 스탯 변화 애니메이션(스케일/글로우) 등을 확장하기 쉽도록 구성한다.
    /// </remarks>
    public sealed class HandlerAbilityBuff : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.ApplyBuff;

        /// <summary>
        /// 버프 적용 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">UI 루트 및 슬롯/아이콘 조회에 필요한 프리젠테이션 컨텍스트.</param>
        /// <param name="step">공격/대상 위치 및 페이로드(버프 종류/수치)를 포함한 스텝 정보.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 필요한 UI 오브젝트를 찾지 못하면 안전하게 조기 종료한다.
        /// 페이로드 타입이 버프 페이로드가 아니면 연출을 수행하지 않는다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            // 공격자/대상 위치 정보(현재 구현에서는 공격자 측 이동 연출의 기반으로 사용)
            var attackerSide = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            var defenderWindow = ctx.GetUIWindow(defenderZone);
            if (attackerWindow == null || defenderWindow == null) yield break;

            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);

            UISlot defenderSlot = defenderWindow.GetSlotByIndex(defenderIndex);
            UIIcon defenderIcon = defenderWindow.GetIconByIndex(defenderIndex);

            // 아이콘/슬롯이 아직 준비되지 않은 프레임(로드/갱신 타이밍)일 수 있어 짧게 대기 후 종료
            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 대상 위치(현재 구현에서는 참고용으로만 계산됨)
            Vector3 targetPos = defenderIcon.transform.position;

            var iconTr = attackerIcon.transform;

            // 이동/연출 중 캔버스 정렬(소팅/레이아웃) 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

            // 공격자 슬롯은 즉시 숨김 처리(아이콘만 분리되어 연출되는 형태를 전제)
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);

            // 버프 페이로드가 아니면 처리하지 않는다.
            if (step.Payload is not TcgAbilityPayloadBuff payload) yield break;

            // 공격력/체력 버프에 따라 텍스트 및 이펙트를 분기한다.
            if (payload.Type == TcgAbilityConstants.TcgAbilityType.BuffAttack)
            {
                ShowBuffText(defenderIcon, payload.BuffValue);

                if (payload.BuffValue > 0)
                    ShowEffect(defenderIcon, EffectUidBuffAttack);
                else if (payload.BuffValue < 0)
                    ShowEffect(defenderIcon, EffectUidDeBuffAttack);
            }
            else if (payload.Type == TcgAbilityConstants.TcgAbilityType.BuffHealth)
            {
                ShowBuffText(defenderIcon, payload.BuffValue);

                if (payload.BuffValue > 0)
                    ShowEffect(defenderIcon, EffectUidBuffHealth);
                else if (payload.BuffValue < 0)
                    ShowEffect(defenderIcon, EffectUidDeBuffHealth);
            }
        }
    }
}
