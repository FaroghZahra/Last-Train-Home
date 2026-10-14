using LastTrainHome.Domain;
using UnityEngine;

namespace LastTrainHome.World
{
    /// <summary>One stop of a journey to play back: where, at what time, and how long to hold the train there.</summary>
    public readonly struct JourneyStop
    {
        public JourneyStop(Vector3 position, ClockTime time, float holdSeconds)
        {
            Position = position;
            Time = time;
            HoldSeconds = Mathf.Max(0f, holdSeconds);
        }

        public Vector3 Position { get; }

        public ClockTime Time { get; }

        public float HoldSeconds { get; }
    }
}
