namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One big night still to come.</summary>
    public sealed class WeekAheadTile
    {
        /// <summary>Whose night it is, for colour; null when the player doesn't know yet.</summary>
        public string Passenger { get; set; }

        public string Header { get; set; }

        public string Text { get; set; }

        /// <summary>Tomorrow.</summary>
        public bool IsNext { get; set; }
    }
}
