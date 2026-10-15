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
    /// The composition root: the one place that knows every piece. Loads the content, builds the plain C# game
    /// objects, hands them the scene's views, drives them each frame and tears them down. Nothing else holds
    /// cross-object references.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private LevelCatalog _levels;
        [SerializeField] private GameSettings _settings;

        [Header("Map")]
        [SerializeField] private RailMapView _railMap;
        [SerializeField] private RouteView _route;

        [Header("Screens")]
        [SerializeField] private TitleScreenView _title;
        [SerializeField] private NightCardView _nightCard;
        [SerializeField] private PlatformView _platform;
        [SerializeField] private PlanView _plan;
        [SerializeField] private RideView _ride;
        [SerializeField] private LogbookView _logbook;

        [SerializeField, Tooltip("Optional; the game is silent without it.")]
        private AudioView _audio;

        private GameInput _input;
        private GameFlow _flow;

        private void Awake()
        {
            string[] missing = MissingReferences().ToArray();
            if (missing.Length > 0)
            {
                Fail($"{nameof(GameBootstrap)} on '{name}' is missing: {string.Join(", ", missing)}.");
                return;
            }

            // Every screen starts hidden; the flow shows them one at a time.
            _nightCard.Hide();
            _platform.Hide();
            _plan.Hide();
            _ride.Hide();
            _logbook.Hide();
            _title.Hide();
        }

        private void Start()
        {
            GameContent content;
            try
            {
                content = _levels.LoadContent();
            }
            catch (FormatException exception)
            {
                Fail(exception.Message, _levels);
                return;
            }

            var views = new GameViews(_railMap, _route, _title, _nightCard, _platform, _plan, _ride, _logbook);
            _input = new GameInput();
            _flow = new GameFlow(content, views, _settings, _input, new Persistence.PlayerPrefsProgressStore(),
                _audio != null ? _audio : null);
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
                (_levels, nameof(_levels)), (_settings, nameof(_settings)), (_railMap, nameof(_railMap)),
                (_route, nameof(_route)), (_title, nameof(_title)), (_nightCard, nameof(_nightCard)),
                (_platform, nameof(_platform)), (_plan, nameof(_plan)), (_ride, nameof(_ride)),
                (_logbook, nameof(_logbook)),
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
