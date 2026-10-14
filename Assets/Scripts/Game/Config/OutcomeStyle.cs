using System;
using LastTrainHome.Domain.Passengers;
using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>
    /// How outcomes and passengers' own words look wherever they appear (passenger list, arrival log, summary),
    /// so the three stay visually consistent.
    /// </summary>
    [CreateAssetMenu(fileName = "OutcomeStyle", menuName = "Last Train Home/Outcome Style")]
    public sealed class OutcomeStyle : ScriptableObject
    {
        [SerializeField] private Color _onTimeColor = new Color(0.45f, 0.85f, 0.5f);
        [SerializeField] private Color _lateColor = new Color(1f, 0.7f, 0.3f);
        [SerializeField] private Color _notReachedColor = new Color(0.55f, 0.58f, 0.65f);

        [SerializeField, Tooltip("Passengers' motivations and outcome lines.")]
        private Color _storyColor = new Color(0.7f, 0.74f, 0.82f);

        [SerializeField, Range(0.5f, 1f)] private float _storyTextScale = 0.8f;

        public Color StoryColor => _storyColor;

        public float StoryTextScale => _storyTextScale;

        public Color GetColor(ArrivalStatus status) => status switch
        {
            ArrivalStatus.OnTime => _onTimeColor,
            ArrivalStatus.Late => _lateColor,
            ArrivalStatus.NotReached => _notReachedColor,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
