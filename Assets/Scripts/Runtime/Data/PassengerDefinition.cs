using System;
using LastTrainHome.Domain;

namespace LastTrainHome.Data
{
    /// <summary>Authored content for one passenger: the rules data plus the words the player reads.</summary>
    public sealed class PassengerDefinition
    {
        private readonly string _onTimeLine;
        private readonly string _lateLine;
        private readonly string _notReachedLine;

        /// <param name="motivation">What they tell you when they board. One line, about 12 words.</param>
        public PassengerDefinition(string name, string destinationStationId, ClockTime deadline, string motivation,
            string onTimeLine, string lateLine, string notReachedLine)
        {
            Name = name;
            DestinationStationId = destinationStationId;
            Deadline = deadline;
            Motivation = motivation;
            _onTimeLine = onTimeLine;
            _lateLine = lateLine;
            _notReachedLine = notReachedLine;
        }

        public string Name { get; }

        public string DestinationStationId { get; }

        public ClockTime Deadline { get; }

        public string Motivation { get; }

        public Passenger ToPassenger() => new Passenger(Name, DestinationStationId, Deadline);

        public string GetOutcomeLine(ArrivalStatus status) => status switch
        {
            ArrivalStatus.OnTime => _onTimeLine,
            ArrivalStatus.Late => _lateLine,
            ArrivalStatus.NotReached => _notReachedLine,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
