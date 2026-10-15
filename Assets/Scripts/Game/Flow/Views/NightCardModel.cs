using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>What a night card shows.</summary>
    public sealed class NightCardModel
    {
        public string Kicker { get; set; }

        public string Day { get; set; }

        /// <summary>The passenger whose big night it is, or null.</summary>
        public string Featured { get; set; }

        public string FeaturedLine { get; set; }

        public IReadOnlyList<string> Lines { get; set; }

        /// <summary>Brighter, after the lines; empty for none.</summary>
        public string Emphasis { get; set; }

        public IReadOnlyList<WeekDayTile> Week { get; set; }

        public string Footnote { get; set; }

        public string ContinueLabel { get; set; }
    }
}
