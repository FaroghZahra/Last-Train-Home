using System;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// The JSON shape of Data/Story/Week.json, read with JsonUtility: the title screen, the days, the regulars and the
    /// keepsakes they can leave in the cab. <see cref="WeekParser"/> checks it.
    /// </summary>
    [Serializable]
    internal sealed class WeekFile
    {
        public string title;
        public string[] intro;
        public string introEmphasis;
        public string footnote;
        public string[] days; // short day names, one per level, e.g. "Mon"
        public CastMember[] cast;
        public Keepsake[] keepsakes;
        public Ending[] endings; // one per number of big nights kept, 0 to the number of big nights

        [Serializable]
        internal sealed class CastMember
        {
            public string name; // matches the level files and PassengerArt
            public int bigNight; // index into days; -1 for a guest with no big night this week
            public string bigNightLabel; // e.g. "Final warning"
            public string epilogueKept; // the ending's line about them when their big night was kept
            public string epilogueMissed; // ... and when it wasn't
        }

        [Serializable]
        internal sealed class Ending
        {
            public int kept; // how many big nights were kept
            public string title;
            public string line;
        }

        [Serializable]
        internal sealed class Keepsake
        {
            public string id;
            public string name; // e.g. "Priya's setlist"
            public string line; // shown when it's left, e.g. "Her setlist, taped to your cab window."
        }
    }
}
