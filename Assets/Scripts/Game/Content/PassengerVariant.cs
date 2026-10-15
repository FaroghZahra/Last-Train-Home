using System;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Passengers;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// Carry-over: when a passenger's outcome on an earlier night matched (and, if <see cref="IfPromised"/>, the player
    /// had promised them that night), one of tonight's passengers changes. Null parts keep what the level file says.
    /// </summary>
    public sealed class PassengerVariant
    {
        public PassengerVariant(string ifNightId, string ifPassenger, StatusMatch ifStatus, string passengerName,
            ClockTime? deadline, string platformLine, string planQuote, string askLine,
            OutcomePatch onTime, OutcomePatch late, OutcomePatch notReached, bool ifPromised = false,
            string destination = null)
        {
            IfPromised = ifPromised;
            Destination = Blank(destination);
            IfNightId = ifNightId;
            IfPassenger = ifPassenger;
            IfStatus = ifStatus;
            PassengerName = passengerName;
            Deadline = deadline;
            PlatformLine = Blank(platformLine);
            PlanQuote = Blank(planQuote);
            AskLine = Blank(askLine);
            OnTime = onTime;
            Late = late;
            NotReached = notReached;
        }

        public string IfNightId { get; }

        public string IfPassenger { get; }

        public StatusMatch IfStatus { get; }

        /// <summary>Only when the player had promised <see cref="IfPassenger"/> to get them there that night.</summary>
        public bool IfPromised { get; }

        /// <summary>Where they go tonight instead, or null.</summary>
        public string Destination { get; }

        public string PassengerName { get; }

        public ClockTime? Deadline { get; }

        public string PlatformLine { get; }

        public string PlanQuote { get; }

        public string AskLine { get; }

        public OutcomePatch OnTime { get; }

        public OutcomePatch Late { get; }

        public OutcomePatch NotReached { get; }

        public bool Matches(ArrivalStatus earlierStatus) => IfStatus switch
        {
            StatusMatch.OnTime => earlierStatus == ArrivalStatus.OnTime,
            StatusMatch.Late => earlierStatus == ArrivalStatus.Late,
            StatusMatch.NotReached => earlierStatus == ArrivalStatus.NotReached,
            StatusMatch.Missed => earlierStatus != ArrivalStatus.OnTime,
            _ => throw new ArgumentOutOfRangeException(nameof(IfStatus), IfStatus, null),
        };

        /// <summary>The passenger with this variant's changes.</summary>
        /// <exception cref="ArgumentException">It's a different passenger.</exception>
        public Passenger ApplyTo(Passenger passenger)
        {
            if (passenger.Name != PassengerName)
            {
                throw new ArgumentException($"This variant changes {PassengerName}, not {passenger.Name}.", nameof(passenger));
            }

            PassengerStory story = passenger.Story;
            // A new platform line without a new quote would leave the card quoting the old line.
            string planQuote = PlanQuote ?? (PlatformLine != null ? null : story.PlanQuote);
            var changed = new PassengerStory(PlatformLine ?? story.PlatformLine, planQuote, AskLine ?? story.AskLine,
                story.RevealsBigNight,
                Patch(OnTime, story.GetOutcome(ArrivalStatus.OnTime)),
                Patch(Late, story.GetOutcome(ArrivalStatus.Late)),
                Patch(NotReached, story.GetOutcome(ArrivalStatus.NotReached)),
                Deadline != null ? null : story.StatedDeadline); // a new deadline is one they tell the player
            return new Passenger(passenger.Name, Destination ?? passenger.DestinationStationId, Deadline ?? passenger.Deadline,
                changed, passenger.OriginStationId);
        }

        private static OutcomeStory Patch(OutcomePatch patch, OutcomeStory original) =>
            patch != null ? patch.ApplyTo(original) : original;

        private static string Blank(string text) => string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
