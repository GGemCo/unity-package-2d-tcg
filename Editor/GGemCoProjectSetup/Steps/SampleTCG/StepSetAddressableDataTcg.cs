using GGemCo2DCoreEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// Addressables 데이터 구성을 추가로 적용하는 에디터 설정 스텝입니다.
    /// TCG 카드(Addressable) 설정을 구성하고, 윈도우 테이블을 다시 등록합니다.
    /// </summary>
    /// <remarks>
    /// Settings ScriptableObject 및 기본 테이블 구성은 다른 스텝(예: StepSetDefaultAddressableDataTcg)에서 처리되며,
    /// 이 스텝은 TCG 전용 추가 구성과 테이블 재등록을 담당합니다.
    /// </remarks>
    public class StepSetAddressableDataTcg : SetupStepBase
    {
        /// <summary>
        /// 이 스텝은 별도 사전 조건이 없으므로 항상 유효합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, Addressables 편집기 및 로그 처리에 사용됩니다.</param>
        /// <param name="msg">검증 메시지 출력용 파라미터로, 항상 <c>null</c>을 반환합니다.</param>
        /// <returns>항상 <c>true</c>를 반환합니다.</returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            msg = null;
            return true;
        }

        /// <summary>
        /// TCG 카드(Addressables) 구성을 적용하고, 윈도우 테이블을 재등록합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 설정 적용 및 로그 출력에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            // 컨텍스트에서 공유되는 Addressables 편집기(공통/기본 설정용)
            var addressableEditor = ctx.addressableEditor;

            // TCG 전용 Addressables 편집 유틸리티 인스턴스
            var addressableEditorTcg = ScriptableObject.CreateInstance<AddressableEditorTcg>();

            // NOTE: settings 스크립터블 오브젝트 및 기본 테이블 구성은
            //       다른 스텝(예: StepSetDefaultAddressableDataTcg)에서 처리한다.

            // TCG 카드(Addressable) 관련 그룹/엔트리/라벨 등을 구성
            var settingTcgCard = new SettingTcgCard(addressableEditorTcg);
            settingTcgCard.Setup(ctx);

            // 윈도우 테이블을 재등록(갱신)하여 최신 상태로 반영
            var settingTable = new SettingTable(addressableEditor);
            settingTable.Setup(ctx);
        }
    }
}