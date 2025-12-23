#if UNITY_EDITOR
using System;
using GGemCo2DTcg;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// GGemCoTcgSettings 전용 커스텀 에디터
    /// - ShuffleMode에 따라 허용되는 ShuffleSettings ScriptableObject 타입을 제한
    /// - 잘못된 타입 지정 시 경고 표시 및 자동 무효화
    /// </summary>
    [CustomEditor(typeof(GGemCoTcgSettings))]
    public sealed class GGemCoTcgSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _playerShuffleMode;
        private SerializedProperty _playerShuffleSettings;
        private SerializedProperty _enemyDeckPreset;
        private SerializedProperty _enemyShuffleMode;
        private SerializedProperty _enemyShuffleSettings;

        private void OnEnable()
        {
            _playerShuffleMode     = serializedObject.FindProperty("playerShuffleMode");
            _playerShuffleSettings = serializedObject.FindProperty("playerShuffleSettings");
            _enemyDeckPreset       = serializedObject.FindProperty("enemyDeckPreset");
            _enemyShuffleMode      = serializedObject.FindProperty("enemyShuffleMode");
            _enemyShuffleSettings  = serializedObject.FindProperty("enemyShuffleSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultExceptShuffleSection();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("카드 섞기", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawShuffleBlock(
                label: "플레이어 셔플 설정",
                deckProp: null,
                modeProp: _playerShuffleMode,
                settingsProp: _playerShuffleSettings);

            EditorGUILayout.Space(6);

            DrawShuffleBlock(
                label: "적 셔플 설정",
                deckProp: _enemyDeckPreset,
                modeProp: _enemyShuffleMode,
                settingsProp: _enemyShuffleSettings);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 기본 Inspector에서 Shuffle 관련 필드만 제외하고 그립니다.
        /// </summary>
        private void DrawDefaultExceptShuffleSection()
        {
            DrawPropertiesExcluding(
                serializedObject,
                "playerShuffleMode",
                "playerShuffleSettings",
                "enemyDeckPreset",
                "enemyShuffleMode",
                "enemyShuffleSettings");
        }

        /// <summary>
        /// ShuffleMode + Settings ObjectField 한 세트 UI
        /// </summary>
        private void DrawShuffleBlock(
            string label,
            SerializedProperty deckProp,
            SerializedProperty modeProp,
            SerializedProperty settingsProp)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (deckProp != null)
                EditorGUILayout.PropertyField(deckProp);
            
            EditorGUILayout.PropertyField(modeProp);

            var shuffleMode = (ConfigCommonTcg.ShuffleMode)modeProp.enumValueIndex;

            Type expectedType = GetExpectedSettingsType(shuffleMode);

            DrawSettingsField(
                settingsProp,
                shuffleMode,
                expectedType);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Settings ObjectField + 타입 검증 + HelpBox
        /// </summary>
        private void DrawSettingsField(
            SerializedProperty settingsProp,
            ConfigCommonTcg.ShuffleMode mode,
            Type expectedType)
        {
            EditorGUILayout.Space(2);

            if (expectedType == null)
            {
                // 설정이 필요 없는 모드
                EditorGUILayout.HelpBox(
                    "선택한 셔플 모드는 별도의 설정 파일이 필요하지 않습니다.",
                    MessageType.Info);

                settingsProp.objectReferenceValue = null;
                return;
            }

            EditorGUILayout.ObjectField(
                settingsProp,
                typeof(ScriptableObject),
                new GUIContent("모드 설정 파일"));

            var obj = settingsProp.objectReferenceValue;

            if (obj == null)
            {
                EditorGUILayout.HelpBox(
                    $"이 모드({mode})는 {expectedType.Name} 타입의 설정 파일이 필요합니다.",
                    MessageType.Warning);
                return;
            }

            if (!expectedType.IsInstanceOfType(obj))
            {
                EditorGUILayout.HelpBox(
                    $"잘못된 설정 파일입니다.\n" +
                    $"필요 타입: {expectedType.Name}\n" +
                    $"현재 타입: {obj.GetType().Name}",
                    MessageType.Error);

                // 자동 무효화
                settingsProp.objectReferenceValue = null;
            }
        }

        /// <summary>
        /// ShuffleMode에 따라 허용되는 Settings ScriptableObject 타입 반환
        /// </summary>
        private static Type GetExpectedSettingsType(ConfigCommonTcg.ShuffleMode mode)
        {
            switch (mode)
            {
                case ConfigCommonTcg.ShuffleMode.PureRandom:
                case ConfigCommonTcg.ShuffleMode.SeededReplay:
                    return null;

                case ConfigCommonTcg.ShuffleMode.Weighted:
                    return typeof(GGemCoTcgWeightedShuffleSettings);

                case ConfigCommonTcg.ShuffleMode.PhaseWeighted:
                    return typeof(GGemCoTcgPhaseShuffleSettings);

                default:
                    return null;
            }
        }
    }
}
#endif
