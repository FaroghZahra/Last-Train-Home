using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Passengers;

namespace LastTrainHome.Domain.Nights
{
    /// <summary>
    /// Everything that makes one night different: the map, where the train starts, when the line closes, who rides.
    /// Always playable: the constructor rejects any night that breaks the rules.
    /// </summary>
    public sealed class NightDefinition
    {
        private readonly Dictionary<string, StationDefinition> _stationsById;

        /// <exception cref="ArgumentException">The night can't be played; the message lists every problem.</exception>
        public NightDefinition(string id, string title, string startStationId, ClockTime departure, ClockTime closing,
            IReadOnlyList<StationDefinition> stations, IReadOnlyList<Track> tracks, IReadOnlyList<Passenger> passengers)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A night needs an id.", nameof(id));
            }

            Id = id;
            Title = string.IsNullOrWhiteSpace(title) ? id : title;
            StartStationId = startStationId;
            Departure = departure;
            Closing = closing;
            Stations = stations ?? throw new ArgumentNullException(nameof(stations));
            Passengers = passengers ?? throw new ArgumentNullException(nameof(passengers));

            var problems = new List<string>();
            Network = BuildNetwork(stations, tracks ?? throw new ArgumentNullException(nameof(tracks)), problems);
            _stationsById = stations
                .Where(station => !string.IsNullOrWhiteSpace(station.Id))
                .GroupBy(station => station.Id, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            CheckRules(problems);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Night '{id}' cannot be played:\n- {string.Join("\n- ", problems)}");
            }
        }

        public string Id { get; }

        public string Title { get; }

        public string StartStationId { get; }

        public ClockTime Departure { get; }

        public ClockTime Closing { get; }

        public IReadOnlyList<StationDefinition> Stations { get; }

        public IReadOnlyList<Passenger> Passengers { get; }

        public RailNetwork Network { get; }

        /// <exception cref="KeyNotFoundException">No station has this id.</exception>
        public StationDefinition GetStation(string stationId) =>
            stationId != null && _stationsById.TryGetValue(stationId, out StationDefinition station)
                ? station
                : throw new KeyNotFoundException($"Night '{Id}' has no station '{stationId}'.");

        private static RailNetwork BuildNetwork(
            IReadOnlyList<StationDefinition> stations, IReadOnlyList<Track> tracks, List<string> problems)
        {
            try
            {
                return new RailNetwork(stations.Select(station => station.Id), tracks,
                    stations.Where(station => station.IsClosed).Select(station => station.Id));
            }
            catch (ArgumentException exception)
            {
                problems.Add(FirstLine(exception.Message));
                return null;
            }
        }

        private void CheckRules(List<string> problems)
        {
            if (Closing < Departure)
            {
                problems.Add("The line closes before the train departs.");
            }

            if (Network != null && !Network.ContainsStation(StartStationId))
            {
                problems.Add($"Start station '{StartStationId}' is not on the map.");
            }

            if (Passengers.Count == 0)
            {
                problems.Add("There are no passengers.");
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (Passenger passenger in Passengers)
            {
                if (!names.Add(passenger.Name))
                {
                    problems.Add($"Two passengers are called '{passenger.Name}'.");
                }

                if (Network != null && !Network.ContainsStation(passenger.DestinationStationId))
                {
                    problems.Add($"{passenger.Name}: destination '{passenger.DestinationStationId}' is not on the map.");
                }

                if (Network != null && passenger.IsPickup && !Network.ContainsStation(passenger.OriginStationId))
                {
                    problems.Add($"{passenger.Name}: boards at '{passenger.OriginStationId}', which is not on the map.");
                }
                else if (Network != null && passenger.IsPickup && Network.IsClosed(passenger.OriginStationId))
                {
                    problems.Add($"{passenger.Name}: boards at '{passenger.OriginStationId}', which is closed tonight.");
                }
            }
        }

        // Argument exceptions append "(Parameter '...')" on a second line; players and designers don't need it.
        private static string FirstLine(string message) => message.Split('\n')[0].Trim();
    }
}
