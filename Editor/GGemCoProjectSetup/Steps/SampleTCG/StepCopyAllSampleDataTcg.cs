using System.Collections.Generic;
using System.IO;
using GGemCo2DCore;
using GGemCo2DCoreEditor;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 패키지의 Samples~ 내 샘플 데이터 폴더들을 프로젝트 기본 경로로 일괄 복사하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 복사 대상은 <c>Packages/com.ggemco.2d.tcg/Samples~/</c> 하위 폴더이며,
    /// 대상 경로는 <see cref="ConfigDefine.PathGGemCo"/> 하위로 구성됩니다.
    /// </para>
    /// <para>
    /// 폴더 처리 순서가 중요할 수 있으므로(<c>Data</c> → <c>DataAddressable</c> → <c>Localization</c> → <c>UIWindows</c>),
    /// 목록 순서를 변경하지 않는 것을 권장합니다.
    /// </para>
    /// </remarks>
    public class StepCopyAllSampleDataTcg : SetupStepBase
    {
        /// <summary>
        /// 복사할 샘플 폴더 이름 목록입니다. (목록의 순서가 의미를 가질 수 있습니다.)
        /// </summary>
        private readonly List<string> _folderName = new List<string>
        {
            // 폴더 순서 중요
            "Data",
            "DataAddressable",
            "Localization",
            "UIWindows"
        };

        /// <summary>
        /// 복사 대상 샘플 폴더들이 패키지 내에 존재하는지 검증합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        /// <param name="msg">검증 실패 시 에디터에 표시될 메시지입니다.</param>
        /// <returns>
        /// 모든 소스 폴더가 존재하면 <c>true</c>,
        /// 하나라도 누락되면 <c>false</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            foreach (var name in _folderName)
            {
                // NOTE: 과거/다른 스텝과 공통 패턴으로 남아있는 예외 처리로 보이며,
                //       현재 _folderName에는 "EmptyDataTable"이 포함되어 있지 않습니다.
                if (name == "EmptyDataTable") continue;

                var src = $"Packages/com.ggemco.2d.tcg/Samples~/{name}";
                if (!Directory.Exists(src))
                {
                    msg = $"소스 폴더 없음: {src}";
                    return false;
                }
            }

            msg = null;
            return true;
        }

        /// <summary>
        /// 샘플 폴더들을 프로젝트 대상 경로로 순차 복사하고, 복사 결과를 로그로 남깁니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            foreach (var name in _folderName)
            {
                var sourceFolder = $"Packages/com.ggemco.2d.tcg/Samples~/{name}";
                var targetFolder = $"{ConfigDefine.PathGGemCo}/{name}";

                // 샘플 폴더를 프로젝트 경로로 재귀 복사
                HelperFile.CopyDirectory(sourceFolder, targetFolder);

                // 복사 완료 로그 출력
                HelperLog.Info(
                    $"[{nameof(StepCopyAllSampleDataTcg)}] {sourceFolder} -> {targetFolder} 복사 완료.",
                    ctx);
            }
        }
    }
}
