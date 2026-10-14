namespace LastTrainHome.Domain
{
    public enum AddStopStatus
    {
        Added,
        UnknownStation,
        AlreadyAtStation,
        NoTrack,
        PastClosing,
    }

    /// <summary>What happened when the player tried to extend the route.</summary>
    public readonly struct AddStopResult
    {
        private AddStopResult(AddStopStatus status, ClockTime attemptedArrival)
        {
            Status = status;
            AttemptedArrival = attemptedArrival;
        }

        public AddStopStatus Status { get; }

        /// <summary>
        /// When the train would reach the station. Only meaningful for <see cref="AddStopStatus.Added"/>
        /// and <see cref="AddStopStatus.PastClosing"/>.
        /// </summary>
        public ClockTime AttemptedArrival { get; }

        public bool Succeeded => Status == AddStopStatus.Added;

        internal static AddStopResult Added(ClockTime arrival) => new AddStopResult(AddStopStatus.Added, arrival);

        internal static AddStopResult TooLate(ClockTime arrival) => new AddStopResult(AddStopStatus.PastClosing, arrival);

        internal static AddStopResult Rejected(AddStopStatus status) => new AddStopResult(status, default);
    }
}
