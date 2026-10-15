namespace LastTrainHome.Game.Content
{
    /// <summary>When a planning coach line applies. The level file names these in camelCase.</summary>
    public enum CoachCondition
    {
        /// <summary>The route hasn't left the start yet.</summary>
        Empty,

        /// <summary>The route has exactly <c>count</c> stops, counting the start.</summary>
        Stops,

        AllOnTime,

        /// <summary>Exactly <c>count</c> passengers would be on time.</summary>
        OnTime,

        AnyLate,

        AnyNotReached,

        /// <summary>The route ends at <c>station</c> and someone hasn't been reached.</summary>
        DeadEnd,

        /// <summary>The promised passenger wouldn't be on time.</summary>
        PromiseBroken,

        Always,
    }
}
