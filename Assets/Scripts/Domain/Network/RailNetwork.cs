using System;
using System.Collections.Generic;

namespace LastTrainHome.Domain.Network
{
    /// <summary>
    /// The fixed rail map for a night: which stations exist and which are directly connected. Closed stations and tracks
    /// are on the map but can't be travelled to or along.
    /// </summary>
    public sealed class RailNetwork
    {
        private readonly Dictionary<string, Dictionary<string, int>> _travelMinutes =
            new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);

        private readonly List<Track> _tracks = new List<Track>();
        private readonly HashSet<(string, string)> _joined = new HashSet<(string, string)>();
        private readonly HashSet<string> _closedStations = new HashSet<string>(StringComparer.Ordinal);

        /// <exception cref="ArgumentException">
        /// A station id is empty or duplicated, a track references an unknown station, or two tracks join the same pair.
        /// </exception>
        public RailNetwork(IEnumerable<string> stationIds, IEnumerable<Track> tracks,
            IEnumerable<string> closedStationIds = null)
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

            foreach (string stationId in closedStationIds ?? Array.Empty<string>())
            {
                if (!_travelMinutes.ContainsKey(stationId ?? string.Empty))
                {
                    throw new ArgumentException($"Closed station '{stationId}' is not on the map.", nameof(closedStationIds));
                }

                _closedStations.Add(stationId);
            }

            foreach (Track track in tracks)
            {
                AddTrack(track);
            }
        }

        public IReadOnlyCollection<string> StationIds => _travelMinutes.Keys;

        public IReadOnlyList<Track> Tracks => _tracks;

        public bool ContainsStation(string stationId) =>
            stationId != null && _travelMinutes.ContainsKey(stationId);

        public bool IsClosed(string stationId) => stationId != null && _closedStations.Contains(stationId);

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

            bool isNewPair = string.CompareOrdinal(track.StationA, track.StationB) < 0
                ? _joined.Add((track.StationA, track.StationB))
                : _joined.Add((track.StationB, track.StationA));
            if (!isNewPair)
            {
                throw new ArgumentException(
                    $"'{track.StationA}' and '{track.StationB}' are joined by more than one track.", nameof(track));
            }

            _tracks.Add(track);
            if (track.IsClosed || IsClosed(track.StationA) || IsClosed(track.StationB))
            {
                return;
            }

            fromA.Add(track.StationB, track.Minutes);
            fromB.Add(track.StationA, track.Minutes);
        }
    }
}
