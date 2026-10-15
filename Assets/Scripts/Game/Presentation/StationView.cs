using System;
using System.Collections;
using LastTrainHome.Game.Content;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// A clickable station on the map: a pin at the station's point with its building behind it. Clicks arrive through
    /// the EventSystem, which needs a Physics2DRaycaster on the camera; UI on top of the map blocks them automatically.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StationView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private SpriteRenderer _building;

        [SerializeField, Tooltip("Resized to cover the pin and the building, so either can be clicked.")]
        private BoxCollider2D _hitArea;

        [SerializeField] private Color _refusedColor = new Color(1f, 0.3f, 0.3f);

        [SerializeField, Tooltip("How much a station the train can go to next swells and shrinks.")]
        private float _highlightPulse = 0.18f;

        [SerializeField, Min(0.1f), Tooltip("Pulses per second for a station the train can go to next.")]
        private float _highlightRate = 1.4f;

        [SerializeField, Range(0f, 1f), Tooltip("How visible a closed station's building is.")]
        private float _closedBuildingAlpha = 0.45f;
        [SerializeField, Min(0f)] private float _refusedFlashSeconds = 0.4f;

        [SerializeField, Min(0f), Tooltip("How far the name sits from the pin's centre; it overlaps the building, like a map tag.")]
        private float _labelDistance = 0.42f;

        [SerializeField, Tooltip("Sliced sprite behind the name, a child of the label; resized to fit it.")]
        private SpriteRenderer _labelTag;

        [SerializeField] private Vector2 _labelTagPadding = new Vector2(0.18f, 0.08f);

        [SerializeField, Tooltip("The tag's pointer, drawn pointing down with its pivot at the top; turned to face the pin.")]
        private SpriteRenderer _labelTail;

        [SerializeField, Tooltip("How far the pointer tucks under the tag's edge, to hide the border where they meet.")]
        private float _labelTailOverlap = 0.035f;

        private string _id;
        private Color _bodyColor;
        private Coroutine _flash;
        private bool _isHighlighted;
        private Vector3 _bodyScale;

        /// <summary>Raised with this station's id.</summary>
        public event Action<string> Clicked;

        /// <summary>
        /// Gives a station cloned from a template its identity, its building (null for the pin alone), its pin colour
        /// and puts its name on one side.
        /// </summary>
        /// <param name="isClosed">Shown faded; the presenter refuses clicks on it.</param>
        public void Configure(string id, string displayName, LabelSide labelSide, Sprite building, Color pinColor,
            bool isClosed = false)
        {
            _id = id;
            name = id;
            _bodyColor = pinColor;
            _body.color = pinColor;
            _building.sprite = building;
            _building.enabled = building != null;
            _building.color = new Color(1f, 1f, 1f, isClosed ? _closedBuildingAlpha : 1f);

            Bounds art = LocalBounds(_body);
            if (building != null)
            {
                art.Encapsulate(LocalBounds(_building));
            }

            _hitArea.offset = art.center;
            _hitArea.size = art.size;

            _label.SetText(displayName);
            PlaceLabel(labelSide);
        }

        /// <summary>Makes the pin pulse to show the train can go here next.</summary>
        public void SetHighlighted(bool isHighlighted)
        {
            _isHighlighted = isHighlighted;
            if (!isHighlighted)
            {
                _body.transform.localScale = _bodyScale;
            }
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
                Clicked?.Invoke(_id);
            }
        }

        private void Awake()
        {
            _bodyColor = _body.color;
            _bodyScale = _body.transform.localScale;
        }

        private void Update()
        {
            if (_isHighlighted)
            {
                float pulse = 1f + _highlightPulse * (0.5f + 0.5f * Mathf.Sin(Time.time * _highlightRate * 2f * Mathf.PI));
                _body.transform.localScale = _bodyScale * pulse;
            }
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

        private static Bounds LocalBounds(SpriteRenderer renderer)
        {
            Transform child = renderer.transform;
            Bounds bounds = renderer.sprite.bounds;
            return new Bounds(child.localPosition + Vector3.Scale(bounds.center, child.localScale),
                Vector3.Scale(bounds.size, child.localScale));
        }

        private void PlaceLabel(LabelSide side)
        {
            (Vector2 pivot, Vector2 direction, TextAlignmentOptions alignment) = side switch
            {
                LabelSide.Above => (new Vector2(0.5f, 0.5f), Vector2.up, TextAlignmentOptions.Center),
                LabelSide.Below => (new Vector2(0.5f, 0.5f), Vector2.down, TextAlignmentOptions.Center),
                LabelSide.Left => (new Vector2(1f, 0.5f), Vector2.left, TextAlignmentOptions.Right),
                LabelSide.Right => (new Vector2(0f, 0.5f), Vector2.right, TextAlignmentOptions.Left),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
            };

            RectTransform rect = _label.rectTransform;
            rect.pivot = pivot;
            rect.anchoredPosition = direction * _labelDistance;
            _label.alignment = alignment;

            // The tag behind the name hugs the text as laid out, and its pointer sits on the edge facing the pin, so
            // the name reads as a speech bubble coming from the station.
            _label.ForceMeshUpdate();
            Bounds text = _label.textBounds;
            Vector2 tagSize = (Vector2)text.size + _labelTagPadding * 2f;
            _labelTag.transform.localPosition = new Vector3(text.center.x, text.center.y, 0f);
            _labelTag.size = tagSize;

            Vector2 towardsPin = -direction;
            Vector2 edge = (Vector2)text.center + Vector2.Scale(towardsPin, tagSize * 0.5f) - towardsPin * _labelTailOverlap;
            _labelTail.transform.localPosition = new Vector3(edge.x, edge.y, 0f);
            float angle = Mathf.Atan2(towardsPin.y, towardsPin.x) * Mathf.Rad2Deg + 90f;
            _labelTail.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
