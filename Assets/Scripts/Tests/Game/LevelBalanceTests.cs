using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using NUnit.Framework;

namespace LastTrainHome.Game.Tests
{
    /// <summary>
    /// Guards the design of the shipped week: Monday can be solved, and on every later night nobody can save everyone.
    /// On a passenger's big night saving them is never free: every best route leaves them waiting, and saving them
    /// costs someone else, whom the player chooses. On a night two passengers share, the cost is each other: either
    /// can be saved, never both, and saving either gets as many home, so the choice is only about whose night it is. Explores every legal route through <see cref="NightSession"/>;
    /// Design/level_solver.py prints the same routes, and with --variants checks every carry-over combination.
    /// </summary>
    public sealed class LevelBalanceTests
    {
        private static Level Load(string nightId) => TestContent.GameLevel(nightId);

        private static int NightIndex(string nightId) =>
            TestContent.Game().Levels.ToList().FindIndex(level => level.Night.Id == nightId);

        /// <summary>Every reachable combination of outcomes, in passenger order.</summary>
        private static List<ArrivalStatus[]> ReachableOutcomes(NightDefinition night)
        {
            var session = new NightSession(night);
            string[] stations = night.Network.StationIds.ToArray();
            var seen = new HashSet<string>();
            var outcomes = new List<ArrivalStatus[]>();

            void Explore()
            {
                ArrivalStatus[] statuses = session.Outcomes.Select(outcome => outcome.Status).ToArray();
                if (seen.Add(string.Join(",", statuses)))
                {
                    outcomes.Add(statuses);
                }

                foreach (string station in stations)
                {
                    if (session.TryAddStop(station).Succeeded)
                    {
                        Explore();
                        session.TryUndo();
                    }
                }
            }

            Explore();
            return outcomes;
        }

        private static int OnTime(ArrivalStatus[] statuses) => statuses.Count(status => status == ArrivalStatus.OnTime);

        private static int Best(List<ArrivalStatus[]> outcomes) => outcomes.Max(OnTime);

        /// <summary>The most on time with the big-night passenger among them, and who else those routes save.</summary>
        private static (int Most, string[] Alongside) SavingThem(NightDefinition night, List<ArrivalStatus[]> outcomes, string star)
        {
            int i = night.Passengers.ToList().FindIndex(passenger => passenger.Name == star);
            ArrivalStatus[][] saving = outcomes.Where(statuses => statuses[i] == ArrivalStatus.OnTime).ToArray();
            if (saving.Length == 0)
            {
                return (0, new string[0]);
            }

            int most = saving.Max(OnTime);
            string[] alongside = saving
                .Where(statuses => OnTime(statuses) == most)
                .Select(statuses => string.Join("+", night.Passengers
                    .Where((passenger, j) => j != i && statuses[j] == ArrivalStatus.OnTime)
                    .Select(passenger => passenger.Name)))
                .Distinct()
                .ToArray();
            return (most, alongside);
        }

        private static string[] Stars(string nightId) =>
            TestContent.Game().Week.BigNightsOn(NightIndex(nightId)).Select(member => member.Name).ToArray();

        private static bool BothOnTime(NightDefinition night, ArrivalStatus[] statuses, string[] stars) =>
            stars.All(star => statuses[night.Passengers.ToList().FindIndex(passenger => passenger.Name == star)] == ArrivalStatus.OnTime);

        [Test]
        public void Catalog_LoadsTheWeekInOrder()
        {
            GameContent content = TestContent.Game();

            Assert.That(content.Levels.Select(level => level.Night.Id), Is.EqualTo(new[] { "Night1", "Night2", "Night3", "Night4", "Night5" }));
            Assert.That(content.Levels.Select(level => level.Day), Is.EqualTo(new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }));
        }

        [Test]
        public void Monday_EveryoneCanArriveOnTime()
        {
            NightDefinition night = Load("Night1").Night;

            Assert.That(Best(ReachableOutcomes(night)), Is.EqualTo(night.Passengers.Count));
        }

        [TestCase("Night2")]
        [TestCase("Night3")]
        [TestCase("Night4")]
        [TestCase("Night5")]
        public void DilemmaNight_NoRouteGetsEveryoneThereOnTime(string nightId)
        {
            NightDefinition night = Load(nightId).Night;

            Assert.That(Best(ReachableOutcomes(night)), Is.LessThan(night.Passengers.Count));
        }

        [TestCase("Night2")]
        [TestCase("Night3")]
        public void BigNight_SavingThemCostsSomeoneElse_AndThePlayerChoosesWho(string nightId)
        {
            NightDefinition night = Load(nightId).Night;
            string star = Stars(nightId).Single();
            List<ArrivalStatus[]> outcomes = ReachableOutcomes(night);

            (int most, string[] alongside) = SavingThem(night, outcomes, star);

            Assert.That(most, Is.GreaterThan(0), $"{star} can't be saved at all.");
            Assert.That(most, Is.LessThan(Best(outcomes)), $"Saving {star} is free.");
            Assert.That(alongside, Has.Length.GreaterThanOrEqualTo(2), $"Saving {star} leaves no choice of who else.");
        }

