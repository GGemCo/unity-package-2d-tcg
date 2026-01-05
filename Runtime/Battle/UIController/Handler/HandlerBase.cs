using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class HandlerBase
    {
        protected const int EffectUidHit = 1001;
        protected const int EffectUidHeal = 1002;
        protected const int EffectUidBuffAttack = 1003;
        protected const int EffectUidBuffHealth = 1004;
        protected const int EffectUidDeBuffAttack = 1005;
        protected const int EffectUidDeBuffHealth = 1006;

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
    }
}