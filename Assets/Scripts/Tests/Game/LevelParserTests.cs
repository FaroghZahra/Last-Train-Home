using System;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Game.Content;
using NUnit.Framework;
using UnityEngine;

namespace LastTrainHome.Game.Tests
{
    public sealed class LevelParserTests
    {
        private const string ValidLevel = TestContent.Level;

        [Test]
        public void Parse_ReadsRulesAndLayout()
        {
            Level level = LevelParser.Parse(ValidLevel, "Test");

            Assert.That(level.Night.Id, Is.EqualTo("Test"));
            Assert.That(level.Day, Is.EqualTo("Monday"));
            Assert.That(level.Night.Closing, Is.EqualTo(new ClockTime(60)));
            Assert.That(level.Night.GetStation("Harbour").DisplayName, Is.EqualTo("The Harbour"));
            Assert.That(level.Night.GetStation("Depot").IsClosed, Is.True);
            Assert.That(level.Night.Network.TryGetTravelMinutes("Harbour", "Central", out int minutes), Is.True);
            Assert.That(minutes, Is.EqualTo(10));
            Assert.That(level.Night.Network.TryGetTravelMinutes("Market", "Depot", out _), Is.False);
            Assert.That(level.GetPlacement("Central").Position.y, Is.EqualTo(2f));
            Assert.That(level.GetPlacement("Harbour").LabelSide, Is.EqualTo(LabelSide.Left));
        }

        [Test]
        public void Parse_ReadsEveryPassengerLine()
        {
            Passenger agnes = LevelParser.Parse(ValidLevel, "Test").Night.Passengers[0];
            Passenger dev = LevelParser.Parse(ValidLevel, "Test").Night.Passengers[1];

            Assert.That(agnes.Story.PlatformLine, Is.EqualTo("The ferry."));
            Assert.That(agnes.Story.PlanQuote, Is.EqualTo("The ferry."), "No quote: the card uses the platform line.");
            Assert.That(dev.Story.PlanQuote, Is.EqualTo("Exam at nine."));
            Assert.That(agnes.Story.AskLine, Is.EqualTo("My grandson."));
            Assert.That(agnes.Story.RevealsBigNight, Is.True);

            OutcomeStory onTime = agnes.Story.GetOutcome(ArrivalStatus.OnTime);
            Assert.That(onTime.Line, Is.EqualTo("Made the ferry."));
            Assert.That(onTime.LogLine, Is.EqualTo("Made the ferry."), "No log line: the logbook uses the line.");
            Assert.That(onTime.KeepsakeId, Is.EqualTo("ticket"));
            Assert.That(onTime.WeekNote, Is.EqualTo("Waved him off."));
            Assert.That(agnes.Story.GetOutcome(ArrivalStatus.Late).LogLine, Is.EqualTo("Missed."));
            Assert.That(agnes.Story.GetOutcome(ArrivalStatus.Late).Carry, Is.EqualTo("She posts it tomorrow."));
        }

        [Test]
        public void Parse_ReadsTheNightsStory()
        {
            NightStory story = LevelParser.Parse(ValidLevel, "Test").Story;

            Assert.That(story.AsksAllowed, Is.EqualTo(1));
            Assert.That(story.NightCard.Featured, Is.EqualTo("Agnes"));
            Assert.That(story.NightCard.Lines, Is.EqualTo(new[] { "Someone waits." }));
            Assert.That(story.Platform.Headline, Is.EqualTo("Meet them."));
            Assert.That(story.LogbookTomorrow, Is.EqualTo("Tomorrow is tight."));
            Assert.That(story.Coach.Select(rule => rule.Condition),
                Is.EqualTo(new[] { CoachCondition.Empty, CoachCondition.AnyLate, CoachCondition.DeadEnd, CoachCondition.Always }));
            Assert.That(story.Coach[2].Station, Is.EqualTo("Harbour"));
        }

