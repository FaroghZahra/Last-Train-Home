using System;
using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>The picture of each keepsake in the cab, matched by the keepsake ids in Data/Story/Week.json.</summary>
    [CreateAssetMenu(fileName = "KeepsakeArt", menuName = "Last Train Home/Keepsake Art")]
    public sealed class KeepsakeArt : ScriptableObject
    {
        [SerializeField] private Entry[] _keepsakes = Array.Empty<Entry>();

        /// <summary>The keepsake's picture, or null when it has none yet (the cab shows its name instead).</summary>
        public Sprite Get(string keepsakeId)
        {
            foreach (Entry entry in _keepsakes)
            {
                if (string.Equals(entry.Id, keepsakeId, StringComparison.Ordinal))
                {
                    return entry.Sprite;
                }
            }

            return null;
        }

        [Serializable]
        private sealed class Entry
        {
            [Tooltip("Matches a keepsake id in Data/Story/Week.json.")]
            public string Id;

            public Sprite Sprite;
        }
    }
}
