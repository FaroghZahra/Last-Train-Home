using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>A track drawn between two points on the map, labelled with its travel time.</summary>
    [DisallowMultipleComponent]
    public sealed class TrackView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField] private TMP_Text _minutesLabel;

        [SerializeField, Tooltip("How far the minutes label sits from the line.")]
        private float _labelOffset = 0.55f;

        public void Draw(string trackName, Vector3 from, Vector3 to, int minutes)
        {
            name = trackName;
            _line.positionCount = 2;
            _line.SetPosition(0, from);
            _line.SetPosition(1, to);

            // Perpendicular to the line, always on its upper side so labels read consistently.
            Vector3 side = Vector3.Cross((to - from).normalized, Vector3.forward);
            if (side.y < 0f)
            {
                side = -side;
            }

            _minutesLabel.transform.position = (from + to) * 0.5f + side * _labelOffset;
            _minutesLabel.SetText("{0} min", minutes);
        }
    }
}
