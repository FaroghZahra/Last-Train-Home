using System.Collections.Generic;
using LastTrainHome.Game.Flow;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>A track drawn along a path on the map, labelled with its travel time halfway along.</summary>
    [DisallowMultipleComponent]
    public sealed class TrackView : MonoBehaviour
    {
        [SerializeField, Tooltip("Rails; its material tiles along the line, so it needs Texture Mode set to Tile.")]
        private LineRenderer _line;

        [SerializeField] private TMP_Text _minutesLabel;

        [SerializeField, Tooltip("How far the minutes label sits from the line.")]
        private float _labelOffset = 0.55f;

        [SerializeField, Range(0f, 1f), Tooltip("How visible a closed track is.")]
        private float _closedAlpha = 0.35f;

        /// <param name="label">Shown halfway along, e.g. "10 min" or "closed".</param>
        public void Draw(string trackName, IReadOnlyList<Vector3> path, string label, bool isClosed)
        {
            name = trackName;
            _line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                _line.SetPosition(i, path[i]);
            }

            // Perpendicular to the track at its midpoint, always on the upper side so labels read consistently.
            Vector3 middle = Polyline.PointAt(path, 0.5f);
            Vector3 along = Polyline.PointAt(path, 0.55f) - Polyline.PointAt(path, 0.45f);
            Vector3 side = Vector3.Cross(along.normalized, Vector3.forward);
            if (side.y < 0f)
            {
                side = -side;
            }

            _minutesLabel.transform.position = middle + side * _labelOffset;
            _minutesLabel.SetText(label);
            float alpha = isClosed ? _closedAlpha : 1f;
            _line.startColor = WithAlpha(_line.startColor, alpha);
            _line.endColor = WithAlpha(_line.endColor, alpha);
            _minutesLabel.alpha = isClosed ? Mathf.Max(_closedAlpha, 0.7f) : 1f;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
