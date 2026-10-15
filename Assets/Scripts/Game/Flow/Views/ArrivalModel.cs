using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One passenger's arrival card.</summary>
    public sealed class ArrivalModel
    {
        public string Name { get; set; }

        /// <summary>Who and where, e.g. "Priya · Old Town".</summary>
        public string Title { get; set; }

        public IReadOnlyList<TagModel> Tags { get; set; }

        public string Line { get; set; }

        /// <summary>The keepsake line, or the line that says there's none; empty for neither.</summary>
        public string Keepsake { get; set; }

        public bool HasKeepsake { get; set; }

        /// <summary>Empty for none.</summary>
        public string Aside { get; set; }

        public string StillAboardLabel { get; set; }

        /// <summary>Who is still on the train, by name.</summary>
        public IReadOnlyList<string> StillAboard { get; set; }

        /// <summary>Late or never reached: slower, quieter, no chime.</summary>
        public bool IsMuted { get; set; }

        public bool IsPromised { get; set; }

        public string ContinueLabel { get; set; }
    }
}
