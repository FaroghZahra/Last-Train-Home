using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>Tuning for how the night plays out on screen. Rules live in the level files, not here.</summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Last Train Home/Game Settings")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField, Min(0.001f), Tooltip("Real seconds per minute of track time. 0.1 = 1 s per 10 min.")]
        private float _secondsPerMinute = 0.1f;

        [SerializeField, Min(1f)] private float _fastForwardMultiplier = 4f;

        [SerializeField, Min(0f), Tooltip("Hold at a stop where someone gets off.")]
        private float _arrivalHoldSeconds = 0.6f;

        [SerializeField, Min(0f), Tooltip("Hold at a stop where someone gets off with an outcome line to read.")]
        private float _storyHoldSeconds = 2f;

        public float SecondsPerMinute => _secondsPerMinute;

        public float FastForwardMultiplier => _fastForwardMultiplier;

        public float ArrivalHoldSeconds => _arrivalHoldSeconds;

        public float StoryHoldSeconds => _storyHoldSeconds;
    }
}
