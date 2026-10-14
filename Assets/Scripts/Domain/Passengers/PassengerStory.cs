using System;

namespace LastTrainHome.Domain.Passengers
{
    /// <summary>The words a passenger brings: why they're riding, and one line for each way the night can end.</summary>
    public sealed class PassengerStory
    {
        public static readonly PassengerStory Empty = new PassengerStory(null, null, null, null);

        private readonly string _onTimeLine;
        private readonly string _lateLine;
        private readonly string _notReachedLine;

        public PassengerStory(string motivation, string onTimeLine, string lateLine, string notReachedLine)
        {
            Motivation = motivation ?? string.Empty;
            _onTimeLine = onTimeLine ?? string.Empty;
            _lateLine = lateLine ?? string.Empty;
            _notReachedLine = notReachedLine ?? string.Empty;
        }

        /// <summary>What they say when they board. Empty when there is nothing to say.</summary>
        public string Motivation { get; }

        /// <summary>What happened to them. Empty when there is no line for this outcome.</summary>
        public string GetOutcomeLine(ArrivalStatus status) => status switch
        {
            ArrivalStatus.OnTime => _onTimeLine,
            ArrivalStatus.Late => _lateLine,
            ArrivalStatus.NotReached => _notReachedLine,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
