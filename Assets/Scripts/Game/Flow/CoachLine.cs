using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Session;
using LastTrainHome.Game.Content;

namespace LastTrainHome.Game.Flow
{
    /// <summary>Picks the planning coach line for the route so far: the first of the level's rules that matches.</summary>
    public static class CoachLine
    {
        /// <returns>The filled-in line, or empty when no rule matches.</returns>
        public static string Pick(IReadOnlyList<CoachRule> rules, NightSession session, UiText ui) =>
            Pick(rules, session, session.Night.Passengers, session.Outcomes, ui);

        /// <summary>The coach line for the passengers and outcomes as the player knows them.</summary>
        public static string Pick(IReadOnlyList<CoachRule> rules, NightSession session,
            IReadOnlyList<Passenger> passengers, IReadOnlyList<PassengerOutcome> outcomes, UiText ui)
        {
            var late = new List<string>();
            var notReached = new List<string>();
            var waiting = new List<Passenger>();
            for (int i = 0; i < passengers.Count; i++)
            {
                ArrivalStatus status = outcomes[i].Status;
                if (status == ArrivalStatus.Late)
                {
                    late.Add(passengers[i].Name);
                }
                else if (status == ArrivalStatus.NotReached)
                {
                    notReached.Add(passengers[i].Name);
                }

                if (status != ArrivalStatus.OnTime)
                {
                    waiting.Add(passengers[i]);
                }
            }

            int onTime = passengers.Count - waiting.Count;
            string promised = session.PromisedPassenger;
            int promisedIndex = promised == null ? -1 : passengers.ToList().FindIndex(passenger => passenger.Name == promised);
            bool promiseBroken = promisedIndex >= 0 && outcomes[promisedIndex].Status != ArrivalStatus.OnTime;

            foreach (CoachRule rule in rules)
            {
                bool matches = rule.Condition switch
                {
                    CoachCondition.Empty => !session.HasLeftStart,
                    CoachCondition.Stops => session.Stops.Count == rule.Count,
                    CoachCondition.AllOnTime => waiting.Count == 0,
                    CoachCondition.OnTime => onTime == rule.Count,
                    CoachCondition.AnyLate => late.Count > 0,
                    CoachCondition.AnyNotReached => notReached.Count > 0,
                    CoachCondition.DeadEnd => session.LastStop.StationId == rule.Station && notReached.Count > 0,
                    CoachCondition.PromiseBroken => promiseBroken,
                    CoachCondition.Always => true,
                    _ => throw new ArgumentOutOfRangeException(nameof(rules), rule.Condition, null),
                };

                if (matches)
                {
                    return TextTemplate.Fill(rule.Text,
                        ("late", ui.JoinNames(late)),
                        ("notReached", ui.JoinNames(notReached)),
                        ("waiting", ui.JoinNames(waiting.Select(passenger => passenger.Name).ToArray())),
                        ("nextDeadline", waiting.OrderBy(passenger => passenger.Deadline).FirstOrDefault()?.Name),
                        ("promised", promised));
                }
            }

            return string.Empty;
        }
    }
}
