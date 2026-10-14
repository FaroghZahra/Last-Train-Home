using LastTrainHome.Game.Content;
using UnityEngine;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>Everything the map needs to draw one station.</summary>
    public readonly struct StationMarker
    {
        public StationMarker(string id, string displayName, Vector2 position, LabelSide labelSide)
        {
            Id = id;
            DisplayName = displayName;
            Position = position;
            LabelSide = labelSide;
        }

        public string Id { get; }

        public string DisplayName { get; }

        /// <summary>Relative to the map's origin.</summary>
        public Vector2 Position { get; }

        public LabelSide LabelSide { get; }
    }
}
