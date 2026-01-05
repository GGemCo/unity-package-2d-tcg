#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using AbilityType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityType;
using AbilityTargetType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTargetType;
using AbilityTriggerType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTriggerType;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// Ability TSV(탭 구분) → Localization StringTable 자동 생성/갱신 툴.
    ///
    /// 생성/갱신 대상:
    /// - TCG_Ability: {Uid} (Smart String 템플릿)
    /// - TCG_Term_Trigger: trigger_{TriggerType}_prefix
    /// - TCG_Term_Target: target_{TargetType}_{Case}
    ///
    /// 특징:
    /// - Ability 템플릿에서 Trigger/Target은 {Trigger}/{Target} 형태로 "중첩 localized" 사용
    /// - Trigger/Target 표기명은 별도 테이블로 분리하여 조사/표현 차이를 안전하게 관리
    /// - 누락/불일치 검증 리포트 제공
    ///
    /// 참고:
    /// - Nested localized는 LocalizedString을 인자로 전달하고 Smart String에서 suffix로 평가하는 방식이 일반적입니다.
    /// - Persistent Variables Source는 스크립트 없이 인자를 제공하는 기능입니다.
    ///
    /// 변경 사항(요청 반영):
    /// - Locale이 '한글/영어 고정'이 아니라, Localization Settings에 등록된 Locales 전체를 자동으로 감지해 처리합니다.
    /// - ko(한국어)는 Ko 텍스트 생성, 그 외 Locale은 기본적으로 En 텍스트를 fallback으로 사용합니다.
    /// </summary>
    public sealed class TcgAbilityLocalizationGeneratorWindow : EditorWindow
    {
        private const string Title = "Ability Localization 만들기";
        [Header("입력")]
        private TextAsset _spellTsv;
        private TextAsset _equipmentTsv;
        private TextAsset _permanentTsv;
        private TextAsset _eventTsv;

        [Header("컬렉션")] 
        private const string AbilityCollectionName = LocalizationConstantsTcg.Tables.AbilityDescription;
        private const string TriggerCollectionName = LocalizationConstantsTcg.Tables.AbilityTrigger;
        private const string TargetCollectionName = LocalizationConstantsTcg.Tables.AbilityTarget;

        [Header("출력")]
        private DefaultAsset _outputFolder;
        private bool _overwriteExisting = false;
        private bool _generateTermTables = true;

        [Header("로케일")]
        [Tooltip("ko(한국어)는 Ko 텍스트를 사용하고, 그 외 Locale은 En 텍스트를 기본값으로 사용합니다.")]
        private bool _useEnglishFallbackForNonKorean = true;

        [Header("검증")]
        private bool _validateAfterGenerate = true;
        private bool _logVerbose = false;

        private Vector2 _scroll;
        private string _lastReport = "";

        [MenuItem(ConfigEditorTcg.NameToolCreateAbilityLocalization, false, (int)ConfigEditorTcg.ToolOrdering.CreateAbilityLocalization)]
        public static void Open()
        {
            var window = GetWindow<TcgAbilityLocalizationGeneratorWindow>();
            window.titleContent = new GUIContent(Title);
            window.minSize = new Vector2(680, 520);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPrefs();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Ability txt → Localization (Ability + Term Tables)", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _spellTsv = (TextAsset)EditorGUILayout.ObjectField("Spell txt 파일(tcg_card_spell)", _spellTsv, typeof(TextAsset), false);
                _equipmentTsv = (TextAsset)EditorGUILayout.ObjectField("Equipment txt 파일(tcg_card_equipment)", _equipmentTsv, typeof(TextAsset), false);
                _permanentTsv = (TextAsset)EditorGUILayout.ObjectField("Permanent txt 파일(tcg_card_permanent)", _permanentTsv, typeof(TextAsset), false);
                _eventTsv = (TextAsset)EditorGUILayout.ObjectField("Event txt 파일(tcg_card_event)", _eventTsv, typeof(TextAsset), false);

                _outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("출력 폴더(Assets/)", _outputFolder, typeof(DefaultAsset), false);

                EditorGUILayout.Space(4);
                _overwriteExisting = EditorGUILayout.ToggleLeft("기존 항목 덮어쓰기. (변경된 내용이 없으면 갱신하지 않습니다.)", _overwriteExisting);
                _generateTermTables = EditorGUILayout.ToggleLeft("용어 테이블(Trigger/Target) 생성/갱신", _generateTermTables);
                _validateAfterGenerate = EditorGUILayout.ToggleLeft("생성 후 검증 실행", _validateAfterGenerate);
                _logVerbose = EditorGUILayout.ToggleLeft("상세 로그 출력(Verbose)", _logVerbose);
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("생성되는 Localization String Table 이름", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Ability 컬렉션: {AbilityCollectionName}");
                EditorGUILayout.LabelField($"Trigger 용어 컬렉션: {TriggerCollectionName}");
                EditorGUILayout.LabelField($"Target 용어 컬렉션: {TargetCollectionName}");
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("로케일(자동 감지)", EditorStyles.boldLabel);

                var locales = LocalizationEditorSettings.GetLocales();
                if (locales == null || locales.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Localization Settings에 Locales가 없습니다.\n" +
                        "Project Settings > Localization 에서 Locale Generator로 Locales를 먼저 추가해주세요.",
                        MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"등록된 로케일 수: {locales.Count}");
                    using (new EditorGUI.DisabledScope(true))
                    {
                        foreach (var locale in locales)
                            EditorGUILayout.TextField($"{locale.Identifier.Code} ({locale.LocaleName})");
                    }
                }

                EditorGUILayout.Space(4);
                _useEnglishFallbackForNonKorean = EditorGUILayout.ToggleLeft("한국어(ko) 외 로케일은 영어(En) 텍스트를 기본값으로 사용", _useEnglishFallbackForNonKorean);
            }

            EditorGUILayout.Space(12);

            using (new EditorGUI.DisabledScope((_spellTsv == null && _equipmentTsv == null && _permanentTsv == null && _eventTsv == null) || _outputFolder == null))
            {
                if (GUILayout.Button("생성 / 갱신 실행", GUILayout.Height(36)))
                {
                    try
                    {
                        GenerateAll();
                        SavePrefs();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog("Ability 지역화", $"생성 실패:\n{e.Message}", "확인");
                    }
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("최근 리포트", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(_lastReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(
                "TSV 컬럼(탭 구분):\n" +
                "Uid\tName\tAbilityType\tTriggerType\tTargetType\tParamA\tParamB\tParamC\tDescription\n" +
                "- 첫 줄 헤더 및 # 주석 라인은 무시됩니다.\n" +
                "- Description 컬럼은 무시됩니다(템플릿은 규칙 기반으로 생성).",
                MessageType.Info);
        }

        private void GenerateAll()
        {
            if (_spellTsv == null && _equipmentTsv == null && _permanentTsv == null && _eventTsv == null) throw new InvalidOperationException("tcg_ability 데이터 테이블이 null 입니다.");
            if (_outputFolder == null) throw new InvalidOperationException("출력 폴더가 null 입니다.");

            var outputPath = AssetDatabase.GetAssetPath(_outputFolder);
            if (string.IsNullOrWhiteSpace(outputPath) || !outputPath.StartsWith("Assets", StringComparison.Ordinal))
                throw new InvalidOperationException("출력 폴더는 Assets/ 하위여야 합니다.");

            // 1) Localization Settings의 Locales 감지
            var locales = LocalizationEditorSettings.GetLocales();
            if (locales == null || locales.Count == 0)
                throw new InvalidOperationException("Localization Settings에 Locale이 없습니다. 먼저 Locale을 추가해주세요.");

            // 2) 컬렉션/테이블 준비
            var abilityCollection = HelperLocalization.EnsureStringTableCollection(AbilityCollectionName, outputPath);
            var triggerCollection = _generateTermTables ? HelperLocalization.EnsureStringTableCollection(TriggerCollectionName, outputPath) : null;
            var targetCollection  = _generateTermTables ? HelperLocalization.EnsureStringTableCollection(TargetCollectionName, outputPath)  : null;

            // 3) 로케일별 테이블 준비
            var abilityTablesByLocale = new Dictionary<Locale, StringTable>(locales.Count);
            var triggerTablesByLocale = _generateTermTables ? new Dictionary<Locale, StringTable>(locales.Count) : null;
            var targetTablesByLocale  = _generateTermTables ? new Dictionary<Locale, StringTable>(locales.Count) : null;

            foreach (var locale in locales)
            {
                abilityTablesByLocale[locale] = HelperLocalization.EnsureLocaleTable(abilityCollection, locale);

                if (_generateTermTables)
                {
                    triggerTablesByLocale![locale] = HelperLocalization.EnsureLocaleTable(triggerCollection, locale);
                    targetTablesByLocale![locale]  = HelperLocalization.EnsureLocaleTable(targetCollection, locale);
                }
            }

            // 4) TSV 파싱
            var rows = new List<TcgAbilityTsvRow>(256);
            if (_spellTsv != null) rows.AddRange(ParseTsv(_spellTsv.text));
            if (_equipmentTsv != null) rows.AddRange(ParseTsv(_equipmentTsv.text));
            if (_permanentTsv != null) rows.AddRange(ParseTsv(_permanentTsv.text));
            if (_eventTsv != null) rows.AddRange(ParseTsv(_eventTsv.text));

            // 5) 사용된 Trigger/Target 수집
            var usedTriggers = new HashSet<AbilityTriggerType>();
            var usedTargets  = new HashSet<AbilityTargetType>();
            foreach (var row in rows)
            {
                usedTriggers.Add(row.TcgAbilityTriggerType);
                usedTargets.Add(row.TcgAbilityTargetType);
            }

            var report = new TcgLocalizationReport();

            // 6) 용어(Trigger/Target) 항목 생성/갱신
            if (_generateTermTables)
            {
                foreach (var locale in locales)
                {
                    GenerateTriggerTerms(triggerTablesByLocale![locale], locale, usedTriggers, report);
                    GenerateTargetTerms(targetTablesByLocale![locale], locale, usedTargets, report);
                }
            }

            // 7) Ability 템플릿(스마트 스트링) 생성/갱신
            foreach (var row in rows)
            {
                var key = $"{row.Uid}";

                foreach (var locale in locales)
                {
                    var value = BuildAbilityTemplateByLocale(locale, row);
                    UpsertEntry(
                        abilityTablesByLocale[locale],
                        key,
                        value,
                        _overwriteExisting,
                        report,
                        $"Ability({locale.Identifier.Code})",
                        markSmart: true);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 8) 검증
            if (_validateAfterGenerate)
                ValidateAll(rows, locales, abilityTablesByLocale, triggerTablesByLocale, targetTablesByLocale, report);

            _lastReport = report.ToString();
            if (_logVerbose) Debug.Log(_lastReport);

            EditorUtility.DisplayDialog(
                Title,
                $"완료되었습니다.\n{report.SummaryLine}\n\n세부 내용은 '최근 리포트'에서 확인하세요.",
                "확인");
        }

        #region Locale Routing
        private string BuildAbilityTemplateByLocale(Locale locale, TcgAbilityTsvRow row)
        {
            var lang = GetLanguageCode(locale);

            // 명시적으로 한국어 처리
            if (string.Equals(lang, "ko", StringComparison.OrdinalIgnoreCase))
                return TcgAbilitySmartTemplate.BuildKorean(row);

            // 영어 또는 기타 locale fallback
            if (_useEnglishFallbackForNonKorean)
                return TcgAbilitySmartTemplate.BuildEnglish(row);

            // fallback을 끄는 경우: 영어를 기본으로 두되, 추후 확장 포인트로 남김.
            return TcgAbilitySmartTemplate.BuildEnglish(row);
        }

        private string BuildTriggerTermByLocale(Locale locale, AbilityTriggerType tcgAbilityTriggerType)
        {
            var lang = GetLanguageCode(locale);
            if (string.Equals(lang, "ko", StringComparison.OrdinalIgnoreCase))
                return TcgTermDefaultText.KoTriggerPrefix(tcgAbilityTriggerType);

            return TcgTermDefaultText.EnTriggerPrefix(tcgAbilityTriggerType);
        }

        private string BuildTargetTermByLocale(Locale locale, AbilityTargetType tcgAbilityTargetType, string @case)
        {
            var lang = GetLanguageCode(locale);
            if (string.Equals(lang, "ko", StringComparison.OrdinalIgnoreCase))
            {
                return @case switch
                {
                    "to"    => TcgTermDefaultText.KoTargetTo(tcgAbilityTargetType),
                    "obj"   => TcgTermDefaultText.KoTargetObj(tcgAbilityTargetType),
                    "noun1" => TcgTermDefaultText.KoTargetNoun1(tcgAbilityTargetType),
                    _       => TcgTermDefaultText.KoTargetObj(tcgAbilityTargetType)
                };
            }

            return @case switch
            {
                "to"    => TcgTermDefaultText.EnTargetTo(tcgAbilityTargetType),
                "obj"   => TcgTermDefaultText.EnTargetObj(tcgAbilityTargetType),
                "noun1" => TcgTermDefaultText.EnTargetNoun1(tcgAbilityTargetType),
                _       => TcgTermDefaultText.EnTargetObj(tcgAbilityTargetType)
            };
        }

        private static string GetLanguageCode(Locale locale)
        {
            // LocaleIdentifier.Code 예: "en", "ko", "en-US", "ko-KR" 등
            var code = locale?.Identifier.Code;
            if (string.IsNullOrWhiteSpace(code)) return "en";

            var dash = code.IndexOf('-', StringComparison.Ordinal);
            return dash >= 0 ? code[..dash] : code;
        }
        #endregion

        #region Term Generation
        private void GenerateTriggerTerms(StringTable table, Locale locale, HashSet<AbilityTriggerType> usedTriggers, TcgLocalizationReport report)
        {
            foreach (var trigger in usedTriggers)
            {
                var key = TcgTermKey.TriggerPrefixKey(trigger);
                var val = BuildTriggerTermByLocale(locale, trigger);

                // Term은 Smart로 만들 필요가 없으므로 markSmart=false
                UpsertEntry(table, key, val, overwrite: true, report, $"TriggerTerm({locale.Identifier.Code})", markSmart: false);
            }
        }

        private void GenerateTargetTerms(StringTable table, Locale locale, HashSet<AbilityTargetType> usedTargets, TcgLocalizationReport report)
        {
            foreach (var target in usedTargets)
            {
                // to / obj / noun1 케이스 모두 생성
                var keyTo = TcgTermKey.TargetKey(target, "to");
                UpsertEntry(table, keyTo, BuildTargetTermByLocale(locale, target, "to"), overwrite: true, report, $"TargetTerm({locale.Identifier.Code})", markSmart: false);

                var keyObj = TcgTermKey.TargetKey(target, "obj");
                UpsertEntry(table, keyObj, BuildTargetTermByLocale(locale, target, "obj"), overwrite: true, report, $"TargetTerm({locale.Identifier.Code})", markSmart: false);

                var keyNoun1 = TcgTermKey.TargetKey(target, "noun1");
                UpsertEntry(table, keyNoun1, BuildTargetTermByLocale(locale, target, "noun1"), overwrite: true, report, $"TargetTerm({locale.Identifier.Code})", markSmart: false);
            }
        }
        #endregion

        #region Validation
        private void ValidateAll(
            List<TcgAbilityTsvRow> rows,
            IList<Locale> locales,
            Dictionary<Locale, StringTable> abilityTablesByLocale,
            Dictionary<Locale, StringTable> triggerTablesByLocale,
            Dictionary<Locale, StringTable> targetTablesByLocale,
            TcgLocalizationReport report)
        {
            foreach (var row in rows)
            {
                var abilityKey = $"{row.Uid}";

                foreach (var locale in locales)
                {
                    if (!abilityTablesByLocale.TryGetValue(locale, out var abilityTable) || abilityTable == null)
                        continue;

                    ValidateEntryExists(abilityTable, abilityKey, report, $"Ability 누락({locale.Identifier.Code})", row.Uid);

                    var entry = abilityTable.GetEntry(abilityKey);
                    ValidateTemplateTokens(entry?.Value, row, report, locale.Identifier.Code);

                    if (_generateTermTables && triggerTablesByLocale != null && targetTablesByLocale != null)
                    {
                        var trigKey = TcgTermKey.TriggerPrefixKey(row.TcgAbilityTriggerType);
                        if (triggerTablesByLocale.TryGetValue(locale, out var trigTable) && trigTable != null)
                            ValidateEntryExists(trigTable, trigKey, report, $"Trigger 용어 누락({locale.Identifier.Code})", row.Uid);

                        var targetKey = TcgTermKey.TargetKeyForAbility(row.AbilityType, row.TcgAbilityTargetType);
                        if (targetTablesByLocale.TryGetValue(locale, out var tgtTable) && tgtTable != null)
                            ValidateEntryExists(tgtTable, targetKey, report, $"Target 용어 누락({locale.Identifier.Code})", row.Uid);
                    }
                }
            }
        }

        private static void ValidateEntryExists(StringTable table, string key, TcgLocalizationReport report, string category, int uid)
        {
            if (table == null) return;
            if (table.GetEntry(key) == null)
                report.AddWarn(category, $"uid={uid} key='{key}' table='{table.TableCollectionName}'");
        }

        private static void ValidateTemplateTokens(string template, TcgAbilityTsvRow row, TcgLocalizationReport report, string localeCode)
        {
            if (string.IsNullOrWhiteSpace(template)) return;

            // Trigger는 대부분 템플릿에서 쓰도록 통일
            if (!template.Contains("{Trigger}", StringComparison.Ordinal))
                report.AddWarn("템플릿 토큰", $"[{localeCode}] uid={row.Uid} '{{Trigger}}' 누락 (template='{template}')");

            // Target은 타겟 없는 능력(Draw/GainMana/ExtraAction 등)에서는 없을 수 있음
            if (row.AbilityType is AbilityType.Damage or AbilityType.Heal
                or AbilityType.BuffAttack or AbilityType.BuffHealth
                or AbilityType.BuffAttackHealth)
            {
                if (!template.Contains("{Target}", StringComparison.Ordinal))
                {
                    report.AddWarn(
                        "템플릿 토큰",
                        $"[{localeCode}] uid={row.Uid} '{{Target}}' 누락 (template='{template}')");
                }
            }

            // 값 토큰 체크
            if (row.AbilityType == AbilityType.BuffAttackHealth)
            {
                if (!template.Contains("{ValueA}", StringComparison.Ordinal) || !template.Contains("{ValueB}", StringComparison.Ordinal))
                    report.AddWarn("템플릿 토큰", $"[{localeCode}] uid={row.Uid} '{{ValueA}}/{{ValueB}}' 누락 (template='{template}')");
            }
            else if (row.AbilityType is AbilityType.Damage
                     or AbilityType.Heal or AbilityType.Draw
                     or AbilityType.GainMana or AbilityType.ExtraAction
                     or AbilityType.BuffAttack or AbilityType.BuffHealth)
            {
                if (!template.Contains("{Value}", StringComparison.Ordinal) &&
                    !template.Contains("{ValueA}", StringComparison.Ordinal))
                {
                    report.AddWarn(
                        "템플릿 토큰",
                        $"[{localeCode}] uid={row.Uid} '{{Value}}' 또는 '{{ValueA}}' 누락 (template='{template}')");
                }
            }
        }
        #endregion

        private static void UpsertEntry(
            StringTable table,
            string key,
            string value,
            bool overwrite,
            TcgLocalizationReport report,
            string category,
            bool markSmart)
        {
            if (table == null) return;

            var entry = table.GetEntry(key);
            if (entry == null)
            {
                entry = table.AddEntry(key, value);
                report.Created++;
                report.AddInfo(category, $"생성됨 key='{key}'");
            }
            else
            {
                if (!overwrite)
                {
                    report.Skipped++;
                    return;
                }

                if (entry.Value != value)
                {
                    entry.Value = value;
                    report.Updated++;
                    report.AddInfo(category, $"갱신됨 key='{key}'");
                }
                else
                {
                    report.Skipped++;
                }
            }

            if (markSmart)
                entry.IsSmart = true;

            EditorUtility.SetDirty(table);
        }

        #region TSV Parse
                private static List<TcgAbilityTsvRow> ParseTsv(string text)
        {
            const int defaultCapacity = 256;
            var result = new List<TcgAbilityTsvRow>(defaultCapacity);

            if (string.IsNullOrWhiteSpace(text))
                return result;

            using var reader = new StringReader(text);

            // 1) header
            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
                return result;

            var headers = headerLine.Split('\t');
            int idxUid = Array.IndexOf(headers, "Uid");
            int idxName = Array.IndexOf(headers, "Name"); // optional
            int idxAbilityType = Array.IndexOf(headers, "AbilityType");
            int idxTriggerType = Array.IndexOf(headers, "TriggerType");
            int idxTargetType = Array.IndexOf(headers, "TargetType");
            int idxParamA = Array.IndexOf(headers, "ParamA");
            int idxParamB = Array.IndexOf(headers, "ParamB");
            int idxParamC = Array.IndexOf(headers, "ParamC");
            int idxDesc = Array.IndexOf(headers, "Description"); // optional

            // 최소 요구 컬럼 체크
            if (idxUid < 0 || idxAbilityType < 0 || idxTriggerType < 0 || idxTargetType < 0 || idxParamA < 0 || idxParamB < 0 || idxParamC < 0)
            {
                GcLogger.LogWarning("[AbilityLocalizationGenerator] TSV header mismatch. Required: Uid, AbilityType, TriggerType, TargetType, ParamA, ParamB, ParamC");
                return result;
            }

            // 2) rows
            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#", StringComparison.Ordinal)) continue;

                var cols = line.Split('\t');
                if (cols.Length <= idxUid) continue;

                if (!int.TryParse(cols[idxUid], NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid) || uid <= 0)
                    continue;

                var name = (idxName >= 0 && idxName < cols.Length) ? cols[idxName] : string.Empty;
                var desc = (idxDesc >= 0 && idxDesc < cols.Length) ? cols[idxDesc] : string.Empty;

                var abilityType = (idxAbilityType < cols.Length) ? EnumHelper.ConvertEnum<AbilityType>(cols[idxAbilityType]) : AbilityType.None;
                var triggerType = (idxTriggerType < cols.Length) ? EnumHelper.ConvertEnum<AbilityTriggerType>(cols[idxTriggerType]) : AbilityTriggerType.None;
                var targetType = (idxTargetType < cols.Length) ? EnumHelper.ConvertEnum<AbilityTargetType>(cols[idxTargetType]) : AbilityTargetType.None;

                var paramA = (idxParamA < cols.Length) ? MathHelper.ParseInt(cols[idxParamA]) : 0;
                var paramB = (idxParamB < cols.Length) ? MathHelper.ParseInt(cols[idxParamB]) : 0;
                var paramC = (idxParamC < cols.Length) ? MathHelper.ParseInt(cols[idxParamC]) : 0;

                result.Add(new TcgAbilityTsvRow(
                    uid: uid,
                    name: name,
                    abilityType: abilityType,
                    tcgAbilityTriggerType: triggerType,
                    tcgAbilityTargetType: targetType,
                    paramA: paramA,
                    paramB: paramB,
                    paramC: paramC));
            }

            return result;
        }

#endregion
        
        #region Prefs
        private static class PrefKeys
        {
            // 프로젝트별로 분리되도록 Application.productName을 포함 (원하면 더 강하게: CompanyName + ProductName)
            private static readonly string Prefix = $"GGemCo.TcgAbilityLocalizationGeneratorWindow.";

            public static readonly string SpellTsvGuid = Prefix + "spell_tsv_guid";
            public static readonly string EquipmentTsvGuid = Prefix + "equipment_tsv_guid";
            public static readonly string PermanentTsvGuid = Prefix + "permanent_tsv_guid";
            public static readonly string EventTsvGuid = Prefix + "event_tsv_guid";
public static readonly string OutputFolderGuid = Prefix + "OutputFolderGuid";

            public static readonly string OverwriteExisting = Prefix + "OverwriteExisting";
            public static readonly string GenerateTermTables = Prefix + "GenerateTermTables";
            public static readonly string UseEnglishFallbackForNonKorean = Prefix + "UseEnglishFallbackForNonKorean";

            public static readonly string ValidateAfterGenerate = Prefix + "ValidateAfterGenerate";
            public static readonly string LogVerbose = Prefix + "LogVerbose";
        }
        private void SavePrefs()
        {
            // bool 옵션
            EditorPrefs.SetBool(PrefKeys.OverwriteExisting, _overwriteExisting);
            EditorPrefs.SetBool(PrefKeys.GenerateTermTables, _generateTermTables);
            EditorPrefs.SetBool(PrefKeys.UseEnglishFallbackForNonKorean, _useEnglishFallbackForNonKorean);

            EditorPrefs.SetBool(PrefKeys.ValidateAfterGenerate, _validateAfterGenerate);
            EditorPrefs.SetBool(PrefKeys.LogVerbose, _logVerbose);

            // Unity Object → GUID
            EditorPrefs.SetString(PrefKeys.SpellTsvGuid, GetGuid(_spellTsv));
            EditorPrefs.SetString(PrefKeys.EquipmentTsvGuid, GetGuid(_equipmentTsv));
            EditorPrefs.SetString(PrefKeys.PermanentTsvGuid, GetGuid(_permanentTsv));
            EditorPrefs.SetString(PrefKeys.EventTsvGuid, GetGuid(_eventTsv));
EditorPrefs.SetString(PrefKeys.OutputFolderGuid, GetGuid(_outputFolder));
        }

        private void LoadPrefs()
        {
            // bool 옵션
            _overwriteExisting = EditorPrefs.GetBool(PrefKeys.OverwriteExisting, false);
            _generateTermTables = EditorPrefs.GetBool(PrefKeys.GenerateTermTables, true);
            _useEnglishFallbackForNonKorean = EditorPrefs.GetBool(PrefKeys.UseEnglishFallbackForNonKorean, true);

            _validateAfterGenerate = EditorPrefs.GetBool(PrefKeys.ValidateAfterGenerate, true);
            _logVerbose = EditorPrefs.GetBool(PrefKeys.LogVerbose, false);

            // Unity Object는 GUID로 저장/복원
            var spellGuid = EditorPrefs.GetString(PrefKeys.SpellTsvGuid, string.Empty);
            var equipGuid = EditorPrefs.GetString(PrefKeys.EquipmentTsvGuid, string.Empty);
            var permGuid = EditorPrefs.GetString(PrefKeys.PermanentTsvGuid, string.Empty);
            var eventGuid = EditorPrefs.GetString(PrefKeys.EventTsvGuid, string.Empty);
var folderGuid = EditorPrefs.GetString(PrefKeys.OutputFolderGuid, string.Empty);

            _spellTsv = LoadAssetByGuid<TextAsset>(spellGuid);
            _equipmentTsv = LoadAssetByGuid<TextAsset>(equipGuid);
            _permanentTsv = LoadAssetByGuid<TextAsset>(permGuid);
            _eventTsv = LoadAssetByGuid<TextAsset>(eventGuid);
_outputFolder = LoadAssetByGuid<DefaultAsset>(folderGuid);
        }
        
        private static string GetGuid(UnityEngine.Object obj)
        {
            if (obj == null) return string.Empty;

            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return AssetDatabase.AssetPathToGUID(path);
        }

        private static T LoadAssetByGuid<T>(string guid) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(guid)) return null;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path)) return null;

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        #endregion
    }
}
#endif
