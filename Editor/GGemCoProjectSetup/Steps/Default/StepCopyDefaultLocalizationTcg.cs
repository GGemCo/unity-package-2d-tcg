using System.Collections.Generic;
using System.IO;
using GGemCo2DCore;

namespace GGemCo2DCoreEditor
{
    /// <summary>
    /// TCG 기본 로컬라이제이션(Localization) 문자열 테이블을
    /// 패키지 샘플 경로에서 프로젝트 로컬 경로로 복사하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// Localization StringTable의 초기 구조(Ability, Lifetime, UIWindow)를
    /// 프로젝트에 자동으로 배치하는 용도로 사용됩니다.
    /// </remarks>
    public class StepCopyDefaultLocalizationTcg : SetupStepBase
    {
        /// <summary>
        /// 패키지 내 기본 로컬라이제이션 샘플 경로입니다.
        /// </summary>
        private const string PathSrc =
            "Packages/com.ggemco.2d.tcg/Samples~/Localization/StringTable";

        /// <summary>
        /// 프로젝트 내 로컬라이제이션 데이터가 복사될 대상 경로입니다.
        /// </summary>
        private const string PathDist =
            ConfigDefine.PathGGemCo + "/Localization/StringTable";

        /// <summary>
        /// 복사 대상이 되는 소스 하위 폴더 목록입니다.
        /// </summary>
        private readonly List<string> _pathSrc = new List<string>
        {
            $"{PathSrc}/Ability",
            $"{PathSrc}/Lifetime",
            $"{PathSrc}/UIWindow",
        };

        /// <summary>
        /// 각 소스 폴더에 대응하는 대상 하위 폴더 목록입니다.
        /// 인덱스 기준으로 <see cref="_pathSrc"/>와 1:1 매칭됩니다.
        /// </summary>
        private readonly List<string> _pathDist = new List<string>
        {
            $"{PathDist}/Ability",
            $"{PathDist}/Lifetime",
            $"{PathDist}/UIWindow",
        };

        /// <summary>
        /// 로컬라이제이션 복사를 수행하기 전에,
        /// 모든 소스 폴더가 정상적으로 존재하는지 검증합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 로그 출력 및 공통 설정에 사용됩니다.
        /// </param>
        /// <param name="msg">
        /// 검증 실패 시 에디터에 표시될 오류 메시지입니다.
        /// </param>
        /// <returns>
        /// 모든 소스 폴더가 존재하면 <c>true</c>,
        /// 하나라도 누락된 경우 <c>false</c>를 반환합니다.
        /// </returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            foreach (var src in _pathSrc)
            {
                // 내용이 비어 있는 테이블(또는 placeholder)로 간주되는 항목은 검증에서 제외
                // TODO: 실제 빈 테이블 판별 로직이 필요하다면 명확한 조건으로 교체 필요
                if (src == "EmptyDataTable") continue;

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
        /// 검증이 완료된 후, 기본 로컬라이제이션 문자열 테이블을
        /// 프로젝트 대상 경로로 복사합니다.
        /// </summary>
        /// <param name="ctx">
        /// 에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.
        /// </param>
        public override void Execute(EditorSetupContext ctx)
        {
            for (int i = 0; i < _pathSrc.Count; ++i)
            {
                var src = _pathSrc[i];
                var dist = _pathDist[i];

                // 소스 디렉터리를 대상 경로로 재귀 복사
                HelperFile.CopyDirectory(src, dist);

                // 에디터 로그에 복사 결과를 기록
                HelperLog.Info(
                    $"[{nameof(StepCopyDefaultLocalizationTcg)}] {src} -> {dist} 복사",
                    ctx);
            }
        }
    }
}
