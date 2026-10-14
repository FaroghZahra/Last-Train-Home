namespace LastTrainHome.Domain.Session
{
    public enum NightPhase
    {
        /// <summary>The player is building a route.</summary>
        Planning,

        /// <summary>The route is locked and the train is running it.</summary>
        Travelling,

        /// <summary>The train reached the end of the line; outcomes are final.</summary>
        Finished,
    }
}