        [Test]
        public void Parse_ReadsVariants()
        {
            PassengerVariant variant = LevelParser.Parse(ValidLevel, "Test").Variants.Single();

            Assert.That(variant.IfNightId, Is.EqualTo("Night0"));
            Assert.That(variant.IfPassenger, Is.EqualTo("Dev"));
            Assert.That(variant.IfStatus, Is.EqualTo(StatusMatch.Missed));
            Assert.That(variant.Deadline, Is.EqualTo(new ClockTime(20)));
            Assert.That(variant.OnTime.Line, Is.EqualTo("Early, for once."));
            Assert.That(variant.Late, Is.Null, "An outcome the variant doesn't mention stays as it was.");
        }

        [TestCase("\"destination\": \"Harbour\"", "\"destination\": \"Nowhere\"", "destination 'Nowhere'")]
        [TestCase("\"deadline\": \"00:10\"", "\"deadline\": \"10\"", "deadline '10'")]
        [TestCase("\"startStation\": \"Central\"", "\"startStation\": \"Yard\"", "Start station 'Yard'")]
        [TestCase("\"to\": \"Harbour\"", "\"to\": \"Yard\"", "unknown station 'Yard'")]
        [TestCase("\"closing\": \"01:00\"", "\"closing\": \"later\"", "Closing 'later'")]
        [TestCase("\"label\": \"left\"", "\"label\": \"under\"", "label 'under'")]
        [TestCase("\"minutes\": 10 }", "\"minutes\": 0 }", "Travel time must be positive")]
        [TestCase("\"name\": \"Agnes\"", "\"name\": \"\"", "Passenger name is required")]
        [TestCase("\"ask\": \"My grandson.\"", "\"ask\": \"\"", "Agnes has no answer")]
        [TestCase("\"line\": \"Taxi.\"", "\"line\": \"\"", "Agnes's notReached has no line")]
        [TestCase("\"asks\": 1", "\"asks\": 0", "rules.asks")]
        [TestCase("\"when\": \"empty\"", "\"when\": \"sometimes\"", "'when' should be one of")]
        [TestCase("{late} would", "{lat} would", "uses {lat}")]
        [TestCase("Night {night} of", "Night {nite} of", "uses {nite}")]
        [TestCase("\"station\": \"Harbour\"", "\"station\": \"Pier\"", "'deadEnd' needs a station")]
        [TestCase("\"passenger\": \"Dev\", \"deadline\"", "\"passenger\": \"Zed\", \"deadline\"", "changes 'Zed'")]
        [TestCase("\"status\": \"missed\"", "\"status\": \"sad\"", "status should be")]
        [TestCase("\"tomorrow\": \"Tomorrow is tight.\"", "\"tomorrow\": \"\"", "no line for tomorrow")]
        public void Parse_ListsProblemsWithTheLevel(string valid, string broken, string expectedProblem)
        {
            Assert.That(ValidLevel, Does.Contain(valid), "The test's starting point changed.");
            string json = ValidLevel.Replace(valid, broken);

            var exception = Assert.Throws<FormatException>(() => LevelParser.Parse(json, "Test"));
            Assert.That(exception.Message, Does.Contain(expectedProblem));
        }

        [Test]
        public void Parse_ReadsTrackBendsInEitherDirection()
        {
            string json = ValidLevel.Replace("\"to\": \"Harbour\", \"minutes\": 10 }",
                "\"to\": \"Harbour\", \"minutes\": 10, \"via\": [ { \"x\": 1, \"y\": 1 }, { \"x\": 2, \"y\": 0 } ] }");

            Level level = LevelParser.Parse(json, "Test");

            Assert.That(level.GetTrackBends("Central", "Harbour"), Is.EqualTo(new[] { new Vector2(1f, 1f), new Vector2(2f, 0f) }));
            Assert.That(level.GetTrackBends("Harbour", "Central"), Is.EqualTo(new[] { new Vector2(2f, 0f), new Vector2(1f, 1f) }));
            Assert.That(LevelParser.Parse(ValidLevel, "Test").GetTrackBends("Central", "Harbour"), Is.Empty);
        }

        [Test]
        public void Parse_RejectsMalformedJson()
        {
            Assert.Throws<FormatException>(() => LevelParser.Parse("{ not json", "Test"));
        }
    }
}
