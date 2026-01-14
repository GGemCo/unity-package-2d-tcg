using System.IO;
using GGemCo2DCore;
using GGemCo2DCoreEditor;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 패키지 내 Samples~/EmptyDataTable 폴더를
    /// 프로젝트의 Addressable Tables 경로로 복사하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// 실제 데이터가 없는 초기 상태에서도 Addressable 기반 테이블 구조를
    /// 미리 구성하기 위한 용도로 사용됩니다.
    /// </remarks>
    public class StepCopyEmptyDataTableTcg : SetupStepBase
    {
        /// <summary>
        /// 패키지에 포함된 빈 데이터 테이블 샘플 소스 경로입니다.
        /// </summary>
        private readonly string _srcTables =
            ConfigEditorTcg.PathPackageCore + "/Samples~/EmptyDataTable";

        /// <summary>
        /// EmptyDataTable이 복사될 Addressable Tables 대상 루트 경로입니다.
        /// </summary>
        private readonly string _dstTables =
            ConfigAddressablePath.Root + "/Tables";

        /// <summary>
        /// EmptyDataTable 복사를 수행하기 전에,
        /// 소스 폴더가 정상적으로 존재하는지 검증합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 공통 설정 및 로그 출력에 사용됩니다.
        /// </param>
        /// <param name="msg">
        /// 검증 실패 시 에디터에 표시될 오류 메시지입니다.
        /// </param>
        /// <returns>
        /// 소스 폴더가 존재하면 <c>true</c>,
        /// 존재하지 않으면 <c>false</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            var src = _srcTables;

            if (!Directory.Exists(src))
            {
                msg = $"소스 폴더 없음: {src}";
                return false;
            }

            msg = null;
            return true;
        }

        /// <summary>
        /// EmptyDataTable 폴더를 Addressable Tables 경로로 복사합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.
        /// </param>
        public override void Execute(EditorSetupContext ctx)
        {
            // 빈 데이터 테이블 샘플을 Addressable Tables 경로로 복사
            HelperFile.CopyDirectory(_srcTables, _dstTables);

            // 복사 완료 로그 출력
            HelperLog.Info(
                $"{_srcTables} -> {_dstTables} 경로로 복사 완료했습니다.",
                ctx);
        }
    }
}
