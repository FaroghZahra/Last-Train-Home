using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LastTrainHome.Game.Content
{
    /// <summary>
    /// Fills "{token}" placeholders in content text. Content is checked when it loads (<see cref="FindUnknownTokens"/>),
    /// so a missing value at play time is a programming error.
    /// </summary>
    public static class TextTemplate
    {
        private static readonly Regex Token = new Regex(@"\{([A-Za-z]+)\}", RegexOptions.Compiled);

        /// <exception cref="ArgumentException">The template uses a token no value was given for.</exception>
        public static string Fill(string template, params (string Token, string Value)[] values)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            return Token.Replace(template, match =>
            {
                foreach ((string token, string value) in values)
                {
                    if (token == match.Groups[1].Value)
                    {
                        return value ?? string.Empty;
                    }
                }

                throw new ArgumentException($"No value for '{match.Value}' in \"{template}\".", nameof(values));
            });
        }

        /// <summary>The tokens in <paramref name="template"/> that aren't in <paramref name="allowed"/>, e.g. "{nmae}".</summary>
        public static IEnumerable<string> FindUnknownTokens(string template, IReadOnlyCollection<string> allowed) =>
            string.IsNullOrEmpty(template)
                ? Enumerable.Empty<string>()
                : Token.Matches(template).Cast<Match>()
                    .Where(match => !allowed.Contains(match.Groups[1].Value))
                    .Select(match => match.Value);
    }
}
