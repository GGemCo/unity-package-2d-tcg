using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 버프(공격/체력 변경) 연출 핸들러.
    /// 
    /// 현재 구현은 숫자 팝업만 제공하며,
    /// 추후:
    /// - 버프 아이콘 표시
    /// - 이펙트 프리팹 분기
    /// - 스탯 변화 애니메이션(스케일/글로우)
    /// 등을 연결하기 쉬운 형태로 구성합니다.
    /// </summary>
    public sealed class HandlerAbilityBuff : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.ApplyBuff;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
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
            
            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            Vector3 targetPos = defenderIcon.transform.position;

            var iconTr = attackerIcon.transform;

            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);
            
            // 슬롯은 안보이게 
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);

            if (step.Payload is not TcgAbilityPayloadBuff payload) yield break;

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
