using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>One level file, read: the night's rules, its story, and how its map is laid out on screen.</summary>
    public sealed class Level
    {
        private readonly IReadOnlyDictionary<string, StationPlacement> _placements;
        private readonly IReadOnlyDictionary<(string From, string To), Vector2[]> _trackBends;

        /// <param name="trackBends">Where each track bends, keyed by its stations in the order the bends run.</param>
        public Level(NightDefinition night, IReadOnlyDictionary<string, StationPlacement> placements,
            IReadOnlyDictionary<(string From, string To), Vector2[]> trackBends, NightStory story = null,
            IReadOnlyList<PassengerVariant> variants = null, IReadOnlyList<Secret> secrets = null)
        {
            Secrets = secrets ?? Array.Empty<Secret>();
            Night = night ?? throw new ArgumentNullException(nameof(night));
            _placements = placements ?? throw new ArgumentNullException(nameof(placements));
            _trackBends = trackBends ?? throw new ArgumentNullException(nameof(trackBends));
            Story = story ?? NightStory.Empty;
            Variants = variants ?? Array.Empty<PassengerVariant>();
        }

        public NightDefinition Night { get; }

        /// <summary>The day this night falls on, e.g. "Monday".</summary>
        public string Day => Night.Title;

        public NightStory Story { get; }

        /// <summary>Keepsakes the train finds by stopping at a station early enough.</summary>
        public IReadOnlyList<Secret> Secrets { get; }

        /// <summary>Carry-over changes, applied in order by <see cref="WithCarryOver"/>.</summary>
        public IReadOnlyList<PassengerVariant> Variants { get; }

        /// <summary>
        /// This level as it plays after the earlier nights: every variant whose condition matches is applied. The
        /// lookup gives a passenger's outcome on an earlier night, or null when they didn't ride or it wasn't played;
        /// <paramref name="wasPromised"/> says whether the player promised them that night (never, when left out).
        /// </summary>
        public Level WithCarryOver(Func<string, string, ArrivalStatus?> earlierStatus,
            Func<string, string, bool> wasPromised = null)
        {
            if (earlierStatus == null)
            {
                throw new ArgumentNullException(nameof(earlierStatus));
            }

            Passenger[] passengers = Night.Passengers.ToArray();
            bool changed = false;
            foreach (PassengerVariant variant in Variants)
            {
                ArrivalStatus? status = earlierStatus(variant.IfNightId, variant.IfPassenger);
                if (status == null || !variant.Matches(status.Value)
                    || (variant.IfPromised && (wasPromised == null || !wasPromised(variant.IfNightId, variant.IfPassenger))))
                {
                    continue;
                }

                int index = Array.FindIndex(passengers, passenger => passenger.Name == variant.PassengerName);
                passengers[index] = variant.ApplyTo(passengers[index]);
                changed = true;
            }

            if (!changed)
            {
                return this;
            }

            var night = new NightDefinition(Night.Id, Night.Title, Night.StartStationId, Night.Departure, Night.Closing,
                Night.Stations, Night.Network.Tracks, passengers);
            return new Level(night, _placements, _trackBends, Story, Variants, Secrets);
        }

        /// <exception cref="KeyNotFoundException">The station isn't on this level's map.</exception>
        public StationPlacement GetPlacement(string stationId) =>
            _placements.TryGetValue(stationId, out StationPlacement placement)
                ? placement
                : throw new KeyNotFoundException($"Level '{Night.Id}' has no placement for '{stationId}'.");

        /// <summary>Where the track between two stations bends, in order from <paramref name="from"/>; empty when straight.</summary>
        public IReadOnlyList<Vector2> GetTrackBends(string from, string to)
        {
            if (_trackBends.TryGetValue((from, to), out Vector2[] bends))
            {
                return bends;
            }

            if (_trackBends.TryGetValue((to, from), out bends))
            {
                var reversed = (Vector2[])bends.Clone();
                Array.Reverse(reversed);
                return reversed;
            }

            return Array.Empty<Vector2>();
        }
    }
}
