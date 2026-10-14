using System.Collections.Generic;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace LastTrainHome.Game.Tests
{
    public sealed class NightPresenterTests
    {
        // Harbour and Market hang off Central, 10 minutes each. Agnes needs Harbour by 00:10, Dev needs Market by 00:30.
        private const string Level = @"{
            ""startStation"": ""Central"", ""departure"": ""00:00"", ""closing"": ""01:00"",
            ""stations"": [ { ""id"": ""Central"" }, { ""id"": ""Harbour"", ""y"": 3 }, { ""id"": ""Market"", ""x"": 3 } ],
            ""tracks"": [ { ""from"": ""Central"", ""to"": ""Harbour"", ""minutes"": 10 },
                          { ""from"": ""Central"", ""to"": ""Market"", ""minutes"": 10 } ],
            ""passengers"": [
                { ""name"": ""Agnes"", ""destination"": ""Harbour"", ""deadline"": ""00:10"", ""onTime"": ""Made the ferry."" },
                { ""name"": ""Dev"", ""destination"": ""Market"", ""deadline"": ""00:30"" } ]
        }";

        private readonly FakeRailMap _map = new FakeRailMap();
        private readonly FakeRoute _route = new FakeRoute();
        private readonly FakeClock _clock = new FakeClock();
        private readonly FakeHint _hint = new FakeHint();
        private readonly FakePassengerList _passengers = new FakePassengerList();
        private readonly FakeArrivalLog _log = new FakeArrivalLog();
        private readonly FakeSummary _summary = new FakeSummary();
        private readonly FakeControls _controls = new FakeControls();
        private readonly FakeInput _input = new FakeInput();

        private readonly List<NightResult> _results = new List<NightResult>();
        private GameSettings _settings;
        private OutcomeStyle _style;
        private NightPresenter _presenter;
        private int _nextNightRequests;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<GameSettings>();
            _style = ScriptableObject.CreateInstance<OutcomeStyle>();
            _presenter = CreatePresenter(hasNextNight: true);
            _presenter.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_style);
        }

        private NightPresenter CreatePresenter(bool hasNextNight)
        {
            var views = new NightViews(_map, _route, _clock, _hint, _passengers, _log, _summary, _controls);
            var presenter = new NightPresenter(LevelParser.Parse(Level, "Test"), hasNextNight, views,
                new NightText(_style), _settings, _input);
            presenter.NightFinished += _results.Add;
            presenter.NextNightRequested += () => _nextNightRequests++;
            return presenter;
        }

        private void PlayNight(params string[] stations)
        {
            foreach (string station in stations)
            {
                _map.Click(station);
            }

            _controls.ClickDepart();
            _presenter.Tick(1000f);
        }

        [Test]
        public void Start_DrawsTheMapAndShowsAnEmptyPlan()
        {
            Assert.That(_map.Stations, Has.Count.EqualTo(3));
            Assert.That(_map.TrackCount, Is.EqualTo(2));
            Assert.That(_route.Points, Has.Count.EqualTo(1));
            Assert.That(_passengers.Rows.Count, Is.EqualTo(2));
            Assert.That(_passengers.Rows[0], Does.Contain("Not reached"));
            Assert.That(_clock.Text, Does.Contain("Route ends 00:00"));
            Assert.That(_controls.State, Is.EqualTo("planning-empty"));
            Assert.That(_summary.IsShown, Is.False);
        }

        [Test]
        public void ClickingAConnectedStation_ExtendsTheRouteAndPreviewsOutcomes()
        {
            _map.Click("Harbour");

            Assert.That(_route.Points, Has.Count.EqualTo(2));
            Assert.That(_route.Train, Is.EqualTo(new Vector3(0f, 3f, 0f)));
            Assert.That(_passengers.Rows[0], Does.Contain("On time · 00:10"));
            Assert.That(_controls.State, Is.EqualTo("planning"));
        }

        [Test]
        public void ClickingAStationWithoutATrack_IsRefusedWithAHint()
        {
            _map.Click("Harbour");
            _map.Click("Market");

            Assert.That(_route.Points, Has.Count.EqualTo(2));
            Assert.That(_map.Flashed, Is.EqualTo(new[] { "Market" }));
            Assert.That(_hint.Text, Does.Contain("No track from Harbour to Market"));
        }

        [Test]
        public void UndoShortcut_And_ClearButton_EditThePlan()
        {
            _map.Click("Harbour");
            _map.Click("Central");

            _input.PressUndo();
            Assert.That(_route.Points, Has.Count.EqualTo(2));

            _controls.ClickClear();
            Assert.That(_route.Points, Has.Count.EqualTo(1));
        }

        [Test]
        public void Departing_PlaysTheJourneyThenShowsTheSummary()
        {
            PlayNight("Harbour", "Central", "Market");

            Assert.That(_log.Entries, Has.Count.EqualTo(3), "Two arrivals and the end of the line.");
            Assert.That(_log.Entries[0], Does.Contain("Agnes").And.Contain("Made the ferry."));
            Assert.That(_log.Entries[2], Does.Contain("End of the line at Market, 00:30"));
            Assert.That(_clock.Text, Does.Contain("Now 00:30"));
            Assert.That(_controls.State, Is.EqualTo("finished"));
            Assert.That(_summary.IsShown, Is.True);
            Assert.That(_summary.Body, Does.StartWith("2 of 2 arrived on time."));
            Assert.That(_summary.CanContinue, Is.True);
            Assert.That(_results, Has.Count.EqualTo(1));
            Assert.That(_results[0].OnTimeCount, Is.EqualTo(2));
        }

        [Test]
        public void PassengersNeverReached_AreLoggedAtTheEnd()
        {
            PlayNight("Market");

            Assert.That(_log.Entries, Has.Some.Contains("Agnes</b> never reached Harbour"));
        }

        [Test]
        public void PlanningInput_IsIgnoredWhileTravelling()
        {
            _map.Click("Harbour");
            _controls.ClickDepart();

            _map.Click("Central");
            _input.PressUndo();

            Assert.That(_route.Points, Has.Count.EqualTo(2));
            Assert.That(_controls.State, Is.EqualTo("travelling"));
        }

        [Test]
        public void FastForward_TogglesWhileTravelling()
        {
            _map.Click("Harbour");
            _controls.ClickDepart();

            _controls.ClickFastForward();
            Assert.That(_controls.State, Is.EqualTo("travelling-fast"));

            _controls.ClickFastForward();
            Assert.That(_controls.State, Is.EqualTo("travelling"));
        }

        [Test]
        public void PlanAgain_ReturnsToAnEmptyPlan()
        {
            PlayNight("Harbour");

            _summary.ClickPlanAgain();

            Assert.That(_summary.IsShown, Is.False);
            Assert.That(_log.Entries, Is.Empty);
            Assert.That(_route.Points, Has.Count.EqualTo(1));
            Assert.That(_controls.State, Is.EqualTo("planning-empty"));
        }

        [Test]
        public void NextNight_IsOnlyRequestedOnceTheNightIsFinished()
        {
            _summary.ClickNextNight();
            Assert.That(_nextNightRequests, Is.Zero);

            PlayNight("Harbour");
            _summary.ClickNextNight();
            Assert.That(_nextNightRequests, Is.EqualTo(1));
        }

        [Test]
        public void LastNight_DoesNotOfferToContinue()
        {
            _presenter.Dispose();
            _presenter = CreatePresenter(hasNextNight: false);
            _presenter.Start();

            PlayNight("Harbour");
            _summary.ClickNextNight();

            Assert.That(_summary.CanContinue, Is.False);
            Assert.That(_nextNightRequests, Is.Zero);
        }

        [Test]
        public void Dispose_StopsListeningToTheViews()
        {
            _presenter.Dispose();

            _map.Click("Harbour");

            Assert.That(_route.Points, Has.Count.EqualTo(1));
        }
    }
}
