namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One passenger's line in the logbook.</summary>
    public sealed class LogEntryModel
    {
        public string Name { get; set; }

        public TagModel Tag { get; set; }

        public string Line { get; set; }
    }
}
