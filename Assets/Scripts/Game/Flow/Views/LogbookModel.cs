using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>What a logbook page shows.</summary>
    public sealed class LogbookModel
    {
        public string Day { get; set; }

        public string Kicker { get; set; }

        public IReadOnlyList<LogEntryModel> Entries { get; set; }

        public string Summary { get; set; }

        /// <summary>Everyone home and any promise kept.</summary>
        public bool SummaryIsGood { get; set; }

        public string CabHeader { get; set; }

        public IReadOnlyList<KeepsakeSlot> Cab { get; set; }

        public string WeekHeader { get; set; }

        public IReadOnlyList<WeekAheadTile> WeekAhead { get; set; }

        public string NoteHeader { get; set; }

        public string Note { get; set; }

        public string Footer { get; set; }

        public string NextLabel { get; set; }
    }
}
