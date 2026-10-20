using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow;
using NUnit.Framework;

namespace LastTrainHome.Game.Tests
{
    /// <summary>The week, the interface wording, how the files fit together, carry-over and the coach line.</summary>
    public sealed class ContentTests
    {
        private const string Week = @"{
            ""title"": ""Last Train Home"", ""intro"": [ ""Five nights."" ], ""days"": [ ""Mon"", ""Tue"" ],
            ""cast"": [ { ""name"": ""Agnes"", ""bigNight"": 1, ""bigNightLabel"": ""Ferry"", ""epilogueKept"": ""Waved."", ""epilogueMissed"": ""Posted."" },
                        { ""name"": ""Dev"", ""bigNight"": 0, ""epilogueKept"": ""Slept."", ""epilogueMissed"": ""Woke."" } ],
            ""keepsakes"": [ { ""id"": ""ticket"", ""name"": ""Ticket"", ""line"": ""A ticket."" } ],
            ""endings"": [ { ""kept"": 0, ""title"": ""None"", ""line"": ""No one."" }, { ""kept"": 1, ""title"": ""One"", ""line"": ""Someone."" },
                           { ""kept"": 2, ""title"": ""Both"", ""line"": ""Everyone."" } ]
        }";

        private static UiText Ui() => UiText.FromDictionary(new Dictionary<string, string>
        {
            [UiKey.ListAnd] = " and ",
            [UiKey.ListComma] = ", ",
        });

        [Test]
        public void TextTemplate_FillsTokens_AndFindsUnknownOnes()
        {
            Assert.That(TextTemplate.Fill("{name} waits.", ("name", "Wen")), Is.EqualTo("Wen waits."));
            Assert.That(TextTemplate.FindUnknownTokens("{name} at {tim}", new[] { "name", "time" }), Is.EqualTo(new[] { "{tim}" }));
            Assert.Throws<ArgumentException>(() => TextTemplate.Fill("{name}"));
        }

        [Test]
        public void UiText_TheGamesFileHasEveryKey()
        {
            UiText ui = TestContent.Game().Ui;

            foreach (string key in UiKey.Tokens.Keys)
            {
                Assert.That(ui.Get(key), Is.Not.EqualTo(key), $"'{key}' has no text.");
            }
        }

        [Test]
        public void UiText_ReportsMissingUnknownAndBadTokens()
        {
            const string json = @"{ ""entries"": [ { ""key"": ""plan.depart"", ""text"": ""Go {now}"" }, { ""key"": ""nope"", ""text"": ""x"" } ] }";

            var exception = Assert.Throws<FormatException>(() => UiText.Parse(json, "UiText"));

            Assert.That(exception.Message, Does.Contain("'plan.depart' uses {now}"));
            Assert.That(exception.Message, Does.Contain("'nope' isn't a key"));
            Assert.That(exception.Message, Does.Contain("'plan.undo' is missing"));
        }

        [Test]
        public void UiText_JoinsNamesLikeASentence()
        {
            UiText ui = Ui();

            Assert.That(ui.JoinNames(new[] { "Wen" }), Is.EqualTo("Wen"));
            Assert.That(ui.JoinNames(new[] { "Wen", "Agnes" }), Is.EqualTo("Wen and Agnes"));
            Assert.That(ui.JoinNames(new[] { "Wen", "Agnes", "Priya" }), Is.EqualTo("Wen, Agnes and Priya"));
        }

        [Test]
        public void Week_ReadsTheCastAndKeepsakes()
        {
            Week week = WeekParser.Parse(Week, "Week");

            Assert.That(week.Days, Is.EqualTo(new[] { "Mon", "Tue" }));
            Assert.That(week.FindCastMember("Agnes").BigNightLabel, Is.EqualTo("Ferry"));
            Assert.That(week.BigNightsOn(0).Select(member => member.Name), Is.EqualTo(new[] { "Dev" }));
            Assert.That(week.FindKeepsake("ticket").Line, Is.EqualTo("A ticket."));
        }

