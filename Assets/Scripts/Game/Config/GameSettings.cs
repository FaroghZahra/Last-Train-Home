using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>Tuning for how the night plays out on screen. Rules and words live in the data files, not here.</summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Last Train Home/Game Settings")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField, Min(0.001f), Tooltip("Real seconds per minute of track time. 0.1 = 1 s per 10 min.")]
        private float _secondsPerMinute = 0.1f;

        [SerializeField, Min(0f), Tooltip("The train waits this long at the start before pulling away.")]
        private float _departHoldSeconds = 0.6f;

        [SerializeField, Min(0.5f), Tooltip("An on-time arrival card stays this long before the train moves on.")]
        private float _arrivalCardSeconds = 3.5f;

        [SerializeField, Min(0.5f), Tooltip("A late or missed arrival card stays this long. Never shorter than on time.")]
        private float _mutedArrivalCardSeconds = 5f;

        public float SecondsPerMinute => _secondsPerMinute;

        public float DepartHoldSeconds => _departHoldSeconds;

        public float ArrivalCardSeconds => _arrivalCardSeconds;

        public float MutedArrivalCardSeconds => Mathf.Max(_mutedArrivalCardSeconds, _arrivalCardSeconds);
    }
}
