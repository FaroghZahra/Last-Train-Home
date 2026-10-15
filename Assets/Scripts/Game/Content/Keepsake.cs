namespace LastTrainHome.Game.Content
{
    /// <summary>Something a passenger leaves in the cab.</summary>
    public sealed class Keepsake
    {
        public Keepsake(string id, string name, string line)
        {
            Id = id;
            Name = name ?? string.Empty;
            Line = line ?? string.Empty;
        }

        public string Id { get; }

        public string Name { get; }

        /// <summary>Shown on the arrival card when it's left.</summary>
        public string Line { get; }
    }
}
