using System;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Flow
{
    /// <summary>The card that opens a night: which night, whose it is, and the week so far.</summary>
    public sealed class NightCardPresenter : IFlowStep
    {
        private readonly NightContext _night;
        private readonly INightCardView _view;

        public NightCardPresenter(NightContext night, INightCardView view)
        {
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public event Action Finished;

        public void Start()
        {
            _view.ContinueClicked += OnContinue;
            NightCardText card = _night.Level.Story.NightCard;
            _view.Show(new NightCardModel
            {
                Kicker = _night.FillHeader(card.Kicker),
                Day = _night.Level.Day,
                Featured = card.Featured,
                FeaturedLine = card.FeaturedLine,
                Lines = card.Lines,
                Emphasis = card.Emphasis,
                Week = WeekStrip.Days(_night),
                Footnote = card.Footnote,
                ContinueLabel = _night.Ui.Get(UiKey.NightCardContinue),
            });
        }

        public void Tick(float deltaSeconds)
        {
        }

        public void Dispose()
        {
            _view.ContinueClicked -= OnContinue;
            _view.Hide();
        }

        private void OnContinue() => Finished?.Invoke();
    }
}
