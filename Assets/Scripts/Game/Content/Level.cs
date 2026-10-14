using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Nights;

namespace LastTrainHome.Game.Content
{
    /// <summary>One level file, read: the night's rules and how its map is laid out on screen.</summary>
    public sealed class Level
    {
        private readonly IReadOnlyDictionary<string, StationPlacement> _placements;

        public Level(NightDefinition night, IReadOnlyDictionary<string, StationPlacement> placements)
        {
            Night = night ?? throw new ArgumentNullException(nameof(night));
            _placements = placements ?? throw new ArgumentNullException(nameof(placements));
        }

        public NightDefinition Night { get; }

        /// <exception cref="KeyNotFoundException">The station isn't on this level's map.</exception>
        public StationPlacement GetPlacement(string stationId) =>
            _placements.TryGetValue(stationId, out StationPlacement placement)
                ? placement
                : throw new KeyNotFoundException($"Level '{Night.Id}' has no placement for '{stationId}'.");
    }
}
