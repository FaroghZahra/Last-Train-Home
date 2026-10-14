using System;
using System.Globalization;
using LastTrainHome.Domain;
using UnityEngine;

namespace LastTrainHome.Data
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

        /// <summary>e.g. "Late · 00:50 (10 min late)", coloured by outcome.</summary>
        public string FormatStatus(PassengerOutcome outcome)
        {
            string text = outcome.Status switch
            {
                ArrivalStatus.OnTime => $"On time · {outcome.Arrival}",
                ArrivalStatus.Late => $"Late · {outcome.Arrival} ({outcome.MinutesLate} min late)",
                ArrivalStatus.NotReached => "Not reached",
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome.Status, null),
            };
            return Colorize(text, outcome.Status);
        }

        public string Colorize(string text, ArrivalStatus status) =>
            $"<color=#{ColorUtility.ToHtmlStringRGB(GetColor(status))}>{text}</color>";

        /// <summary>Smaller, italic and muted. Returns an empty string for empty input.</summary>
        public string FormatStory(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string percent = Mathf.RoundToInt(_storyTextScale * 100f).ToString(CultureInfo.InvariantCulture);
            return $"<size={percent}%><i><color=#{ColorUtility.ToHtmlStringRGB(_storyColor)}>{text}</color></i></size>";
        }

        private Color GetColor(ArrivalStatus status) => status switch
        {
            ArrivalStatus.OnTime => _onTimeColor,
            ArrivalStatus.Late => _lateColor,
            ArrivalStatus.NotReached => _notReachedColor,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
