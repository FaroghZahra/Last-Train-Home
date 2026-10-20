using System;
using System.Collections.Generic;
using System.Linq;

namespace LastTrainHome.Game.Content
{
    /// <summary>The card that opens a night, before the platform.</summary>
    public sealed class NightCardText
    {
        public NightCardText(string kicker, IReadOnlyList<string> featured, string featuredLine, IReadOnlyList<string> lines,
            string emphasis, string footnote)
        {
            Kicker = kicker ?? string.Empty;
            Featured = featured?.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray() ?? Array.Empty<string>();
            FeaturedLine = featuredLine ?? string.Empty;
            Lines = lines ?? Array.Empty<string>();
            Emphasis = emphasis ?? string.Empty;
            Footnote = footnote ?? string.Empty;
        }

        public string Kicker { get; }

        /// <summary>The passengers whose big night it is, shown with their portraits; empty for none.</summary>
        public IReadOnlyList<string> Featured { get; }

        public string FeaturedLine { get; }

        public IReadOnlyList<string> Lines { get; }

        public string Emphasis { get; }

        public string Footnote { get; }
    }
}
