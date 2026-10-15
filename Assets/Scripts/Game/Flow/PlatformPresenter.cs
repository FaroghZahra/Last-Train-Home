using System;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// The platform before the map: meet tonight's passengers, ask some of them how they are, and promise one of them.
    /// Asking someone whose answer names their big night adds it to the week. The map waits for a promise.
    /// </summary>
    public sealed class PlatformPresenter : IFlowStep
    {
        private readonly NightContext _night;
        private readonly IPlatformView _view;

        public PlatformPresenter(NightContext night, IPlatformView view)
        {
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public event Action Finished;

        private PlatformSession Platform => _night.Platform;

        private NightSession Session => _night.Session;

        private UiText Ui => _night.Ui;

        public void Start()
        {
            _view.AskClicked += OnAsk;
            _view.PromiseClicked += OnPromise;
            _view.ContinueClicked += OnContinue;
            Show();
        }

        public void Tick(float deltaSeconds)
        {
        }

        public void Dispose()
        {
            _view.AskClicked -= OnAsk;
            _view.PromiseClicked -= OnPromise;
            _view.ContinueClicked -= OnContinue;
            _view.Hide();
        }

        private void OnAsk(string passengerName)
        {
            if (!Platform.TryAsk(passengerName))
            {
                return;
            }

            if (Find(passengerName).Story.RevealsBigNight)
            {
                _night.Campaign.RevealBigNight(passengerName);
            }

            Show();
        }

        private void OnPromise(string passengerName)
        {
            Session.Promise(Session.PromisedPassenger == passengerName ? null : passengerName);
            Show();
        }

        private void OnContinue()
        {
            if (Session.PromisedPassenger != null)
            {
                Finished?.Invoke();
            }
        }

        private void Show()
        {
            PlatformText text = _night.Level.Story.Platform;
            string promised = Session.PromisedPassenger;
            _view.Show(new PlatformModel
            {
                Kicker = _night.FillHeader(text.Kicker),
                Headline = _night.FillHeader(text.Headline),
                Coach = _night.FillHeader(text.Coach),
                Cards = _night.Night.Passengers.Select(ToCard).ToArray(),
                Footer = promised != null
                    ? Ui.Format(UiKey.PlatformFooterPromised, ("name", promised))
                    : Ui.Get(UiKey.PlatformFooterNone),
                ContinueLabel = Ui.Get(UiKey.PlatformContinue),
                CanContinue = promised != null,
            });
        }

        private PlatformCardModel ToCard(Passenger passenger)
        {
            bool asked = Platform.HasAsked(passenger.Name);
            bool canAsk = Platform.CanAsk(passenger.Name);
            bool promised = Session.PromisedPassenger == passenger.Name;
            (string Token, string Value)[] where =
            {
                ("destination", _night.Trip(passenger)),
                ("deadline", _night.KnownDeadline(passenger).ToString()),
            };

            return new PlatformCardModel
            {
                Name = passenger.Name,
                Subtitle = Ui.Format(_night.IsBigNightTonight(passenger.Name) ? UiKey.PlatformSubBigNight : UiKey.PlatformSub, where),
                PlatformLine = passenger.Story.PlatformLine,
                IsAsked = asked,
                AskedHeader = asked ? AskedHeader(passenger) : string.Empty,
                AskedLine = asked ? passenger.Story.AskLine : string.Empty,
                AskLabel = Ui.Get(asked ? UiKey.PlatformAsked : canAsk ? UiKey.PlatformAsk : UiKey.PlatformAskUsed),
                CanAsk = canAsk,
                PromiseLabel = Ui.Get(promised ? UiKey.PlatformPromised : UiKey.PlatformPromise),
                IsPromised = promised,
            };
        }

        private string AskedHeader(Passenger passenger)
        {
            CastMember member = _night.Week.FindCastMember(passenger.Name);
            bool revealsAhead = passenger.Story.RevealsBigNight && member != null && member.BigNight > _night.NightIndex;
            return revealsAhead
                ? Ui.Format(UiKey.PlatformYouAskedReveal, ("day", _night.DayName(member.BigNight)))
                : Ui.Get(UiKey.PlatformYouAsked);
        }

        private Passenger Find(string passengerName) =>
            _night.Night.Passengers.First(passenger => passenger.Name == passengerName);
    }
}
