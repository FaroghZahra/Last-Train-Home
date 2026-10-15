using System;
using System.Linq;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;

namespace LastTrainHome.Domain.Routing
{
    /// <summary>Tries every legal route through a night, to say what the best one could have done.</summary>
    public static class RouteExplorer
    {
        /// <summary>The most passengers any legal route gets there on time.</summary>
        public static int BestOnTime(NightDefinition night)
        {
            if (night == null)
            {
                throw new ArgumentNullException(nameof(night));
            }

            var route = new Route(night.Network, night.StartStationId, night.Departure, night.Closing);
            string[] stations = night.Network.StationIds.ToArray();
            int everyone = night.Passengers.Count;
            int best = 0;

            void Explore()
            {
                int onTime = OutcomeEvaluator.EvaluateAll(night.Passengers, route.Stops)
                    .Count(outcome => outcome.Status == ArrivalStatus.OnTime);
                best = Math.Max(best, onTime);
                foreach (string station in stations)
                {
                    if (best == everyone)
                    {
                        return;
                    }

                    if (route.TryAddStop(station).Succeeded)
                    {
                        Explore();
                        route.TryRemoveLastStop();
                    }
                }
            }

            Explore();
            return best;
        }
    }
}
