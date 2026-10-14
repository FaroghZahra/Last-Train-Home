using UnityEngine;

namespace LastTrainHome.Data
{
    /// <summary>Which side of the station its name sits on. Pick the side no track leaves from.</summary>
    public enum LabelSide
    {
        Above,
        Below,
        Left,
        Right,
    }

    /// <summary>One station of a level's map: its id, what the player sees, and where it sits on the map.</summary>
    public sealed class StationDefinition
    {
        public StationDefinition(string id, string displayName, Vector2 position, LabelSide labelSide)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            Position = position;
            LabelSide = labelSide;
        }

        public string Id { get; }

        public string DisplayName { get; }

        /// <summary>Relative to the rail map.</summary>
        public Vector2 Position { get; }

        public LabelSide LabelSide { get; }
    }
}
