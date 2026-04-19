using UnityEngine;

namespace Match3
{
    // 보드(8x8)가 어떤 해상도/종횡비에서도 화면에 꽉 차도록
    // 카메라의 orthographicSize를 런타임에 자동 계산합니다.
    [RequireComponent(typeof(Camera))]
    public class BoardCameraFit : MonoBehaviour
    {
        [Header("Board Config")]
        [SerializeField] private int boardXDim = 8;
        [SerializeField] private int boardYDim = 8;
        [SerializeField] private float padding = 0.5f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Start()
        {
            Fit();
        }

        private void Fit()
        {
            if (_cam == null) { return; }

            float boardWidth  = boardXDim + padding * 2f;
            float boardHeight = boardYDim + padding * 2f;
            float screenAspect = (float)Screen.width / Screen.height;

            float sizeByHeight = boardHeight / 2f;
            float sizeByWidth  = boardWidth / (2f * screenAspect);

            _cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

            var pos = transform.position;
            pos.x = 0f;
            pos.y = 0f;
            transform.position = pos;
        }
    }
}
