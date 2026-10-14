using LastTrainHome.Domain.Clock;

namespace LastTrainHome.Domain.Routing
{
    /// <summary>A station on the planned route and when the train gets there.</summary>
    public readonly struct RouteStop
    {
        public RouteStop(string stationId, ClockTime arrival)
        {
            StationId = stationId;
            Arrival = arrival;
        }

        public string StationId { get; }

        public ClockTime Arrival { get; }
    }
}
