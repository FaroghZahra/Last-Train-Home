using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Journey;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Input;
using UnityEngine;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// Plays one night on the views: turns player intents into <see cref="NightSession"/> calls and session, journey
    /// and outcome changes into what the views show. Owns no Unity objects; the caller ticks it and disposes it.
    /// </summary>
    public sealed class NightPresenter : IDisposable
    {
        private readonly Level _level;
        private readonly NightSession _session;
        private readonly NightViews _views;
        private readonly NightText _text;
        private readonly GameSettings _settings;
        private readonly IGameInput _input;
        private readonly bool _hasNextNight;
        private readonly List<Vector3> _routePoints = new List<Vector3>();

        private JourneyTimeline _journey;

        public NightPresenter(Level level, bool hasNextNight, NightViews views, NightText text, GameSettings settings,
            IGameInput input)
        {
            _level = level ?? throw new ArgumentNullException(nameof(level));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _hasNextNight = hasNextNight;
            _session = new NightSession(level.Night);
        }

        /// <summary>Raised when the train reaches the end of the line.</summary>
        public event Action<NightResult> NightFinished;

        public event Action NextNightRequested;

        private NightDefinition Night => _level.Night;

        private IReadOnlyList<Passenger> Passengers => Night.Passengers;

        private bool IsPlanning => _session.Phase == NightPhase.Planning;

        /// <summary>Draws the night's map and starts planning. Call once.</summary>
        public void Start()
        {
            Subscribe();
            _views.RailMap.Build(Night.Stations.Select(ToMarker).ToArray(), Night.Network.Tracks);
            ResetForPlanning();
            ShowRoute();
        }

        /// <summary>Advances the journey while the train is running.</summary>
        public void Tick(float deltaSeconds)
        {
            if (_journey == null)
            {
                return;
            }

            _journey.Advance(deltaSeconds);
            if (_journey != null)
            {
                JourneyLeg leg = _journey.CurrentLeg;
                _views.Route.PlaceTrain(Vector3.Lerp(_routePoints[leg.FromStopIndex], _routePoints[leg.ToStopIndex], leg.Progress));
            }
        }

        public void Dispose()
        {
            StopJourney();
            Unsubscribe();
        }

        private void Subscribe()
        {
            _session.RouteChanged += ShowRoute;
            _session.PhaseChanged += OnPhaseChanged;
            _views.RailMap.StationClicked += OnStationClicked;
            _views.Controls.UndoClicked += OnUndo;
            _views.Controls.ClearClicked += OnClear;
            _views.Controls.DepartClicked += OnDepart;
            _views.Controls.FastForwardClicked += OnFastForward;
            _views.Summary.PlanAgainClicked += OnPlanAgain;
            _views.Summary.NextNightClicked += OnNextNight;
            _input.UndoRequested += OnUndo;
        }

        private void Unsubscribe()
        {
            _session.RouteChanged -= ShowRoute;
            _session.PhaseChanged -= OnPhaseChanged;
            _views.RailMap.StationClicked -= OnStationClicked;
            _views.Controls.UndoClicked -= OnUndo;
            _views.Controls.ClearClicked -= OnClear;
            _views.Controls.DepartClicked -= OnDepart;
            _views.Controls.FastForwardClicked -= OnFastForward;
            _views.Summary.PlanAgainClicked -= OnPlanAgain;
            _views.Summary.NextNightClicked -= OnNextNight;
            _input.UndoRequested -= OnUndo;
        }

        private void OnStationClicked(string stationId)
        {
            if (!IsPlanning)
            {
                return;
            }

            string from = StationName(_session.LastStop.StationId);
            AddStopResult result = _session.TryAddStop(stationId);
            if (result.Succeeded)
            {
                return;
            }

            _views.RailMap.FlashRefused(stationId);
            _views.Hint.Show(RouteMessages.DescribeRefusal(result, from, StationName(stationId), Night.Closing));
        }

        private void OnUndo()
        {
            if (IsPlanning)
            {
                _session.TryUndo();
            }
        }

        private void OnClear()
        {
            if (IsPlanning)
            {
                _session.Clear();
            }
        }

        private void OnDepart()
        {
            if (_session.CanDepart)
            {
                _session.Depart();
            }
        }

        private void OnFastForward()
        {
            if (_journey == null)
            {
                return;
            }

            bool isFastForward = _journey.Speed <= 1f;
            _journey.Speed = isFastForward ? _settings.FastForwardMultiplier : 1f;
            _views.Controls.ShowTravelling(isFastForward);
        }

        private void OnPlanAgain()
        {
            if (_session.Phase == NightPhase.Finished)
            {
                _session.Replan();
            }
        }

        private void OnNextNight()
        {
            if (_session.Phase == NightPhase.Finished && _hasNextNight)
            {
                NextNightRequested?.Invoke();
            }
        }

        private void OnPhaseChanged(NightPhase phase)
        {
            switch (phase)
            {
                case NightPhase.Planning:
                    ResetForPlanning();
                    break;
                case NightPhase.Travelling:
                    _views.Hint.Clear();
                    _views.Controls.ShowTravelling(isFastForward: false);
                    StartJourney();
                    break;
                case NightPhase.Finished:
                    _views.Controls.ShowFinished();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        private void ResetForPlanning()
        {
            StopJourney();
            _views.Summary.Hide();
            _views.ArrivalLog.Clear();
            _views.Hint.Clear();
        }

        private void ShowRoute()
        {
            _routePoints.Clear();
            _routePoints.AddRange(_session.Stops.Select(stop => _views.RailMap.GetStationPosition(stop.StationId)));

            _views.Route.ShowRoute(_routePoints);
            _views.Route.PlaceTrain(_routePoints[_routePoints.Count - 1]);
            _views.Clock.Show(_text.PlannedClock(_session.LastStop.Arrival, Night.Closing));
            _views.Passengers.ShowRows(Passengers
                .Select((passenger, i) => _text.PassengerRow(passenger, _session.Outcomes[i], StationName(passenger.DestinationStationId)))
                .ToArray());
            _views.Controls.ShowPlanning(_session.HasLeftStart);
        }

        private void StartJourney()
        {
            float[] holds = Enumerable.Range(0, _session.Stops.Count).Select(HoldSecondsAt).ToArray();
            _journey = new JourneyTimeline(_session.Stops, holds, _settings.SecondsPerMinute);
            _journey.TimeChanged += OnJourneyTimeChanged;
            _journey.StopReached += OnJourneyStopReached;
            _journey.Completed += OnJourneyCompleted;
            _journey.Start();
        }

        private void StopJourney()
        {
            if (_journey == null)
            {
                return;
            }

            _journey.TimeChanged -= OnJourneyTimeChanged;
            _journey.StopReached -= OnJourneyStopReached;
            _journey.Completed -= OnJourneyCompleted;
            _journey = null;
        }

        /// <summary>Longer holds where there is an outcome line to read; none where nobody gets off.</summary>
        private float HoldSecondsAt(int stopIndex)
        {
            float hold = 0f;
            for (int i = 0; i < Passengers.Count; i++)
            {
                PassengerOutcome outcome = _session.Outcomes[i];
                if (outcome.StopIndex != stopIndex)
                {
                    continue;
                }

                bool hasStory = !string.IsNullOrWhiteSpace(Passengers[i].Story.GetOutcomeLine(outcome.Status));
                hold = Mathf.Max(hold, hasStory ? _settings.StoryHoldSeconds : _settings.ArrivalHoldSeconds);
            }

            return hold;
        }

        private void OnJourneyTimeChanged(ClockTime now) =>
            _views.Clock.Show(_text.CurrentClock(now, Night.Closing));

        private void OnJourneyStopReached(int stopIndex)
        {
            string stationName = StationName(_session.Stops[stopIndex].StationId);
            for (int i = 0; i < Passengers.Count; i++)
            {
                if (_session.Outcomes[i].StopIndex == stopIndex)
                {
                    _views.ArrivalLog.AddEntry(_text.Arrival(Passengers[i], _session.Outcomes[i], stationName));
                }
            }
        }

        private void OnJourneyCompleted()
        {
            StopJourney();
            for (int i = 0; i < Passengers.Count; i++)
            {
                if (!_session.Outcomes[i].IsReached)
                {
                    _views.ArrivalLog.AddEntry(_text.NotReached(Passengers[i], StationName(Passengers[i].DestinationStationId)));
                }
            }

            RouteStop end = _session.LastStop;
            _views.Route.PlaceTrain(_routePoints[_routePoints.Count - 1]);
            _views.ArrivalLog.AddEntry(_text.EndOfLine(StationName(end.StationId), end.Arrival));

            NightResult result = _session.Finish();
            _views.Summary.Show(_text.Summary(Passengers, result.Outcomes), _hasNextNight);
            NightFinished?.Invoke(result);
        }

        private StationMarker ToMarker(StationDefinition station)
        {
            StationPlacement placement = _level.GetPlacement(station.Id);
            return new StationMarker(station.Id, station.DisplayName, placement.Position, placement.LabelSide);
        }

        private string StationName(string stationId) => Night.GetStation(stationId).DisplayName;
    }
}
