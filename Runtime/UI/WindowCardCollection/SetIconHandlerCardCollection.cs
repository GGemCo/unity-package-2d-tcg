using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 콜랙션 윈도우 - 아이콘 관리
    /// </summary>
    public class SetIconHandlerCardCollection : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            UIWindowCardCollection uiWindowCardCollection = window as UIWindowCardCollection;
            if (uiWindowCardCollection == null) return;
            // var skillData = SceneGame.Instance.saveDataManager.Skill;
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon != null)
            {
                // skillData.SetSkill(slotIndex, iconUid, iconCount, iconLevel, isLearned);
                // UIElementSkill uiElementSkill = uiWindowSkill.GetElementSkillByIndex(slotIndex);
                // if (uiElementSkill != null)
                // {
                //     UIIconSkill uiIconSkill = icon.GetComponent<UIIconSkill>();
                //     uiElementSkill.UpdateInfos(uiIconSkill.GetTableInfo(), uiIconSkill.GetSaveDataInfo());
                // }
            }
            
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
        }
    }
}