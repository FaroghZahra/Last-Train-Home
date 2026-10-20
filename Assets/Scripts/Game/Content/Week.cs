using System;
using System.Collections.Generic;
using System.Linq;

namespace LastTrainHome.Game.Content
{
    /// <summary>The whole week's story outside any one night: the title screen, the days, the regulars, the keepsakes.</summary>
    public sealed class Week
    {
        public Week(string title, IReadOnlyList<string> intro, string introEmphasis, string footnote,
            IReadOnlyList<string> days, IReadOnlyList<CastMember> cast, IReadOnlyList<Keepsake> keepsakes,
            IReadOnlyList<Ending> endings = null)
        {
            Endings = endings ?? Array.Empty<Ending>();
            Title = title ?? string.Empty;
            Intro = intro ?? Array.Empty<string>();
            IntroEmphasis = introEmphasis ?? string.Empty;
            Footnote = footnote ?? string.Empty;
            Days = days ?? throw new ArgumentNullException(nameof(days));
            Cast = cast ?? throw new ArgumentNullException(nameof(cast));
            Keepsakes = keepsakes ?? Array.Empty<Keepsake>();
        }

        public string Title { get; }

        public IReadOnlyList<string> Intro { get; }

        public string IntroEmphasis { get; }

        public string Footnote { get; }

        /// <summary>Short day names, one per night, e.g. "Mon".</summary>
        public IReadOnlyList<string> Days { get; }

        public IReadOnlyList<CastMember> Cast { get; }

        public IReadOnlyList<Keepsake> Keepsakes { get; }

        /// <summary>How the week can end, one per number of big nights kept.</summary>
        public IReadOnlyList<Ending> Endings { get; }

        /// <summary>The ending for this many big nights kept, or null.</summary>
        public Ending EndingFor(int kept) => Endings.FirstOrDefault(ending => ending.Kept == kept);

        /// <summary>The cast member with this name, or null.</summary>
        public CastMember FindCastMember(string name) => Cast.FirstOrDefault(member => member.Name == name);

        /// <summary>The keepsake with this id, or null.</summary>
        public Keepsake FindKeepsake(string id) => Keepsakes.FirstOrDefault(keepsake => keepsake.Id == id);

        /// <summary>Whose big nights fall on this night, in cast order; empty for none. Two can share a night.</summary>
        public IReadOnlyList<CastMember> BigNightsOn(int nightIndex) =>
            Cast.Where(member => member.BigNight == nightIndex).ToArray();

        /// <summary>The most big nights a week can keep: only one of those sharing a night can be kept.</summary>
        public int KeepableBigNights => KeepableBigNightsOf(Cast);

        internal static int KeepableBigNightsOf(IEnumerable<CastMember> cast) =>
            cast.Where(member => member.HasBigNight).Select(member => member.BigNight).Distinct().Count();
    }
}
