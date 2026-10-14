using System;
using System.Collections;
using LastTrainHome.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace LastTrainHome.World
{
    /// <summary>
    /// A clickable station on the map. Clicks arrive through the EventSystem, which needs a
    /// Physics2DRaycaster on the camera; UI on top of the map blocks them automatically.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class StationView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, FormerlySerializedAs("stationName"), Tooltip("Stable id used by night data, e.g. \"Old Town\".")]
        private string _id = "Station";

        [SerializeField, Tooltip("Shown on the map. Leave empty to show the id.")]
        private string _displayName;

        [SerializeField, FormerlySerializedAs("label")] private TMP_Text _label;

        [SerializeField, Min(0f), Tooltip("How far the name sits from the station's centre.")]
        private float _labelDistance = 0.6f;

        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private Color _refusedColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField, Min(0f)] private float _refusedFlashSeconds = 0.4f;

        private Color _bodyColor;
        private Coroutine _flash;

        public event Action<StationView> Clicked;

        public string Id => _id;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? _id : _displayName;

        public Vector3 Position => transform.position;

        /// <summary>Gives a station cloned from a template its identity and puts its name on one side.</summary>
        public void Configure(string id, string displayName, LabelSide labelSide)
        {
            _id = id;
            _displayName = displayName;
            name = id;
            ApplyLabel();
            PlaceLabel(labelSide);
        }

        /// <summary>Briefly tints the station to show a click was refused.</summary>
        public void FlashRefused()
        {
            if (_flash != null)
            {
                StopCoroutine(_flash);
            }

            _flash = StartCoroutine(FlashRefusedRoutine());
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Clicked?.Invoke(this);
            }
        }

        private void Awake()
        {
            if (!RequiredReferences.Check(this, (_label, nameof(_label)), (_body, nameof(_body))))
            {
                return;
            }

            _bodyColor = _body.color;
            ApplyLabel();
        }

        private void OnDisable()
        {
            if (_flash == null)
            {
                return;
            }

            StopCoroutine(_flash);
            _flash = null;
            _body.color = _bodyColor;
        }

        private IEnumerator FlashRefusedRoutine()
        {
            for (float elapsed = 0f; elapsed < _refusedFlashSeconds; elapsed += Time.deltaTime)
            {
                _body.color = Color.Lerp(_refusedColor, _bodyColor, elapsed / _refusedFlashSeconds);
                yield return null;
            }

            _body.color = _bodyColor;
            _flash = null;
        }

        private void ApplyLabel()
        {
            if (_label != null && _label.text != DisplayName)
            {
                _label.text = DisplayName;
            }
        }

        private void PlaceLabel(LabelSide side)
        {
            RectTransform rect = _label.rectTransform;
            (Vector2 pivot, Vector2 direction, TextAlignmentOptions alignment) = side switch
            {
                LabelSide.Above => (new Vector2(0.5f, 0.5f), Vector2.up, TextAlignmentOptions.Center),
                LabelSide.Below => (new Vector2(0.5f, 0.5f), Vector2.down, TextAlignmentOptions.Center),
                LabelSide.Left => (new Vector2(1f, 0.5f), Vector2.left, TextAlignmentOptions.Right),
                LabelSide.Right => (new Vector2(0f, 0.5f), Vector2.right, TextAlignmentOptions.Left),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
            };

            rect.pivot = pivot;
            rect.anchoredPosition = direction * _labelDistance;
            _label.alignment = alignment;
        }

        private void Reset()
        {
            _label = GetComponentInChildren<TMP_Text>();
            _body = GetComponentInChildren<SpriteRenderer>();
        }

#if UNITY_EDITOR
        // Deferred: changing TMP text inside OnValidate triggers "SendMessage cannot be called during OnValidate".
        private void OnValidate() => UnityEditor.EditorApplication.delayCall += ApplyLabelIfAlive;

        private void ApplyLabelIfAlive()
        {
            if (this != null)
            {
                ApplyLabel();
            }
        }
#endif
    }
}
