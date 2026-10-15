using LastTrainHome.Game.Content;
using UnityEngine;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>Everything the map needs to draw one station.</summary>
    public readonly struct StationMarker
    {
        /// <param name="displayName">The name as shown, e.g. "Market · closed".</param>
        /// <param name="destinationOf">A passenger heading here, for the pin's colour; null when nobody is.</param>
        public StationMarker(string id, string displayName, Vector2 position, LabelSide labelSide, bool isStart,
            string destinationOf, bool isClosed = false)
        {
            Id = id;
            DisplayName = displayName;
            Position = position;
            LabelSide = labelSide;
            IsStart = isStart;
            DestinationOf = destinationOf;
            IsClosed = isClosed;
        }

        public string Id { get; }

        public string DisplayName { get; }

        /// <summary>Relative to the map's origin.</summary>
        public Vector2 Position { get; }

        public LabelSide LabelSide { get; }

        public bool IsStart { get; }

        public string DestinationOf { get; }

        public bool IsClosed { get; }
    }
}
