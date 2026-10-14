using System;
using LastTrainHome.Domain.Network;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class RailNetworkTests
    {
        [Test]
        public void TryGetTravelMinutes_WorksInBothDirections()
        {
            RailNetwork network = TestMaps.Night2Network();

            Assert.That(network.TryGetTravelMinutes(TestMaps.OldTown, TestMaps.Hillside, out int there), Is.True);
            Assert.That(network.TryGetTravelMinutes(TestMaps.Hillside, TestMaps.OldTown, out int back), Is.True);
            Assert.That(there, Is.EqualTo(20));
            Assert.That(back, Is.EqualTo(20));
        }

        [Test]
        public void TryGetTravelMinutes_IsFalseWithoutADirectTrack()
        {
            RailNetwork network = TestMaps.Night2Network();

            Assert.That(network.TryGetTravelMinutes(TestMaps.Central, TestMaps.Harbour, out _), Is.False);
            Assert.That(network.TryGetTravelMinutes(TestMaps.Central, "Nowhere", out _), Is.False);
            Assert.That(network.TryGetTravelMinutes(null, TestMaps.Central, out _), Is.False);
        }

        [Test]
        public void Constructor_RejectsDuplicateStations()
        {
            Assert.Throws<ArgumentException>(() =>
                new RailNetwork(new[] { "A", "A" }, Array.Empty<Track>()));
        }

        [Test]
        public void Constructor_RejectsTracksToUnknownStations()
        {
            Assert.Throws<ArgumentException>(() =>
                new RailNetwork(new[] { "A" }, new[] { new Track("A", "B", 5) }));
        }

        [Test]
        public void Constructor_RejectsTwoTracksBetweenTheSamePair()
        {
            Assert.Throws<ArgumentException>(() =>
                new RailNetwork(new[] { "A", "B" }, new[] { new Track("A", "B", 5), new Track("B", "A", 7) }));
        }

        [Test]
        public void Track_RejectsLoopsAndNonPositiveTimes()
        {
            Assert.Throws<ArgumentException>(() => new Track("A", "A", 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Track("A", "B", 0));
        }
    }
}
