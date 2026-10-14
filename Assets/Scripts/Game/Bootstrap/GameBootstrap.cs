using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Input;
using LastTrainHome.Game.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastTrainHome.Game.Bootstrap
{
    /// <summary>
    /// The composition root: the one place that knows every piece. Builds the plain C# game objects, hands them the
    /// scene's views, drives them each frame and tears them down. Nothing else holds cross-object references.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private LevelCatalog _levels;
        [SerializeField] private GameSettings _settings;
        [SerializeField] private OutcomeStyle _outcomeStyle;

        [Header("Views")]
        [SerializeField] private RailMapView _railMap;
        [SerializeField] private RouteView _route;
        [SerializeField] private ClockView _clock;
        [SerializeField] private HintView _hint;
        [SerializeField] private PassengerListView _passengers;
        [SerializeField] private ArrivalLogView _arrivalLog;
        [SerializeField] private SummaryView _summary;
        [SerializeField] private ControlsView _controls;

        private GameInput _input;
        private GameFlow _flow;

        private void Awake()
        {
            string[] missing = MissingReferences().ToArray();
            if (missing.Length > 0)
            {
                Fail($"{nameof(GameBootstrap)} on '{name}' is missing: {string.Join(", ", missing)}.");
            }
        }

        private void Start()
        {
            IReadOnlyList<Level> levels;
            try
            {
                levels = _levels.LoadLevels();
            }
            catch (FormatException exception)
            {
                Fail(exception.Message, _levels);
                return;
            }

            var views = new NightViews(_railMap, _route, _clock, _hint, _passengers, _arrivalLog, _summary, _controls);
            _input = new GameInput();
            _flow = new GameFlow(levels, views, new NightText(_outcomeStyle), _settings, _input);
            _flow.Start();
        }

        private void Update() => _flow?.Tick(Time.deltaTime);

        private void OnDestroy()
        {
            _flow?.Dispose();
            _input?.Dispose();
        }

        private IEnumerable<string> MissingReferences()
        {
            var references = new (Object value, string field)[]
            {
                (_levels, nameof(_levels)), (_settings, nameof(_settings)), (_outcomeStyle, nameof(_outcomeStyle)),
                (_railMap, nameof(_railMap)), (_route, nameof(_route)), (_clock, nameof(_clock)), (_hint, nameof(_hint)),
                (_passengers, nameof(_passengers)), (_arrivalLog, nameof(_arrivalLog)), (_summary, nameof(_summary)),
                (_controls, nameof(_controls)),
            };
            return references.Where(reference => reference.value == null).Select(reference => reference.field);
        }

        private void Fail(string message, Object context = null)
        {
            Debug.LogError(message, context != null ? context : this);
            enabled = false;
        }
    }
}
