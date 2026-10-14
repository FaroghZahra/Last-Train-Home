using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;
using NUnit.Framework;
using UnityEditor;

namespace LastTrainHome.Game.Tests
{
    /// <summary>
    /// Guards the design of the shipped levels (Implementation_Plan.md §3 and §5, Day 3): Night 1 can be solved, and on
    /// every dilemma night nobody can save everyone, yet each passenger can be the only one who is late.
    /// Explores every legal route through <see cref="NightSession"/>. Design/level_solver.py prints the same routes.
    /// </summary>
    public sealed class LevelBalanceTests
    {
        private const string CatalogPath = "Assets/Data/LevelCatalog.asset";

        private static NightDefinition Load(string nightId)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            Assert.That(catalog, Is.Not.Null, $"No level catalog at {CatalogPath}.");
            return catalog.LoadLevels().Single(level => level.Night.Id == nightId).Night;
        }

        /// <summary>Every reachable combination of outcomes, e.g. "OnTime,Late,OnTime", in passenger order.</summary>
        private static HashSet<string> ReachableOutcomes(NightDefinition night)
        {
            var session = new NightSession(night);
            string[] stations = night.Network.StationIds.ToArray();
            var outcomes = new HashSet<string>();

            void Explore()
            {
                outcomes.Add(string.Join(",", session.Outcomes.Select(outcome => outcome.Status)));
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

        private static string AllOnTime(int passengerCount) =>
            string.Join(",", Enumerable.Repeat(nameof(ArrivalStatus.OnTime), passengerCount));

        private static string OnlyLate(int latePassenger, int passengerCount) =>
            string.Join(",", Enumerable.Range(0, passengerCount)
                .Select(i => i == latePassenger ? nameof(ArrivalStatus.Late) : nameof(ArrivalStatus.OnTime)));

        [Test]
        public void Catalog_LoadsEveryLevelInOrder()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);

            Assert.That(catalog.LoadLevels().Select(level => level.Night.Id), Is.EqualTo(new[] { "Night1", "Night2", "Night3" }));
        }

        [Test]
        public void Night1_EveryoneCanArriveOnTime()
        {
            NightDefinition night = Load("Night1");

            Assert.That(ReachableOutcomes(night), Does.Contain(AllOnTime(night.Passengers.Count)));
        }

        [TestCase("Night2")]
        [TestCase("Night3")]
        public void DilemmaNight_NoRouteGetsEveryoneThereOnTime(string nightId)
        {
            NightDefinition night = Load(nightId);

            Assert.That(ReachableOutcomes(night), Does.Not.Contain(AllOnTime(night.Passengers.Count)));
        }

        [TestCase("Night2")]
        [TestCase("Night3")]
        public void DilemmaNight_EachPassengerCanBeTheOnlyOneLate(string nightId)
        {
            NightDefinition night = Load(nightId);
            HashSet<string> outcomes = ReachableOutcomes(night);

            for (int i = 0; i < night.Passengers.Count; i++)
            {
                Assert.That(outcomes, Does.Contain(OnlyLate(i, night.Passengers.Count)),
                    $"{night.Passengers[i].Name} cannot be the only one late.");
            }
        }
    }
}
