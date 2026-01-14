#if UNITY_EDITOR
using GGemCo2DCoreEditor;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// TCG 예제 기준에 맞게 MainCamera의 카메라 설정을 적용하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// 이 스텝은 태그가 <c>MainCamera</c>로 지정된 카메라를 대상으로 하며,
    /// Orthographic 모드와 예제용 Size 값을 설정합니다.
    /// </remarks>
    public class StepSetCameraTcg : SetupStepBase
    {
        /// <summary>
        /// 현재 씬에 MainCamera가 존재하는지 검증합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        /// <param name="msg">검증 실패 시 에디터에 표시될 메시지입니다.</param>
        /// <returns>
        /// MainCamera가 존재하면 <c>true</c>,
        /// 존재하지 않으면 <c>false</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                msg =
                    $"[{nameof(StepSetCameraTcg)}] MainCamera not found. " +
                    "Make sure a Camera with tag 'MainCamera' exists.";
                return false;
            }

            msg = null;
            return true;
        }

        /// <summary>
        /// MainCamera를 Orthographic 카메라로 설정하고,
        /// TCG 예제 기준에 맞는 기본 파라미터를 적용합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            // 1) MainCamera 획득
            Camera cam = Camera.main;
            if (cam == null)
            {
                // Validate 이후라도 씬 변경 등으로 인해 카메라가 없을 수 있으므로 방어 코드 유지
                HelperLog.Warn(
                    $"[{nameof(StepSetCameraTcg)}] MainCamera not found. " +
                    "Make sure a Camera with tag 'MainCamera' exists.",
                    ctx);
                return;
            }

            // 2) Undo 기록 (에디터에서 되돌리기 가능하도록)
            Undo.RecordObject(cam, "Set TCG Camera Settings");

            // 3) 카메라 값 설정 (TCG 예제 기준)
            cam.orthographic = true;
            cam.orthographicSize = 90f;

            // 4) 변경 사항 저장 처리
            EditorUtility.SetDirty(cam);
            EditorSceneManager.MarkSceneDirty(cam.gameObject.scene);

            HelperLog.Info(
                $"[{nameof(StepSetCameraTcg)}] MainCamera projection has been set to " +
                $"Orthographic (Size: {cam.orthographicSize}).",
                ctx);
        }
    }
}
#endif
