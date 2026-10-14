using System;
using System.Collections;
using System.Collections.Generic;
using LastTrainHome.Domain;
using UnityEngine;

namespace LastTrainHome.World
{
    /// <summary>
    /// Plays a journey back in real time: moves the train between stops, advances the clock and holds at stops.
    /// Knows nothing about passengers; listeners react to <see cref="StopReached"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class JourneyPlayer : MonoBehaviour
    {
        [SerializeField] private RouteView _routeView;

        [SerializeField, Min(0.001f), Tooltip("Real seconds per minute of track time. 0.1 = 1 s per 10 min.")]
        private float _secondsPerMinute = 0.1f;

        [SerializeField, Min(1f)] private float _fastForwardMultiplier = 4f;

        private Coroutine _playback;
        private float _speed = 1f;

        /// <summary>Raised whenever the displayed minute changes.</summary>
        public event Action<ClockTime> TimeChanged;

        /// <summary>Raised with the stop's index when the train arrives, before it holds there.</summary>
        public event Action<int> StopReached;

        public event Action Completed;

        public bool IsPlaying => _playback != null;

        public bool IsFastForward => _speed > 1f;

        /// <exception cref="InvalidOperationException">A journey is already playing.</exception>
        public void Play(IReadOnlyList<JourneyStop> stops)
        {
            if (stops == null || stops.Count == 0)
            {
                throw new ArgumentException("A journey needs at least one stop.", nameof(stops));
            }

            if (IsPlaying)
            {
                throw new InvalidOperationException("A journey is already playing.");
            }

            _speed = 1f;
            _playback = StartCoroutine(PlayRoutine(stops));
        }

        public void SetFastForward(bool isOn) => _speed = isOn ? _fastForwardMultiplier : 1f;

        /// <summary>Stops playback without raising <see cref="Completed"/>.</summary>
        public void Stop()
        {
            if (_playback != null)
            {
                StopCoroutine(_playback);
                _playback = null;
            }

            _speed = 1f;
        }

        private void Awake() => RequiredReferences.Check(this, (_routeView, nameof(_routeView)));

        private void OnDisable() => Stop();

        private IEnumerator PlayRoutine(IReadOnlyList<JourneyStop> stops)
        {
            yield return ArriveAt(stops, 0);

            for (int i = 1; i < stops.Count; i++)
            {
                yield return Travel(stops[i - 1], stops[i]);
                yield return ArriveAt(stops, i);
            }

            _playback = null;
            Completed?.Invoke();
        }

        private IEnumerator Travel(JourneyStop from, JourneyStop to)
        {
            int minutes = to.Time.MinutesSince(from.Time);
            float duration = minutes * _secondsPerMinute;
            int shownMinute = -1;

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime * _speed)
            {
                float progress = elapsed / duration;
                _routeView.PlaceTrain(Vector3.Lerp(from.Position, to.Position, progress));

                int minute = Mathf.FloorToInt(progress * minutes);
                if (minute != shownMinute)
                {
                    shownMinute = minute;
                    TimeChanged?.Invoke(from.Time.AddMinutes(minute));
                }

                yield return null;
            }
        }

        private IEnumerator ArriveAt(IReadOnlyList<JourneyStop> stops, int index)
        {
            JourneyStop stop = stops[index];
            _routeView.PlaceTrain(stop.Position);
            TimeChanged?.Invoke(stop.Time);
            StopReached?.Invoke(index);

            for (float held = 0f; held < stop.HoldSeconds; held += Time.deltaTime * _speed)
            {
                yield return null;
            }
        }
    }
}
