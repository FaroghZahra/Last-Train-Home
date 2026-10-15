namespace LastTrainHome.Domain.Nights
{
    /// <summary>A station's identity: the id the rules use and the name the player reads.</summary>
    public sealed class StationDefinition
    {
        /// <param name="isClosed">On the map, but the train can't stop there tonight.</param>
        public StationDefinition(string id, string displayName = null, bool isClosed = false)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            IsClosed = isClosed;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public bool IsClosed { get; }
    }
}
