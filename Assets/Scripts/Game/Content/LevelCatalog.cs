using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>The level files the game plays, in order. Each file's asset name becomes its night id.</summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Last Train Home/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private TextAsset[] _levels = Array.Empty<TextAsset>();

        /// <exception cref="FormatException">A level is missing or can't be played; names the level and every problem.</exception>
        public IReadOnlyList<Level> LoadLevels()
        {
            if (_levels.Length == 0)
            {
                throw new FormatException($"Level catalog '{name}' has no levels.");
            }

            var levels = new Level[_levels.Length];
            for (int i = 0; i < _levels.Length; i++)
            {
                TextAsset file = _levels[i];
                levels[i] = file != null
                    ? LevelParser.Parse(file.text, file.name)
                    : throw new FormatException($"Level catalog '{name}' has an empty entry at {i}.");
            }

            return levels;
        }
    }
}
