using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 프리젠테이션 핸들러들이 공통으로 사용하는 연출 유틸리티(이펙트/숫자 팝업 표시)를 제공하는 기반 클래스.
    /// </summary>
    /// <remarks>
    /// SceneGame 싱글톤과 하위 매니저(EffectManager, damageTextManager, canvasUI)가 준비되지 않은 경우
    /// 안전하게 no-op으로 종료한다.
    /// </remarks>
    public class HandlerBase
    {
        /// <summary>히트(피해) 이펙트 UID.</summary>
        protected const int EffectUidHit = 1001;

        /// <summary>회복/긍정 효과 계열 이펙트 UID.</summary>
        protected const int EffectUidHeal = 1002;

        /// <summary>공격력 버프 이펙트 UID.</summary>
        protected const int EffectUidBuffAttack = 1003;

        /// <summary>체력 버프 이펙트 UID.</summary>
        protected const int EffectUidBuffHealth = 1004;

        /// <summary>공격력 디버프 이펙트 UID.</summary>
        protected const int EffectUidDeBuffAttack = 1005;

        /// <summary>체력 디버프 이펙트 UID.</summary>
        protected const int EffectUidDeBuffHealth = 1006;

        /// <summary>
        /// 지정한 아이콘 위치에 이펙트를 생성하여 표시한다.
        /// </summary>
        /// <param name="icon">이펙트를 붙일 기준 아이콘.</param>
        /// <param name="effectUid">생성할 이펙트 UID.</param>
        /// <remarks>
        /// 이펙트는 <c>SceneGame.Instance.canvasUI</c> 하위로 추가되며,
        /// 위치는 <paramref name="icon"/>의 월드 좌표를 사용한다.
        /// </remarks>
        protected void ShowEffect(UIIcon icon, int effectUid)
        {
            if (icon == null) return;
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.EffectManager == null) return;
            if (SceneGame.Instance.canvasUI == null) return;

            var effect = SceneGame.Instance.EffectManager.CreateEffect(effectUid);
            effect.gameObject.transform.SetParent(SceneGame.Instance.canvasUI.transform, false);
            effect.transform.position = icon.gameObject.transform.position;
        }

        /// <summary>
        /// 지정한 아이콘 위치에 피해/회복 숫자 팝업을 표시한다.
        /// </summary>
        /// <param name="icon">팝업을 표시할 기준 아이콘.</param>
        /// <param name="damageValue">
        /// 표시할 값. 0이면 표시하지 않으며, 양수는 초록(긍정), 음수는 빨강(부정)으로 표시된다.
        /// </param>
        /// <remarks>
        /// 표시 색상 규칙은 값의 부호를 기준으로 한다(양수: 초록, 음수: 빨강).
        /// </remarks>
        protected void ShowDamageText(UIIcon icon, int damageValue)
        {
            if (icon == null) return;
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.damageTextManager == null) return;
            if (damageValue == 0) return;

            var color = Color.red;
            if (damageValue > 0)
                color = Color.green;

            var metadata = new MetadataDamageText
            {
                Damage = damageValue,
                Color = color,
                WorldPosition = icon.gameObject.transform.position,
                FontSize = 50
            };

            SceneGame.Instance.damageTextManager.ShowDamageText(metadata);
        }

        /// <summary>
        /// 지정한 아이콘 위치에 버프/디버프 수치 팝업을 표시한다.
        /// </summary>
        /// <param name="icon">팝업을 표시할 기준 아이콘.</param>
        /// <param name="value">
        /// 표시할 값. 0이면 표시하지 않으며, 양수는 초록(버프), 음수는 빨강(디버프)으로 표시된다.
        /// </param>
        /// <remarks>
        /// 내부적으로는 데미지 텍스트 시스템(<c>damageTextManager</c>)을 재사용한다.
        /// </remarks>
        protected void ShowBuffText(UIIcon icon, int value)
        {
            if (icon == null) return;
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.damageTextManager == null) return;
            if (value == 0) return;

            var color = Color.red;
            if (value > 0)
                color = Color.green;

            var metadata = new MetadataDamageText
            {
                Damage = value,
                Color = color,
                WorldPosition = icon.gameObject.transform.position,
                FontSize = 50
            };

            SceneGame.Instance.damageTextManager.ShowDamageText(metadata);
        }

        /// <summary>
        /// UIIcon이 없는 지점(버튼/컨테이너 위치 등)에 숫자 팝업을 표시한다.
        /// </summary>
        /// <param name="worldPosition">팝업을 표시할 월드 좌표.</param>
        /// <param name="value">
        /// 표시할 값. 0이면 표시하지 않으며, 양수는 초록, 음수는 빨강으로 표시된다.
        /// </param>
        /// <remarks>
        /// 아이콘이 아닌 임의 위치에서도 동일한 텍스트 시스템을 통해 수치를 표시할 수 있도록 제공한다.
        /// </remarks>
        protected void ShowValueTextAt(Vector3 worldPosition, int value)
        {
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.damageTextManager == null) return;
            if (value == 0) return;

            var color = Color.red;
            if (value > 0)
                color = Color.green;

            var metadata = new MetadataDamageText
            {
                Damage = value,
                Color = color,
                WorldPosition = worldPosition,
                FontSize = 50
            };

            SceneGame.Instance.damageTextManager.ShowDamageText(metadata);
        }
    }
}
