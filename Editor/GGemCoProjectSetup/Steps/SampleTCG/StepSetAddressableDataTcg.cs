using GGemCo2DCoreEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public class StepSetAddressableDataTcg : SetupStepBase
    {
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            msg = null;
            return true;
        }
        public override void Execute(EditorSetupContext ctx)
        {
            var addressableEditor = ScriptableObject.CreateInstance<AddressableEditor>();
            var addressableEditorTcg = ScriptableObject.CreateInstance<AddressableEditorTcg>();
            
            // settings 스크립터블 오브젝트, 테이블은 StepSetDefaultAddressableData 클래스에서 처리
            
            var settingTcgCard = new SettingTcgCard(addressableEditorTcg);
            settingTcgCard.Setup(ctx);
            
            // window 테이블 다시 등록
            var settingTable = new SettingTable(addressableEditor);
            settingTable.Setup(ctx);
        }
    }
}