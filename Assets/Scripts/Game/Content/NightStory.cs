using System;
using System.Collections.Generic;

namespace LastTrainHome.Game.Content
{
    /// <summary>A night's words outside the passengers' own: its night card, platform, coach and logbook.</summary>
    public sealed class NightStory
    {
        /// <summary>Tokens the night card and platform text may use.</summary>
        public static readonly string[] HeaderTokens = { "day", "night", "nights" };

        public NightStory(NightCardText nightCard, PlatformText platform, int asksAllowed,
            IReadOnlyList<CoachRule> coach, string logbookTomorrow)
        {
            NightCard = nightCard ?? throw new ArgumentNullException(nameof(nightCard));
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
            AsksAllowed = asksAllowed;
            Coach = coach ?? Array.Empty<CoachRule>();
            LogbookTomorrow = logbookTomorrow ?? string.Empty;
        }

        public static NightStory Empty { get; } = new NightStory(
            new NightCardText(null, null, null, null, null, null), new PlatformText(null, null, null), -1, null, null);

        public NightCardText NightCard { get; }

        public PlatformText Platform { get; }

        /// <summary>Questions the player may ask on the platform; negative for as many as they like.</summary>
        public int AsksAllowed { get; }

        public IReadOnlyList<CoachRule> Coach { get; }

        public string LogbookTomorrow { get; }
    }
}
