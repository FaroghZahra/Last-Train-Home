using System;
using System.Collections.Generic;

namespace LastTrainHome.Game.Content
{
    /// <summary>The card that opens a night, before the platform.</summary>
    public sealed class NightCardText
    {
        public NightCardText(string kicker, string featured, string featuredLine, IReadOnlyList<string> lines,
            string emphasis, string footnote)
        {
            Kicker = kicker ?? string.Empty;
            Featured = string.IsNullOrWhiteSpace(featured) ? null : featured;
            FeaturedLine = featuredLine ?? string.Empty;
            Lines = lines ?? Array.Empty<string>();
            Emphasis = emphasis ?? string.Empty;
            Footnote = footnote ?? string.Empty;
        }

        public string Kicker { get; }

        /// <summary>The passenger whose big night it is, shown with their portrait, or null.</summary>
        public string Featured { get; }

        public string FeaturedLine { get; }

        public IReadOnlyList<string> Lines { get; }

        public string Emphasis { get; }

        public string Footnote { get; }
    }
}
