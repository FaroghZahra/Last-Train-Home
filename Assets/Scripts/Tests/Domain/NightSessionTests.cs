using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Domain.Session;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class NightSessionTests
    {
        private NightSession _session;

        [SetUp]
        public void SetUp() => _session = new NightSession(TestMaps.Night2());

        [Test]
        public void NewSession_IsPlanningAtTheStartWithNobodyReached()
        {
            Assert.That(_session.Phase, Is.EqualTo(NightPhase.Planning));
            Assert.That(_session.Stops, Has.Count.EqualTo(1));
            Assert.That(_session.CanDepart, Is.False);
            Assert.That(_session.Outcomes, Has.All.Matches<PassengerOutcome>(o => o.Status == ArrivalStatus.NotReached));
        }

        [Test]
        public void TryAddStop_UpdatesOutcomesBeforeRaisingRouteChanged()
        {
            ArrivalStatus? priyaWhenRaised = null;
            _session.RouteChanged += () => priyaWhenRaised = _session.Outcomes[2].Status;

            AddStopResult result = _session.TryAddStop(TestMaps.OldTown);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(priyaWhenRaised, Is.EqualTo(ArrivalStatus.OnTime));
        }

        [Test]
        public void TryUndo_And_Clear_EditTheRoute()
        {
            _session.TryAddStop(TestMaps.Market);
            _session.TryAddStop(TestMaps.Harbour);

            Assert.That(_session.TryUndo(), Is.True);
            Assert.That(_session.LastStop.StationId, Is.EqualTo(TestMaps.Market));

            _session.Clear();
            Assert.That(_session.HasLeftStart, Is.False);
            Assert.That(_session.TryUndo(), Is.False);
        }

        [Test]
        public void FullNight_MovesThroughEveryPhaseAndReportsTheResult()
        {
            var phases = new List<NightPhase>();
            _session.PhaseChanged += phases.Add;
            _session.TryAddStop(TestMaps.OldTown);

            _session.Depart();
            NightResult result = _session.Finish();
            _session.Replan();

            Assert.That(phases, Is.EqualTo(new[] { NightPhase.Travelling, NightPhase.Finished, NightPhase.Planning }));
            Assert.That(result.NightId, Is.EqualTo("Night2"));
            Assert.That(result.Route.Count, Is.EqualTo(2));
            Assert.That(result.OnTimeCount, Is.EqualTo(1));
            Assert.That(_session.HasLeftStart, Is.False, "Replanning starts from the start station again.");
        }

        [Test]
        public void Depart_WithoutLeavingTheStart_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _session.Depart());
        }

        [Test]
        public void RouteEdits_OutsidePlanning_Throw()
        {
            _session.TryAddStop(TestMaps.OldTown);
            _session.Depart();

            Assert.Throws<InvalidOperationException>(() => _session.TryAddStop(TestMaps.Central));
            Assert.Throws<InvalidOperationException>(() => _session.TryUndo());
            Assert.Throws<InvalidOperationException>(() => _session.Clear());
            Assert.Throws<InvalidOperationException>(() => _session.Replan());
        }

        [Test]
        public void Finish_BeforeDeparting_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _session.Finish());
        }
    }
}
