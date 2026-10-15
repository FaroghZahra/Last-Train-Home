using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Game.Content
{
    /// <summary>A keepsake nobody hands over: the train finds it by stopping at a station early enough.</summary>
    public sealed class Secret
    {
        public Secret(string stationId, ClockTime by, string keepsakeId)
        {
            StationId = stationId;
            By = by;
            KeepsakeId = keepsakeId;
        }

        public string StationId { get; }

        /// <summary>The latest the train can stop there and still find it.</summary>
        public ClockTime By { get; }

        public string KeepsakeId { get; }

        public bool IsFoundAt(RouteStop stop) => stop.StationId == StationId && stop.Arrival <= By;
    }
}
