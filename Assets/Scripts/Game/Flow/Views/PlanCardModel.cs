using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>One passenger's card while planning.</summary>
    public sealed class PlanCardModel
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public OutcomeKind Outcome { get; set; }

        public string Quote { get; set; }

        /// <summary>Where they're going and by when.</summary>
        public string Detail { get; set; }

        /// <summary>The destination station's name as it appears in <see cref="Detail"/>, shown in their colour.</summary>
        public string Destination { get; set; }

        /// <summary>Their deadline along the night, 0 at departure and 1 at closing.</summary>
        public float Deadline { get; set; }

        /// <summary>When they'd arrive along the night, or null when they wouldn't.</summary>
        public float? Arrival { get; set; }

        public bool IsPromised { get; set; }

        public IReadOnlyList<PipKind> Week { get; set; }
    }
}
