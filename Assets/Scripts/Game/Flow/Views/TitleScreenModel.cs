using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>What the title screen shows.</summary>
    public sealed class TitleScreenModel
    {
        public string Title { get; set; }

        /// <summary>Paragraphs under the title.</summary>
        public IReadOnlyList<string> Intro { get; set; }

        /// <summary>Brighter, at the end of the last paragraph.</summary>
        public string Emphasis { get; set; }

        public string Footnote { get; set; }

        public string BeginLabel { get; set; }

        /// <summary>Whose portraits to show, by name.</summary>
        public IReadOnlyList<string> Cast { get; set; }
    }
}
