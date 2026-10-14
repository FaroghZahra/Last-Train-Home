using System;
using System.Collections.Generic;
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
            List<Passenger> passengers = ReadPassengers(file.passengers, problems);

            if (problems.Count > 0)
            {
                throw new FormatException($"Level '{levelId}' cannot be played:\n- {string.Join("\n- ", problems)}");
            }

            try
            {
                var night = new NightDefinition(levelId, file.title, file.startStation, departure, closing,
                    stations, tracks, passengers);
                return new Level(night, placements);
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
                stations.Add(new StationDefinition(entry.id, entry.displayName));
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
                    tracks.Add(new Track(entry.from, entry.to, entry.minutes));
                }
                catch (ArgumentException exception)
                {
                    problems.Add($"Track '{entry.from}' - '{entry.to}': {FirstLine(exception.Message)}");
                }
            }

            return tracks;
        }

        private static List<Passenger> ReadPassengers(LevelFile.Passenger[] entries, List<string> problems)
        {
            var passengers = new List<Passenger>();
            for (int i = 0; i < (entries?.Length ?? 0); i++)
            {
                LevelFile.Passenger entry = entries[i];
                ClockTime deadline = ReadTime(entry.deadline, $"{entry.name}'s deadline", problems);
                var story = new PassengerStory(entry.motivation, entry.onTime, entry.late, entry.notReached);
                try
                {
                    passengers.Add(new Passenger(entry.name, entry.destination, deadline, story));
                }
                catch (ArgumentException exception)
                {
                    problems.Add($"Passenger {i}: {FirstLine(exception.Message)}");
                }
            }

            return passengers;
        }

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

        // Argument exceptions append "(Parameter '...')" on a second line; designers don't need it.
        private static string FirstLine(string message) => message.Split('\n')[0].Trim();
    }
}
