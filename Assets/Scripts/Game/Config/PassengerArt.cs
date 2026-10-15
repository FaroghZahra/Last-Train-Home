using System;
using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>
    /// How each passenger looks: a figure on the map, a portrait on their card and a colour that marks their card and
    /// their destination. Matched by name.
    /// </summary>
    [CreateAssetMenu(fileName = "PassengerArt", menuName = "Last Train Home/Passenger Art")]
    public sealed class PassengerArt : ScriptableObject
    {
        [SerializeField] private Entry[] _passengers = Array.Empty<Entry>();

        [SerializeField, Tooltip("For a passenger with no entry.")]
        private Color _defaultColor = new Color(0.6f, 0.58f, 0.55f);

        /// <summary>The standing figure, or null when the passenger has none.</summary>
        public Sprite GetFigure(string passengerName) => Find(passengerName)?.Figure;

        /// <summary>The card portrait, or null when the passenger has none.</summary>
        public Sprite GetPortrait(string passengerName) => Find(passengerName)?.Portrait;

        public Color GetColor(string passengerName) => Find(passengerName)?.Color ?? _defaultColor;

        private Entry Find(string passengerName)
        {
            foreach (Entry entry in _passengers)
            {
                if (string.Equals(entry.Name, passengerName, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        [Serializable]
        private sealed class Entry
        {
            [Tooltip("Matches the passenger's name in the level files.")]
            public string Name;

            public Sprite Figure;

            public Sprite Portrait;

            public Color Color = Color.gray;
        }
    }
}
