using System.Linq;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Flow;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace LastTrainHome.Game.Tests
{
    /// <summary>Plays the game's own Monday through every screen on fake views, as a player would.</summary>
    public sealed class GameFlowTests
    {
        private FakeRailMap _map;
        private FakeRoute _route;
        private FakeTitleScreen _title;
        private FakeNightCard _nightCard;
        private FakePlatform _platform;
        private FakePlan _plan;
        private FakeRide _ride;
        private FakeLogbook _logbook;
        private FakeInput _input;

        private GameSettings _settings;
        private GameFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _map = new FakeRailMap();
            _route = new FakeRoute();
            _title = new FakeTitleScreen();
            _nightCard = new FakeNightCard();
            _platform = new FakePlatform();
            _plan = new FakePlan();
            _ride = new FakeRide();
            _logbook = new FakeLogbook();
            _input = new FakeInput();
            _settings = ScriptableObject.CreateInstance<GameSettings>();
            var views = new GameViews(_map, _route, _title, _nightCard, _platform, _plan, _ride, _logbook);
            _flow = new GameFlow(TestContent.Game(), views, _settings, _input);
            _flow.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _flow.Dispose();
            Object.DestroyImmediate(_settings);
        }

        private void ToThePlatform()
        {
            _title.Begin();
            _nightCard.Continue();
        }

        private void ToTheMap(string promise)
        {
            ToThePlatform();
            _platform.Promise(promise);
            _platform.Continue();
        }

        private void Click(params string[] stations)
        {
            foreach (string station in stations)
            {
                _map.Click(station);
            }
        }

        /// <summary>Ticks the ride until its last card waits for the player, then continues to the logbook.</summary>
        private void RideToTheLogbook()
        {
            for (int i = 0; i < 1000 && !_logbook.IsVisible; i++)
            {
                _flow.Tick(0.1f);
                if (_ride.IsShowingArrival && _ride.Arrivals.Last().ContinueLabel == "To the logbook")
                {
                    _ride.Continue();
                }
            }

            Assert.That(_logbook.IsVisible, Is.True, "The ride never reached the logbook.");
        }

        [Test]
        public void Title_OpensMondaysNightCard()
        {
            Assert.That(_title.IsVisible, Is.True);
            Assert.That(_title.Shown.BeginLabel, Is.EqualTo("Begin Monday"));

            _title.Begin();

            Assert.That(_title.IsVisible, Is.False);
            Assert.That(_nightCard.Shown.Day, Is.EqualTo("Monday"));
            Assert.That(_nightCard.Shown.Week.Select(day => day.Text), Is.EqualTo(new[] { "tonight", "?", "?", "?", "last train" }));
        }

        [Test]
        public void Platform_AskingRevealsBigNights_AndTheMapWaitsForAPromise()
        {
            ToThePlatform();

            _platform.Ask("Priya");
            PlatformCardModel priya = _platform.Shown.Cards.Single(card => card.Name == "Priya");
            Assert.That(priya.IsAsked, Is.True);
            Assert.That(priya.AskedHeader, Is.EqualTo("You asked · Thursday added to the week"));
            Assert.That(_flow.CurrentNight.Campaign.IsBigNightRevealed("Priya"), Is.True);

            _platform.Continue();
            Assert.That(_platform.IsVisible, Is.True, "No promise yet.");
            Assert.That(_platform.Shown.CanContinue, Is.False);

            _platform.Promise("Wen");
            _platform.Continue();
            Assert.That(_plan.IsVisible, Is.True);
            Assert.That(_plan.Shown.PromiseLine, Is.EqualTo("Wen → Riverside by 00:40"));
        }

        [Test]
        public void Plan_NamesWhoWaits_AndExplainsRefusals()
        {
            ToTheMap("Wen");
            Assert.That(_plan.Shown.Coach, Does.StartWith("The train is at Central."));
            Assert.That(_map.Highlighted, Is.EquivalentTo(new[] { "Old Town", "Riverside", "Harbour" }));

            Click("Harbour");
            Assert.That(_plan.Shown.DepartDetail, Is.EqualTo("Priya, Tomás and Wen won't get there"));

            Click("Market");
            Assert.That(_plan.Refusals.Last(), Is.EqualTo("Market is closed for works this week."));

            _plan.Clear();
            Click("Riverside", "Hillside", "Old Town");
            Assert.That(_plan.Shown.DepartDetail, Is.EqualTo("Priya will be late · Agnes won't get there"));
            Assert.That(_plan.Shown.Coach, Is.EqualTo("Priya would be late. Undo and try the earliest deadline first."));
            Assert.That(_plan.Shown.PromiseKept, Is.True);
        }

        [Test]
        public void Monday_PlayedThrough_FillsTheLogbook_AndOpensTuesday()
        {
            ToThePlatform();
            _platform.Ask("Tomás");
            _platform.Promise("Wen");
            _platform.Continue();
            Click("Old Town", "Hillside", "Riverside", "Central", "Harbour");
            Assert.That(_plan.Shown.DepartDetail, Is.EqualTo("Everyone gets home on time"));

            _plan.Depart();
            Assert.That(_ride.IsVisible, Is.True);
            RideToTheLogbook();

            Assert.That(_ride.Arrivals.Select(card => card.Name), Is.EqualTo(new[] { "Priya", "Tomás", "Wen", "Agnes" }));
            Assert.That(_ride.Arrivals[0].Keepsake, Is.EqualTo("Keepsake: her setlist, taped to your cab window."));
            Assert.That(_ride.Arrivals[2].Tags.Select(tag => tag.Text), Does.Contain("Promise kept"));

            LogbookModel log = _logbook.Shown;
            Assert.That(log.Summary, Is.EqualTo("Everyone home. Promise to Wen kept."));
            Assert.That(log.Cab[0].Label, Is.EqualTo("Priya's setlist"));
            Assert.That(log.Cab[0].IsNew, Is.True);
            Assert.That(log.WeekAhead[0].Header, Is.EqualTo("Tue · Tomás · next"));
            Assert.That(log.WeekAhead[1].Text, Is.EqualTo("?"), "Nobody asked Agnes.");
            Assert.That(log.NextLabel, Is.EqualTo("Next night: Tuesday"));

            _logbook.Next();
            Assert.That(_nightCard.Shown.Day, Is.EqualTo("Tuesday"));
            Assert.That(_nightCard.Shown.Featured, Is.EqualTo(new[] { "Tomás" }));
            Assert.That(_nightCard.Shown.Week[0].Text, Is.EqualTo("4 of 4 home"));
        }

        [Test]
        public void ALateArrival_IsMuted_AndCarriesIntoTomorrow()
        {
            ToTheMap("Priya");
            Click("Riverside", "Hillside", "Old Town"); // Priya late at 00:30; Agnes never reached
            _plan.Depart();
            RideToTheLogbook();

            ArrivalModel priya = _ride.Arrivals.Single(card => card.Name == "Priya");
            Assert.That(priya.IsMuted, Is.True);
            Assert.That(priya.Keepsake, Is.EqualTo("No keepsake tonight."));
            Assert.That(priya.Tags.Select(tag => tag.Text), Does.Contain("Promise broken"));
            Assert.That(_logbook.Shown.Summary, Is.EqualTo("You promised Priya. Priya and Agnes waited."));
        }

        /// <summary>Plays Monday with everyone home and opens Tuesday's platform.</summary>
        private void ToTuesdaysPlatform()
        {
            ToTheMap("Wen");
            Click("Old Town", "Hillside", "Riverside", "Central", "Harbour");
            _plan.Depart();
            RideToTheLogbook();
            _logbook.Next();
            _nightCard.Continue();
        }

        [Test]
        public void UnaskedPassenger_IsPlannedWithWhatTheySaid_AndTheRideTellsTheTruth()
        {
            ToTuesdaysPlatform();
            Assert.That(_platform.Shown.Cards.Single(card => card.Name == "Agnes").Subtitle, Is.EqualTo("Harbour · by 00:55"));

            _platform.Promise("Tomás");
            _platform.Continue();
            Click("Old Town", "Riverside", "Central", "Old Town", "Central", "Harbour"); // Harbour at 00:55
            PlanCardModel agnes = _plan.Shown.Cards.Single(card => card.Name == "Agnes");
            Assert.That(agnes.Detail, Is.EqualTo("Harbour · by 00:55"));
            Assert.That(agnes.Outcome, Is.EqualTo(OutcomeKind.OnTime), "As far as the player knows.");

            _plan.Depart();
            RideToTheLogbook();

            ArrivalModel arrival = _ride.Arrivals.Last(card => card.Name == "Agnes"); // Monday's cards are there too
            Assert.That(arrival.IsMuted, Is.True);
            Assert.That(arrival.Tags.Select(tag => tag.Text), Does.Contain("really needed 00:45, not 00:55"));
        }

        [Test]
        public void AskingTellsTheRealDeadline()
        {
            ToTuesdaysPlatform();

            _platform.Ask("Agnes");

            Assert.That(_platform.Shown.Cards.Single(card => card.Name == "Agnes").Subtitle, Is.EqualTo("Harbour · by 00:45"));
            _platform.Promise("Agnes");
            _platform.Continue();
            Assert.That(_plan.Shown.Cards.Single(card => card.Name == "Agnes").Detail, Is.EqualTo("Harbour · by 00:45"));
        }

        [Test]
        public void TheWholeWeek_EndsWithWhoseBigNightsWereKept_AndRemembersTheCollection()
        {
            ToTuesdaysPlatform();
            Assert.That(_logbook.Shown.Footer, Does.StartWith("4 of 4 home: the best anyone could do tonight."));

            // Tuesday through Friday: promise the first passenger and take the first open station, then Old Town on
            // Tuesday for the lantern, which is only there for a train by 00:05.
            for (int night = 1; night < 5; night++)
            {
                if (night > 1)
                {
                    _nightCard.Continue();
                }

                _platform.Promise(_platform.Shown.Cards[0].Name);
                _platform.Continue();
                Click(night == 1 ? "Old Town" : _map.Highlighted[0]);
                _plan.Depart();
                RideToTheLogbook();
                if (night == 1)
                {
                    Assert.That(_logbook.Shown.Cab.Select(slot => slot.KeepsakeId), Does.Contain("lantern"));
                }

                _logbook.Next();
            }

            NightCardModel ending = _nightCard.Shown;
            Assert.That(ending.ContinueLabel, Is.EqualTo("Ride the week again"));
            Assert.That(ending.Lines.Count, Is.EqualTo(4), "One line for each regular.");
            Assert.That(ending.Week.Select(day => day.Text), Has.Some.EndsWith("· missed"));
            Assert.That(ending.Footnote, Does.Contain("endings seen"));

            _nightCard.Continue();
            Assert.That(_title.IsVisible, Is.True);
            Assert.That(_title.Shown.Footnote, Does.Contain("keepsakes found"));
        }
    }
}
