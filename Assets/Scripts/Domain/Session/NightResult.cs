using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Domain.Session
{
    /// <summary>How a finished night went: the route the train took and what that meant for each passenger.</summary>
    public sealed class NightResult
    {
        public NightResult(string nightId, IReadOnlyList<RouteStop> route, IReadOnlyList<PassengerOutcome> outcomes)
        {
            NightId = nightId ?? throw new ArgumentNullException(nameof(nightId));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
        }

        public string NightId { get; }

        public IReadOnlyList<RouteStop> Route { get; }

        /// <summary>One per passenger, in the night's passenger order.</summary>
        public IReadOnlyList<PassengerOutcome> Outcomes { get; }

        public int OnTimeCount => Outcomes.Count(outcome => outcome.Status == ArrivalStatus.OnTime);
    }
}
