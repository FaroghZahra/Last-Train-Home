namespace LastTrainHome.Game.Content
{
    /// <summary>How the week ends, chosen by how many big nights the player kept.</summary>
    public sealed class Ending
    {
        public Ending(int kept, string title, string line)
        {
            Kept = kept;
            Title = title ?? string.Empty;
            Line = line ?? string.Empty;
        }

        /// <summary>How many big nights were kept.</summary>
        public int Kept { get; }

        public string Title { get; }

        public string Line { get; }
    }
}
