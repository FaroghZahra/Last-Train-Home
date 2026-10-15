namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>How a day looks on the week strip.</summary>
    public enum WeekDayKind
    {
        /// <summary>Already played.</summary>
        Past,
        Tonight,
        /// <summary>Ahead, and the player knows whose night it is.</summary>
        Known,
        /// <summary>Ahead, and nobody has said yet.</summary>
        Unknown,
    }
}