        [Test]
        public void Thursday_PriyaAndWenShareABigNight_AndOnlyOneCanBeKept()
        {
            NightDefinition night = Load("Night4").Night;
            string[] stars = Stars("Night4");
            List<ArrivalStatus[]> outcomes = ReachableOutcomes(night);

            Assert.That(stars, Is.EquivalentTo(new[] { "Priya", "Wen" }));
            Assert.That(outcomes.Any(statuses => BothOnTime(night, statuses, stars)), Is.False, "Some route keeps both.");
            int[] most = stars.Select(star => SavingThem(night, outcomes, star).Most).ToArray();
            Assert.That(most, Has.All.GreaterThan(0), "One of them can't be saved at all.");
            Assert.That(most.Distinct().Count(), Is.EqualTo(1), "Saving one of them gets more home than saving the other.");
        }

        [Test]
        public void Friday_TheLastTrain_IsNobodysBigNight()
        {
            Assert.That(Stars("Night5"), Is.Empty);
        }

        [Test]
        public void EveryCarryOverCombination_KeepsTheBigNightExpensive()
        {
            GameContent content = TestContent.Game();
            for (int index = 1; index < content.Levels.Count; index++)
            {
                Level level = content.Levels[index];
                string[] stars = content.Week.BigNightsOn(index).Select(member => member.Name).ToArray();
                var conditions = level.Variants.Select(variant => (variant.IfNightId, variant.IfPassenger, variant.IfStatus)).ToArray();
                for (int mask = 0; mask < 1 << conditions.Length; mask++)
                {
                    int chosen = mask;
                    bool Chosen(string night, string passenger, out int index)
                    {
                        for (index = 0; index < conditions.Length; index++)
                        {
                            if ((chosen & (1 << index)) != 0 && conditions[index].IfNightId == night && conditions[index].IfPassenger == passenger)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    Level played = level.WithCarryOver(
                        (night, passenger) => Chosen(night, passenger, out int i)
                            ? conditions[i].IfStatus == StatusMatch.OnTime ? ArrivalStatus.OnTime : ArrivalStatus.Late
                            : (ArrivalStatus?)null,
                        (night, passenger) => Chosen(night, passenger, out _));

                    List<ArrivalStatus[]> outcomes = ReachableOutcomes(played.Night);
                    string where = $"{level.Night.Id}, variants {mask}";
                    Assert.That(Best(outcomes), Is.LessThan(played.Night.Passengers.Count), $"{where}: someone must wait.");
                    if (stars.Length == 1 && played.Night.Passengers.Any(passenger => passenger.Name == stars[0]))
                    {
                        (int most, _) = SavingThem(played.Night, outcomes, stars[0]);
                        Assert.That(most, Is.InRange(1, Best(outcomes) - 1), $"{where}: saving {stars[0]} must be possible, and cost someone.");
                    }
                    else if (stars.Length > 1)
                    {
                        Assert.That(outcomes.Any(statuses => BothOnTime(played.Night, statuses, stars)), Is.False,
                            $"{where}: some route keeps every big night at once.");
                        foreach (string star in stars)
                        {
                            Assert.That(SavingThem(played.Night, outcomes, star).Most, Is.GreaterThan(0), $"{where}: {star} can't be saved.");
                        }
                    }
                }
            }
        }

        [Test]
        public void BrokenPromise_IsRemembered_OnlyWhenThePromiseWasMade()
        {
            Level wednesday = Load("Night3");
            ArrivalStatus? TomasLateOnTuesday(string night, string passenger) =>
                night == "Night2" && passenger == "Tomás" ? ArrivalStatus.Late : (ArrivalStatus?)null;

            string Platform(Level level) => level.Night.Passengers.Single(passenger => passenger.Name == "Tomás").Story.PlatformLine;

            Assert.That(Platform(wednesday.WithCarryOver(TomasLateOnTuesday)), Does.StartWith("Friday's my last shift"));
            Assert.That(Platform(wednesday.WithCarryOver(TomasLateOnTuesday, (night, passenger) => passenger == "Tomás")),
                Does.StartWith("You said you'd get me there"));
        }

        [Test]
        public void MissingPriyasGig_SendsHerToHerMumsOnFriday()
        {
            Level friday = Load("Night5").WithCarryOver((night, passenger) =>
                night == "Night4" && passenger == "Priya" ? ArrivalStatus.NotReached : (ArrivalStatus?)null);

            Assert.That(friday.Night.Passengers.Single(passenger => passenger.Name == "Priya").DestinationStationId, Is.EqualTo("Hillside"));
        }
    }
}
