using GGemCo2DCoreEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// Addressables 기본 데이터 구성을 에디터에서 초기화하는 설정 스텝입니다.
    /// 설정 ScriptableObject와 테이블(Table) 관련 Addressable 설정을 순차적으로 적용합니다.
    /// </summary>
    /// <remarks>
    /// 실제 구성 내용(그룹/라벨/경로/엔트리 생성 등)은 각 설정 클래스의 <c>Setup</c> 구현에 위임됩니다.
    /// </remarks>
    public class StepSetDefaultAddressableDataTcg : SetupStepBase
    {
        /// <summary>
        /// Addressables 기본 데이터 설정은 별도 사전 조건이 없으므로 항상 유효합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 설정 적용 및 로그 출력에 사용됩니다.
        /// </param>
        /// <param name="msg">
        /// 검증 메시지 출력용 파라미터로, 항상 <c>null</c>을 반환합니다.
        /// </param>
        /// <returns>
        /// 항상 <c>true</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            msg = null;
            return true;
        }

        /// <summary>
        /// Addressables 편집기를 생성하고, 기본 Addressable 설정 및 테이블 구성을 적용합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 설정 적용 및 로그 출력에 사용됩니다.
        /// </param>
        public override void Execute(EditorSetupContext ctx)
        {
            // Addressables 설정 편집을 위한 에디터용 ScriptableObject 인스턴스 생성
            var addressableEditor = ScriptableObject.CreateInstance<AddressableEditorTcg>();

            // Settings 관련 ScriptableObject(Addressables 설정 포함) 구성
            var settingScriptableObject = new SettingScriptableObjectTcg(addressableEditor);
            settingScriptableObject.Setup(ctx);

            // 테이블(Table) 관련 Addressable 구성(예: Tables 그룹/엔트리/라벨 등)
            var settingTable = new SettingTableTcg(addressableEditor);
            settingTable.Setup(ctx);
        }
    }
}