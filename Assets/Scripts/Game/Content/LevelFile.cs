using System;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// The JSON shape of a level file in Assets/Data/Levels, read with JsonUtility. Field names are the JSON keys,
    /// so renaming one breaks every level file. Times are "HH:MM" after midnight. <see cref="LevelParser"/> checks it.
    /// A level holds one night's rules and its whole story; every line the player reads that night comes from here,
    /// apart from interface wording (Data/Text/UiText.json) and the week's cast (Data/Story/Week.json).
    /// </summary>
    [Serializable]
    internal sealed class LevelFile
    {
        public string title; // the day, e.g. "Monday"
        public string startStation;
        public string departure;
        public string closing;
        public Station[] stations;
        public Track[] tracks;
        public Rules rules;
        public NightCard nightCard;
        public Platform platform;
        public Passenger[] passengers;
        public Variant[] variants; // optional; carry-over from earlier nights
        public CoachRule[] coach; // optional; the planning coach line, first match wins
        public Secret[] secrets; // optional; keepsakes found by passing a station early enough
        public Logbook logbook;

        [Serializable]
        internal sealed class Station
        {
            public string id;
            public string displayName; // optional; the id is shown when empty
            public string label; // optional; "above" (default), "below", "left" or "right"
            public bool closed; // optional; drawn, but the train can't stop there
            public float x;
            public float y;
        }

        [Serializable]
        internal sealed class Track
        {
            public string from;
            public string to;
            public int minutes;
            public bool closed; // optional; drawn, but the train can't use it
            public Point[] via; // optional; where the drawn track bends on its way from 'from' to 'to'
        }

        [Serializable]
        internal sealed class Point
        {
            public float x;
            public float y;
        }

        [Serializable]
        internal sealed class Rules
        {
            public int asks; // questions on the platform tonight; -1 for as many as the player likes
        }

        [Serializable]
        internal sealed class NightCard
        {
            public string kicker;
            public string featured; // optional; the passenger whose big night it is
            public string featuredLine; // optional; shown beside their portrait
            public string[] lines;
            public string emphasis; // optional; the last, brighter line
            public string footnote; // optional
        }

        [Serializable]
        internal sealed class Platform
        {
            public string kicker;
            public string headline;
            public string coach;
        }

        [Serializable]
        internal sealed class Passenger
        {
            public string name; // must be in the week's cast
            public string destination;
            public string deadline;
            public string says; // optional; the deadline they give until asked, when it isn't the real one
            public string boards; // optional; where the train picks them up, when it isn't the start station
            public string platform; // what they say as they board
            public string planQuote; // optional; the short version on the planning card
            public string ask; // their answer when asked how they are
            public bool revealsBigNight; // optional; asking tells the player which night is theirs
            public Outcome onTime;
            public Outcome late;
            public Outcome notReached;
        }

        [Serializable]
        internal sealed class Outcome
        {
            public string line;
            public string logLine; // optional; shorter, for the logbook
            public string keepsake; // optional; an id from the week's keepsakes
            public string aside; // optional; a small moment beside the arrival
            public string carry; // optional; what this changes about tomorrow
            public string weekNote; // optional; added to their big night in the week ahead

            public bool IsEmpty => string.IsNullOrEmpty(line) && string.IsNullOrEmpty(logLine)
                                   && string.IsNullOrEmpty(keepsake) && string.IsNullOrEmpty(aside)
                                   && string.IsNullOrEmpty(carry) && string.IsNullOrEmpty(weekNote);
        }

        /// <summary>Replaces parts of one passenger when an earlier night went a certain way. Empty fields keep theirs.</summary>
        [Serializable]
        internal sealed class Variant
        {
            public Condition @if;
            public string passenger;
            public string deadline;
            public string destination; // optional; somewhere else tonight
            public string platform;
            public string planQuote;
            public string ask;
            public Outcome onTime;
            public Outcome late;
            public Outcome notReached;
        }

        [Serializable]
        internal sealed class Condition
        {
            public string night; // an earlier level's id, e.g. "Night1"
            public string passenger;
            public string status; // "onTime", "late", "notReached" or "missed" (late or not reached)
            public bool promised; // optional; only when the player had promised them that night
        }

        [Serializable]
        internal sealed class CoachRule
        {
            public string when; // see CoachCondition
            public int count; // for "stops" and "onTime"
            public string station; // for "deadEnd"
            public string text;
        }

        [Serializable]
        internal sealed class Secret
        {
            public string station;
            public string by; // the train must stop there at or before this time
            public string keepsake; // an id from the week's keepsakes
        }

        [Serializable]
        internal sealed class Logbook
        {
            public string tomorrow;
        }
    }
}
