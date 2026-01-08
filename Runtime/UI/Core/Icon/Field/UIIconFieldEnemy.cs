using GGemCo2DCore;
using UnityEngine.EventSystems;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 적 필드(Field Enemy)에 배치된 카드 아이콘을 표현하는 UI 컴포넌트입니다.
    /// <see cref="UIIconCard"/> 기반으로 카드 메타/이미지를 표시하며,
    /// 테이블에 존재하지 않는 카드 UID가 들어오는 경우를 방어합니다.
    /// </summary>
    public class UIIconFieldEnemy : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 카드 정보(툴팁/상세)를 표시하는 UI 윈도우입니다.
        /// </summary>
        private UIWindowTcgCardInfo _windowTcgCardInfo;

        /// <summary>
        /// 필드에 놓인 카드의 전투 상태/런타임 데이터를 담는 객체입니다.
        /// (예: 소유자, 위치, 현재 스탯, 공격 가능 여부 등)
        /// </summary>
        private TcgBattleDataCardInField _tcgBattleDataCardInField;

        // private readonly CompositeDisposable _bindDisposables = new();

        /// <summary>
        /// Unity 생명주기: 오브젝트 초기화 시 호출됩니다.
        /// 필드용 카드 보더(border) 리소스 키 프리픽스를 설정합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            borderKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageBorderField}_";
        }

        /// <summary>
        /// Unity 생명주기: 첫 프레임 시작 시 호출됩니다.
        /// 카드 정보 윈도우 참조를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }

        /// <summary>
        /// Unity 생명주기: 오브젝트가 비활성화될 때 호출됩니다.
        /// 카드 정보 창이 떠있다면 닫아 잔상/오표시를 방지합니다.
        /// </summary>
        private void OnDisable()
        {
            // 선택/오버 UI 이미지는 현재 로직에서 관리하지 않도록 주석 처리된 상태입니다.
            _windowTcgCardInfo?.Show(false);
        }

        /// <summary>
        /// 카드 UID를 기준으로 아이콘 표시 정보를 갱신합니다.
        /// 적 필드 카드의 경우, 먼저 카드 테이블에 UID가 존재하는지 검증한 뒤
        /// 기본 카드 아이콘 갱신 로직(<see cref="UIIconCard.ChangeInfoByUid"/>)을 수행합니다.
        /// </summary>
        /// <param name="cardUid">표시할 카드의 고유 UID입니다.</param>
        /// <param name="iconCount">수량(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconLevel">레벨(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconIsLearn">학습/해금 여부(기본 구현과의 호환용)입니다.</param>
        /// <param name="remainCoolTime">남은 쿨타임(기본 구현과의 호환용)입니다.</param>
        /// <returns>테이블 검증 및 갱신 성공 시 true, 실패 시 false입니다.</returns>
        public override bool ChangeInfoByUid(
            int cardUid,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            // 테이블에 없는 카드 UID가 들어오면 UI 표시는 중단하고 오류를 남깁니다.
            var info = tableTcgCard.GetDataByUid(cardUid);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {cardUid}");
                return false;
            }

            base.ChangeInfoByUid(cardUid, iconCount, iconLevel, iconIsLearn, remainCoolTime);
            return true;
        }

        /// <summary>
        /// 마우스 포인터가 아이콘 위로 올라왔을 때 호출됩니다.
        /// 현재는 별도 처리 로직이 없으며, 필요 시 오버 표시/카드 정보 표시 등을 구현할 수 있습니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        /// <summary>
        /// 마우스 포인터가 아이콘을 벗어났을 때 호출됩니다.
        /// 현재는 별도 처리 로직이 없으며, 필요 시 오버 표시 해제/카드 정보 숨김 등을 구현할 수 있습니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
        }

        /// <summary>
        /// 적 필드 카드 아이콘 클릭 시 호출됩니다.
        /// 전투 런타임 데이터(<see cref="TcgBattleDataCardInField"/>)가 준비되지 않은 경우를 방어합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public override void OnPointerClick(PointerEventData eventData)
        {
            // NOTE: 기존 주석(덱 선택 시 덱에 포함)은 이 클래스(UIIconFieldEnemy)의 역할과 맞지 않아 제거/수정했습니다.
            // TODO: 적 필드 카드 클릭 시 수행할 동작(예: 정보 표시, 타겟 선택, 공격 대상 지정 등)을 명확히 정의하고 구현하세요.

            if (_tcgBattleDataCardInField == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleDataCardInField)} 정보가 없습니다.");
                return;
            }

            // TODO: _tcgBattleDataCardInField 기반으로 클릭 동작을 수행합니다.
        }
    }
}
