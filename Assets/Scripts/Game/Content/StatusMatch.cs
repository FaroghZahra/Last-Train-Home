namespace LastTrainHome.Game.Content
{
    /// <summary>Which earlier outcomes a variant reacts to. The level file names these in camelCase.</summary>
    public enum StatusMatch
    {
        OnTime,
        Late,
        NotReached,

        /// <summary>Late or not reached.</summary>
        Missed,
    }
}
