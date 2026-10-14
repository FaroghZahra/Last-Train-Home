using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Session;

namespace LastTrainHome.Domain.Progress
{
    /// <summary>
    /// The nights in play order, which one is current, and how each finished night went.
    /// Results are kept so later nights can react to earlier ones (carry-over).
    /// </summary>
    public sealed class Campaign
    {
        private readonly IReadOnlyList<string> _nightIds;
        private readonly Dictionary<string, NightResult> _results = new Dictionary<string, NightResult>(StringComparer.Ordinal);

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
    }
}
