using System;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Routing;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    /// <summary>Closed stations and tracks are on the map but out of service.</summary>
    public sealed class ClosedLineTests
    {
        private static RailNetwork Network() => new RailNetwork(
            new[] { "Central", "Market", "Harbour" },
            new[]
            {
                new Track("Central", "Harbour", 10),
                new Track("Central", "Market", 15, isClosed: true),
                new Track("Market", "Harbour", 10),
            },
            closedStationIds: new[] { "Market" });

        [Test]
        public void ClosedTracks_AreDrawnButNotTravelled()
        {
            RailNetwork network = Network();

            Assert.That(network.Tracks, Has.Count.EqualTo(3));
            Assert.That(network.TryGetTravelMinutes("Central", "Market", out _), Is.False);
        }

        [Test]
        public void TracksIntoAClosedStation_AreNotTravelledEither()
        {
            Assert.That(Network().TryGetTravelMinutes("Harbour", "Market", out _), Is.False);
            Assert.That(Network().TryGetTravelMinutes("Harbour", "Central", out _), Is.True);
        }

        [Test]
        public void ARouteCannotStopAtAClosedStation()
        {
            var route = new Route(Network(), "Central", TestMaps.Midnight, TestMaps.Closing);
            route.TryAddStop("Harbour");

            AddStopResult result = route.TryAddStop("Market");

            Assert.That(result.Status, Is.EqualTo(AddStopStatus.StationClosed));
            Assert.That(route.LastStop.StationId, Is.EqualTo("Harbour"));
        }

        [Test]
        public void AClosedTrack_StillCountsAsTheOneTrackBetweenItsStations()
        {
            Assert.Throws<ArgumentException>(() => new RailNetwork(new[] { "A", "B" },
                new[] { new Track("A", "B", 10, isClosed: true), new Track("B", "A", 10) }));
        }

        [Test]
        public void ClosedStations_MustBeOnTheMap()
        {
            Assert.Throws<ArgumentException>(() => new RailNetwork(new[] { "A" }, Array.Empty<Track>(), new[] { "Z" }));
        }
    }
}
