namespace LastTrainHome.Domain.Journey
{
    /// <summary>Where the train is along the route: between two stops, or at one when both indices match.</summary>
    public readonly struct JourneyLeg
    {
        public JourneyLeg(int fromStopIndex, int toStopIndex, float progress)
        {
            FromStopIndex = fromStopIndex;
            ToStopIndex = toStopIndex;
            Progress = progress;
        }

        public int FromStopIndex { get; }

        public int ToStopIndex { get; }

        /// <summary>0 at <see cref="FromStopIndex"/>, 1 at <see cref="ToStopIndex"/>.</summary>
        public float Progress { get; }

        public static JourneyLeg AtStop(int stopIndex) => new JourneyLeg(stopIndex, stopIndex, 0f);
    }
}
