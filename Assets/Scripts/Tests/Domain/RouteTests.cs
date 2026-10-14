using System;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Routing;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class RouteTests
    {
        [Test]
        public void NewRoute_StartsAtStartStationAtDeparture()
        {
            Route route = TestMaps.Night2Route();

            Assert.That(route.Stops, Has.Count.EqualTo(1));
            Assert.That(route.LastStop.StationId, Is.EqualTo(TestMaps.Central));
            Assert.That(route.LastStop.Arrival, Is.EqualTo(TestMaps.Midnight));
            Assert.That(route.HasLeftStart, Is.False);
        }

        [Test]
        public void Constructor_RejectsUnknownStartAndClosingBeforeDeparture()
        {
            RailNetwork network = TestMaps.Night2Network();

            Assert.Throws<ArgumentException>(() => new Route(network, "Nowhere", TestMaps.Midnight, TestMaps.Closing));
            Assert.Throws<ArgumentException>(() => new Route(network, TestMaps.Central, new ClockTime(30), new ClockTime(20)));
        }

        [Test]
        public void TryAddStop_AddsConnectedStationWithCumulativeArrival()
        {
            Route route = TestMaps.Night2Route();

            route.TryAddStop(TestMaps.OldTown);
            AddStopResult result = route.TryAddStop(TestMaps.Hillside);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.AttemptedArrival, Is.EqualTo(new ClockTime(30)));
            Assert.That(route.LastStop.StationId, Is.EqualTo(TestMaps.Hillside));
        }

        [Test]
        public void TryAddStop_AllowsRevisitingStations()
        {
            Route route = TestMaps.Night2Route();

            route.TryAddStop(TestMaps.OldTown);
            AddStopResult back = route.TryAddStop(TestMaps.Central);

            Assert.That(back.Succeeded, Is.True);
            Assert.That(route.Stops, Has.Count.EqualTo(3));
        }

        [TestCase(TestMaps.Harbour, AddStopStatus.NoTrack)]
        [TestCase(TestMaps.Central, AddStopStatus.AlreadyAtStation)]
        [TestCase("Nowhere", AddStopStatus.UnknownStation)]
        [TestCase(null, AddStopStatus.UnknownStation)]
        public void TryAddStop_RejectsInvalidStopsWithoutChangingRoute(string stationId, AddStopStatus expected)
        {
            Route route = TestMaps.Night2Route();
            int changes = 0;
            route.Changed += () => changes++;

            AddStopResult result = route.TryAddStop(stationId);

            Assert.That(result.Status, Is.EqualTo(expected));
            Assert.That(route.Stops, Has.Count.EqualTo(1));
            Assert.That(changes, Is.Zero);
        }

        [Test]
        public void TryAddStop_RejectsArrivalAfterClosing_AndReportsWhen()
        {
            Route route = TestMaps.Night2Route();
            foreach (string stop in new[] { TestMaps.Market, TestMaps.Harbour, TestMaps.Market, TestMaps.Hillside })
            {
                route.TryAddStop(stop);
            }

            AddStopResult result = route.TryAddStop(TestMaps.OldTown); // 00:40 + 20 = 01:00, allowed
            AddStopResult tooLate = route.TryAddStop(TestMaps.Hillside); // 01:00 + 20 = 01:20

            Assert.That(result.Succeeded, Is.True);
            Assert.That(tooLate.Status, Is.EqualTo(AddStopStatus.PastClosing));
            Assert.That(tooLate.AttemptedArrival, Is.EqualTo(new ClockTime(80)));
        }

        [Test]
        public void TryRemoveLastStop_RemovesStopsButNeverTheStart()
        {
            Route route = TestMaps.Night2Route();
            route.TryAddStop(TestMaps.Market);

            Assert.That(route.TryRemoveLastStop(), Is.True);
            Assert.That(route.TryRemoveLastStop(), Is.False);
            Assert.That(route.LastStop.StationId, Is.EqualTo(TestMaps.Central));
        }

        [Test]
        public void Reset_ReturnsToStart_AndAlwaysRaisesChanged()
        {
            Route route = TestMaps.Night2Route();
            route.TryAddStop(TestMaps.Market);
            route.TryAddStop(TestMaps.Harbour);
            int changes = 0;
            route.Changed += () => changes++;

            route.Reset();
            route.Reset();

            Assert.That(route.Stops, Has.Count.EqualTo(1));
            Assert.That(changes, Is.EqualTo(2));
        }
    }
}
