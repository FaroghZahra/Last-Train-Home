using LastTrainHome.Domain.Passengers;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// The parts of an outcome a variant replaces; null keeps the original. A new line brings its own logbook line,
    /// so a short version of the old line is never shown beside the new one.
    /// </summary>
    public sealed class OutcomePatch
    {
        public OutcomePatch(string line, string logLine, string keepsakeId, string aside, string carry, string weekNote)
        {
            Line = Blank(line);
            LogLine = Blank(logLine);
            KeepsakeId = Blank(keepsakeId);
            Aside = Blank(aside);
            Carry = Blank(carry);
            WeekNote = Blank(weekNote);
        }

        public string Line { get; }

        public string LogLine { get; }

        public string KeepsakeId { get; }

        public string Aside { get; }

        public string Carry { get; }

        public string WeekNote { get; }

        public OutcomeStory ApplyTo(OutcomeStory original) => new OutcomeStory(
            Line ?? original.Line,
            LogLine ?? (Line != null ? null : original.LogLine),
            KeepsakeId ?? original.KeepsakeId,
            Aside ?? original.Aside,
            Carry ?? original.Carry,
            WeekNote ?? original.WeekNote);

        private static string Blank(string text) => string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
