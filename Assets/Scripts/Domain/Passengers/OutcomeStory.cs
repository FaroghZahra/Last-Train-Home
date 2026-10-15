namespace LastTrainHome.Domain.Passengers
{
    /// <summary>What happened to a passenger for one way the night can end, and what it leaves behind.</summary>
    public sealed class OutcomeStory
    {
        public static readonly OutcomeStory Empty = new OutcomeStory(null);

        public OutcomeStory(string line, string logLine = null, string keepsakeId = null, string aside = null,
            string carry = null, string weekNote = null)
        {
            Line = line ?? string.Empty;
            LogLine = string.IsNullOrWhiteSpace(logLine) ? Line : logLine;
            KeepsakeId = string.IsNullOrWhiteSpace(keepsakeId) ? null : keepsakeId;
            Aside = aside ?? string.Empty;
            Carry = carry ?? string.Empty;
            WeekNote = weekNote ?? string.Empty;
        }

        /// <summary>Shown when the train arrives (or, if they were never reached, at the end of the line).</summary>
        public string Line { get; }

        /// <summary>The logbook's shorter version; the full line when there is none.</summary>
        public string LogLine { get; }

        /// <summary>What they leave in the cab, or null.</summary>
        public string KeepsakeId { get; }

        /// <summary>A small moment beside the arrival, or empty.</summary>
        public string Aside { get; }

        /// <summary>What this outcome changes about tomorrow, shown in the logbook, or empty.</summary>
        public string Carry { get; }

        /// <summary>Added to their big night in the week ahead, or empty.</summary>
        public string WeekNote { get; }
    }
}
