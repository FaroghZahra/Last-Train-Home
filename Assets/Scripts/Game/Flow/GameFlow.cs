using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Progress;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Input;

namespace LastTrainHome.Game.Flow
{
    /// <summary>Plays the levels in order: one <see cref="NightPresenter"/> per night, recording each result.</summary>
    public sealed class GameFlow : IDisposable
    {
        private readonly IReadOnlyList<Level> _levels;
        private readonly Campaign _campaign;
        private readonly NightViews _views;
        private readonly NightText _text;
        private readonly GameSettings _settings;
        private readonly IGameInput _input;

        private NightPresenter _night;

        public GameFlow(IReadOnlyList<Level> levels, NightViews views, NightText text, GameSettings settings, IGameInput input)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _campaign = new Campaign(levels.Select(level => level.Night.Id).ToArray());
        }

        public void Start() => StartCurrentNight();

        public void Tick(float deltaSeconds) => _night?.Tick(deltaSeconds);

        public void Dispose() => EndCurrentNight();

        private void StartCurrentNight()
        {
            EndCurrentNight();
            _night = new NightPresenter(_levels[_campaign.CurrentIndex], _campaign.HasNextNight, _views, _text, _settings, _input);
            _night.NightFinished += OnNightFinished;
            _night.NextNightRequested += OnNextNightRequested;
            _night.Start();
        }

        private void EndCurrentNight()
        {
            if (_night == null)
            {
                return;
            }

            _night.NightFinished -= OnNightFinished;
            _night.NextNightRequested -= OnNextNightRequested;
            _night.Dispose();
            _night = null;
        }

        private void OnNightFinished(NightResult result) => _campaign.Record(result);

        private void OnNextNightRequested()
        {
            _campaign.AdvanceToNextNight();
            StartCurrentNight();
        }
    }
}
