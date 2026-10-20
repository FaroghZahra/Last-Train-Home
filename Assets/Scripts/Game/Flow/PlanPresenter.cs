using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Input;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// Planning tonight's route on the map. Turns station clicks and buttons into <see cref="NightSession"/> calls and
    /// shows what the route means for each passenger. Depart names who waits; pressing it finishes this step.
    /// </summary>
    public sealed class PlanPresenter : IFlowStep
    {
        private readonly NightContext _night;
        private readonly GameViews _views;
        private readonly RouteDrawing _drawing;
        private readonly IGameInput _input;
        private readonly IGameAudio _audio;

        public PlanPresenter(NightContext night, GameViews views, RouteDrawing drawing, IGameInput input,
            IGameAudio audio = null)
        {
            _audio = audio ?? new SilentAudio();
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _drawing = drawing ?? throw new ArgumentNullException(nameof(drawing));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public event Action Finished;

        private NightSession Session => _night.Session;

        private NightDefinition Night => _night.Night;

        private UiText Ui => _night.Ui;

        private bool IsPlanning => Session.Phase == NightPhase.Planning;

        public void Start()
        {
            Session.RouteChanged += Show;
            _views.RailMap.StationClicked += OnStationClicked;
            _views.Plan.UndoClicked += OnUndo;
            _views.Plan.ClearClicked += OnClear;
            _views.Plan.DepartClicked += OnDepart;
            _input.UndoRequested += OnUndo;
            Show();
        }

        public void Tick(float deltaSeconds)
        {
        }

        public void Dispose()
        {
            Session.RouteChanged -= Show;
            _views.RailMap.StationClicked -= OnStationClicked;
            _views.Plan.UndoClicked -= OnUndo;
            _views.Plan.ClearClicked -= OnClear;
            _views.Plan.DepartClicked -= OnDepart;
            _input.UndoRequested -= OnUndo;
            _views.RailMap.HighlightStations(Array.Empty<string>());
            _views.Plan.Hide();
        }

        private void OnStationClicked(string stationId)
        {
            if (!IsPlanning)
            {
                return;
            }

            string from = _night.StationName(Session.LastStop.StationId);
            AddStopResult result = Session.TryAddStop(stationId);
            if (result.Succeeded)
            {
                _audio.Play(GameSound.StopAdded);
                return;
            }

            _audio.Play(GameSound.Refused);
            _views.RailMap.FlashRefused(stationId);
            _views.Plan.ShowRefusal(RouteMessages.DescribeRefusal(result, from, _night.StationName(stationId), Night.Closing, Ui));
        }

        private void OnUndo()
        {
            if (IsPlanning)
            {
                Session.TryUndo();
            }
        }

        private void OnClear()
        {
            if (IsPlanning)
            {
                Session.Clear();
            }
        }

        private void OnDepart()
        {
            if (!Session.CanDepart)
            {
                return;
            }

            Session.Depart();
            _audio.Play(GameSound.Depart);
            Finished?.Invoke();
        }

        private void Show()
        {
            _drawing.Redraw();
            _views.RailMap.HighlightStations(NextStations());
            _views.Plan.Show(BuildModel());
        }

        private PlanModel BuildModel()
        {
            // Everything here is as the player knows it: someone who hasn't been asked may have a different real deadline.
            IReadOnlyList<Passenger> passengers = _night.KnownPassengers;
            IReadOnlyList<PassengerOutcome> outcomes = _night.KnownOutcomes;
            string promised = Session.PromisedPassenger;
            int promisedIndex = passengers.ToList().FindIndex(passenger => passenger.Name == promised);
            Passenger promisedPassenger = promisedIndex >= 0 ? passengers[promisedIndex] : null;
            PassengerOutcome promisedOutcome = promisedIndex >= 0 ? outcomes[promisedIndex] : default;
            bool promiseKept = promisedPassenger != null && promisedOutcome.Status == ArrivalStatus.OnTime;

            return new PlanModel
            {
                NightLabel = Ui.Format(UiKey.PlanNight,
                    ("day", _night.ShortDay(_night.NightIndex)),
                    ("night", (_night.NightIndex + 1).ToString(CultureInfo.InvariantCulture)),
                    ("nights", _night.NightCount.ToString(CultureInfo.InvariantCulture))),
                ClockLabel = Ui.Format(UiKey.PlanClock, ("end", Session.LastStop.Arrival.ToString()),
                    ("closing", Night.Closing.ToString())),
                Coach = CoachLine.Pick(_night.Level.Story.Coach, Session, passengers, outcomes, Ui),
                Cards = passengers.Select((passenger, i) => ToCard(passenger, outcomes[i])).ToArray(),
                HasPromise = promisedPassenger != null,
                PromiseHeader = Ui.Get(UiKey.PlanPromiseHeader),
                PromiseLine = promisedPassenger == null ? string.Empty : Ui.Format(UiKey.PlanPromiseLine,
                    ("name", promised), ("destination", _night.StationName(promisedPassenger.DestinationStationId)),
                    ("deadline", promisedPassenger.Deadline.ToString())),
                PromiseStatus = promiseKept
                    ? Ui.Format(UiKey.PlanPromiseKept, ("time", promisedOutcome.Arrival.ToString()))
                    : Ui.Get(UiKey.PlanPromiseBroken),
                PromiseKept = promiseKept,
                RouteHeader = Ui.Get(UiKey.PlanRouteHeader),
                Stops = Session.Stops.Select((stop, i) => ToStop(stop, i)).ToArray(),
                UndoLabel = Ui.Get(UiKey.PlanUndo),
                ClearLabel = Ui.Get(UiKey.PlanClear),
                CanEdit = Session.HasLeftStart,
                DepartLabel = Ui.Get(UiKey.PlanDepart),
                DepartDetail = WhoWaits(passengers, outcomes),
                CanDepart = Session.CanDepart,
            };
        }

        private PlanCardModel ToCard(Passenger passenger, PassengerOutcome outcome)
        {
            (string status, OutcomeKind kind) = outcome.Status switch
            {
                ArrivalStatus.OnTime => (Ui.Format(UiKey.PlanChipOnTime, ("time", outcome.Arrival.ToString())), OutcomeKind.OnTime),
                ArrivalStatus.Late => (Ui.Format(UiKey.PlanChipLate, ("time", outcome.Arrival.ToString())), OutcomeKind.Late),
                _ => (Ui.Get(UiKey.PlanChipNotReached), OutcomeKind.NotReached),
            };

            return new PlanCardModel
            {
                Name = passenger.Name,
                Status = status,
                Outcome = kind,
                Quote = passenger.Story.PlanQuote,
                Detail = Ui.Format(UiKey.PlanCard, ("destination", _night.Trip(passenger)),
                    ("deadline", passenger.Deadline.ToString())),
                Destination = _night.StationName(passenger.DestinationStationId),
                Deadline = AlongTheNight(passenger.Deadline),
                Arrival = outcome.IsReached ? AlongTheNight(outcome.Arrival) : (float?)null,
                IsPromised = passenger.Name == Session.PromisedPassenger,
                Week = WeekStrip.Pips(_night, passenger.Name),
            };
        }

        private PlanStopModel ToStop(RouteStop stop, int index)
        {
            string station = _night.StationName(stop.StationId);
            string[] alighting = Night.Passengers
                .Where((passenger, i) => Session.Outcomes[i].IsReached && Session.Outcomes[i].StopIndex == index)
                .Select(passenger => passenger.Name)
                .ToArray();
            string[] boarding = Night.Passengers
                .Where((passenger, i) => passenger.IsPickup && Session.Outcomes[i].BoardStopIndex == index)
                .Select(passenger => passenger.Name)
                .ToArray();
            string label = alighting.Length > 0
                ? Ui.Format(UiKey.PlanStopWith, ("station", station), ("names", Ui.JoinNames(alighting)))
                : station;
            if (boarding.Length > 0)
            {
                label += Ui.Get(UiKey.ListDot) + Ui.Format(UiKey.PlanStopBoards, ("names", Ui.JoinNames(boarding)));
            }

            return new PlanStopModel { Label = label, Time = stop.Arrival.ToString() };
        }

        /// <summary>What Depart would mean right now, e.g. "Priya will be late · Agnes won't get there".</summary>
        private string WhoWaits(IReadOnlyList<Passenger> passengers, IReadOnlyList<PassengerOutcome> outcomes)
        {
            if (!Session.HasLeftStart)
            {
                return Ui.Get(UiKey.PlanDepartEmpty);
            }

            string[] NamesWith(ArrivalStatus status) => passengers
                .Where((passenger, i) => outcomes[i].Status == status)
                .Select(passenger => passenger.Name)
                .ToArray();

            string[] late = NamesWith(ArrivalStatus.Late);
            string[] notReached = NamesWith(ArrivalStatus.NotReached);
            if (late.Length == 0 && notReached.Length == 0)
            {
                return Ui.Get(UiKey.PlanDepartAll);
            }

            var parts = new List<string>();
            if (late.Length > 0)
            {
                parts.Add(Ui.Format(UiKey.PlanDepartLate, ("names", Ui.JoinNames(late))));
            }

            if (notReached.Length > 0)
            {
                parts.Add(Ui.Format(UiKey.PlanDepartNotReached, ("names", Ui.JoinNames(notReached))));
            }

            return string.Join(Ui.Get(UiKey.ListDot), parts);
        }

        /// <summary>Stations the train could go to next: joined to the last stop, open, and reachable before closing.</summary>
        private IReadOnlyCollection<string> NextStations()
        {
            RouteStop last = Session.LastStop;
            return Night.Stations
                .Where(station => Night.Network.TryGetTravelMinutes(last.StationId, station.Id, out int minutes)
                                  && last.Arrival.AddMinutes(minutes) <= Night.Closing)
                .Select(station => station.Id)
                .ToArray();
        }

        private float AlongTheNight(ClockTime time)
        {
            float length = Night.Closing.MinutesSince(Night.Departure);
            return length <= 0f ? 0f : Math.Min(1f, time.MinutesSince(Night.Departure) / length);
        }
    }
}
