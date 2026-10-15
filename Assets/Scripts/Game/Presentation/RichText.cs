using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>Small TextMeshPro rich-text helpers for joining content lines.</summary>
    public static class RichText
    {
        public static string Colored(string text, Color color) =>
            string.IsNullOrEmpty(text) ? string.Empty : $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

        /// <summary>Paragraphs one under another, with <paramref name="emphasis"/> brighter at the end of the last.</summary>
        public static string Paragraphs(IReadOnlyList<string> lines, string emphasis, Color emphasisColor, bool emphasisOnItsOwnLine)
        {
            string body = string.Join("\n", lines ?? new string[0]);
            if (string.IsNullOrEmpty(emphasis))
            {
                return body;
            }

            string separator = body.Length == 0 ? string.Empty : emphasisOnItsOwnLine ? "\n" : " ";
            return body + separator + $"<b>{Colored(emphasis, emphasisColor)}</b>";
        }
    }
}
