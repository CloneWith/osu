// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;

namespace osu.Game.Tournament
{
    /// <summary>
    /// Utilities for string manipulation.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Truncate a string with ellipsis attached.
        /// </summary>
        /// <author>NaughtyChas</author>
        /// <param name="text">The string to be truncated.</param>
        /// <param name="maxLength">The maximum string length including ellipsis.</param>
        /// <returns>A truncated string.</returns>
        /// <example>The function <code>TruncateWithEllipsis("Long string", 11)</code>
        /// will return <code>Long str...</code>
        /// </example>
        public static string TruncateWithEllipsis(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return string.Concat(text.AsSpan(0, maxLength - 3), "...");
        }

        public static string ExtractSongTitleFromMetadata(this string displayTitle)
        {
            string[] songNameList = displayTitle.Split(' ');

            int firstHyphenIndex = 0;

            // Find the first " - " (Hopefully it isn't in the Artists field)
            for (int i = 0; i < songNameList.Length; i++)
            {
                string obj = songNameList.ElementAt(i);

                if (obj != "-") continue;

                firstHyphenIndex = i;
                break;
            }

            var titleList = songNameList.Skip(firstHyphenIndex + 1);

            // Re-construct
            string songName = string.Empty;

            for (int i = 0; i < titleList.Count(); i++)
            {
                songName += titleList.ElementAt(i).Trim();
                if (i != titleList.Count() - 1) songName += ' ';
            }

            return songName;
        }
    }
}
