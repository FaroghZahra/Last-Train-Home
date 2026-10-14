using System;
using LastTrainHome.Data;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class NightDefinitionTests
    {
        private const string ValidLevel = @"{
            ""title"": ""Test"", ""startStation"": ""Central"", ""departure"": ""00:00"", ""closing"": ""01:00"",
            ""stations"": [ { ""id"": ""Central"", ""x"": 1, ""y"": 2 }, { ""id"": ""Harbour"", ""displayName"": ""The Harbour"", ""label"": ""left"" } ],
            ""tracks"": [ { ""from"": ""Central"", ""to"": ""Harbour"", ""minutes"": 10 } ],
            ""passengers"": [ { ""name"": ""Agnes"", ""destination"": ""Harbour"", ""deadline"": ""00:40"", ""late"": ""Missed it."" } ]
        }";

        [Test]
        public void FromJson_ReadsMapTimesAndPassengers()
        {
            NightDefinition night = NightDefinition.FromJson(ValidLevel, "test");

            Assert.That(night.Title, Is.EqualTo("Test"));
            Assert.That(night.Closing, Is.EqualTo(new ClockTime(60)));
            Assert.That(night.Stations[0].Position.x, Is.EqualTo(1f));
            Assert.That(night.Stations[0].DisplayName, Is.EqualTo("Central"));
            Assert.That(night.Stations[1].DisplayName, Is.EqualTo("The Harbour"));
            Assert.That(night.Stations[0].LabelSide, Is.EqualTo(LabelSide.Above));
            Assert.That(night.Stations[1].LabelSide, Is.EqualTo(LabelSide.Left));
            Assert.That(night.Network.TryGetTravelMinutes("Harbour", "Central", out int minutes), Is.True);
            Assert.That(minutes, Is.EqualTo(10));
            Assert.That(night.Passengers[0].Deadline, Is.EqualTo(new ClockTime(40)));
            Assert.That(night.Passengers[0].GetOutcomeLine(ArrivalStatus.Late), Is.EqualTo("Missed it."));
        }

        [TestCase("\"destination\": \"Harbour\"", "\"destination\": \"Nowhere\"", "destination 'Nowhere'")]
        [TestCase("\"deadline\": \"00:40\"", "\"deadline\": \"40\"", "deadline '40'")]
        [TestCase("\"startStation\": \"Central\"", "\"startStation\": \"Depot\"", "Start station 'Depot'")]
        [TestCase("\"to\": \"Harbour\"", "\"to\": \"Depot\"", "unknown station 'Depot'")]
        [TestCase("\"closing\": \"01:00\"", "\"closing\": \"later\"", "Closing 'later'")]
        [TestCase("\"label\": \"left\"", "\"label\": \"under\"", "label 'under'")]
        public void FromJson_ListsProblemsWithTheLevel(string valid, string broken, string expectedProblem)
        {
            string json = ValidLevel.Replace(valid, broken);

            var exception = Assert.Throws<FormatException>(() => NightDefinition.FromJson(json, "test"));
            Assert.That(exception.Message, Does.Contain(expectedProblem));
        }

        [Test]
        public void FromJson_RejectsMalformedJson()
        {
            Assert.Throws<FormatException>(() => NightDefinition.FromJson("{ not json", "test"));
        }
    }
}
