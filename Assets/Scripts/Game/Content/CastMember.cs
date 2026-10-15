namespace LastTrainHome.Game.Content
{
    /// <summary>One of the regulars who ride all week.</summary>
    public sealed class CastMember
    {
        public CastMember(string name, int bigNight, string bigNightLabel, string epilogueKept = null,
            string epilogueMissed = null)
        {
            Name = name;
            BigNight = bigNight;
            BigNightLabel = bigNightLabel ?? string.Empty;
            EpilogueKept = epilogueKept ?? string.Empty;
            EpilogueMissed = epilogueMissed ?? string.Empty;
        }

        public bool HasBigNight => BigNight >= 0;

        /// <summary>The ending's line about them when their big night was kept.</summary>
        public string EpilogueKept { get; }

        /// <summary>The ending's line about them when their big night wasn't kept.</summary>
        public string EpilogueMissed { get; }

        public string Name { get; }

        /// <summary>Index of the night that matters most to them this week, or -1 for a guest with none.</summary>
        public int BigNight { get; }

        /// <summary>What happens on their big night, e.g. "Final warning".</summary>
        public string BigNightLabel { get; }
    }
}
