using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어가 현재 보유한 핸드 UI를 담당하는 윈도우입니다.
    /// <para>- 마나 텍스트 및 마나 토글(구슬) 표시</para>
    /// <para>- 플레이어 전용 아이콘/드래그 전략 제공</para>
    /// </summary>
    public class UIWindowTcgHandPlayer : UIWindowTcgHandBase
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("마나 토글 컴포넌트를 넣을 Transform")]
        public Transform containerToggleMana;

        [Tooltip("마나 토글 프리팹")]
        public GameObject prefabToggleMana;

        /// <summary>
        /// 생성된 마나 토글 목록입니다. 인덱스가 마나 슬롯(0..N-1)에 대응합니다.
        /// </summary>
        private readonly List<Toggle> _toggleManaList = new List<Toggle>();

        /// <summary>
        /// 플레이어 핸드 윈도우의 고정 UID를 반환합니다.
        /// </summary>
        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgHandPlayer;

        /// <summary>
        /// 플레이어 핸드 아이콘 세팅 핸들러를 생성합니다.
        /// </summary>
        /// <returns>플레이어 핸드용 아이콘 세팅 핸들러.</returns>
        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerHandPlayer();

        /// <summary>
        /// 플레이어 핸드 드래그/드랍 전략을 생성합니다.
        /// </summary>
        /// <returns>플레이어 핸드용 드래그/드랍 전략.</returns>
        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyHandPlayer();

        /// <summary>
        /// 플레이어 전용 초기화(마나 토글 생성)를 수행합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            CreateToggleMana();
        }

        /// <summary>
        /// 전투에서 사용될 최대 마나 수만큼 마나 토글 UI를 생성하고 초기 상태로 설정합니다.
        /// </summary>
        /// <remarks>
        /// NOTE: <see cref="AddressableLoaderSettingsTcg"/> 또는 프리팹/컨테이너가 준비되지 않은 경우
        /// 토글 생성이 스킵될 수 있습니다.
        /// </remarks>
        private void CreateToggleMana()
        {
            if (!AddressableLoaderSettingsTcg.Instance) return;

            int countMaxManaInBattle = AddressableLoaderSettingsTcg.Instance
                .tcgSettings.countMaxManaInBattle;

            _toggleManaList.Clear();

            for (int i = 0; i < countMaxManaInBattle; i++)
            {
                // 프리팹 기반으로 토글 오브젝트 생성(기본은 비활성화)
                var go = Instantiate(prefabToggleMana, containerToggleMana);
                go.SetActive(false);

                var toggle = go.GetComponent<Toggle>();
                if (toggle == null)
                {
                    GcLogger.LogError($"마나 토글 프리팹에 {nameof(Toggle)} 컴포넌트가 없습니다.");
                    continue;
                }

                toggle.isOn = false;
                _toggleManaList.Add(toggle);
            }
        }

        /// <summary>
        /// 현재/최대 마나를 표시합니다.
        /// 기본 텍스트 표시 후, 플레이어 전용 마나 토글의 활성/체크 상태를 갱신합니다.
        /// </summary>
        /// <param name="currentMana">현재 마나.</param>
        /// <param name="maxMana">최대 마나.</param>
        public override void SetMana(int currentMana, int maxMana)
        {
            // 1) 기본 텍스트 처리
            base.SetMana(currentMana, maxMana);

            // 2) Player 전용 토글 처리
            var count = Mathf.Min(maxMana, _toggleManaList.Count);
            for (int i = 0; i < _toggleManaList.Count; i++)
            {
                var toggle = _toggleManaList[i];
                if (toggle == null) continue;

                // 최대 마나까지만 표시
                toggle.gameObject.SetActive(i < count);

                // 현재 마나 수만큼 점등(ON)
                toggle.isOn = i < currentMana;
            }
        }
    }
}
