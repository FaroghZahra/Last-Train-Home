using System;
using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>What a night card shows.</summary>
    public sealed class NightCardModel
    {
        public string Kicker { get; set; }

        public string Day { get; set; }

        /// <summary>The passengers whose big night it is; empty for none.</summary>
        public IReadOnlyList<string> Featured { get; set; } = Array.Empty<string>();

        public string FeaturedLine { get; set; }

        public IReadOnlyList<string> Lines { get; set; }

        /// <summary>Brighter, after the lines; empty for none.</summary>
        public string Emphasis { get; set; }

        public IReadOnlyList<WeekDayTile> Week { get; set; }

        public string Footnote { get; set; }

        public string ContinueLabel { get; set; }
    }
}
