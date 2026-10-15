using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>What the platform shows.</summary>
    public sealed class PlatformModel
    {
        public string Kicker { get; set; }

        public string Headline { get; set; }

        /// <summary>Empty hides the coach box.</summary>
        public string Coach { get; set; }

        public IReadOnlyList<PlatformCardModel> Cards { get; set; }

        public string Footer { get; set; }

        public string ContinueLabel { get; set; }

        public bool CanContinue { get; set; }
    }
}
