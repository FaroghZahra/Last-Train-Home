using System;
using System.Collections.Generic;

namespace LastTrainHome.Domain
{
    /// <summary>
    /// Decides each passenger's outcome. The first time the route reaches a passenger's destination is when they
    /// get off; later visits to that station don't change anything.
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

            for (int i = 0; i < stops.Count; i++)
            {
                if (string.Equals(stops[i].StationId, passenger.DestinationStationId, StringComparison.Ordinal))
                {
                    return PassengerOutcome.Reached(i, stops[i].Arrival, passenger.Deadline);
                }
            }

            return PassengerOutcome.NotReached();
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
    }
}
