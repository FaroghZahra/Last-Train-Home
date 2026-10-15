using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// Turns a level file (see <see cref="LevelFile"/>) into a playable <see cref="Level"/>. Format problems are
    /// collected here; rule problems come from <see cref="NightDefinition"/>. Either way the designer sees them all at once.
    /// Checks that need the other levels or the week (cast, keepsakes, which nights came earlier) are in
    /// <see cref="ContentChecks"/>.
    /// </summary>
    public static class LevelParser
    {
        /// <param name="levelId">Becomes the night's id and names the file in error messages.</param>
        /// <exception cref="FormatException">The file is not a playable level; the message lists every problem.</exception>
        public static Level Parse(string json, string levelId)
        {
            LevelFile file = Deserialize(json, levelId);
            var problems = new List<string>();

            ClockTime departure = ReadTime(file.departure, "Departure", problems);
            ClockTime closing = ReadTime(file.closing, "Closing", problems);
            var stations = new List<StationDefinition>();
            var placements = new Dictionary<string, StationPlacement>(StringComparer.Ordinal);
            ReadStations(file.stations, stations, placements, problems);
            List<Track> tracks = ReadTracks(file.tracks, problems);
            Dictionary<(string, string), Vector2[]> trackBends = ReadTrackBends(file.tracks);
            List<Passenger> passengers = ReadPassengers(file.passengers, problems);
            NightStory story = ReadStory(file, stations, problems);
            List<PassengerVariant> variants = ReadVariants(file.variants, passengers, stations, problems);
            List<Secret> secrets = ReadSecrets(file.secrets, stations, problems);

            if (problems.Count > 0)
            {
                throw new FormatException($"Level '{levelId}' cannot be played:\n- {string.Join("\n- ", problems)}");
            }

            try
            {
                var night = new NightDefinition(levelId, file.title, file.startStation, departure, closing,
                    stations, tracks, passengers);
                return new Level(night, placements, trackBends, story, variants, secrets);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException(exception.Message);
            }
        }

        private static LevelFile Deserialize(string json, string levelId)
        {
            LevelFile file;
            try
            {
                file = JsonUtility.FromJson<LevelFile>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException($"Level '{levelId}' is not valid JSON: {exception.Message}");
            }

            return file ?? throw new FormatException($"Level '{levelId}' is empty.");
        }

        private static void ReadStations(LevelFile.Station[] entries, List<StationDefinition> stations,
            Dictionary<string, StationPlacement> placements, List<string> problems)
        {
            foreach (LevelFile.Station entry in entries ?? Array.Empty<LevelFile.Station>())
            {
                stations.Add(new StationDefinition(entry.id, entry.displayName, entry.closed));
                if (!string.IsNullOrWhiteSpace(entry.id))
                {
                    placements[entry.id] = new StationPlacement(new Vector2(entry.x, entry.y), ReadLabelSide(entry, problems));
                }
            }
        }

        private static List<Track> ReadTracks(LevelFile.Track[] entries, List<string> problems)
        {
            var tracks = new List<Track>();
            foreach (LevelFile.Track entry in entries ?? Array.Empty<LevelFile.Track>())
            {
                try
                {
                    tracks.Add(new Track(entry.from, entry.to, entry.minutes, entry.closed));
                }
                catch (ArgumentException exception)
                {
                    problems.Add($"Track '{entry.from}' - '{entry.to}': {FirstLine(exception.Message)}");
                }
            }

            return tracks;
        }

        private static Dictionary<(string, string), Vector2[]> ReadTrackBends(LevelFile.Track[] entries)
        {
            var bends = new Dictionary<(string, string), Vector2[]>();
            foreach (LevelFile.Track entry in entries ?? Array.Empty<LevelFile.Track>())
            {
                if (entry.via != null && entry.via.Length > 0)
                {
                    bends[(entry.from, entry.to)] = Array.ConvertAll(entry.via, point => new Vector2(point.x, point.y));
                }
            }

            return bends;
        }

        private static List<Passenger> ReadPassengers(LevelFile.Passenger[] entries, List<string> problems)
        {
            var passengers = new List<Passenger>();
            for (int i = 0; i < (entries?.Length ?? 0); i++)
            {
                LevelFile.Passenger entry = entries[i];
                string who = string.IsNullOrWhiteSpace(entry.name) ? $"Passenger {i}" : entry.name;
                ClockTime deadline = ReadTime(entry.deadline, $"{who}'s deadline", problems);
                ClockTime? says = null;
                if (!string.IsNullOrWhiteSpace(entry.says))
                {
                    says = ReadTime(entry.says, $"{who}'s 'says'", problems);
                    if (says == deadline)
                    {
                        problems.Add($"{who} says {entry.says}, which is already their deadline; leave 'says' out.");
                    }
                }

                Require(entry.platform, $"{who} has no platform line.", problems);
                Require(entry.ask, $"{who} has no answer for when they're asked.", problems);
                var story = new PassengerStory(entry.platform, entry.planQuote, entry.ask, entry.revealsBigNight,
                    ReadOutcome(entry.onTime, $"{who}'s onTime", problems),
                    ReadOutcome(entry.late, $"{who}'s late", problems),
                    ReadOutcome(entry.notReached, $"{who}'s notReached", problems), says);
                try
                {
                    passengers.Add(new Passenger(entry.name, entry.destination, deadline, story, entry.boards));
                }
                catch (ArgumentException exception)
                {
                    problems.Add($"Passenger {i}: {FirstLine(exception.Message)}");
                }
            }

            return passengers;
        }

        private static OutcomeStory ReadOutcome(LevelFile.Outcome entry, string field, List<string> problems)
        {
            Require(entry?.line, $"{field} has no line.", problems);
            return entry == null
                ? OutcomeStory.Empty
                : new OutcomeStory(entry.line, entry.logLine, entry.keepsake, entry.aside, entry.carry, entry.weekNote);
        }

        private static NightStory ReadStory(LevelFile file, IReadOnlyList<StationDefinition> stations, List<string> problems)
        {
            LevelFile.NightCard card = file.nightCard ?? new LevelFile.NightCard();
            LevelFile.Platform platform = file.platform ?? new LevelFile.Platform();
            Require(card.kicker, "The night card has no kicker.", problems);
            Require(platform.headline, "The platform has no headline.", problems);
            Require(file.logbook?.tomorrow, "The logbook has no line for tomorrow.", problems);
            if (card.lines == null || card.lines.Length == 0)
            {
                problems.Add("The night card has no lines.");
            }

            foreach ((string text, string field) in new[]
                     {
                         (card.kicker, "nightCard.kicker"), (platform.kicker, "platform.kicker"),
                         (platform.headline, "platform.headline"), (platform.coach, "platform.coach"),
                     })
            {
                CheckTokens(text, NightStory.HeaderTokens, field, problems);
            }

            if (file.rules == null || file.rules.asks == 0)
            {
                problems.Add("rules.asks should be -1 (ask everyone) or how many questions the player gets.");
            }

            var coach = new List<CoachRule>();
            LevelFile.CoachRule[] entries = file.coach ?? Array.Empty<LevelFile.CoachRule>();
            for (int i = 0; i < entries.Length; i++)
            {
                CoachRule rule = ReadCoachRule(entries[i], $"Coach line {i}", stations, problems);
                if (rule != null)
                {
                    coach.Add(rule);
                }
            }

            return new NightStory(
                new NightCardText(card.kicker, card.featured, card.featuredLine, card.lines, card.emphasis, card.footnote),
                new PlatformText(platform.kicker, platform.headline, platform.coach),
                file.rules?.asks ?? -1, coach, file.logbook?.tomorrow);
        }

        private static CoachRule ReadCoachRule(LevelFile.CoachRule entry, string field,
            IReadOnlyList<StationDefinition> stations, List<string> problems)
        {
            if (!TryReadEnum(entry.when, out CoachCondition condition))
            {
                problems.Add($"{field}: 'when' should be one of {string.Join(", ", Enum.GetNames(typeof(CoachCondition)).Select(Camel))}, not '{entry.when}'.");
                return null;
            }

            Require(entry.text, $"{field} has no text.", problems);
            CheckTokens(entry.text, CoachRule.Tokens, field, problems);
            if ((condition == CoachCondition.Stops || condition == CoachCondition.OnTime) && entry.count <= 0)
            {
                problems.Add($"{field}: '{Camel(condition.ToString())}' needs a count.");
            }

            if (condition == CoachCondition.DeadEnd && stations.All(station => station.Id != entry.station))
            {
                problems.Add($"{field}: 'deadEnd' needs a station on the map, not '{entry.station}'.");
            }

            return new CoachRule(condition, entry.count, entry.station, entry.text);
        }

        private static List<PassengerVariant> ReadVariants(LevelFile.Variant[] entries, IReadOnlyList<Passenger> passengers,
            IReadOnlyList<StationDefinition> stations, List<string> problems)
        {
            var variants = new List<PassengerVariant>();
            for (int i = 0; i < (entries?.Length ?? 0); i++)
            {
                LevelFile.Variant entry = entries[i];
                string field = $"Variant {i}";
                LevelFile.Condition condition = entry.@if ?? new LevelFile.Condition();
                Require(condition.night, $"{field}: 'if' needs the night it looks back at.", problems);
                Require(condition.passenger, $"{field}: 'if' needs a passenger.", problems);
                if (!TryReadEnum(condition.status, out StatusMatch status))
                {
                    problems.Add($"{field}: 'if' status should be onTime, late, notReached or missed, not '{condition.status}'.");
                }

                if (passengers.All(passenger => passenger.Name != entry.passenger))
                {
                    problems.Add($"{field} changes '{entry.passenger}', who isn't riding tonight.");
                }

                ClockTime? deadline = null;
                if (!string.IsNullOrWhiteSpace(entry.deadline))
                {
                    deadline = ReadTime(entry.deadline, $"{field}'s deadline", problems);
                }

                if (!string.IsNullOrWhiteSpace(entry.destination) && stations.All(station => station.Id != entry.destination))
                {
                    problems.Add($"{field} sends {entry.passenger} to '{entry.destination}', which is not on the map.");
                }

                variants.Add(new PassengerVariant(condition.night, condition.passenger, status, entry.passenger, deadline,
                    entry.platform, entry.planQuote, entry.ask,
                    ReadPatch(entry.onTime), ReadPatch(entry.late), ReadPatch(entry.notReached),
                    condition.promised, entry.destination));
            }

            return variants;
        }

        private static List<Secret> ReadSecrets(LevelFile.Secret[] entries, IReadOnlyList<StationDefinition> stations,
            List<string> problems)
        {
            var secrets = new List<Secret>();
            for (int i = 0; i < (entries?.Length ?? 0); i++)
            {
                LevelFile.Secret entry = entries[i];
                if (stations.All(station => station.Id != entry.station))
                {
                    problems.Add($"Secret {i}: '{entry.station}' is not on the map.");
                }

                Require(entry.keepsake, $"Secret {i} has no keepsake.", problems);
                secrets.Add(new Secret(entry.station, ReadTime(entry.by, $"Secret {i}'s 'by'", problems), entry.keepsake));
            }

            return secrets;
        }

        private static OutcomePatch ReadPatch(LevelFile.Outcome entry) =>
            entry == null || entry.IsEmpty
                ? null
                : new OutcomePatch(entry.line, entry.logLine, entry.keepsake, entry.aside, entry.carry, entry.weekNote);

        private static ClockTime ReadTime(string text, string field, List<string> problems)
        {
            if (ClockTime.TryParse(text, out ClockTime time))
            {
                return time;
            }

            problems.Add($"{field} '{text}' is not a time like \"00:40\".");
            return default;
        }

        private static LabelSide ReadLabelSide(LevelFile.Station entry, List<string> problems)
        {
            if (string.IsNullOrEmpty(entry.label))
            {
                return LabelSide.Above;
            }

            if (Enum.TryParse(entry.label, ignoreCase: true, out LabelSide side) && Enum.IsDefined(typeof(LabelSide), side))
            {
                return side;
            }

            problems.Add($"Station '{entry.id}': label '{entry.label}' should be above, below, left or right.");
            return LabelSide.Above;
        }

        private static bool TryReadEnum<T>(string text, out T value) where T : struct, Enum =>
            Enum.TryParse(text, ignoreCase: true, out value) && Enum.IsDefined(typeof(T), value)
                                                              && !int.TryParse(text, out _);

        private static void Require(string text, string problem, List<string> problems)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                problems.Add(problem);
            }
        }

        private static void CheckTokens(string text, IReadOnlyCollection<string> allowed, string field, List<string> problems)
        {
            foreach (string token in TextTemplate.FindUnknownTokens(text, allowed))
            {
                problems.Add($"{field} uses {token}; it can use {string.Join(", ", allowed.Select(t => "{" + t + "}"))}.");
            }
        }

        private static string Camel(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        // Argument exceptions append "(Parameter '...')" on a second line; designers don't need it.
        private static string FirstLine(string message) => message.Split('\n')[0].Trim();
    }
}
