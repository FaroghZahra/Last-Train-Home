using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>Turns Data/Story/Week.json (see <see cref="WeekFile"/>) into a <see cref="Week"/>, listing every problem.</summary>
    public static class WeekParser
    {
        /// <exception cref="FormatException">The file can't be used; the message lists every problem.</exception>
        public static Week Parse(string json, string fileName)
        {
            WeekFile file;
            try
            {
                file = JsonUtility.FromJson<WeekFile>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException($"'{fileName}' is not valid JSON: {exception.Message}");
            }

            if (file == null)
            {
                throw new FormatException($"'{fileName}' is empty.");
            }

            var problems = new List<string>();
            string[] days = file.days ?? Array.Empty<string>();
            if (string.IsNullOrWhiteSpace(file.title))
            {
                problems.Add("The week has no title.");
            }

            if (days.Length == 0 || days.Any(string.IsNullOrWhiteSpace))
            {
                problems.Add("Every day needs a short name, e.g. \"Mon\".");
            }

            var cast = new List<CastMember>();
            foreach (WeekFile.CastMember entry in file.cast ?? Array.Empty<WeekFile.CastMember>())
            {
                if (string.IsNullOrWhiteSpace(entry.name))
                {
                    problems.Add("A cast member has no name.");
                    continue;
                }

                if (cast.Any(member => member.Name == entry.name))
                {
                    problems.Add($"{entry.name} is in the cast twice.");
                }

                if (entry.bigNight < -1 || entry.bigNight >= days.Length)
                {
                    problems.Add($"{entry.name}'s bigNight {entry.bigNight} isn't one of the {days.Length} days (0 is the first, -1 for none).");
                }

                if (entry.bigNight >= 0 && (string.IsNullOrWhiteSpace(entry.epilogueKept) || string.IsNullOrWhiteSpace(entry.epilogueMissed)))
                {
                    problems.Add($"{entry.name} has a big night, so needs epilogueKept and epilogueMissed for the ending.");
                }

                cast.Add(new CastMember(entry.name, entry.bigNight, entry.bigNightLabel, entry.epilogueKept, entry.epilogueMissed));
            }

            var keepsakes = new List<Keepsake>();
            foreach (WeekFile.Keepsake entry in file.keepsakes ?? Array.Empty<WeekFile.Keepsake>())
            {
                if (string.IsNullOrWhiteSpace(entry.id) || keepsakes.Any(keepsake => keepsake.Id == entry.id))
                {
                    problems.Add($"Keepsake ids must be present and unique; '{entry.id}' isn't.");
                }

                if (string.IsNullOrWhiteSpace(entry.name) || string.IsNullOrWhiteSpace(entry.line))
                {
                    problems.Add($"Keepsake '{entry.id}' needs a name and a line.");
                }

                keepsakes.Add(new Keepsake(entry.id, entry.name, entry.line));
            }

            var endings = new List<Ending>();
            foreach (WeekFile.Ending entry in file.endings ?? Array.Empty<WeekFile.Ending>())
            {
                if (string.IsNullOrWhiteSpace(entry.title) || string.IsNullOrWhiteSpace(entry.line))
                {
                    problems.Add($"The ending for {entry.kept} big nights kept needs a title and a line.");
                }

                endings.Add(new Ending(entry.kept, entry.title, entry.line));
            }

            int keepable = Week.KeepableBigNightsOf(cast);
            for (int kept = 0; kept <= keepable; kept++)
            {
                if (endings.Count(ending => ending.Kept == kept) != 1)
                {
                    problems.Add($"There should be exactly one ending for {kept} big nights kept.");
                }
            }

            if (problems.Count > 0)
            {
                throw new FormatException($"'{fileName}' cannot be used:\n- {string.Join("\n- ", problems)}");
            }

            return new Week(file.title, file.intro, file.introEmphasis, file.footnote, days, cast, keepsakes, endings);
        }
    }
}
