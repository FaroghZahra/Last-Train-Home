using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Session;

namespace LastTrainHome.Domain.Progress
{
    /// <summary>
    /// The week so far: the nights in play order, which one is current, how each finished night went, whose big
    /// nights the player has learned about, and the keepsakes left in the cab. Later nights react to all of it.
    /// </summary>
    public sealed class Campaign
    {
        private readonly IReadOnlyList<string> _nightIds;
        private readonly Dictionary<string, NightResult> _results = new Dictionary<string, NightResult>(StringComparer.Ordinal);
        private readonly HashSet<string> _revealedBigNights = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> _keepsakes = new List<string>();
        private readonly Dictionary<string, string> _weekNotes = new Dictionary<string, string>(StringComparer.Ordinal);

        public Campaign(IReadOnlyList<string> nightIds)
        {
            if (nightIds == null || nightIds.Count == 0)
            {
                throw new ArgumentException("A campaign needs at least one night.", nameof(nightIds));
            }

            if (new HashSet<string>(nightIds, StringComparer.Ordinal).Count != nightIds.Count)
            {
                throw new ArgumentException("Night ids must be unique.", nameof(nightIds));
            }

            _nightIds = nightIds;
        }

        public int CurrentIndex { get; private set; }

        public string CurrentNightId => _nightIds[CurrentIndex];

        public bool HasNextNight => CurrentIndex + 1 < _nightIds.Count;

        public int NightCount => _nightIds.Count;

        public IReadOnlyList<string> NightIds => _nightIds;

        /// <summary>Keepsake ids in the order they were left, each once.</summary>
        public IReadOnlyList<string> Keepsakes => _keepsakes;

        /// <exception cref="InvalidOperationException">This is the last night.</exception>
        public void AdvanceToNextNight()
        {
            if (!HasNextNight)
            {
                throw new InvalidOperationException("There is no night after the last one.");
            }

            CurrentIndex++;
        }

        /// <summary>Keeps the latest result of the current night; playing it again replaces the result.</summary>
        /// <exception cref="ArgumentException">The result is for a different night.</exception>
        public void Record(NightResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!string.Equals(result.NightId, CurrentNightId, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Expected a result for '{CurrentNightId}', got '{result.NightId}'.", nameof(result));
            }

            _results[result.NightId] = result;
        }

        public bool TryGetResult(string nightId, out NightResult result) => _results.TryGetValue(nightId, out result);

        /// <summary>The player now knows which night this week is this passenger's.</summary>
        public void RevealBigNight(string passengerName)
        {
            if (string.IsNullOrWhiteSpace(passengerName))
            {
                throw new ArgumentException("Passenger name is required.", nameof(passengerName));
            }

            _revealedBigNights.Add(passengerName);
        }

        public bool IsBigNightRevealed(string passengerName) =>
            passengerName != null && _revealedBigNights.Contains(passengerName);

        /// <summary>Notes how a passenger's big night is shaping up, e.g. "Scarf: red."; replaces any earlier note.</summary>
        public void SetWeekNote(string passengerName, string note)
        {
            if (string.IsNullOrWhiteSpace(passengerName))
            {
                throw new ArgumentException("Passenger name is required.", nameof(passengerName));
            }

            _weekNotes[passengerName] = note ?? string.Empty;
        }

        /// <summary>The latest note on a passenger's big night, or empty.</summary>
        public string GetWeekNote(string passengerName) =>
            passengerName != null && _weekNotes.TryGetValue(passengerName, out string note) ? note : string.Empty;

        /// <summary>Leaves a keepsake in the cab. Leaving the same one again changes nothing.</summary>
        public void AddKeepsake(string keepsakeId)
        {
            if (string.IsNullOrWhiteSpace(keepsakeId))
            {
                throw new ArgumentException("Keepsake id is required.", nameof(keepsakeId));
            }

            if (!_keepsakes.Contains(keepsakeId))
            {
                _keepsakes.Add(keepsakeId);
            }
        }
    }
}
