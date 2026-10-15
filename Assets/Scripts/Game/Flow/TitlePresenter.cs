using System;
using System.Linq;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Flow
{
    /// <summary>The title screen, worded by the week file. Finishes when the player begins the first night.</summary>
    public sealed class TitlePresenter : IFlowStep
    {
        private readonly GameContent _content;
        private readonly ITitleScreenView _view;
        private readonly IProgressStore _progress;

        public TitlePresenter(GameContent content, ITitleScreenView view, IProgressStore progress = null)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _progress = progress ?? new MemoryProgressStore();
        }

        public event Action Finished;

        public void Start()
        {
            _view.BeginClicked += OnBegin;
            Week week = _content.Week;
            _view.Show(new TitleScreenModel
            {
                Title = week.Title,
                Intro = week.Intro,
                Emphasis = week.IntroEmphasis,
                // After a first week, the footnote is the player's collection so far.
                Footnote = string.IsNullOrEmpty(Collection.Describe(_content, _progress))
                    ? week.Footnote
                    : Collection.Describe(_content, _progress),
                BeginLabel = _content.Ui.Format(UiKey.TitleBegin, ("day", _content.Levels[0].Day)),
                Cast = week.Cast.Where(member => member.HasBigNight).Select(member => member.Name).ToArray(),
            });
        }

        public void Tick(float deltaSeconds)
        {
        }

        public void Dispose()
        {
            _view.BeginClicked -= OnBegin;
            _view.Hide();
        }

        private void OnBegin() => Finished?.Invoke();
    }
}
