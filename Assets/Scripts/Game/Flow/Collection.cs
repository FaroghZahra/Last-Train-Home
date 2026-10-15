using System.Globalization;
using System.Linq;
using LastTrainHome.Game.Content;

namespace LastTrainHome.Game.Flow
{
    /// <summary>The player's collection across every week played: keepsakes found and endings seen.</summary>
    public static class Collection
    {
        /// <summary>E.g. "5 of 12 keepsakes found · 2 of 5 endings seen"; empty before anything is found.</summary>
        public static string Describe(GameContent content, IProgressStore progress)
        {
            int found = content.Week.Keepsakes.Count(keepsake => progress.FoundKeepsakes.Contains(keepsake.Id));
            int endings = content.Week.Endings.Count(ending => progress.EndingsSeen.Contains(ending.Kept));
            if (found == 0 && endings == 0)
            {
                return string.Empty;
            }

            return content.Ui.Format(UiKey.ProgressLine,
                ("found", found.ToString(CultureInfo.InvariantCulture)),
                ("total", content.Week.Keepsakes.Count.ToString(CultureInfo.InvariantCulture)),
                ("endings", endings.ToString(CultureInfo.InvariantCulture)),
                ("endingCount", content.Week.Endings.Count.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
