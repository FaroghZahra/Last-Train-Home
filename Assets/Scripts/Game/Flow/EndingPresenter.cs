using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Progress;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// After the last train: whose big nights the player kept, a line about where each regular ended up, and the
    /// ending for that many kept. Shown on the night card. Finishes when the player chooses to ride the week again.
    /// </summary>
    public sealed class EndingPresenter : IFlowStep
    {
        private readonly GameContent _content;
        private readonly Campaign _campaign;
        private readonly IProgressStore _progress;
        private readonly INightCardView _view;

        public EndingPresenter(GameContent content, Campaign campaign, IProgressStore progress, INightCardView view)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _campaign = campaign ?? throw new ArgumentNullException(nameof(campaign));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public event Action Finished;

        private UiText Ui => _content.Ui;

        /// <summary>How many big nights the player kept this week.</summary>
        public int Kept { get; private set; }

        public void Start()
        {
            CastMember[] regulars = _content.Week.Cast
                .Where(member => member.HasBigNight && member.BigNight < _content.Levels.Count)
                .OrderBy(member => member.BigNight)
                .ToArray();
            Kept = regulars.Count(WasKept);
            _progress.AddEnding(Kept);
            Ending ending = _content.Week.EndingFor(Kept);

            _view.ContinueClicked += OnContinue;
            _view.Show(new NightCardModel
            {
                Kicker = Ui.Get(UiKey.EndingKicker),
                Day = ending?.Title ?? string.Empty,
                Lines = regulars.Select(member => WasKept(member) ? member.EpilogueKept : member.EpilogueMissed).ToArray(),
                Emphasis = ending?.Line ?? string.Empty,
                Week = Days(),
                Footnote = Collection.Describe(_content, _progress),
                ContinueLabel = Ui.Get(UiKey.EndingAgain),
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

        private bool WasKept(CastMember member) =>
            _campaign.TryGetResult(_content.Levels[member.BigNight].Night.Id, out NightResult result)
            && result.TryGetStatus(member.Name, out ArrivalStatus status)
            && status == ArrivalStatus.OnTime;

        /// <summary>The week again: each big night kept or missed; a night with none, how many got home.</summary>
        private IReadOnlyList<WeekDayTile> Days()
        {
            var tiles = new List<WeekDayTile>();
            for (int i = 0; i < _content.Levels.Count; i++)
            {
                CastMember member = _content.Week.BigNightOf(i);
                string day = _content.Week.Days[i];
                if (member != null)
                {
                    bool kept = WasKept(member);
                    tiles.Add(new WeekDayTile
                    {
                        Day = day,
                        Text = Ui.Format(kept ? UiKey.WeekKept : UiKey.WeekMissed, ("name", member.Name)),
                        Kind = kept ? WeekDayKind.Tonight : WeekDayKind.Unknown,
                    });
                }
                else
                {
                    string home = _campaign.TryGetResult(_content.Levels[i].Night.Id, out NightResult result)
                        ? Ui.Format(UiKey.WeekHome, ("onTime", result.OnTimeCount.ToString(CultureInfo.InvariantCulture)),
                            ("total", result.Outcomes.Count.ToString(CultureInfo.InvariantCulture)))
                        : string.Empty;
                    tiles.Add(new WeekDayTile { Day = day, Text = home, Kind = WeekDayKind.Past });
                }
            }

            return tiles;
        }
    }
}
