using System;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Game.Content;

namespace LastTrainHome.Game.Flow
{
    /// <summary>Player-facing explanations for refused route changes, worded by Data/Text/UiText.json.</summary>
    public static class RouteMessages
    {
        public static string DescribeRefusal(AddStopResult result, string fromStation, string toStation, ClockTime closing,
            UiText ui) =>
            result.Status switch
            {
                AddStopStatus.AlreadyAtStation => ui.Format(UiKey.RefuseAlready, ("station", toStation)),
                AddStopStatus.NoTrack => ui.Format(UiKey.RefuseNoTrack, ("from", fromStation), ("station", toStation)),
                AddStopStatus.PastClosing => ui.Format(UiKey.RefusePastClosing, ("station", toStation),
                    ("time", result.AttemptedArrival.ToString()), ("closing", closing.ToString())),
                AddStopStatus.UnknownStation => ui.Format(UiKey.RefuseUnknown, ("station", toStation)),
                AddStopStatus.StationClosed => ui.Format(UiKey.RefuseClosed, ("station", toStation)),
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Only refusals have a message."),
            };
    }
}
