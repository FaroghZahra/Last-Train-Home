using System;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Game.Content;
using NUnit.Framework;

namespace LastTrainHome.Game.Tests
{
    public sealed class LevelParserTests
    {
        private const string ValidLevel = @"{
            ""title"": ""Test"", ""startStation"": ""Central"", ""departure"": ""00:00"", ""closing"": ""01:00"",
            ""stations"": [ { ""id"": ""Central"", ""x"": 1, ""y"": 2 }, { ""id"": ""Harbour"", ""displayName"": ""The Harbour"", ""label"": ""left"" } ],
            ""tracks"": [ { ""from"": ""Central"", ""to"": ""Harbour"", ""minutes"": 10 } ],
            ""passengers"": [ { ""name"": ""Agnes"", ""destination"": ""Harbour"", ""deadline"": ""00:40"", ""late"": ""Missed it."" } ]
        }";

        [Test]
        public void Parse_ReadsRulesStoryAndLayout()
        {
            Level level = LevelParser.Parse(ValidLevel, "Test");

            Assert.That(level.Night.Id, Is.EqualTo("Test"));
            Assert.That(level.Night.Closing, Is.EqualTo(new ClockTime(60)));
            Assert.That(level.Night.GetStation("Harbour").DisplayName, Is.EqualTo("The Harbour"));
            Assert.That(level.Night.Network.TryGetTravelMinutes("Harbour", "Central", out int minutes), Is.True);
            Assert.That(minutes, Is.EqualTo(10));

            Passenger agnes = level.Night.Passengers[0];
            Assert.That(agnes.Deadline, Is.EqualTo(new ClockTime(40)));
            Assert.That(agnes.Story.GetOutcomeLine(ArrivalStatus.Late), Is.EqualTo("Missed it."));
            Assert.That(agnes.Story.GetOutcomeLine(ArrivalStatus.OnTime), Is.Empty);

            Assert.That(level.GetPlacement("Central").Position.y, Is.EqualTo(2f));
            Assert.That(level.GetPlacement("Central").LabelSide, Is.EqualTo(LabelSide.Above));
            Assert.That(level.GetPlacement("Harbour").LabelSide, Is.EqualTo(LabelSide.Left));
        }

        [TestCase("\"destination\": \"Harbour\"", "\"destination\": \"Nowhere\"", "destination 'Nowhere'")]
        [TestCase("\"deadline\": \"00:40\"", "\"deadline\": \"40\"", "deadline '40'")]
        [TestCase("\"startStation\": \"Central\"", "\"startStation\": \"Depot\"", "Start station 'Depot'")]
        [TestCase("\"to\": \"Harbour\"", "\"to\": \"Depot\"", "unknown station 'Depot'")]
        [TestCase("\"closing\": \"01:00\"", "\"closing\": \"later\"", "Closing 'later'")]
        [TestCase("\"label\": \"left\"", "\"label\": \"under\"", "label 'under'")]
        [TestCase("\"minutes\": 10", "\"minutes\": 0", "Travel time must be positive")]
        [TestCase("\"name\": \"Agnes\"", "\"name\": \"\"", "Passenger name is required")]
        public void Parse_ListsProblemsWithTheLevel(string valid, string broken, string expectedProblem)
        {
            string json = ValidLevel.Replace(valid, broken);

            var exception = Assert.Throws<FormatException>(() => LevelParser.Parse(json, "Test"));
            Assert.That(exception.Message, Does.Contain(expectedProblem));
        }

        [Test]
        public void Parse_RejectsMalformedJson()
        {
            Assert.Throws<FormatException>(() => LevelParser.Parse("{ not json", "Test"));
        }
    }
}
