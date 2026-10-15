using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>Everything the map needs to draw one track.</summary>
    public readonly struct TrackMarker
    {
        /// <param name="label">Shown halfway along, e.g. "10 min" or "closed".</param>
        public TrackMarker(string stationA, string stationB, string label, IReadOnlyList<Vector2> bends,
            bool isClosed = false)
        {
            StationA = stationA;
            StationB = stationB;
            Label = label ?? string.Empty;
            Bends = bends ?? throw new ArgumentNullException(nameof(bends));
            IsClosed = isClosed;
        }

        public string StationA { get; }

        public string StationB { get; }

        public string Label { get; }

        public bool IsClosed { get; }

        /// <summary>Where the track bends on its way from A to B, relative to the map's origin. Empty when straight.</summary>
        public IReadOnlyList<Vector2> Bends { get; }
    }
}
