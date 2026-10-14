using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>Where a station is drawn on the map, relative to the map's origin, and where its name goes.</summary>
    public readonly struct StationPlacement
    {
        public StationPlacement(Vector2 position, LabelSide labelSide)
        {
            Position = position;
            LabelSide = labelSide;
        }

        public Vector2 Position { get; }

        public LabelSide LabelSide { get; }
    }
}
