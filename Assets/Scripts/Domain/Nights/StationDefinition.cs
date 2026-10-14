namespace LastTrainHome.Domain.Nights
{
    /// <summary>A station's identity: the id the rules use and the name the player reads.</summary>
    public sealed class StationDefinition
    {
        public StationDefinition(string id, string displayName = null)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
        }

        public string Id { get; }

        public string DisplayName { get; }
    }
}
