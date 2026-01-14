#if UNITY_EDITOR
using System.Collections.Generic;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// TCG에서 사용하는 주요 씬(PreIntro/Loading/Game)에 필수 오브젝트 구성을 적용하고,
    /// 씬을 저장하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 이 스텝은 <see cref="ConfigDefine.PathSceneAsset"/>에 등록된 씬 경로를 기준으로
    /// 씬 에셋 존재 여부를 검증한 뒤, 각 씬에 대응하는 Configurator를 실행합니다.
    /// </para>
    /// <para>
    /// 실행 과정에서의 상세 정책(중단/로깅 등)은 내부의 <c>ConfigureAndSave</c> 구현 및
    /// Runner 컨텍스트 정책(Profile.stopOnFirstError 등)에 의해 좌우될 수 있습니다.
    /// </para>
    /// </remarks>
    public sealed class StepSetSceneRequireObjectTcg : SetupStepBase
    {
        /// <summary>
        /// Game 씬 구성 시 샘플 리소스를 함께 구성할지 여부입니다.
        /// </summary>
        private readonly bool _needSampleResources;

        /// <summary>
        /// 필수 구성을 적용할 대상 씬 이름 목록입니다.
        /// </summary>
        private readonly List<string> _sceneNames = new List<string>
        {
            ConfigDefine.SceneNamePreIntro,
            ConfigDefine.SceneNameLoading,
            ConfigDefine.SceneNameGame
        };

        /// <summary>
        /// 씬 구성 시 샘플 리소스 포함 여부를 지정하여 스텝을 생성합니다.
        /// </summary>
        /// <param name="needSampleResources">샘플 리소스를 함께 구성할지 여부입니다.</param>
        public StepSetSceneRequireObjectTcg(bool needSampleResources)
        {
            _needSampleResources = needSampleResources;
        }

        /// <summary>
        /// 대상 씬들이 프로젝트에 존재하는지 검증하고,
        /// 이후 단계에서 사용할 수 있도록 컨텍스트 공유 저장소에 씬 에셋을 등록합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 공유 데이터 저장 및 로그 출력에 사용됩니다.</param>
        /// <param name="message">검증 실패 시 에디터에 표시될 메시지입니다.</param>
        /// <returns>
        /// 모든 대상 씬이 경로 매핑되어 있고 에셋이 로드되면 <c>true</c>,
        /// 하나라도 누락되면 <c>false</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            // 별도 선행 조건은 없음. (열린 씬이 없어도 구성 자체는 씬 에셋 기준으로 수행 가능)
            foreach (var sceneName in _sceneNames)
            {
                // 씬 이름 -> 에셋 경로 매핑 확인
                string path = ConfigDefine.PathSceneAsset.GetValueOrDefault(sceneName);
                if (string.IsNullOrEmpty(path))
                {
                    // NOTE: path 자체가 비어 있을 수 있으므로, 메시지는 "경로 없음" 의미로 해석된다.
                    message = $"씬 없음: {sceneName}";
                    return false;
                }

                // 씬 에셋 존재 여부 확인
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (scene == null)
                {
                    message = $"씬을 먼저 생성해주세요.: {path}";
                    return false;
                }

                // Execute 단계에서 재사용할 수 있도록 컨텍스트에 씬 에셋을 공유 저장
                ctx.SetShared(sceneName, scene);
            }

            message = null;
            return true;
        }

        /// <summary>
        /// 검증 단계에서 확보한 씬 에셋을 대상으로,
        /// 각 씬에 대응하는 Configurator를 실행한 뒤 씬을 저장합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 씬 처리 및 로그 출력에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            // PreIntro: Canvas + 로딩 퍼센트 텍스트 구성
            ConfigureAndSave(
                ctx.GetShared<SceneAsset>(ConfigDefine.SceneNamePreIntro),
                () => ScriptableObject.CreateInstance<ScenePreIntroConfiguratorTcg>().ConfigureInEditor(ctx),
                ctx);

            // Loading: SceneLoading + Canvas + 퍼센트 텍스트 구성
            ConfigureAndSave(
                ctx.GetShared<SceneAsset>(ConfigDefine.SceneNameLoading),
                () => ScriptableObject.CreateInstance<SceneLoadingConfiguratorTcg>().ConfigureInEditor(ctx),
                ctx);

            // Game: SceneGame + Camera/Canvas/World/Black + Popup/Sound/WindowManager 구성
            ConfigureAndSave(
                ctx.GetShared<SceneAsset>(ConfigDefine.SceneNameGame),
                () => ScriptableObject.CreateInstance<SceneGameConfiguratorTcg>().ConfigureInEditor(ctx, _needSampleResources),
                ctx);
        }
    }
}
#endif