        [TestCase("\"bigNight\": 1", "\"bigNight\": 7", "isn't one of the 2 days")]
        [TestCase("\"name\": \"Dev\"", "\"name\": \"Agnes\"", "Agnes is in the cast twice")]
        [TestCase("\"line\": \"A ticket.\"", "\"line\": \"\"", "needs a name and a line")]
        [TestCase("\"kept\": 2", "\"kept\": 1", "exactly one ending for 2 big nights kept")]
        [TestCase("\"epilogueMissed\": \"Posted.\"", "\"epilogueMissed\": \"\"", "needs epilogueKept and epilogueMissed")]
        public void Week_ListsProblems(string valid, string broken, string expectedProblem)
        {
            var exception = Assert.Throws<FormatException>(() => WeekParser.Parse(Week.Replace(valid, broken), "Week"));

            Assert.That(exception.Message, Does.Contain(expectedProblem));
        }

        [Test]
        public void ContentChecks_FindPassengersOutsideTheCast_UnknownKeepsakes_AndVariantsLookingForward()
        {
            Week week = WeekParser.Parse(Week.Replace("\"name\": \"Dev\"", "\"name\": \"Mina\"")
                .Replace("\"id\": \"ticket\"", "\"id\": \"stub\""), "Week");
            Level level = LevelParser.Parse(TestContent.Level, "Night0");

            string[] problems = ContentChecks.FindProblems(week, new[] { level }).ToArray();

            Assert.That(problems, Has.Some.Contains("The week has 2 days but there are 1 levels"));
            Assert.That(problems, Has.Some.Contains("Dev isn't in the week's cast"));
            Assert.That(problems, Has.Some.Contains("keepsake 'ticket'"));
            Assert.That(problems, Has.Some.Contains("isn't an earlier level"), "Night0 can't look back at itself.");
        }

        [Test]
        public void ContentChecks_PassTheGamesOwnContent()
        {
            Assert.DoesNotThrow(() => TestContent.Game());
        }

        [Test]
        public void CarryOver_AppliesOnlyMatchingVariants()
        {
            Level level = LevelParser.Parse(TestContent.Level, "Night1");

            Level unchanged = level.WithCarryOver((night, passenger) => ArrivalStatus.OnTime);
            Level changed = level.WithCarryOver((night, passenger) =>
                night == "Night0" && passenger == "Dev" ? ArrivalStatus.Late : (ArrivalStatus?)null);

            Assert.That(unchanged, Is.SameAs(level));
            Passenger dev = changed.Night.Passengers[1];
            Assert.That(dev.Deadline, Is.EqualTo(new ClockTime(20)));
            Assert.That(dev.Story.PlatformLine, Is.EqualTo("Told to come early."));
            Assert.That(dev.Story.PlanQuote, Is.EqualTo("Told to come early."), "A new platform line replaces the old quote.");
            Assert.That(dev.Story.AskLine, Is.EqualTo("Dates."), "What the variant doesn't mention stays.");
            Assert.That(dev.Story.GetOutcome(ArrivalStatus.OnTime).Line, Is.EqualTo("Early, for once."));
            Assert.That(dev.Story.GetOutcome(ArrivalStatus.Late).Line, Is.EqualTo("Awake."));
            Assert.That(changed.Night.Passengers[0], Is.SameAs(level.Night.Passengers[0]));
        }

        [Test]
        public void CoachLine_UsesTheFirstMatchingRule()
        {
            Level level = LevelParser.Parse(TestContent.Level, "Test");
            var session = new NightSession(level.Night);
            UiText ui = Ui();

            Assert.That(CoachLine.Pick(level.Story.Coach, session, ui), Is.EqualTo("Click a station."));

            session.TryAddStop("Market"); // Dev on time; Agnes not reached
            Assert.That(CoachLine.Pick(level.Story.Coach, session, ui), Is.EqualTo("Still waiting: Agnes. Agnes is next."));

            session.TryAddStop("Central");
            session.TryAddStop("Harbour"); // Agnes late at 00:30
            Assert.That(CoachLine.Pick(level.Story.Coach, session, ui), Is.EqualTo("Agnes would be late."));
        }

        [Test]
        public void CoachLine_IsEmptyWhenNoRuleMatches()
        {
            var rules = new[] { new CoachRule(CoachCondition.OnTime, 2, null, "Both!") };
            var session = new NightSession(LevelParser.Parse(TestContent.Level, "Test").Night);

            Assert.That(CoachLine.Pick(rules, session, Ui()), Is.Empty);
        }
    }
}
