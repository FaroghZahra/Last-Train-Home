using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Domain.Session
{
    /// <summary>
    /// How a finished night went: the route the train took, what that meant for each passenger, and whether the
    /// player's promise was kept.
    /// </summary>
    public sealed class NightResult
    {
        private readonly IReadOnlyList<string> _passengerNames;

        /// <param name="passengerNames">One per outcome, in the same order.</param>
        /// <param name="promisedPassenger">Who the player promised to get there on time, or null.</param>
        public NightResult(string nightId, IReadOnlyList<RouteStop> route, IReadOnlyList<PassengerOutcome> outcomes,
            IReadOnlyList<string> passengerNames = null, string promisedPassenger = null)
        {
            NightId = nightId ?? throw new ArgumentNullException(nameof(nightId));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
            _passengerNames = passengerNames ?? Array.Empty<string>();
            if (_passengerNames.Count != 0 && _passengerNames.Count != outcomes.Count)
            {
                throw new ArgumentException("Every outcome needs exactly one passenger name.", nameof(passengerNames));
            }

            if (promisedPassenger != null && !_passengerNames.Contains(promisedPassenger))
            {
                throw new ArgumentException($"'{promisedPassenger}' isn't one of tonight's passengers.", nameof(promisedPassenger));
            }

            PromisedPassenger = promisedPassenger;
        }

        public string NightId { get; }

        public IReadOnlyList<RouteStop> Route { get; }

        /// <summary>One per passenger, in the night's passenger order.</summary>
        public IReadOnlyList<PassengerOutcome> Outcomes { get; }

        public int OnTimeCount => Outcomes.Count(outcome => outcome.Status == ArrivalStatus.OnTime);

        /// <summary>Who the player promised to get there on time, or null.</summary>
        public string PromisedPassenger { get; }

        public bool PromiseKept => PromisedPassenger != null
                                   && TryGetStatus(PromisedPassenger, out ArrivalStatus status)
                                   && status == ArrivalStatus.OnTime;

        /// <summary>How the night ended for a passenger, if they rode.</summary>
        public bool TryGetStatus(string passengerName, out ArrivalStatus status)
        {
            for (int i = 0; i < _passengerNames.Count; i++)
            {
                if (string.Equals(_passengerNames[i], passengerName, StringComparison.Ordinal))
                {
                    status = Outcomes[i].Status;
                    return true;
                }
            }

            status = default;
            return false;
        }
    }
}
