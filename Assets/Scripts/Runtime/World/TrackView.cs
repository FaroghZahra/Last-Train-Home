using System;
using LastTrainHome.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace LastTrainHome.World
{
    /// <summary>
    /// A track drawn between two stations, labelled with its travel time. In the editor it follows its stations
    /// as they are dragged; in play it is drawn once.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class TrackView : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("a")] private StationView _from;
        [SerializeField, FormerlySerializedAs("b")] private StationView _to;
        [SerializeField, FormerlySerializedAs("minutes"), Min(1)] private int _minutes = 10;
        [SerializeField, FormerlySerializedAs("minutesLabel")] private TMP_Text _minutesLabel;

        [SerializeField, FormerlySerializedAs("labelOffset"), Tooltip("How far the minutes label sits from the line.")]
        private float _labelOffset = 0.55f;

        private LineRenderer _line;

        public bool IsConnected => _from != null && _to != null;

        /// <summary>Joins a track cloned from a template to its stations.</summary>
        public void Connect(StationView from, StationView to, int minutes)
        {
            _from = from;
            _to = to;
            _minutes = minutes;
            name = $"{from.Id} - {to.Id}";
            if (_line != null)
            {
                Redraw();
            }
        }

        /// <exception cref="InvalidOperationException">Either end is unassigned.</exception>
        public Track ToTrack()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException($"Track '{name}' needs both stations assigned.");
            }

            return new Track(_from.Id, _to.Id, _minutes);
        }

        private void OnEnable()
        {
            _line = GetComponent<LineRenderer>();
            Redraw();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                Redraw();
            }
        }
#endif

        private void Redraw()
        {
            if (!IsConnected)
            {
                return;
            }

            Vector3 from = _from.Position;
            Vector3 to = _to.Position;
            _line.positionCount = 2;
            _line.SetPosition(0, from);
            _line.SetPosition(1, to);

            if (_minutesLabel == null)
            {
                return;
            }

            // Perpendicular to the line, always on its upper side so labels read consistently.
            Vector3 side = Vector3.Cross((to - from).normalized, Vector3.forward);
            if (side.y < 0f)
            {
                side = -side;
            }

            _minutesLabel.transform.position = (from + to) * 0.5f + side * _labelOffset;

            string text = $"{_minutes} min";
            if (_minutesLabel.text != text)
            {
                _minutesLabel.text = text;
            }
        }
    }
}
