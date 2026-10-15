using System;

namespace LastTrainHome.Domain.Network
{
    /// <summary>A two-way line between two stations and how long it takes to travel.</summary>
    public readonly struct Track
    {
        /// <param name="isClosed">Drawn on the map, but the train can't use it.</param>
        public Track(string stationA, string stationB, int minutes, bool isClosed = false)
        {
            if (string.IsNullOrWhiteSpace(stationA))
            {
                throw new ArgumentException("Station id is required.", nameof(stationA));
            }

            if (string.IsNullOrWhiteSpace(stationB))
            {
                throw new ArgumentException("Station id is required.", nameof(stationB));
            }

            if (string.Equals(stationA, stationB, StringComparison.Ordinal))
            {
                throw new ArgumentException($"A track cannot connect '{stationA}' to itself.", nameof(stationB));
            }

            if (minutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), minutes, "Travel time must be positive.");
            }

            StationA = stationA;
            StationB = stationB;
            Minutes = minutes;
            IsClosed = isClosed;
        }

        public string StationA { get; }

        public string StationB { get; }

        public int Minutes { get; }

        public bool IsClosed { get; }
    }
}
