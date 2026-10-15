using System;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    /// <summary>The platform before the map: limited questions, and a promise judged by the route.</summary>
    public sealed class PlatformAndPromiseTests
    {
        [Test]
        public void OneQuestion_CanBeAskedOnceAndThenNoMore()
        {
            var platform = new PlatformSession(TestMaps.Night2Passengers(), asksAllowed: 1);

            Assert.That(platform.TryAsk("Agnes"), Is.True);
            Assert.That(platform.HasAsked("Agnes"), Is.True);
            Assert.That(platform.TryAsk("Agnes"), Is.False, "Nobody is asked twice.");
            Assert.That(platform.CanAsk("Priya"), Is.False, "Tonight's question is used.");
            Assert.That(platform.TryAsk("Priya"), Is.False);
            Assert.That(platform.Asked, Is.EqualTo(new[] { "Agnes" }));
        }

        [Test]
        public void UnlimitedQuestions_LetThePlayerAskEveryone()
        {
            var platform = new PlatformSession(TestMaps.Night2Passengers(), asksAllowed: -1);

            Assert.That(platform.IsUnlimited, Is.True);
            Assert.That(platform.TryAsk("Agnes") && platform.TryAsk("Tomás") && platform.TryAsk("Priya"), Is.True);
        }

        [Test]
        public void Asking_SomeoneWhoIsNotRiding_IsRefused()
        {
            var platform = new PlatformSession(TestMaps.Night2Passengers(), asksAllowed: -1);

            Assert.That(platform.TryAsk("Nobody"), Is.False);
        }

        [Test]
        public void Promise_CanChangeWhilePlanning_AndIsJudgedByTheRoute()
        {
            var session = new NightSession(TestMaps.Night2());
            session.Promise("Agnes");
            session.Promise("Priya");
            session.TryAddStop(TestMaps.OldTown); // Priya on time at 00:10

            session.Depart();
            NightResult result = session.Finish();

            Assert.That(result.PromisedPassenger, Is.EqualTo("Priya"));
            Assert.That(result.PromiseKept, Is.True);
            Assert.That(result.TryGetStatus("Agnes", out ArrivalStatus agnes), Is.True);
            Assert.That(agnes, Is.EqualTo(ArrivalStatus.NotReached));
        }

        [Test]
        public void Promise_ToSomeoneNotReached_IsBroken()
        {
            var session = new NightSession(TestMaps.Night2());
            session.Promise("Agnes");
            session.TryAddStop(TestMaps.OldTown);
            session.Depart();

            Assert.That(session.Finish().PromiseKept, Is.False);
        }

        [Test]
        public void Promise_IsOnlyForTonightsPassengers_AndOnlyBeforeDeparture()
        {
            var session = new NightSession(TestMaps.Night2());

            Assert.Throws<ArgumentException>(() => session.Promise("Nobody"));

            session.TryAddStop(TestMaps.OldTown);
            session.Depart();
            Assert.Throws<InvalidOperationException>(() => session.Promise("Agnes"));
        }

        [Test]
        public void NoPromise_IsNeitherKeptNorAKnownPassenger()
        {
            var session = new NightSession(TestMaps.Night2());
            session.TryAddStop(TestMaps.OldTown);
            session.Depart();
            NightResult result = session.Finish();

            Assert.That(result.PromisedPassenger, Is.Null);
            Assert.That(result.PromiseKept, Is.False);
            Assert.That(result.TryGetStatus("Nobody", out _), Is.False);
        }
    }
}
