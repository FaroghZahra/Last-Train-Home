using System;
using System.Collections.Generic;

namespace LastTrainHome.Domain
{
    /// <summary>The fixed rail map for a night: which stations exist and which are directly connected.</summary>
    public sealed class RailNetwork
    {
        private readonly Dictionary<string, Dictionary<string, int>> _travelMinutes =
            new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);

        /// <exception cref="ArgumentException">
        /// A station id is empty or duplicated, a track references an unknown station, or two tracks join the same pair.
        /// </exception>
        public RailNetwork(IEnumerable<string> stationIds, IEnumerable<Track> tracks)
        {
            if (stationIds == null)
            {
                throw new ArgumentNullException(nameof(stationIds));
            }

            if (tracks == null)
            {
                throw new ArgumentNullException(nameof(tracks));
            }

            foreach (string stationId in stationIds)
            {
                if (string.IsNullOrWhiteSpace(stationId))
                {
                    throw new ArgumentException("Station ids cannot be empty.", nameof(stationIds));
                }

                if (!_travelMinutes.TryAdd(stationId, new Dictionary<string, int>(StringComparer.Ordinal)))
                {
                    throw new ArgumentException($"Station '{stationId}' is listed twice.", nameof(stationIds));
                }
            }

            foreach (Track track in tracks)
            {
                AddTrack(track);
            }
        }

        public IReadOnlyCollection<string> StationIds => _travelMinutes.Keys;

        public bool ContainsStation(string stationId) =>
            stationId != null && _travelMinutes.ContainsKey(stationId);

        /// <summary>Gets the travel time of the direct track between two stations, if there is one.</summary>
        public bool TryGetTravelMinutes(string fromStationId, string toStationId, out int minutes)
        {
            minutes = 0;
            return fromStationId != null
                   && toStationId != null
                   && _travelMinutes.TryGetValue(fromStationId, out Dictionary<string, int> neighbours)
                   && neighbours.TryGetValue(toStationId, out minutes);
        }

        private void AddTrack(Track track)
        {
            if (!_travelMinutes.TryGetValue(track.StationA, out Dictionary<string, int> fromA))
            {
                throw new ArgumentException($"Track uses unknown station '{track.StationA}'.", nameof(track));
            }

            if (!_travelMinutes.TryGetValue(track.StationB, out Dictionary<string, int> fromB))
            {
                throw new ArgumentException($"Track uses unknown station '{track.StationB}'.", nameof(track));
            }

            if (!fromA.TryAdd(track.StationB, track.Minutes))
            {
                throw new ArgumentException(
                    $"'{track.StationA}' and '{track.StationB}' are joined by more than one track.", nameof(track));
            }

            fromB.Add(track.StationA, track.Minutes);
        }
    }
}
