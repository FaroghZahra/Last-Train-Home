using System;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Progress;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Input;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// Runs the game screen by screen: the title, then each night as night card, platform, plan, ride and logbook.
    /// Each night is played as earlier nights left it (carry-over), and how it went is kept in the <see cref="Campaign"/>.
    /// After the last logbook comes the ending, then the title. Keepsakes and endings are also kept across weeks.
    /// </summary>
    public sealed class GameFlow : IDisposable
    {
        private readonly GameContent _content;
        private readonly GameViews _views;
        private readonly GameSettings _settings;
        private readonly IGameInput _input;
        private readonly IProgressStore _progress;
        private readonly IGameAudio _audio;

        private Campaign _campaign;
        private NightContext _night;
        private RouteDrawing _drawing;
        private IFlowStep _step;

        /// <param name="progress">Where keepsakes and endings are kept across weeks; in memory when left out.</param>
        /// <param name="audio">The game's sounds; silent when left out.</param>
        public GameFlow(GameContent content, GameViews views, GameSettings settings, IGameInput input,
            IProgressStore progress = null, IGameAudio audio = null)
        {
            _progress = progress ?? new MemoryProgressStore();
            _audio = audio ?? new SilentAudio();
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        /// <summary>The screen showing now, for tests.</summary>
        public IFlowStep CurrentStep => _step;

        /// <summary>Tonight, or null on the title screen; for tests.</summary>
        public NightContext CurrentNight => _night;

        public void Start() => ShowTitle();

        public void Tick(float deltaSeconds) => _step?.Tick(deltaSeconds);

        public void Dispose() => EndStep();

        private void ShowTitle()
        {
            _campaign = new Campaign(_content.Levels.Select(level => level.Night.Id).ToArray());
            _night = null;
            Run(new TitlePresenter(_content, _views.Title, _progress), () => StartNight(0));
        }

        private void StartNight(int nightIndex)
        {
            Level level = _content.Levels[nightIndex].WithCarryOver(EarlierStatus, WasPromised);
            _night = new NightContext(_content, _campaign, nightIndex, level);
            CastMember bigNight = _content.Week.BigNightOf(nightIndex);
            if (bigNight != null && level.Night.Passengers.Any(passenger => passenger.Name == bigNight.Name))
            {
                _campaign.RevealBigNight(bigNight.Name);
            }

            _drawing = new RouteDrawing(_night, _views.RailMap, _views.Route);
            _drawing.BuildMap();
            Run(new NightCardPresenter(_night, _views.NightCard), ShowPlatform);
        }

        private void ShowPlatform() => Run(new PlatformPresenter(_night, _views.Platform), ShowPlan);

        private void ShowPlan() => Run(new PlanPresenter(_night, _views, _drawing, _input, _audio), ShowRide);

        private void ShowRide() => Run(new RidePresenter(_night, _views, _drawing, _settings, _audio), ShowLogbook);

        private void ShowLogbook()
        {
            string[] cabBefore = _campaign.Keepsakes.ToArray();
            Record(_night.Result);
            Run(new LogbookPresenter(_night, _views.Logbook, cabBefore), OnLogbookClosed);
        }

        private void OnLogbookClosed()
        {
            if (!_campaign.HasNextNight)
            {
                _night = null;
                Run(new EndingPresenter(_content, _campaign, _progress, _views.NightCard), ShowTitle);
                return;
            }

            _campaign.AdvanceToNextNight();
            StartNight(_campaign.CurrentIndex);
        }

        /// <summary>Keeps the result, the keepsakes left tonight, and what tonight says about the big nights ahead.</summary>
        private void Record(NightResult result)
        {
            _campaign.Record(result);
            for (int i = 0; i < _night.Night.Passengers.Count; i++)
            {
                Passenger passenger = _night.Night.Passengers[i];
                OutcomeStory story = passenger.Story.GetOutcome(result.Outcomes[i].Status);
                if (story.KeepsakeId != null)
                {
                    LeaveKeepsake(story.KeepsakeId);
                }

                if (!string.IsNullOrWhiteSpace(story.WeekNote))
                {
                    _campaign.SetWeekNote(passenger.Name, story.WeekNote);
                }
            }

            foreach (Secret secret in _night.Level.Secrets.Where(secret => result.Route.Any(secret.IsFoundAt)))
            {
                LeaveKeepsake(secret.KeepsakeId);
            }
        }

        private void LeaveKeepsake(string keepsakeId)
        {
            _campaign.AddKeepsake(keepsakeId);
            _progress.AddKeepsake(keepsakeId);
        }

        private ArrivalStatus? EarlierStatus(string nightId, string passengerName) =>
            _campaign.TryGetResult(nightId, out NightResult result) && result.TryGetStatus(passengerName, out ArrivalStatus status)
                ? status
                : (ArrivalStatus?)null;

        private bool WasPromised(string nightId, string passengerName) =>
            _campaign.TryGetResult(nightId, out NightResult result) && result.PromisedPassenger == passengerName;

        private void Run(IFlowStep step, Action next)
        {
            EndStep();
            _step = step;
            _step.Finished += () =>
            {
                if (_step == step)
                {
                    next();
                }
            };
            _step.Start();
        }

        private void EndStep()
        {
            if (_step == null)
            {
                return;
            }

            IFlowStep ending = _step;
            _step = null;
            ending.Dispose();
        }
    }
}
