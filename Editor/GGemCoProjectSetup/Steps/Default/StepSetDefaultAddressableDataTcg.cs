using GGemCo2DCoreEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public class StepSetDefaultAddressableDataTcg : SetupStepBase
    {
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            msg = null;
            return true;
        }
        public override void Execute(EditorSetupContext ctx)
        {
            var addressableEditor = ScriptableObject.CreateInstance<AddressableEditorTcg>();
            // settings 스크립터블 오브젝트
            var settingScriptableObject = new SettingScriptableObjectTcg(addressableEditor);
            settingScriptableObject.Setup(ctx);
            
            // 테이블
            var settingTable = new SettingTableTcg(addressableEditor);
            settingTable.Setup(ctx);
        }
    }
}