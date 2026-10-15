namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One day on a passenger's planning card.</summary>
    public enum PipKind
    {
        /// <summary>A past night they got home on time.</summary>
        OnTime,
        /// <summary>A past night they were late.</summary>
        Late,
        /// <summary>A past night they were never reached.</summary>
        Missed,
        /// <summary>A past night they didn't ride.</summary>
        Absent,
        Tonight,
        /// <summary>Tonight, and it's their big night.</summary>
        TonightBig,
        Ahead,
        /// <summary>Their big night, still ahead.</summary>
        AheadBig,
    }
}
