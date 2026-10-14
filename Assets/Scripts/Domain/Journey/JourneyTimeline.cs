using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Domain.Journey
{
    /// <summary>
    /// Plays a route back in real time. The owner calls <see cref="Advance"/> with elapsed seconds (e.g. once per frame);
    /// the timeline moves the train along its legs, holds it at stops and reports the clock as it goes.
    /// </summary>
    public sealed class JourneyTimeline
    {
        private readonly IReadOnlyList<RouteStop> _stops;
        private readonly IReadOnlyList<float> _holdSeconds;
        private readonly float _secondsPerMinute;

        private int _stopIndex;
        private bool _isHolding;
        private float _phaseElapsed;
        private ClockTime? _shownTime;
        private float _speed = 1f;

        /// <param name="holdSeconds">How long the train waits at each stop, one entry per stop.</param>
        /// <param name="secondsPerMinute">Real seconds per minute of track time.</param>
        public JourneyTimeline(IReadOnlyList<RouteStop> stops, IReadOnlyList<float> holdSeconds, float secondsPerMinute)
        {
            if (stops == null || stops.Count == 0)
            {
                throw new ArgumentException("A journey needs at least one stop.", nameof(stops));
            }

            if (holdSeconds == null || holdSeconds.Count != stops.Count)
            {
                throw new ArgumentException("Every stop needs exactly one hold time.", nameof(holdSeconds));
            }

            for (int i = 0; i < holdSeconds.Count; i++)
            {
                if (holdSeconds[i] < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(holdSeconds), holdSeconds[i], "Hold times cannot be negative.");
                }
            }

            if (secondsPerMinute <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(secondsPerMinute), secondsPerMinute, "Must be positive.");
            }

            _stops = stops;
            _holdSeconds = holdSeconds;
            _secondsPerMinute = secondsPerMinute;
        }

        /// <summary>Raised whenever the shown minute changes.</summary>
        public event Action<ClockTime> TimeChanged;

        /// <summary>Raised with the stop's index when the train arrives, before it holds there.</summary>
        public event Action<int> StopReached;

        public event Action Completed;

        public bool IsStarted { get; private set; }

        public bool IsComplete { get; private set; }

        public JourneyLeg CurrentLeg { get; private set; }

        /// <summary>Playback rate; 1 is normal speed.</summary>
        public float Speed
        {
            get => _speed;
            set => _speed = value > 0f ? value : throw new ArgumentOutOfRangeException(nameof(value), value, "Must be positive.");
        }

        /// <summary>Puts the train at the first stop.</summary>
        /// <exception cref="InvalidOperationException">Already started.</exception>
        public void Start()
        {
            if (IsStarted)
            {
                throw new InvalidOperationException("The journey has already started.");
            }

            IsStarted = true;
            ArriveAt(0);
        }

        /// <summary>Moves the journey forward. Large steps run through as many stops as they cover.</summary>
        public void Advance(float seconds)
        {
            if (!IsStarted || IsComplete)
            {
                return;
            }

            float remaining = Math.Max(0f, seconds) * _speed;
            while (!IsComplete)
            {
                float phaseLength = _isHolding ? _holdSeconds[_stopIndex] : LegSeconds(_stopIndex);
                float needed = phaseLength - _phaseElapsed;
                if (remaining < needed)
                {
                    _phaseElapsed += remaining;
                    if (!_isHolding)
                    {
                        UpdateTravel();
                    }

                    return;
                }

                remaining -= needed;
                if (!_isHolding)
                {
                    ArriveAt(_stopIndex + 1);
                }
                else if (_stopIndex == _stops.Count - 1)
                {
                    IsComplete = true;
                    Completed?.Invoke();
                }
                else
                {
                    _isHolding = false;
                    _phaseElapsed = 0f;
                }
            }
        }

        private float LegSeconds(int fromStop) => LegMinutes(fromStop) * _secondsPerMinute;

        private int LegMinutes(int fromStop) => _stops[fromStop + 1].Arrival.MinutesSince(_stops[fromStop].Arrival);

        private void ArriveAt(int stopIndex)
        {
            _stopIndex = stopIndex;
            _isHolding = true;
            _phaseElapsed = 0f;
            CurrentLeg = JourneyLeg.AtStop(stopIndex);
            ShowTime(_stops[stopIndex].Arrival);
            StopReached?.Invoke(stopIndex);
        }

        private void UpdateTravel()
        {
            float progress = _phaseElapsed / LegSeconds(_stopIndex);
            CurrentLeg = new JourneyLeg(_stopIndex, _stopIndex + 1, progress);

            int minute = (int)Math.Floor(progress * LegMinutes(_stopIndex));
            ShowTime(_stops[_stopIndex].Arrival.AddMinutes(minute));
        }

        private void ShowTime(ClockTime time)
        {
            if (_shownTime == time)
            {
                return;
            }

            _shownTime = time;
            TimeChanged?.Invoke(time);
        }
    }
}
