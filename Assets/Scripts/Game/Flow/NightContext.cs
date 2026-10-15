using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Progress;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;

namespace LastTrainHome.Game.Flow
{
    /// <summary>Everything the screens of one night share: the night as it plays after carry-over, and the week so far.</summary>
    public sealed class NightContext
    {
        /// <param name="level">Tonight's level with carry-over already applied.</param>
        public NightContext(GameContent content, Campaign campaign, int nightIndex, Level level)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Campaign = campaign ?? throw new ArgumentNullException(nameof(campaign));
            Level = level ?? throw new ArgumentNullException(nameof(level));
            if (nightIndex < 0 || nightIndex >= content.Levels.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(nightIndex), nightIndex, "Not one of the levels.");
            }

            NightIndex = nightIndex;
            Session = new NightSession(level.Night);
            Platform = new PlatformSession(level.Night.Passengers, level.Story.AsksAllowed);
        }

        public GameContent Content { get; }

        public Campaign Campaign { get; }

        public int NightIndex { get; }

        public Level Level { get; }

        public NightSession Session { get; }

        public PlatformSession Platform { get; }

        /// <summary>How the night went; set once the train reaches the end of the line.</summary>
        public NightResult Result { get; set; }

        public NightDefinition Night => Level.Night;

        public UiText Ui => Content.Ui;

        public Week Week => Content.Week;

        public int NightCount => Content.Levels.Count;

        public bool IsLastNight => NightIndex == NightCount - 1;

        /// <summary>A night's full day name, e.g. "Tuesday".</summary>
        public string DayName(int nightIndex) => Content.Levels[nightIndex].Day;

        /// <summary>A night's short day name, e.g. "Tue".</summary>
        public string ShortDay(int nightIndex) => Week.Days[nightIndex];

        public string StationName(string stationId) => Night.GetStation(stationId).DisplayName;

        /// <summary>The deadline the player knows: what they said, until the player asks them.</summary>
        public ClockTime KnownDeadline(Passenger passenger) =>
            passenger.Story.StatedDeadline is ClockTime said && !Platform.HasAsked(passenger.Name) ? said : passenger.Deadline;

        /// <summary>Whether the player is planning with a deadline that isn't the real one.</summary>
        public bool IsMisinformed(Passenger passenger) => KnownDeadline(passenger) != passenger.Deadline;

        /// <summary>Tonight's passengers as the player knows them, with the deadlines they were told.</summary>
        public IReadOnlyList<Passenger> KnownPassengers => Night.Passengers
            .Select(passenger => IsMisinformed(passenger)
                ? new Passenger(passenger.Name, passenger.DestinationStationId, KnownDeadline(passenger), passenger.Story,
                    passenger.OriginStationId)
                : passenger)
            .ToArray();

        /// <summary>What the route means for each passenger as far as the player knows, in passenger order.</summary>
        public IReadOnlyList<PassengerOutcome> KnownOutcomes => OutcomeEvaluator.EvaluateAll(KnownPassengers, Session.Stops);

        /// <summary>Where a passenger is going, e.g. "Old Town", or for a pickup "Riverside → Old Town".</summary>
        public string Trip(Passenger passenger) => passenger.IsPickup
            ? Ui.Format(UiKey.TripPickup, ("origin", StationName(passenger.OriginStationId)),
                ("destination", StationName(passenger.DestinationStationId)))
            : StationName(passenger.DestinationStationId);

        public bool IsBigNightTonight(string passengerName) => Week.FindCastMember(passengerName)?.BigNight == NightIndex;

        /// <summary>Fills a night card or platform template's {day}, {night} and {nights}.</summary>
        public string FillHeader(string template) => TextTemplate.Fill(template,
            ("day", Level.Day),
            ("night", (NightIndex + 1).ToString(CultureInfo.InvariantCulture)),
            ("nights", NightCount.ToString(CultureInfo.InvariantCulture)));
    }
}
