using System;
using LastTrainHome.Domain;

namespace LastTrainHome.UI
{
    /// <summary>Player-facing explanations for refused route changes.</summary>
    public static class RouteMessages
    {
        public static string DescribeRefusal(AddStopResult result, string fromStation, string toStation, ClockTime closing) =>
            result.Status switch
            {
                AddStopStatus.AlreadyAtStation => $"The train is already at {toStation}.",
                AddStopStatus.NoTrack => $"No track from {fromStation} to {toStation}.",
                AddStopStatus.PastClosing =>
                    $"{toStation} at {result.AttemptedArrival} is too late. The line closes at {closing}.",
                AddStopStatus.UnknownStation => $"{toStation} isn't on tonight's line.",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Only refusals have a message."),
            };
    }
}
