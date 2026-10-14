using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain;
using UnityEngine;

namespace LastTrainHome.Data
{
    /// <summary>
    /// Everything that makes one night different: the map, where the train starts, when the line closes, who rides.
    /// Read from a level file in Assets/Data/Levels (see <see cref="LevelFile"/> for the format).
    /// </summary>
    public sealed class NightDefinition
    {
        private NightDefinition(string title, string startStationId, ClockTime departure, ClockTime closing,
            StationDefinition[] stations, Track[] tracks, PassengerDefinition[] passengers, RailNetwork network)
        {
            Title = title;
            StartStationId = startStationId;
            Departure = departure;
            Closing = closing;
            Stations = stations;
            Tracks = tracks;
            Passengers = passengers;
            Network = network;
        }

        public string Title { get; }

        public string StartStationId { get; }

        public ClockTime Departure { get; }

        public ClockTime Closing { get; }

        public IReadOnlyList<StationDefinition> Stations { get; }

        public IReadOnlyList<Track> Tracks { get; }

        public IReadOnlyList<PassengerDefinition> Passengers { get; }

        public RailNetwork Network { get; }

        /// <summary>Reads and checks a level file. The result can always be played.</summary>
        /// <param name="sourceName">Names the file in error messages.</param>
        /// <exception cref="FormatException">The file is not a playable level; the message lists every problem.</exception>
        public static NightDefinition FromJson(string json, string sourceName)
        {
            LevelFile file;
            try
            {
                file = JsonUtility.FromJson<LevelFile>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException($"Level '{sourceName}' is not valid JSON: {exception.Message}");
            }

            if (file == null)
            {
                throw new FormatException($"Level '{sourceName}' is empty.");
            }

            var problems = new List<string>();
            NightDefinition night = Read(file, problems);
            if (problems.Count > 0)
            {
                throw new FormatException($"Level '{sourceName}' cannot be played:\n- {string.Join("\n- ", problems)}");
            }

            return night;
        }

        private static NightDefinition Read(LevelFile file, List<string> problems)
        {
            ClockTime departure = ReadTime(file.departure, "Departure", problems);
            ClockTime closing = ReadTime(file.closing, "Closing", problems);
            if (closing < departure)
            {
                problems.Add("The line closes before the train departs.");
            }

            StationDefinition[] stations = (file.stations ?? Array.Empty<StationEntry>())
                .Select(entry => new StationDefinition(entry.id, entry.displayName, new Vector2(entry.x, entry.y),
                    ReadLabelSide(entry, problems)))
                .ToArray();

            var tracks = new List<Track>();
            foreach (TrackEntry entry in file.tracks ?? Array.Empty<TrackEntry>())
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

            RailNetwork network = null;
            try
            {
                network = new RailNetwork(stations.Select(station => station.Id), tracks);
            }
            catch (ArgumentException exception)
            {
                problems.Add(FirstLine(exception.Message));
            }

            if (network != null && !network.ContainsStation(file.startStation))
            {
                problems.Add($"Start station '{file.startStation}' is not on the map.");
            }

            PassengerDefinition[] passengers = ReadPassengers(file.passengers, network, problems);
            return new NightDefinition(file.title, file.startStation, departure, closing,
                stations, tracks.ToArray(), passengers, network);
        }

        private static PassengerDefinition[] ReadPassengers(PassengerEntry[] entries, RailNetwork network, List<string> problems)
        {
            if (entries == null || entries.Length == 0)
            {
                problems.Add("There are no passengers.");
                return Array.Empty<PassengerDefinition>();
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            var passengers = new PassengerDefinition[entries.Length];
            for (int i = 0; i < entries.Length; i++)
            {
                PassengerEntry entry = entries[i];
                if (string.IsNullOrWhiteSpace(entry.name))
                {
                    problems.Add($"Passenger {i} has no name.");
                }
                else if (!names.Add(entry.name))
                {
                    problems.Add($"Two passengers are called '{entry.name}'.");
                }

                if (network != null && !network.ContainsStation(entry.destination))
                {
                    problems.Add($"{entry.name}: destination '{entry.destination}' is not on the map.");
                }

                ClockTime deadline = ReadTime(entry.deadline, $"{entry.name}'s deadline", problems);
                passengers[i] = new PassengerDefinition(entry.name, entry.destination, deadline, entry.motivation,
                    entry.onTime, entry.late, entry.notReached);
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

        private static LabelSide ReadLabelSide(StationEntry entry, List<string> problems)
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

        // Exception messages from argument checks end with "(Parameter '...')" on a new line.
        private static string FirstLine(string message) => message.Split('\n')[0].Trim();
    }
}
