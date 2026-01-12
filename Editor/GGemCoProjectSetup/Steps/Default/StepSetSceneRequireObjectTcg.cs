#if UNITY_EDITOR
using System.Collections.Generic;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 현재 열린 씬에서 ISceneConfigurator를 실행한다.
    /// - 1) 씬 내 MonoBehaviour + ISceneConfigurator (비활성 포함)
    /// - 2) 프로젝트 내 ScriptableObject 에셋(직접 서브클래스) + ISceneConfigurator
    /// - 3) 매개변수 없는 생성자가 있는 일반 클래스 + ISceneConfigurator (TypeCache 기반)
    /// 실행/검증/로깅은 Runner 컨텍스트 정책(Profile.stopOnFirstError)에 따름.
    /// </summary>
    public sealed class StepSetSceneRequireObjectTcg : SetupStepBase
    {
        private bool _needSampleResources;
        private readonly List<string> _sceneNames = new List<string> {ConfigDefine.SceneNamePreIntro, ConfigDefine.SceneNameLoading, ConfigDefine.SceneNameGame};
        
        public StepSetSceneRequireObjectTcg(bool needSampleResources)
        {
            _needSampleResources = needSampleResources;
        }
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            // 별도 선행 조건은 없음. (열린 씬이 없어도 MonoBehaviour 탐색은 빈 결과)
            foreach (var sceneName in _sceneNames)
            {
                string path = ConfigDefine.PathSceneAsset.GetValueOrDefault(sceneName);
                if (string.IsNullOrEmpty(path))
                {
                    message = $"씬 없음: {path}";
                    return false;
                }
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (scene == null)
                {
                    message = $"씬을 먼저 생성해주세요.: {path}";
                    return false;
                }
                ctx.SetShared(sceneName, scene);
            }
            
            message = null;
            return true;
        }

        public override void Execute(EditorSetupContext ctx)
        {
            ConfigureAndSave(ctx.GetShared<SceneAsset>(ConfigDefine.SceneNamePreIntro),
                () => ScriptableObject.CreateInstance<ScenePreIntroConfiguratorTcg>().ConfigureInEditor(ctx), ctx);

            ConfigureAndSave(ctx.GetShared<SceneAsset>(ConfigDefine.SceneNameLoading),
                () => ScriptableObject.CreateInstance<SceneLoadingConfiguratorTcg>().ConfigureInEditor(ctx), ctx);

            ConfigureAndSave(ctx.GetShared<SceneAsset>(ConfigDefine.SceneNameGame),
                () => ScriptableObject.CreateInstance<SceneGameConfiguratorTcg>().ConfigureInEditor(ctx, _needSampleResources), ctx);
        }
    }
}
#endif
