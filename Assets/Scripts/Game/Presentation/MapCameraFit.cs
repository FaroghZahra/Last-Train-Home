using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// Frames the stations in the part of the screen the side panels leave free, at any window size: it zooms so the
    /// stations' area fits between the panels and slides the camera so that area sits centred between them. Nothing on
    /// the map moves, so stations stay where the art expects them.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class MapCameraFit : MonoBehaviour
    {
        [SerializeField, Tooltip("World area that must stay visible: every station with its building and name.")]
        private Rect _content = Rect.MinMaxRect(-4.6f, -3.9f, 5.6f, 3f);

        [SerializeField, Tooltip("World size of the painted map, centred on the origin.")]
        private Vector2 _mapSize = new Vector2(17.78f, 10f);

        [SerializeField, Tooltip("Screen width the left and right panels cover, in canvas reference pixels.")]
        private Vector2 _panelInsets = new Vector2(420f, 380f);

        [SerializeField, Tooltip("The canvas's reference height; the canvas scales to match the screen's height.")]
        private float _referenceHeight = 1080f;

        [SerializeField, Min(1f), Tooltip("Breathing room around the content.")]
        private float _margin = 1f;

        private Camera _camera;
        private Vector2Int _fittedScreen;

        private void Awake() => _camera = GetComponent<Camera>();

        private void LateUpdate()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (screen == _fittedScreen || screen.y == 0)
            {
                return;
            }

            _fittedScreen = screen;
            float uiScale = screen.y / _referenceHeight;
            float left = _panelInsets.x * uiScale;
            float freeWidth = Mathf.Max(1f, screen.x - left - _panelInsets.y * uiScale);

            // World units per screen pixel is 2 * size / height; fit the content's width into the free strip and its
            // height into the screen.
            float sizeForWidth = _content.width * screen.y / (2f * freeWidth);
            float sizeForHeight = _content.height * 0.5f;
            _camera.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight) * _margin;

            float unitsPerPixel = 2f * _camera.orthographicSize / screen.y;
            float freeCentreOffset = (left + freeWidth * 0.5f - screen.x * 0.5f) * unitsPerPixel;
            var centre = new Vector2(_content.center.x - freeCentreOffset, _content.center.y);

            // Keep the view on the painted map where it's big enough, rather than showing past its edges.
            Vector2 halfView = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
            Vector2 halfMap = _mapSize * 0.5f;
            centre.x = halfView.x <= halfMap.x ? Mathf.Clamp(centre.x, halfView.x - halfMap.x, halfMap.x - halfView.x) : 0f;
            centre.y = halfView.y <= halfMap.y ? Mathf.Clamp(centre.y, halfView.y - halfMap.y, halfMap.y - halfView.y) : 0f;
            transform.position = new Vector3(centre.x, centre.y, transform.position.z);
        }
    }
}
