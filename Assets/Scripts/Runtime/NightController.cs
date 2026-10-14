using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Data;
using LastTrainHome.Domain;
using LastTrainHome.UI;
using LastTrainHome.World;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastTrainHome
{
    /// <summary>
    /// Runs the nights in order. Each night: plan a route, depart, watch the journey, read the summary, then plan
    /// again or go on to the next night.
    /// Owns the game state and is the only place that connects the rules (Domain) to the map (World) and the UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NightController : MonoBehaviour
    {
        private enum Phase
        {
            Planning,
            Travelling,
            Finished,
        }

        [Header("Content")]
        [SerializeField, Tooltip("Level files from Assets/Data/Levels, played in this order.")]
        private TextAsset[] _levels = Array.Empty<TextAsset>();

        [Header("World")]
        [SerializeField] private RailMap _railMap;
        [SerializeField] private RouteView _routeView;
        [SerializeField] private JourneyPlayer _journeyPlayer;

        [Header("UI")]
        [SerializeField] private NightControls _controls;
        [SerializeField] private ClockView _clock;
        [SerializeField] private HintView _hint;
        [SerializeField] private PassengerListView _passengerList;
        [SerializeField] private ArrivalLogView _arrivalLog;
        [SerializeField] private NightSummaryView _summary;

        [Header("Pacing")]
        [SerializeField, Min(0f), Tooltip("Hold at a stop where someone gets off.")]
        private float _arrivalHoldSeconds = 0.6f;

        [SerializeField, Min(0f), Tooltip("Hold at a stop with an outcome line to read.")]
        private float _storyHoldSeconds = 2f;

        private readonly List<Vector3> _routePoints = new List<Vector3>();
        private NightDefinition _night;
        private int _levelIndex;
        private Route _route;
        private Passenger[] _passengers;
        private PassengerOutcome[] _outcomes;
        private Phase _phase;

        private IReadOnlyList<PassengerDefinition> PassengerDefinitions => _night.Passengers;

        private void Awake()
        {
            bool assigned = RequiredReferences.Check(this,
                (_railMap, nameof(_railMap)), (_routeView, nameof(_routeView)),
                (_journeyPlayer, nameof(_journeyPlayer)), (_controls, nameof(_controls)), (_clock, nameof(_clock)),
                (_hint, nameof(_hint)), (_passengerList, nameof(_passengerList)),
                (_arrivalLog, nameof(_arrivalLog)), (_summary, nameof(_summary)));
            if (assigned && (_levels.Length == 0 || _levels.Any(level => level == null)))
            {
                Fail($"{nameof(NightController)} on '{name}' needs at least one level and no empty entries.", this);
            }
        }

        private void OnEnable()
        {
            _railMap.StationClicked += OnStationClicked;
            _controls.UndoRequested += OnUndoRequested;
            _controls.ClearRequested += OnClearRequested;
            _controls.DepartRequested += OnDepartRequested;
            _controls.FastForwardToggled += OnFastForwardToggled;
            _journeyPlayer.TimeChanged += OnJourneyTimeChanged;
            _journeyPlayer.StopReached += OnJourneyStopReached;
            _journeyPlayer.Completed += OnJourneyCompleted;
            _summary.PlanAgainRequested += BeginPlanning;
            _summary.NextNightRequested += OnNextNightRequested;
        }

        private void OnDisable()
        {
            if (_route != null)
            {
                _route.Changed -= OnRouteChanged;
            }

            _railMap.StationClicked -= OnStationClicked;
            _controls.UndoRequested -= OnUndoRequested;
            _controls.ClearRequested -= OnClearRequested;
            _controls.DepartRequested -= OnDepartRequested;
            _controls.FastForwardToggled -= OnFastForwardToggled;
            _journeyPlayer.TimeChanged -= OnJourneyTimeChanged;
            _journeyPlayer.StopReached -= OnJourneyStopReached;
            _journeyPlayer.Completed -= OnJourneyCompleted;
            _summary.PlanAgainRequested -= BeginPlanning;
            _summary.NextNightRequested -= OnNextNightRequested;
        }

        private void Start() => LoadNight(0);

        private bool HasNextNight => _levelIndex + 1 < _levels.Length;

        private void LoadNight(int levelIndex)
        {
            TextAsset level = _levels[levelIndex];
            NightDefinition night;
            try
            {
                night = NightDefinition.FromJson(level.text, level.name);
            }
            catch (FormatException exception)
            {
                Fail(exception.Message, level);
                return;
            }

            if (_route != null)
            {
                _route.Changed -= OnRouteChanged;
            }

            _levelIndex = levelIndex;
            _night = night;
            _railMap.Build(night);
            _passengers = PassengerDefinitions.Select(definition => definition.ToPassenger()).ToArray();
            _route = new Route(night.Network, night.StartStationId, night.Departure, night.Closing);
            _route.Changed += OnRouteChanged;
            _passengerList.Build(PassengerDefinitions, StationName);
            BeginPlanning();
        }

        private void OnNextNightRequested()
        {
            if (_phase == Phase.Finished && HasNextNight)
            {
                LoadNight(_levelIndex + 1);
            }
        }

        private void BeginPlanning()
        {
            _journeyPlayer.Stop();
            _phase = Phase.Planning;
            _outcomes = null;
            _summary.Hide();
            _arrivalLog.Clear();
            _hint.Clear();
            _route.Reset(); // raises Changed, which redraws the planning views
        }

        private void OnRouteChanged()
        {
            _routePoints.Clear();
            _routePoints.AddRange(_route.Stops.Select(stop => _railMap.GetStation(stop.StationId).Position));

            _routeView.ShowRoute(_routePoints);
            _routeView.PlaceTrain(_routePoints[_routePoints.Count - 1]);
            _clock.ShowPlannedEnd(_route.LastStop.Arrival, _route.Closing);
            _passengerList.ShowOutcomes(OutcomeEvaluator.EvaluateAll(_passengers, _route.Stops));
            _controls.ShowPlanning(_route.HasLeftStart);
        }

        private void OnStationClicked(StationView station)
        {
            if (_phase != Phase.Planning)
            {
                return;
            }

            string from = StationName(_route.LastStop.StationId);
            AddStopResult result = _route.TryAddStop(station.Id);
            if (result.Succeeded)
            {
                return;
            }

            station.FlashRefused();
            _hint.Show(RouteMessages.DescribeRefusal(result, from, station.DisplayName, _route.Closing));
        }

        private void OnUndoRequested()
        {
            if (_phase == Phase.Planning)
            {
                _route.TryRemoveLastStop();
            }
        }

        private void OnClearRequested()
        {
            if (_phase == Phase.Planning)
            {
                _route.Reset();
            }
        }

        private void OnDepartRequested()
        {
            if (_phase != Phase.Planning || !_route.HasLeftStart)
            {
                return;
            }

            _phase = Phase.Travelling;
            _outcomes = OutcomeEvaluator.EvaluateAll(_passengers, _route.Stops);
            _hint.Clear();
            _controls.ShowTravelling(isFastForward: false);
            _journeyPlayer.Play(BuildJourney());
        }

        private void OnFastForwardToggled()
        {
            if (_phase != Phase.Travelling)
            {
                return;
            }

            _journeyPlayer.SetFastForward(!_journeyPlayer.IsFastForward);
            _controls.ShowTravelling(_journeyPlayer.IsFastForward);
        }

        private void OnJourneyTimeChanged(ClockTime now) => _clock.ShowCurrentTime(now, _route.Closing);

        private void OnJourneyStopReached(int stopIndex)
        {
            string stationName = StationName(_route.Stops[stopIndex].StationId);
            for (int i = 0; i < _outcomes.Length; i++)
            {
                if (_outcomes[i].StopIndex == stopIndex)
                {
                    _arrivalLog.AddArrival(PassengerDefinitions[i], _outcomes[i], stationName);
                }
            }
        }

        private void OnJourneyCompleted()
        {
            for (int i = 0; i < _outcomes.Length; i++)
            {
                if (!_outcomes[i].IsReached)
                {
                    _arrivalLog.AddNotReached(PassengerDefinitions[i], StationName(_passengers[i].DestinationStationId));
                }
            }

            RouteStop end = _route.LastStop;
            _arrivalLog.AddEndOfLine(StationName(end.StationId), end.Arrival);

            _phase = Phase.Finished;
            _controls.ShowFinished();
            _summary.Show(PassengerDefinitions, _outcomes, HasNextNight);
        }

        private JourneyStop[] BuildJourney()
        {
            IReadOnlyList<RouteStop> stops = _route.Stops;
            var journey = new JourneyStop[stops.Count];
            for (int i = 0; i < stops.Count; i++)
            {
                journey[i] = new JourneyStop(_routePoints[i], stops[i].Arrival, HoldSecondsAt(i));
            }

            return journey;
        }

        /// <summary>Longer holds where there is an outcome line to read; none where nobody gets off.</summary>
        private float HoldSecondsAt(int stopIndex)
        {
            float hold = 0f;
            for (int i = 0; i < _outcomes.Length; i++)
            {
                if (_outcomes[i].StopIndex != stopIndex)
                {
                    continue;
                }

                bool hasStory = !string.IsNullOrWhiteSpace(PassengerDefinitions[i].GetOutcomeLine(_outcomes[i].Status));
                hold = Mathf.Max(hold, hasStory ? _storyHoldSeconds : _arrivalHoldSeconds);
            }

            return hold;
        }

        private string StationName(string stationId) => _railMap.GetStation(stationId).DisplayName;

        private void Fail(string message, Object context)
        {
            Debug.LogError(message, context);
            enabled = false;
        }
    }
}
