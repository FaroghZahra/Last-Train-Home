using System;
using NUnit.Framework;

namespace LastTrainHome.Domain.Tests
{
    public sealed class ClockTimeTests
    {
        [TestCase(0, "00:00")]
        [TestCase(40, "00:40")]
        [TestCase(60, "01:00")]
        [TestCase(75, "01:15")]
        [TestCase(24 * 60 + 5, "00:05")]
        public void ToString_FormatsAsTwentyFourHourClock(int minutes, string expected)
        {
            Assert.That(new ClockTime(minutes).ToString(), Is.EqualTo(expected));
        }

        [TestCase("00:00", 0)]
        [TestCase("00:40", 40)]
        [TestCase("01:15", 75)]
        [TestCase("1:05", 65)]
        public void TryParse_ReadsHoursAndMinutes(string text, int expectedMinutes)
        {
            Assert.That(ClockTime.TryParse(text, out ClockTime time), Is.True);
            Assert.That(time.MinutesAfterMidnight, Is.EqualTo(expectedMinutes));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("40")]
        [TestCase("00:4")]
        [TestCase("00:60")]
        [TestCase("-1:00")]
        [TestCase("00:40:00")]
        public void TryParse_RejectsMalformedTimes(string text)
        {
            Assert.That(ClockTime.TryParse(text, out _), Is.False);
        }

        [Test]
        public void Constructor_RejectsTimesBeforeMidnight()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ClockTime(-1));
        }

        [Test]
        public void AddMinutes_And_MinutesSince_AreInverse()
        {
            var start = new ClockTime(20);

            ClockTime later = start.AddMinutes(30);

            Assert.That(later.MinutesAfterMidnight, Is.EqualTo(50));
            Assert.That(later.MinutesSince(start), Is.EqualTo(30));
            Assert.That(start.MinutesSince(later), Is.EqualTo(-30));
        }

        [Test]
        public void Comparisons_OrderByTime()
        {
            var early = new ClockTime(10);
            var late = new ClockTime(20);

            Assert.That(early < late, Is.True);
            Assert.That(late > early, Is.True);
            Assert.That(early <= new ClockTime(10), Is.True);
            Assert.That(early == new ClockTime(10), Is.True);
            Assert.That(early != late, Is.True);
        }
    }
}
