using System;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class NightDefinitionTests
    {
        private static readonly StationDefinition[] Stations =
        {
            new StationDefinition(TestMaps.Central),
            new StationDefinition(TestMaps.Harbour, "The Harbour"),
        };

        private static readonly Track[] Tracks = { new Track(TestMaps.Central, TestMaps.Harbour, 10) };

        private static NightDefinition Create(
            string start = TestMaps.Central, int departure = 0, int closing = 60, params Passenger[] passengers) =>
            new NightDefinition("Test", null, start, new ClockTime(departure), new ClockTime(closing), Stations, Tracks,
                passengers.Length > 0 ? passengers : new[] { new Passenger("Agnes", TestMaps.Harbour, new ClockTime(40)) });

        [Test]
        public void ValidNight_ExposesItsMapAndStations()
        {
            NightDefinition night = Create();

            Assert.That(night.Title, Is.EqualTo("Test"), "The id stands in for a missing title.");
            Assert.That(night.Network.TryGetTravelMinutes(TestMaps.Harbour, TestMaps.Central, out int minutes), Is.True);
            Assert.That(minutes, Is.EqualTo(10));
            Assert.That(night.GetStation(TestMaps.Harbour).DisplayName, Is.EqualTo("The Harbour"));
            Assert.That(night.GetStation(TestMaps.Central).DisplayName, Is.EqualTo(TestMaps.Central));
        }

        [Test]
        public void BrokenNight_ListsEveryProblemAtOnce()
        {
            var exception = Assert.Throws<ArgumentException>(() => Create("Depot", 30, 20,
                new Passenger("Agnes", "Nowhere", TestMaps.Midnight),
                new Passenger("Agnes", TestMaps.Harbour, TestMaps.Midnight)));

            Assert.That(exception.Message, Does.Contain("Start station 'Depot'"));
            Assert.That(exception.Message, Does.Contain("closes before"));
            Assert.That(exception.Message, Does.Contain("destination 'Nowhere'"));
            Assert.That(exception.Message, Does.Contain("Two passengers are called 'Agnes'"));
        }

        [Test]
        public void TrackToUnknownStation_IsReported()
        {
            var exception = Assert.Throws<ArgumentException>(() => new NightDefinition("Test", null, TestMaps.Central,
                TestMaps.Midnight, TestMaps.Closing, Stations, new[] { new Track(TestMaps.Central, "Depot", 5) },
                new[] { new Passenger("Agnes", TestMaps.Harbour, TestMaps.Midnight) }));

            Assert.That(exception.Message, Does.Contain("unknown station 'Depot'"));
        }
    }
}
