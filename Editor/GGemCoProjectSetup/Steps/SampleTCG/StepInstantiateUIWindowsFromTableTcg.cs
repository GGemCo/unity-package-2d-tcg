#if UNITY_EDITOR
using GGemCo2DCoreEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// UIWindow 테이블 정보를 기반으로 테스트(또는 검증)용 UIWindow들을 에디터에서 생성하는 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// 실제 생성 규칙(대상 씬/캔버스, 테이블 로딩 방식, 중복 생성 처리 등)은
    /// <see cref="SceneEditorGameTcg.SetupAllTestWindow"/> 구현에 위임됩니다.
    /// </remarks>
    public sealed class StepInstantiateUIWindowsFromTableTcg : SetupStepBase
    {
        /// <summary>
        /// 이 스텝은 별도 사전 조건이 없으므로 항상 유효합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력 및 설정에 사용됩니다.</param>
        /// <param name="message">검증 메시지 출력용 파라미터로, 항상 <c>null</c>을 반환합니다.</param>
        /// <returns>항상 <c>true</c>를 반환합니다.</returns>
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            message = null;
            return true;
        }

        /// <summary>
        /// <see cref="SceneEditorGameTcg"/>를 생성한 뒤,
        /// UIWindow 테이블 기반으로 테스트용 윈도우들을 씬에 배치합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 윈도우 생성 및 로그 처리에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            // Game 씬 편집 유틸리티를 생성하여 UIWindow 테이블 기반 테스트 윈도우를 구성한다.
            var sceneEditorGameTcg = ScriptableObject.CreateInstance<SceneEditorGameTcg>();

            // UIWindow 테이블을 순회하며 테스트/검증용 윈도우를 씬에 생성한다.
            sceneEditorGameTcg.SetupAllTestWindow(ctx);
        }
    }
}
#endif