using System.Collections.Generic;
using System.IO;
using System.Linq;
using LastTrainHome.Data;
using NUnit.Framework;
using UnityEngine;

namespace LastTrainHome.Domain.Tests
{
    /// <summary>
    /// Guards the design of the shipped level files (Implementation_Plan.md §3 and §5, Day 3): Night 1 can be solved,
    /// and on every dilemma night nobody can save everyone, yet each passenger can be the only one who is late.
    /// Explores every legal route via the Route API. Design/level_solver.py prints the same routes for tuning.
    /// </summary>
    public sealed class LevelBalanceTests
    {
        private static NightDefinition Load(string level)
        {
            string path = Path.Combine(Application.dataPath, "Data", "Levels", level + ".json");
            return NightDefinition.FromJson(File.ReadAllText(path), level);
        }

        /// <summary>Every reachable combination of outcomes, e.g. "OnTime,Late,OnTime", in passenger order.</summary>
        private static HashSet<string> ReachableOutcomes(NightDefinition night)
        {
            var route = new Route(night.Network, night.StartStationId, night.Departure, night.Closing);
            Passenger[] passengers = night.Passengers.Select(passenger => passenger.ToPassenger()).ToArray();
            string[] stations = night.Network.StationIds.ToArray();
            var outcomes = new HashSet<string>();

            void Explore()
            {
                PassengerOutcome[] results = OutcomeEvaluator.EvaluateAll(passengers, route.Stops);
                outcomes.Add(string.Join(",", results.Select(r => r.Status)));

                foreach (string station in stations)
                {
                    if (!route.TryAddStop(station).Succeeded)
                    {
                        continue;
                    }

                    Explore();
                    route.TryRemoveLastStop();
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

        [TestCase("Night1")]
        [TestCase("Night2")]
        [TestCase("Night3")]
        public void LevelFile_CanBePlayed(string level)
        {
            Assert.DoesNotThrow(() => Load(level));
        }

        [Test]
        public void Night1_EveryoneCanArriveOnTime()
        {
            NightDefinition night = Load("Night1");

            Assert.That(ReachableOutcomes(night), Does.Contain(AllOnTime(night.Passengers.Count)));
        }

        [TestCase("Night2")]
        [TestCase("Night3")]
        public void DilemmaNight_NoRouteGetsEveryoneThereOnTime(string level)
        {
            NightDefinition night = Load(level);

            Assert.That(ReachableOutcomes(night), Does.Not.Contain(AllOnTime(night.Passengers.Count)));
        }

        [TestCase("Night2")]
        [TestCase("Night3")]
        public void DilemmaNight_EachPassengerCanBeTheOnlyOneLate(string level)
        {
            NightDefinition night = Load(level);
            HashSet<string> outcomes = ReachableOutcomes(night);

            for (int i = 0; i < night.Passengers.Count; i++)
            {
                Assert.That(outcomes, Does.Contain(OnlyLate(i, night.Passengers.Count)),
                    $"{night.Passengers[i].Name} cannot be the only one late.");
            }
        }
    }
}
