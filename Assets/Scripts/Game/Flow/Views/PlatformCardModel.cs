namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One passenger on the platform.</summary>
    public sealed class PlatformCardModel
    {
        public string Name { get; set; }

        /// <summary>Where they're going and by when.</summary>
        public string Subtitle { get; set; }

        public string PlatformLine { get; set; }

        public bool IsAsked { get; set; }

        public string AskedHeader { get; set; }

        public string AskedLine { get; set; }

        public string AskLabel { get; set; }

        public bool CanAsk { get; set; }

        public string PromiseLabel { get; set; }

        public bool IsPromised { get; set; }
    }
}
