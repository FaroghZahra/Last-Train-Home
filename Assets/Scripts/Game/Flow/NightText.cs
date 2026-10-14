using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Game.Config;
using UnityEngine;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// Every line of night text the player reads (clock, passenger rows, arrival log, summary), styled with
    /// <see cref="OutcomeStyle"/> so the same outcome always looks the same.
    /// </summary>
    public sealed class NightText
    {
        private const string StoryIndent = "      ";

        private readonly OutcomeStyle _style;

        public NightText(OutcomeStyle style) => _style = style != null ? style : throw new ArgumentNullException(nameof(style));

        public string PlannedClock(ClockTime routeEnd, ClockTime closing) =>
            $"Route ends {routeEnd}   ·   line closes {closing}";

        public string CurrentClock(ClockTime now, ClockTime closing) => $"Now {now}   ·   line closes {closing}";

        /// <summary>Who they are, what they said, and how the planned route treats them.</summary>
        public string PassengerRow(Passenger passenger, PassengerOutcome outcome, string destinationName)
        {
            string heading = $"<b>{passenger.Name}</b>, to {destinationName} by {passenger.Deadline}";
            string motivation = Story(Quote(passenger.Story.Motivation));
            string status = Status(outcome);
            return motivation.Length > 0 ? $"{heading}\n{motivation}\n{status}" : $"{heading}\n{status}";
        }

        public string Arrival(Passenger passenger, PassengerOutcome outcome, string stationName)
        {
            string timing = outcome.Status == ArrivalStatus.OnTime ? "on time" : $"{outcome.MinutesLate} min late";
            string line = Colorize($"{outcome.Arrival}  <b>{passenger.Name}</b> reached {stationName}, {timing}.", outcome.Status);
            return WithStory(line, passenger.Story.GetOutcomeLine(outcome.Status), StoryIndent);
        }

        public string NotReached(Passenger passenger, string destinationName)
        {
            string line = Colorize($"<b>{passenger.Name}</b> never reached {destinationName}.", ArrivalStatus.NotReached);
            return WithStory(line, passenger.Story.GetOutcomeLine(ArrivalStatus.NotReached), StoryIndent);
        }

        public string EndOfLine(string stationName, ClockTime time) => $"End of the line at {stationName}, {time}.";

        /// <param name="outcomes">One per passenger, in the same order.</param>
        public string Summary(IReadOnlyList<Passenger> passengers, IReadOnlyList<PassengerOutcome> outcomes)
        {
            if (passengers.Count != outcomes.Count)
            {
                throw new ArgumentException("Every passenger needs exactly one outcome.", nameof(outcomes));
            }

            int onTime = outcomes.Count(outcome => outcome.Status == ArrivalStatus.OnTime);
            IEnumerable<string> entries = passengers.Select((passenger, i) => WithStory(
                $"<b>{passenger.Name}</b>  ·  {Status(outcomes[i])}",
                passenger.Story.GetOutcomeLine(outcomes[i].Status),
                string.Empty));
            return $"{onTime} of {passengers.Count} arrived on time.\n\n{string.Join("\n\n", entries)}";
        }

        /// <summary>e.g. "Late · 00:50 (10 min late)", coloured by outcome.</summary>
        private string Status(PassengerOutcome outcome)
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

        private string Colorize(string text, ArrivalStatus status) =>
            $"<color=#{ColorUtility.ToHtmlStringRGB(_style.GetColor(status))}>{text}</color>";

        private string WithStory(string line, string story, string indent)
        {
            string styled = Story(story);
            return styled.Length > 0 ? $"{line}\n{indent}{styled}" : line;
        }

        /// <summary>Smaller, italic and muted. Empty for empty input.</summary>
        private string Story(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string percent = Mathf.RoundToInt(_style.StoryTextScale * 100f).ToString(CultureInfo.InvariantCulture);
            return $"<size={percent}%><i><color=#{ColorUtility.ToHtmlStringRGB(_style.StoryColor)}>{text}</color></i></size>";
        }

        private static string Quote(string text) => string.IsNullOrWhiteSpace(text) ? string.Empty : $"“{text}”";
    }
}
