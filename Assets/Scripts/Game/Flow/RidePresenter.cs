using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Journey;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// The ride: plays the locked route back on the map and stops for every arrival with its own card, then shows a
    /// card for everyone who was never reached. Each card moves on by itself after a moment or when the player
    /// continues; late and missed ones stay longer. The last card leads to the logbook, which finishes this step.
    /// </summary>
    public sealed class RidePresenter : IFlowStep
    {
        private readonly NightContext _night;
        private readonly GameViews _views;
        private readonly RouteDrawing _drawing;
        private readonly GameSettings _settings;
        private readonly IGameAudio _audio;
        private readonly Queue<int> _waitingCards = new Queue<int>();
        private readonly List<int> _cardOrder;
        private readonly List<string> _aboard;

        private JourneyTimeline _journey;
        private int _showing = -1;
        private int _cardsShown;
        private float _countdown;
        private int _shownSeconds;

        public RidePresenter(NightContext night, GameViews views, RouteDrawing drawing, GameSettings settings,
            IGameAudio audio = null)
        {
            _audio = audio ?? new SilentAudio();
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _drawing = drawing ?? throw new ArgumentNullException(nameof(drawing));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));

            // Everyone reached, in the order the train reaches them, then everyone who never was.
            IReadOnlyList<PassengerOutcome> outcomes = Session.Outcomes;
            _cardOrder = Enumerable.Range(0, Passengers.Count)
                .OrderBy(i => outcomes[i].IsReached ? outcomes[i].StopIndex : int.MaxValue)
                .ThenBy(i => i)
                .ToList();
            _aboard = _cardOrder.Where(i => !Passengers[i].IsPickup).Select(i => Passengers[i].Name).ToList();
        }

        public event Action Finished;

        private NightSession Session => _night.Session;

        private IReadOnlyList<Passenger> Passengers => _night.Night.Passengers;

        private UiText Ui => _night.Ui;

        private int ReachedCount => Session.Outcomes.Count(outcome => outcome.IsReached);

        public void Start()
        {
            if (Session.Phase != NightPhase.Travelling)
            {
                throw new InvalidOperationException("The ride starts once the train has departed.");
            }

            _views.Ride.ContinueClicked += OnContinue;
            _views.Ride.Show();
            _drawing.ShowPickupsWaiting(); // everyone at the start has boarded

            float[] holds = Enumerable.Range(0, Session.Stops.Count).Select(HoldAt).ToArray();
            _journey = new JourneyTimeline(Session.Stops, holds, _settings.SecondsPerMinute);
            _journey.TimeChanged += OnTimeChanged;
            _journey.StopReached += OnStopReached;
            _journey.Completed += OnCompleted;
            _journey.Start();
        }

        public void Tick(float deltaSeconds)
        {
            if (_showing >= 0)
            {
                CountDown(deltaSeconds);
                return;
            }

            if (_journey == null || _journey.IsComplete)
            {
                return;
            }

            _journey.Advance(deltaSeconds);
            if (_journey != null && !_journey.IsComplete)
            {
                _views.Route.PlaceTrain(_drawing.TrainPosition(_journey.CurrentLeg));
            }
        }

        public void Dispose()
        {
            _views.Ride.ContinueClicked -= OnContinue;
            if (_journey != null)
            {
                _journey.TimeChanged -= OnTimeChanged;
                _journey.StopReached -= OnStopReached;
                _journey.Completed -= OnCompleted;
                _journey = null;
            }

            _views.Ride.HideArrival();
            _views.Ride.Hide();
        }

        /// <summary>
        /// A quick pause at the start and wherever someone boards; an open-ended wait wherever someone gets off; none
        /// elsewhere.
        /// </summary>
        private float HoldAt(int stopIndex)
        {
            if (Session.Outcomes.Any(outcome => outcome.IsReached && outcome.StopIndex == stopIndex))
            {
                return float.PositiveInfinity;
            }

            return stopIndex == 0 || BoardingAt(stopIndex).Any() ? _settings.DepartHoldSeconds : 0f;
        }

        /// <summary>Pickups who board at this stop, by index.</summary>
        private IEnumerable<int> BoardingAt(int stopIndex) => Enumerable.Range(0, Passengers.Count)
            .Where(i => Passengers[i].IsPickup && Session.Outcomes[i].BoardStopIndex == stopIndex);

        private void OnTimeChanged(ClockTime now) => ShowProgress(now, null);

        private void OnStopReached(int stopIndex)
        {
            RouteStop stop = Session.Stops[stopIndex];
            _views.Route.PlaceTrain(_drawing.TrainPosition(JourneyLeg.AtStop(stopIndex)));
            foreach (int i in BoardingAt(stopIndex))
            {
                _views.RailMap.HidePassenger(Passengers[i].Name);
                _aboard.Add(Passengers[i].Name);
                _audio.Play(GameSound.Boards);
            }

            ShowProgress(stop.Arrival, stopIndex);

            foreach (int i in _cardOrder.Where(i => Session.Outcomes[i].IsReached && Session.Outcomes[i].StopIndex == stopIndex))
            {
                _views.RailMap.ShowPassenger(Passengers[i].Name, stop.StationId);
                _waitingCards.Enqueue(i);
            }

            ShowNextCard();
        }

        private void OnCompleted()
        {
            foreach (int i in _cardOrder.Where(i => !Session.Outcomes[i].IsReached))
            {
                _waitingCards.Enqueue(i);
            }

            if (!ShowNextCard())
            {
                FinishRide();
            }
        }

        private void OnContinue()
        {
            if (_showing < 0)
            {
                return;
            }

            bool wasLast = _cardsShown == _cardOrder.Count;
            _views.Ride.HideArrival();
            _showing = -1;
            if (ShowNextCard())
            {
                return;
            }

            if (wasLast)
            {
                FinishRide();
            }
            else
            {
                _journey.ReleaseHold();
            }
        }

        /// <returns>False when no card is waiting.</returns>
        private bool ShowNextCard()
        {
            if (_waitingCards.Count == 0)
            {
                return false;
            }

            _showing = _waitingCards.Dequeue();
            _cardsShown++;
            _aboard.Remove(Passengers[_showing].Name);
            ArrivalModel card = BuildCard(_showing, isLast: _cardsShown == _cardOrder.Count);
            _views.Ride.ShowArrival(card);
            _audio.Play(card.IsMuted ? GameSound.Missed : IsJustInTime(_showing) ? GameSound.JustInTime : GameSound.OnTime);

            _countdown = card.IsMuted ? _settings.MutedArrivalCardSeconds : _settings.ArrivalCardSeconds;
            _shownSeconds = -1;
            if (_cardsShown == _cardOrder.Count)
            {
                _countdown = float.PositiveInfinity; // the last card waits for the player
                _views.Ride.ShowCountdown(string.Empty);
            }
            else
            {
                CountDown(0f);
            }

            return true;
        }

        private void CountDown(float deltaSeconds)
        {
            if (float.IsPositiveInfinity(_countdown))
            {
                return;
            }

            _countdown -= deltaSeconds;
            if (_countdown <= 0f)
            {
                OnContinue();
                return;
            }

            int seconds = Mathf.CeilToInt(_countdown);
            if (seconds != _shownSeconds)
            {
                _shownSeconds = seconds;
                _views.Ride.ShowCountdown(Ui.Format(UiKey.RideMovesOn, ("seconds", seconds.ToString(CultureInfo.InvariantCulture))));
            }
        }

        private void FinishRide()
        {
            if (Session.Phase == NightPhase.Travelling)
            {
                _night.Result = Session.Finish();
            }

            Finished?.Invoke();
        }

        private void ShowProgress(ClockTime now, int? stopIndex)
        {
            int at = stopIndex ?? _journey?.CurrentLeg.FromStopIndex ?? 0;
            string station = _night.StationName(Session.Stops[at].StationId);
            string progress;
            if (at == Session.Stops.Count - 1 && stopIndex != null)
            {
                progress = Ui.Get(UiKey.RideLastStop);
            }
            else
            {
                int arrivals = Session.Outcomes.Count(outcome => outcome.IsReached && outcome.StopIndex <= at);
                progress = arrivals == 0
                    ? string.Empty
                    : Ui.Format(UiKey.RideArrival, ("index", arrivals.ToString(CultureInfo.InvariantCulture)),
                        ("count", ReachedCount.ToString(CultureInfo.InvariantCulture)));
            }

            string[] found = stopIndex == null ? Array.Empty<string>()
                : _night.Level.Secrets.Where(secret => secret.IsFoundAt(Session.Stops[stopIndex.Value]))
                    .Select(secret => _night.Week.FindKeepsake(secret.KeepsakeId)?.Name ?? secret.KeepsakeId)
                    .ToArray();
            foreach (string keepsake in found)
            {
                string line = Ui.Format(UiKey.RideFound, ("keepsake", keepsake));
                progress = string.IsNullOrEmpty(progress) ? line : line + Ui.Get(UiKey.ListDot) + progress;
            }

            string[] boarding = stopIndex == null ? Array.Empty<string>()
                : BoardingAt(stopIndex.Value).Select(i => Passengers[i].Name).ToArray();
            if (boarding.Length > 0)
            {
                string boards = Ui.Format(UiKey.RideBoards, ("names", Ui.JoinNames(boarding)));
                progress = string.IsNullOrEmpty(progress) ? boards : boards + Ui.Get(UiKey.ListDot) + progress;
            }

            _views.Ride.ShowProgress(Ui.Format(UiKey.RideNow, ("time", now.ToString())), station, progress);
        }

        /// <summary>On time, arriving at the very minute they needed.</summary>
        private bool IsJustInTime(int passengerIndex) =>
            Session.Outcomes[passengerIndex].Status == ArrivalStatus.OnTime
            && Session.Outcomes[passengerIndex].Arrival == Passengers[passengerIndex].Deadline;

        private ArrivalModel BuildCard(int passengerIndex, bool isLast)
        {
            Passenger passenger = Passengers[passengerIndex];
            PassengerOutcome outcome = Session.Outcomes[passengerIndex];
            OutcomeStory story = passenger.Story.GetOutcome(outcome.Status);
            bool muted = outcome.Status != ArrivalStatus.OnTime;
            bool promised = Session.PromisedPassenger == passenger.Name;

            var tags = new List<TagModel>();
            switch (outcome.Status)
            {
                case ArrivalStatus.OnTime:
                    tags.Add(new TagModel { Text = Ui.Format(UiKey.TagOnTime, ("time", outcome.Arrival.ToString())), Kind = TagKind.OnTime });
                    if (IsJustInTime(passengerIndex))
                    {
                        tags.Add(new TagModel { Text = Ui.Get(UiKey.TagJustInTime), Kind = TagKind.Promise });
                    }

                    break;
                case ArrivalStatus.Late:
                    tags.Add(new TagModel { Text = Ui.Format(UiKey.TagLate, ("time", outcome.Arrival.ToString())), Kind = TagKind.Late });
                    break;
                default:
                    tags.Add(new TagModel
                    {
                        Text = Ui.Get(outcome.IsAboard ? UiKey.TagNotReached : UiKey.TagNeverBoarded),
                        Kind = TagKind.NotReached,
                    });
                    break;
            }

            if (_night.IsMisinformed(passenger))
            {
                // They never told the player; the ride is where the real deadline comes out.
                tags.Add(new TagModel
                {
                    Text = Ui.Format(UiKey.TagNeededNotSaid, ("deadline", passenger.Deadline.ToString()),
                        ("said", _night.KnownDeadline(passenger).ToString())),
                    Kind = TagKind.Neutral,
                });
            }
            else if (outcome.Status != ArrivalStatus.OnTime || _night.NightIndex == 0)
            {
                tags.Add(new TagModel { Text = Ui.Format(UiKey.TagNeeded, ("deadline", passenger.Deadline.ToString())), Kind = TagKind.Neutral });
            }

            if (promised)
            {
                tags.Add(muted
                    ? new TagModel { Text = Ui.Get(UiKey.TagPromiseBroken), Kind = TagKind.PromiseBroken }
                    : new TagModel { Text = Ui.Get(UiKey.TagPromiseKept), Kind = TagKind.Promise });
            }

            if (_night.IsBigNightTonight(passenger.Name))
            {
                tags.Add(new TagModel { Text = Ui.Get(UiKey.TagBigNight), Kind = TagKind.Neutral });
            }

            Keepsake keepsake = story.KeepsakeId != null ? _night.Week.FindKeepsake(story.KeepsakeId) : null;
            return new ArrivalModel
            {
                Name = passenger.Name,
                Title = Ui.Format(UiKey.RideTitle, ("name", passenger.Name),
                    ("station", _night.StationName(passenger.DestinationStationId))),
                Tags = tags,
                Line = story.Line,
                Keepsake = keepsake != null ? keepsake.Line : muted ? Ui.Get(UiKey.RideNoKeepsake) : string.Empty,
                HasKeepsake = keepsake != null,
                Aside = story.Aside,
                StillAboardLabel = Ui.Get(UiKey.RideStillAboard),
                StillAboard = _aboard.ToArray(),
                IsMuted = muted,
                IsPromised = promised,
                ContinueLabel = Ui.Get(isLast ? UiKey.RideToLogbook : UiKey.RideContinue),
            };
        }
    }
}
