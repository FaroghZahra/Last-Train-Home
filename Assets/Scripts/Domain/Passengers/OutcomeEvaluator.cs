using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Domain.Passengers
{
    /// <summary>
    /// Decides each passenger's outcome. Someone who boards at the start is aboard from the first stop; a pickup boards
    /// the first time the route reaches their station. The first time the route reaches their destination after that
    /// is when they get off; later visits change nothing. A pickup the train never collects is not reached.
    /// </summary>
    public static class OutcomeEvaluator
    {
        public static PassengerOutcome Evaluate(Passenger passenger, IReadOnlyList<RouteStop> stops)
        {
            if (passenger == null)
            {
                throw new ArgumentNullException(nameof(passenger));
            }

            if (stops == null)
            {
                throw new ArgumentNullException(nameof(stops));
            }

            int boardIndex = 0;
            if (passenger.IsPickup)
            {
                boardIndex = IndexOf(stops, passenger.OriginStationId, 0);
                if (boardIndex < 0)
                {
                    return PassengerOutcome.NotReached(-1);
                }
            }

            int arrivalIndex = IndexOf(stops, passenger.DestinationStationId, passenger.IsPickup ? boardIndex + 1 : 0);
            return arrivalIndex < 0
                ? PassengerOutcome.NotReached(boardIndex)
                : PassengerOutcome.Reached(boardIndex, arrivalIndex, stops[arrivalIndex].Arrival, passenger.Deadline);
        }

        /// <summary>Evaluates every passenger. Results are in the same order as <paramref name="passengers"/>.</summary>
        public static PassengerOutcome[] EvaluateAll(IReadOnlyList<Passenger> passengers, IReadOnlyList<RouteStop> stops)
        {
            if (passengers == null)
            {
                throw new ArgumentNullException(nameof(passengers));
            }

            var outcomes = new PassengerOutcome[passengers.Count];
            for (int i = 0; i < passengers.Count; i++)
            {
                outcomes[i] = Evaluate(passengers[i], stops);
            }

            return outcomes;
        }

        private static int IndexOf(IReadOnlyList<RouteStop> stops, string stationId, int from)
        {
            for (int i = from; i < stops.Count; i++)
            {
                if (string.Equals(stops[i].StationId, stationId, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
