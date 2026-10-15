namespace LastTrainHome.Game.Content
{
    /// <summary>The platform screen's own words; the passengers bring theirs.</summary>
    public sealed class PlatformText
    {
        public PlatformText(string kicker, string headline, string coach)
        {
            Kicker = kicker ?? string.Empty;
            Headline = headline ?? string.Empty;
            Coach = coach ?? string.Empty;
        }

        public string Kicker { get; }

        public string Headline { get; }

        public string Coach { get; }
    }
}
