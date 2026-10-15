using System;
using LastTrainHome.Domain.Clock;

namespace LastTrainHome.Domain.Passengers
{
    /// <summary>How a route treats one passenger.</summary>
    public readonly struct PassengerOutcome
    {
        private readonly ClockTime _arrival;

        private PassengerOutcome(ArrivalStatus status, int boardStopIndex, int stopIndex, ClockTime arrival, int minutesLate)
        {
            Status = status;
            BoardStopIndex = boardStopIndex;
            StopIndex = stopIndex;
            _arrival = arrival;
            MinutesLate = minutesLate;
        }

        public ArrivalStatus Status { get; }

        public bool IsReached => Status != ArrivalStatus.NotReached;

        /// <summary>Index into the route's stops where the passenger boarded (0 at the start), or -1 if never picked up.</summary>
        public int BoardStopIndex { get; }

        public bool IsAboard => BoardStopIndex >= 0;

        /// <summary>Index into the route's stops where the passenger got off, or -1 if they never did.</summary>
        public int StopIndex { get; }

        /// <exception cref="InvalidOperationException">The passenger was not reached.</exception>
        public ClockTime Arrival => IsReached
            ? _arrival
            : throw new InvalidOperationException("A passenger who was not reached has no arrival time.");

        /// <summary>Zero unless <see cref="Status"/> is <see cref="ArrivalStatus.Late"/>.</summary>
        public int MinutesLate { get; }

        internal static PassengerOutcome NotReached(int boardStopIndex) =>
            new PassengerOutcome(ArrivalStatus.NotReached, boardStopIndex, -1, default, 0);

        internal static PassengerOutcome Reached(int boardStopIndex, int stopIndex, ClockTime arrival, ClockTime deadline)
        {
            int minutesLate = Math.Max(0, arrival.MinutesSince(deadline));
            ArrivalStatus status = minutesLate > 0 ? ArrivalStatus.Late : ArrivalStatus.OnTime;
            return new PassengerOutcome(status, boardStopIndex, stopIndex, arrival, minutesLate);
        }
    }
}
