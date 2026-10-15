namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>A small coloured tag, e.g. "On time · 00:10".</summary>
    public sealed class TagModel
    {
        public string Text { get; set; }

        public TagKind Kind { get; set; }
    }
}
