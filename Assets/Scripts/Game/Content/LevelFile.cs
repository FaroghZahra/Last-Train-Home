using System;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// The JSON shape of a level file in Assets/Data/Levels, read with JsonUtility. Field names are the JSON keys,
    /// so renaming one breaks every level file. Times are "HH:MM" after midnight. <see cref="LevelParser"/> checks it.
    /// </summary>
    [Serializable]
    internal sealed class LevelFile
    {
        public string title;
        public string startStation;
        public string departure;
        public string closing;
        public Station[] stations;
        public Track[] tracks;
        public Passenger[] passengers;

        [Serializable]
        internal sealed class Station
        {
            public string id;
            public string displayName; // optional; the id is shown when empty
            public string label; // optional; "above" (default), "below", "left" or "right"
            public float x;
            public float y;
        }

        [Serializable]
        internal sealed class Track
        {
            public string from;
            public string to;
            public int minutes;
        }

        [Serializable]
        internal sealed class Passenger
        {
            public string name;
            public string destination;
            public string deadline;
            public string motivation;
            public string onTime;
            public string late;
            public string notReached;
        }
    }
}
