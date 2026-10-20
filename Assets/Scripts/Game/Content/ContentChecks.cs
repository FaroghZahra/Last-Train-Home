using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Passengers;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// Checks that need more than one file: every passenger is in the week's cast, every keepsake exists, every
    /// night has a day, and every variant looks back at an earlier night that passenger rode.
    /// </summary>
    public static class ContentChecks
    {
        public static IEnumerable<string> FindProblems(Week week, IReadOnlyList<Level> levels)
        {
            if (week.Days.Count != levels.Count)
            {
                yield return $"The week has {week.Days.Count} days but there are {levels.Count} levels.";
            }

            for (int night = 0; night < levels.Count; night++)
            {
                Level level = levels[night];
                string where = $"Level '{level.Night.Id}'";
                foreach (Passenger passenger in level.Night.Passengers)
                {
                    if (week.FindCastMember(passenger.Name) == null)
                    {
                        yield return $"{where}: {passenger.Name} isn't in the week's cast.";
                    }

                    foreach (ArrivalStatus status in new[] { ArrivalStatus.OnTime, ArrivalStatus.Late, ArrivalStatus.NotReached })
                    {
                        string keepsake = passenger.Story.GetOutcome(status).KeepsakeId;
                        if (keepsake != null && week.FindKeepsake(keepsake) == null)
                        {
                            yield return $"{where}: {passenger.Name} leaves keepsake '{keepsake}', which the week doesn't list.";
                        }
                    }
                }

                foreach (string featured in level.Story.NightCard.Featured)
                {
                    if (level.Night.Passengers.All(passenger => passenger.Name != featured))
                    {
                        yield return $"{where}: the night card features {featured}, who isn't riding.";
                    }
                }

                foreach (Secret secret in level.Secrets)
                {
                    if (week.FindKeepsake(secret.KeepsakeId) == null)
                    {
                        yield return $"{where}: a secret leaves keepsake '{secret.KeepsakeId}', which the week doesn't list.";
                    }
                }

                foreach (PassengerVariant variant in level.Variants)
                {
                    int earlier = IndexOf(levels, variant.IfNightId);
                    if (earlier < 0 || earlier >= night)
                    {
                        yield return $"{where}: a variant looks back at '{variant.IfNightId}', which isn't an earlier level.";
                    }
                    else if (levels[earlier].Night.Passengers.All(passenger => passenger.Name != variant.IfPassenger))
                    {
                        yield return $"{where}: a variant looks back at {variant.IfPassenger}, who didn't ride on '{variant.IfNightId}'.";
                    }

                    foreach (OutcomePatch patch in new[] { variant.OnTime, variant.Late, variant.NotReached })
                    {
                        if (patch?.KeepsakeId != null && week.FindKeepsake(patch.KeepsakeId) == null)
                        {
                            yield return $"{where}: a variant leaves keepsake '{patch.KeepsakeId}', which the week doesn't list.";
                        }
                    }
                }
            }
        }

        private static int IndexOf(IReadOnlyList<Level> levels, string nightId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].Night.Id == nightId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
