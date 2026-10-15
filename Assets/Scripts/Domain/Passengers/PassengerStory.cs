using System;
using LastTrainHome.Domain.Clock;

namespace LastTrainHome.Domain.Passengers
{
    /// <summary>
    /// The words a passenger brings tonight: what they say on the platform, what they answer when asked, and a story for
    /// each way the night can end.
    /// </summary>
    public sealed class PassengerStory
    {
        public static readonly PassengerStory Empty = new PassengerStory(null, null, null, false, null, null, null);

        private readonly OutcomeStory _onTime;
        private readonly OutcomeStory _late;
        private readonly OutcomeStory _notReached;

        /// <param name="statedDeadline">The deadline they give until they're asked, when it isn't the real one.</param>
        public PassengerStory(string platformLine, string planQuote, string askLine, bool revealsBigNight,
            OutcomeStory onTime, OutcomeStory late, OutcomeStory notReached, ClockTime? statedDeadline = null)
        {
            StatedDeadline = statedDeadline;
            PlatformLine = platformLine ?? string.Empty;
            PlanQuote = string.IsNullOrWhiteSpace(planQuote) ? PlatformLine : planQuote;
            AskLine = askLine ?? string.Empty;
            RevealsBigNight = revealsBigNight;
            _onTime = onTime ?? OutcomeStory.Empty;
            _late = late ?? OutcomeStory.Empty;
            _notReached = notReached ?? OutcomeStory.Empty;
        }

        /// <summary>What they say as they board. Empty when there is nothing to say.</summary>
        public string PlatformLine { get; }

        /// <summary>The short version on the planning card; the platform line when there is none.</summary>
        public string PlanQuote { get; }

        /// <summary>Their answer when the player asks how they are. Empty when there is none.</summary>
        public string AskLine { get; }

        /// <summary>
        /// The deadline they give on the platform when it isn't the real one, or null. Asking them how they are tells
        /// the player the real one; until then the plan is made with this.
        /// </summary>
        public ClockTime? StatedDeadline { get; }

        /// <summary>Whether their answer tells the player which night this week is theirs.</summary>
        public bool RevealsBigNight { get; }

        public OutcomeStory GetOutcome(ArrivalStatus status) => status switch
        {
            ArrivalStatus.OnTime => _onTime,
            ArrivalStatus.Late => _late,
            ArrivalStatus.NotReached => _notReached,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };

        /// <summary>What happened to them. Empty when there is no line for this outcome.</summary>
        public string GetOutcomeLine(ArrivalStatus status) => GetOutcome(status).Line;
    }
}
