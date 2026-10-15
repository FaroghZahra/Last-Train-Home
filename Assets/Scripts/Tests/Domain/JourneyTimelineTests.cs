using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Journey;
using LastTrainHome.Domain.Routing;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class JourneyTimelineTests
    {
        // Central 00:00 -> Market 00:10 -> Harbour 00:20, at 0.1 s per minute: legs take 1 s each.
        private static readonly RouteStop[] Stops =
        {
            new RouteStop(TestMaps.Central, new ClockTime(0)),
            new RouteStop(TestMaps.Market, new ClockTime(10)),
            new RouteStop(TestMaps.Harbour, new ClockTime(20)),
        };

        private const float SecondsPerMinute = 0.1f;

        private readonly List<string> _events = new List<string>();

        private JourneyTimeline Create(params float[] holds)
        {
            var timeline = new JourneyTimeline(Stops, holds.Length > 0 ? holds : new float[Stops.Length], SecondsPerMinute);
            timeline.StopReached += index => _events.Add($"stop {index}");
            timeline.TimeChanged += time => _events.Add(time.ToString());
            timeline.Completed += () => _events.Add("done");
            return timeline;
        }

        [SetUp]
        public void SetUp() => _events.Clear();

        [Test]
        public void Start_PutsTheTrainAtTheFirstStop()
        {
            JourneyTimeline timeline = Create();

            timeline.Start();

            Assert.That(_events, Is.EqualTo(new[] { "00:00", "stop 0" }));
            Assert.That(timeline.CurrentLeg.ToStopIndex, Is.EqualTo(0));
        }

        [Test]
        public void Advance_ReportsEachMinuteAndProgressAlongTheLeg()
        {
            JourneyTimeline timeline = Create();
            timeline.Start();
            _events.Clear();

            timeline.Advance(0.55f);

            Assert.That(_events, Is.EqualTo(new[] { "00:05" }));
            Assert.That(timeline.CurrentLeg.FromStopIndex, Is.EqualTo(0));
            Assert.That(timeline.CurrentLeg.ToStopIndex, Is.EqualTo(1));
            Assert.That(timeline.CurrentLeg.Progress, Is.EqualTo(0.55f).Within(0.001f));
        }

        [Test]
        public void Advance_HoldsAtStopsForTheirHoldTime()
        {
            JourneyTimeline timeline = Create(0f, 2f, 0f);
            timeline.Start();
            timeline.Advance(1f);
            _events.Clear();

            timeline.Advance(1.5f);

            Assert.That(_events, Is.Empty, "Still holding at Market.");
            Assert.That(timeline.CurrentLeg.ToStopIndex, Is.EqualTo(1));
        }

        [Test]
        public void Advance_InOneLargeStep_RunsThroughEveryStopInOrder()
        {
            JourneyTimeline timeline = Create(0.5f, 0.5f, 0.5f);
            timeline.Start();

            timeline.Advance(100f);

            Assert.That(_events, Is.EqualTo(new[] { "00:00", "stop 0", "00:10", "stop 1", "00:20", "stop 2", "done" }));
            Assert.That(timeline.IsComplete, Is.True);
        }

        [Test]
        public void Speed_ScalesPlayback()
        {
            JourneyTimeline timeline = Create();
            timeline.Start();
            timeline.Speed = 4f;

            timeline.Advance(0.5f);

            Assert.That(timeline.IsComplete, Is.True, "Two 1 s legs at 4x take 0.5 s.");
        }

        [Test]
        public void Advance_BeforeStartOrAfterCompletion_DoesNothing()
        {
            JourneyTimeline timeline = Create();
            timeline.Advance(10f);
            Assert.That(_events, Is.Empty);

            timeline.Start();
            timeline.Advance(10f);
            _events.Clear();
            timeline.Advance(10f);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void Constructor_RejectsMismatchedHoldsAndNonPositivePace()
        {
            Assert.Throws<ArgumentException>(() => new JourneyTimeline(Stops, new float[2], SecondsPerMinute));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JourneyTimeline(Stops, new[] { 0f, -1f, 0f }, SecondsPerMinute));
            Assert.Throws<ArgumentOutOfRangeException>(() => new JourneyTimeline(Stops, new float[3], 0f));
        }

        [Test]
        public void AnEndlessHold_WaitsUntilReleased()
        {
            JourneyTimeline timeline = Create(0f, float.PositiveInfinity, 0f);
            timeline.Start();

            timeline.Advance(1f); // reaches Market
            timeline.Advance(100f);
            Assert.That(timeline.IsWaiting, Is.True);
            Assert.That(timeline.CurrentLeg.FromStopIndex, Is.EqualTo(1));

            timeline.ReleaseHold();
            Assert.That(timeline.IsWaiting, Is.False);
            timeline.Advance(1f);

            Assert.That(timeline.IsComplete, Is.True);
            Assert.That(_events, Does.Contain("stop 2"));
        }
    }
}
