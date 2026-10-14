using System;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Progress;
using LastTrainHome.Domain.Routing;
using LastTrainHome.Domain.Session;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class CampaignTests
    {
        private static NightResult ResultFor(string nightId) =>
            new NightResult(nightId, Array.Empty<RouteStop>(), Array.Empty<PassengerOutcome>());

        [Test]
        public void Advance_WalksTheNightsInOrderUntilTheLast()
        {
            var campaign = new Campaign(new[] { "Night1", "Night2" });

            Assert.That(campaign.CurrentNightId, Is.EqualTo("Night1"));
            Assert.That(campaign.HasNextNight, Is.True);

            campaign.AdvanceToNextNight();

            Assert.That(campaign.CurrentNightId, Is.EqualTo("Night2"));
            Assert.That(campaign.HasNextNight, Is.False);
            Assert.Throws<InvalidOperationException>(() => campaign.AdvanceToNextNight());
        }

        [Test]
        public void Record_KeepsTheLatestResultOfTheCurrentNight()
        {
            var campaign = new Campaign(new[] { "Night1", "Night2" });
            NightResult second = ResultFor("Night1");

            campaign.Record(ResultFor("Night1"));
            campaign.Record(second);

            Assert.That(campaign.TryGetResult("Night1", out NightResult kept), Is.True);
            Assert.That(kept, Is.SameAs(second));
            Assert.That(campaign.TryGetResult("Night2", out _), Is.False);
        }

        [Test]
        public void Record_ForAnotherNight_Throws()
        {
            var campaign = new Campaign(new[] { "Night1", "Night2" });

            Assert.Throws<ArgumentException>(() => campaign.Record(ResultFor("Night2")));
        }

        [Test]
        public void Constructor_RejectsEmptyOrDuplicateNights()
        {
            Assert.Throws<ArgumentException>(() => new Campaign(Array.Empty<string>()));
            Assert.Throws<ArgumentException>(() => new Campaign(new[] { "Night1", "Night1" }));
        }
    }
}
