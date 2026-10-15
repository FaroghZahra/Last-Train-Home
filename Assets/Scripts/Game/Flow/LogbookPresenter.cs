using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// The driver's log at the end of a night: what happened to each passenger, the promise, the keepsakes in the cab,
    /// the big nights still ahead and what carries into tomorrow. No score and no replay; the next night finishes it.
    /// </summary>
    public sealed class LogbookPresenter : IFlowStep
    {
        private const int CabHooks = 4;

        private readonly NightContext _night;
        private readonly ILogbookView _view;
        private readonly IReadOnlyCollection<string> _keepsakesBeforeTonight;

        /// <param name="keepsakesBeforeTonight">The cab before tonight's keepsakes were added, to mark the new ones.</param>
        public LogbookPresenter(NightContext night, ILogbookView view, IReadOnlyCollection<string> keepsakesBeforeTonight)
        {
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _keepsakesBeforeTonight = keepsakesBeforeTonight ?? Array.Empty<string>();
        }

        public event Action Finished;

        private UiText Ui => _night.Ui;

        public void Start()
        {
            NightResult result = _night.Result ?? throw new InvalidOperationException("The logbook needs a finished night.");
            _view.NextClicked += OnNext;
            (string noteHeader, string note) = Note(result);
            (string summary, bool isGood) = Summary(result);
            _view.Show(new LogbookModel
            {
                Day = _night.Level.Day,
                Kicker = Ui.Format(UiKey.LogKicker, ("night", (_night.NightIndex + 1).ToString(CultureInfo.InvariantCulture))),
                Entries = Entries(result),
                Summary = summary,
                SummaryIsGood = isGood,
                CabHeader = Ui.Get(UiKey.LogCab),
                Cab = Cab(),
                WeekHeader = Ui.Get(UiKey.LogWeek),
                WeekAhead = WeekAhead(),
                NoteHeader = noteHeader,
                Note = note,
                Footer = Best(result),
                NextLabel = _night.IsLastNight
                    ? Ui.Get(UiKey.LogFinish)
                    : Ui.Format(UiKey.LogNext, ("day", _night.DayName(_night.NightIndex + 1))),
            });
        }

        public void Tick(float deltaSeconds)
        {
        }

        public void Dispose()
        {
            _view.NextClicked -= OnNext;
            _view.Hide();
        }

        private void OnNext() => Finished?.Invoke();

        /// <summary>One entry per passenger, in the order the train reached them; anyone never reached comes last.</summary>
        private IReadOnlyList<LogEntryModel> Entries(NightResult result)
        {
            IReadOnlyList<Passenger> passengers = _night.Night.Passengers;
            return Enumerable.Range(0, passengers.Count)
                .OrderBy(i => result.Outcomes[i].IsReached ? result.Outcomes[i].StopIndex : int.MaxValue)
                .ThenBy(i => i)
                .Select(i => new LogEntryModel
                {
                    Name = passengers[i].Name,
                    Tag = Tag(passengers[i], result.Outcomes[i], result.PromisedPassenger),
                    Line = passengers[i].Story.GetOutcome(result.Outcomes[i].Status).LogLine,
                })
                .ToArray();
        }

        private TagModel Tag(Passenger passenger, PassengerOutcome outcome, string promised)
        {
            (string text, TagKind kind) = outcome.Status switch
            {
                ArrivalStatus.OnTime => (Ui.Format(UiKey.TagOnTime, ("time", outcome.Arrival.ToString())), TagKind.OnTime),
                ArrivalStatus.Late => (Ui.Format(UiKey.TagLate, ("time", outcome.Arrival.ToString())), TagKind.Late),
                _ => (Ui.Get(UiKey.TagNotReached), TagKind.NotReached),
            };

            if (passenger.Name == promised)
            {
                string key = outcome.Status == ArrivalStatus.OnTime ? UiKey.LogTagPromiseKept : UiKey.LogTagPromiseBroken;
                text = Ui.Format(key, ("tag", text));
            }

            return new TagModel { Text = text, Kind = kind };
        }

        private (string Text, bool IsGood) Summary(NightResult result)
        {
            string[] waited = _night.Night.Passengers
                .Where((passenger, i) => result.Outcomes[i].Status != ArrivalStatus.OnTime)
                .Select(passenger => passenger.Name)
                .ToArray();
            string promised = result.PromisedPassenger;
            string waitedNames = Ui.JoinNames(waited);

            if (waited.Length == 0)
            {
                return promised != null
                    ? (Ui.Format(UiKey.LogEveryoneHomeKept, ("promised", promised)), true)
                    : (Ui.Get(UiKey.LogEveryoneHome), true);
            }

            if (promised == null)
            {
                return (Ui.Format(UiKey.LogWaited, ("waited", waitedNames)), false);
            }

            return result.PromiseKept
                ? (Ui.Format(UiKey.LogKeptWaited, ("promised", promised), ("waited", waitedNames)), false)
                : (Ui.Format(UiKey.LogBroken, ("promised", promised), ("waited", waitedNames)), false);
        }

        private IReadOnlyList<KeepsakeSlot> Cab()
        {
            var slots = _night.Campaign.Keepsakes
                .Select(id => new KeepsakeSlot
                {
                    KeepsakeId = id,
                    Label = _night.Week.FindKeepsake(id)?.Name ?? id,
                    IsNew = !_keepsakesBeforeTonight.Contains(id),
                })
                .ToList();
            while (slots.Count < CabHooks)
            {
                slots.Add(new KeepsakeSlot { KeepsakeId = null, Label = Ui.Get(UiKey.LogEmptyHook), IsNew = false });
            }

            return slots;
        }

        /// <summary>Every big night after tonight: whose and what, once the player knows; a question mark until then.</summary>
        private IReadOnlyList<WeekAheadTile> WeekAhead() => _night.Week.Cast
            .Where(member => member.BigNight > _night.NightIndex && member.BigNight < _night.NightCount)
            .OrderBy(member => member.BigNight)
            .Select(member =>
            {
                bool known = _night.Campaign.IsBigNightRevealed(member.Name);
                bool isNext = member.BigNight == _night.NightIndex + 1;
                string day = _night.ShortDay(member.BigNight);
                string name = known ? member.Name : Ui.Get(UiKey.WeekUnknown);
                string note = _night.Campaign.GetWeekNote(member.Name);
                return new WeekAheadTile
                {
                    Passenger = known ? member.Name : null,
                    Header = Ui.Format(isNext ? UiKey.LogWeekNext : UiKey.LogWeekTile, ("day", day), ("name", name)),
                    Text = !known ? Ui.Get(UiKey.WeekUnknown)
                        : string.IsNullOrEmpty(note) ? member.BigNightLabel
                        : Ui.Format(UiKey.LogWeekNote, ("label", member.BigNightLabel), ("note", note)),
                    IsNext = isNext,
                };
            })
            .ToArray();

        /// <summary>How many got home against the most any route could have, e.g. "3 of 5 home. The best anyone could do was 3."</summary>
        private string Best(NightResult result)
        {
            int best = RouteExplorer.BestOnTime(_night.Night);
            (string Token, string Value)[] values =
            {
                ("onTime", result.OnTimeCount.ToString(CultureInfo.InvariantCulture)),
                ("total", result.Outcomes.Count.ToString(CultureInfo.InvariantCulture)),
                ("best", best.ToString(CultureInfo.InvariantCulture)),
            };
            return Ui.Format(result.OnTimeCount >= best ? UiKey.LogBestReached : UiKey.LogBestMissed, values);
        }

        /// <summary>What tonight changed about tomorrow; the level's own line when nothing did, or on the last night.</summary>
        private (string Header, string Text) Note(NightResult result)
        {
            string[] carried = _night.IsLastNight
                ? Array.Empty<string>()
                : _night.Night.Passengers
                    .Select((passenger, i) => passenger.Story.GetOutcome(result.Outcomes[i].Status).Carry)
                    .Where(carry => !string.IsNullOrWhiteSpace(carry))
                    .ToArray();

            return carried.Length > 0
                ? (Ui.Format(UiKey.LogCarried, ("day", _night.DayName(_night.NightIndex + 1))), string.Join(" ", carried))
                : (Ui.Get(UiKey.LogTomorrow), _night.Level.Story.LogbookTomorrow);
        }
    }
}
