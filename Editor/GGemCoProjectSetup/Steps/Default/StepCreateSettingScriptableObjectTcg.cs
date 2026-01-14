#if UNITY_EDITOR
using GGemCo2DCoreEditor;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// TCG 전용 설정 ScriptableObject를 생성하고,
    /// 프로젝트에 필요한 기본 설정을 적용하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// 내부적으로 사용자 레이어(8~31) 설정 등 프로젝트 전반 설정을
    /// 초기화할 수 있으며, 실제 적용 내용은
    /// <see cref="SettingGGemCoTcg.CreateSettings"/> 구현에 위임됩니다.
    /// </remarks>
    public sealed class StepCreateSettingScriptableObjectTcg : SetupStepBase
    {
        /// <summary>
        /// 설정 생성 스텝은 사전 조건이 없으므로 항상 유효합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 공통 설정 및 로그 출력에 사용됩니다.
        /// </param>
        /// <param name="message">
        /// 검증 메시지 출력용 파라미터로, 항상 <c>null</c>을 반환합니다.
        /// </param>
        /// <returns>
        /// 항상 <c>true</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            message = null;
            return true;
        }

        /// <summary>
        /// TCG 설정 ScriptableObject를 생성하고,
        /// 프로젝트 기본 설정을 적용합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 설정 생성 및 로그 처리에 사용됩니다.
        /// </param>
        public override void Execute(EditorSetupContext ctx)
        {
            // TCG 전용 설정 ScriptableObject 인스턴스 생성
            var settingScriptableObject = new SettingGGemCoTcg();

            // 프로젝트 기본 설정(레이어, 옵션 등)을 생성 및 적용
            settingScriptableObject.CreateSettings(ctx);
        }
    }
}
#endif