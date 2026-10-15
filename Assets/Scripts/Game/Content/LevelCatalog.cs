using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// The game's content files: the levels it plays, in order (each file's asset name becomes its night id), the week
    /// they belong to, and the interface wording. Loading checks every file and how they fit together.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Last Train Home/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private TextAsset[] _levels = Array.Empty<TextAsset>();
        [SerializeField, Tooltip("Data/Story/Week.json")] private TextAsset _week;
        [SerializeField, Tooltip("Data/Text/UiText.json")] private TextAsset _uiText;

        /// <exception cref="FormatException">A file is missing or can't be used; names it and every problem.</exception>
        public GameContent LoadContent()
        {
            if (_week == null || _uiText == null)
            {
                throw new FormatException($"Level catalog '{name}' needs its week and UI text files.");
            }

            Week week = WeekParser.Parse(_week.text, _week.name);
            UiText ui = UiText.Parse(_uiText.text, _uiText.name);
            IReadOnlyList<Level> levels = LoadLevels();
            string[] problems = ContentChecks.FindProblems(week, levels).ToArray();
            if (problems.Length > 0)
            {
                throw new FormatException($"The content doesn't fit together:\n- {string.Join("\n- ", problems)}");
            }

            return new GameContent(week, ui, levels);
        }

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
