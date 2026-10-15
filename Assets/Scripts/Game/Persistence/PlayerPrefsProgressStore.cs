using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Game.Flow;
using UnityEngine;

namespace LastTrainHome.Game.Persistence
{
    /// <summary>Keeps the player's keepsakes and endings in PlayerPrefs, so they're still there next time.</summary>
    public sealed class PlayerPrefsProgressStore : IProgressStore
    {
        private const string KeepsakesKey = "progress.keepsakes";
        private const string EndingsKey = "progress.endings";

        private readonly HashSet<string> _keepsakes;
        private readonly HashSet<int> _endings;

        public PlayerPrefsProgressStore()
        {
            _keepsakes = new HashSet<string>(Split(PlayerPrefs.GetString(KeepsakesKey, string.Empty)));
            _endings = new HashSet<int>(Split(PlayerPrefs.GetString(EndingsKey, string.Empty))
                .Select(text => int.TryParse(text, out int kept) ? kept : -1)
                .Where(kept => kept >= 0));
        }

        public IReadOnlyCollection<string> FoundKeepsakes => _keepsakes;

        public IReadOnlyCollection<int> EndingsSeen => _endings;

        public void AddKeepsake(string keepsakeId)
        {
            if (_keepsakes.Add(keepsakeId))
            {
                Save(KeepsakesKey, _keepsakes);
            }
        }

        public void AddEnding(int kept)
        {
            if (_endings.Add(kept))
            {
                Save(EndingsKey, _endings.Select(value => value.ToString()));
            }
        }

        private static IEnumerable<string> Split(string text) =>
            text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        private static void Save(string key, IEnumerable<string> values)
        {
            PlayerPrefs.SetString(key, string.Join("\n", values));
            PlayerPrefs.Save();
        }
    }
}
