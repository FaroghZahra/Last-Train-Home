using System;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class OutcomeEvaluatorTests
    {
        private static Route RouteThrough(params string[] stops)
        {
            Route route = TestMaps.Night2Route();
            foreach (string stop in stops)
            {
                Assert.That(route.TryAddStop(stop).Succeeded, Is.True, $"Test route could not reach {stop}.");
            }

            return route;
        }

        [Test]
        public void ArrivingByTheDeadline_IsOnTime()
        {
            var passenger = new Passenger("Agnes", TestMaps.Harbour, new ClockTime(20));
            Route route = RouteThrough(TestMaps.Market, TestMaps.Harbour); // arrives exactly 00:20

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, route.Stops);

            Assert.That(outcome.Status, Is.EqualTo(ArrivalStatus.OnTime));
            Assert.That(outcome.Arrival, Is.EqualTo(new ClockTime(20)));
            Assert.That(outcome.StopIndex, Is.EqualTo(2));
            Assert.That(outcome.MinutesLate, Is.Zero);
        }

        [Test]
        public void ArrivingAfterTheDeadline_IsLateByTheDifference()
        {
            var passenger = new Passenger("Agnes", TestMaps.Harbour, new ClockTime(10));
            Route route = RouteThrough(TestMaps.Market, TestMaps.Harbour);

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, route.Stops);

            Assert.That(outcome.Status, Is.EqualTo(ArrivalStatus.Late));
            Assert.That(outcome.MinutesLate, Is.EqualTo(10));
        }

        [Test]
        public void OnlyTheFirstVisitCounts()
        {
            var passenger = new Passenger("Priya", TestMaps.OldTown, new ClockTime(15));
            Route route = RouteThrough(TestMaps.OldTown, TestMaps.Central, TestMaps.OldTown);

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, route.Stops);

            Assert.That(outcome.Status, Is.EqualTo(ArrivalStatus.OnTime));
            Assert.That(outcome.StopIndex, Is.EqualTo(1));
        }

        [Test]
        public void NeverVisitingTheDestination_IsNotReached_AndHasNoArrival()
        {
            var passenger = new Passenger("Tomás", TestMaps.Hillside, new ClockTime(40));
            Route route = RouteThrough(TestMaps.Market, TestMaps.Harbour);

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, route.Stops);

            Assert.That(outcome.Status, Is.EqualTo(ArrivalStatus.NotReached));
            Assert.That(outcome.IsReached, Is.False);
            Assert.That(outcome.StopIndex, Is.EqualTo(-1));
            Assert.Throws<InvalidOperationException>(() => _ = outcome.Arrival);
        }

        [Test]
        public void EvaluateAll_KeepsPassengerOrder()
        {
            Passenger[] passengers = TestMaps.Night2Passengers();
            Route route = RouteThrough(TestMaps.OldTown, TestMaps.Hillside, TestMaps.Market, TestMaps.Harbour);

            PassengerOutcome[] outcomes = OutcomeEvaluator.EvaluateAll(passengers, route.Stops);

            Assert.That(outcomes[0].Status, Is.EqualTo(ArrivalStatus.Late)); // Agnes 00:50
            Assert.That(outcomes[1].Status, Is.EqualTo(ArrivalStatus.OnTime)); // Tomás 00:30
            Assert.That(outcomes[2].Status, Is.EqualTo(ArrivalStatus.OnTime)); // Priya 00:10
        }

        [Test]
        public void Pickup_OnlyGetsOffAfterBoarding()
        {
            // Boards at Market; the first pass through Old Town comes before the train has collected them.
            var passenger = new Passenger("Dev", TestMaps.OldTown, new ClockTime(60), originStationId: TestMaps.Market);
            Route route = RouteThrough(TestMaps.OldTown, TestMaps.Central, TestMaps.Market, TestMaps.Central, TestMaps.OldTown);

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, route.Stops);

            Assert.That(outcome.BoardStopIndex, Is.EqualTo(3));
            Assert.That(outcome.StopIndex, Is.EqualTo(5));
            Assert.That(outcome.Arrival, Is.EqualTo(new ClockTime(50)));
        }

        [Test]
        public void Pickup_NeverCollected_IsNotReachedAndNeverAboard()
        {
            var passenger = new Passenger("Dev", TestMaps.OldTown, new ClockTime(60), originStationId: TestMaps.Hillside);
            Route route = RouteThrough(TestMaps.OldTown);

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, route.Stops);

            Assert.That(outcome.Status, Is.EqualTo(ArrivalStatus.NotReached));
            Assert.That(outcome.IsAboard, Is.False);
        }

        [Test]
        public void StartBoarder_IsAboardFromTheFirstStop()
        {
            var passenger = new Passenger("Agnes", TestMaps.Harbour, new ClockTime(60));

            PassengerOutcome outcome = OutcomeEvaluator.Evaluate(passenger, RouteThrough(TestMaps.OldTown).Stops);

            Assert.That(outcome.BoardStopIndex, Is.Zero);
            Assert.That(outcome.Status, Is.EqualTo(ArrivalStatus.NotReached));
        }
    }
}
