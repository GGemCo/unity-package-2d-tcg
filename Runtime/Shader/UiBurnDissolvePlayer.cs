using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DCore
{
    public sealed class UiBurnDissolvePlayer : MonoBehaviour
    {
        [SerializeField] private Image[] targets;
        [SerializeField] private Material sourceMaterial;
        [SerializeField] private float duration = 0.8f;

        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        private Material _runtimeMat;

        private void Awake()
        {
            if (targets == null || sourceMaterial == null) return;

            // 사망 연출 시점에만 인스턴스 생성(배칭/메모리 고려)
            _runtimeMat = Instantiate(sourceMaterial);
        }

        private void Start()
        {
            StopAllCoroutines();
            // StartCoroutine(CoPlay());
        }

        public IEnumerator CoPlay()
        {
            foreach (var image in targets)
            {
                image.material = _runtimeMat;
                image.SetMaterialDirty();
                image.SetVerticesDirty();
            }
            
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = Mathf.Clamp01(t / duration);
                _runtimeMat.SetFloat(DissolveId, v);
                yield return null;
            }

            // 여기에서 카드 오브젝트 제거/풀 반환
            foreach (var image in targets)
            {
                image.material = null;
            }
        }

        private void OnDestroy()
        {
            if (_runtimeMat != null) Destroy(_runtimeMat);
        }
    }
}