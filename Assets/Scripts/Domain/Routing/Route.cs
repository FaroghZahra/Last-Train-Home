using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Network;

namespace LastTrainHome.Domain.Routing
{
    /// <summary>
    /// The train's planned route for one night. It always starts at the start station, may revisit stations,
    /// only follows existing tracks and never arrives anywhere after the line closes.
    /// </summary>
    public sealed class Route
    {
        private readonly RailNetwork _network;
        private readonly List<RouteStop> _stops = new List<RouteStop>();

        public Route(RailNetwork network, string startStationId, ClockTime departure, ClockTime closing)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));

            if (!network.ContainsStation(startStationId))
            {
                throw new ArgumentException($"Start station '{startStationId}' is not on the map.", nameof(startStationId));
            }

            if (closing < departure)
            {
                throw new ArgumentException("The line cannot close before the train departs.", nameof(closing));
            }

            Closing = closing;
            Stops = new ReadOnlyCollection<RouteStop>(_stops);
            _stops.Add(new RouteStop(startStationId, departure));
        }

        /// <summary>Raised after every change to <see cref="Stops"/>, including <see cref="Reset"/>.</summary>
        public event Action Changed;

        public ClockTime Closing { get; }

        /// <summary>Stops in travel order. The first stop is always the start station.</summary>
        public IReadOnlyList<RouteStop> Stops { get; }

        public RouteStop LastStop => _stops[_stops.Count - 1];

        public bool HasLeftStart => _stops.Count > 1;

        public AddStopResult TryAddStop(string stationId)
        {
            if (!_network.ContainsStation(stationId))
            {
                return AddStopResult.Rejected(AddStopStatus.UnknownStation);
            }

            RouteStop last = LastStop;
            if (string.Equals(last.StationId, stationId, StringComparison.Ordinal))
            {
                return AddStopResult.Rejected(AddStopStatus.AlreadyAtStation);
            }

            if (!_network.TryGetTravelMinutes(last.StationId, stationId, out int minutes))
            {
                return AddStopResult.Rejected(AddStopStatus.NoTrack);
            }

            ClockTime arrival = last.Arrival.AddMinutes(minutes);
            if (arrival > Closing)
            {
                return AddStopResult.TooLate(arrival);
            }

            _stops.Add(new RouteStop(stationId, arrival));
            Changed?.Invoke();
            return AddStopResult.Added(arrival);
        }

        /// <summary>Removes the most recent stop. The start station is never removed.</summary>
        public bool TryRemoveLastStop()
        {
            if (!HasLeftStart)
            {
                return false;
            }

            _stops.RemoveAt(_stops.Count - 1);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Returns the route to just the start station.</summary>
        public void Reset()
        {
            _stops.RemoveRange(1, _stops.Count - 1);
            Changed?.Invoke();
        }
    }
}
