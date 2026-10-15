using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// The interface's wording from Data/Text/UiText.json: button labels, status chips and templates like
    /// "{names} will be late". Story text lives in the level and week files; this is everything around it.
    /// </summary>
    public sealed class UiText
    {
        private readonly IReadOnlyDictionary<string, string> _texts;

        private UiText(IReadOnlyDictionary<string, string> texts) => _texts = texts;

        /// <summary>Uses the given texts as they are, for tests. Missing keys read as the key itself.</summary>
        public static UiText FromDictionary(IReadOnlyDictionary<string, string> texts) =>
            new UiText(texts ?? throw new ArgumentNullException(nameof(texts)));

        /// <exception cref="FormatException">A key is missing, unknown or doubled, or uses a token it can't.</exception>
        public static UiText Parse(string json, string fileName)
        {
            File file;
            try
            {
                file = JsonUtility.FromJson<File>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException($"'{fileName}' is not valid JSON: {exception.Message}");
            }

            var problems = new List<string>();
            var texts = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (Entry entry in file?.entries ?? Array.Empty<Entry>())
            {
                if (!UiKey.Tokens.TryGetValue(entry.key ?? string.Empty, out string[] tokens))
                {
                    problems.Add($"'{entry.key}' isn't a key the game uses.");
                    continue;
                }

                if (!texts.TryAdd(entry.key, entry.text ?? string.Empty))
                {
                    problems.Add($"'{entry.key}' is listed twice.");
                }

                foreach (string token in TextTemplate.FindUnknownTokens(entry.text, tokens))
                {
                    string allowed = tokens.Length == 0 ? "none" : string.Join(", ", tokens.Select(t => "{" + t + "}"));
                    problems.Add($"'{entry.key}' uses {token}; it can use {allowed}.");
                }
            }

            problems.AddRange(UiKey.Tokens.Keys.Where(key => !texts.ContainsKey(key)).Select(key => $"'{key}' is missing."));
            if (problems.Count > 0)
            {
                throw new FormatException($"'{fileName}' cannot be used:\n- {string.Join("\n- ", problems)}");
            }

            return new UiText(texts);
        }

        public string Get(string key) => _texts.TryGetValue(key, out string text) ? text : key;

        public string Format(string key, params (string Token, string Value)[] values) => TextTemplate.Fill(Get(key), values);

        /// <summary>"Agnes", "Agnes and Wen", "Agnes, Priya and Wen".</summary>
        public string JoinNames(IReadOnlyList<string> names)
        {
            if (names == null || names.Count == 0)
            {
                return string.Empty;
            }

            return names.Count == 1
                ? names[0]
                : string.Join(Get(UiKey.ListComma), names.Take(names.Count - 1)) + Get(UiKey.ListAnd) + names[names.Count - 1];
        }

        [Serializable]
        private sealed class File
        {
            public Entry[] entries;
        }

        [Serializable]
        private sealed class Entry
        {
            public string key;
            public string text;
        }
    }
}
