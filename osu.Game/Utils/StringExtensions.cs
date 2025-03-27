// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Utils
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

        public static bool IsSafeForFilename(this string filename, int maxLength = 255)
            => IsSafeForFilename(filename, out _, maxLength);

        /// <summary>
        /// Check if the given string is safe to be used as a filename.
        /// </summary>
        /// <param name="fileName">The specified string as the filename.</param>
        /// <param name="errorMessage">The issue found in the string as a <see cref="LocalisableString"/>.</param>
        /// <param name="maxLength">The maximum length of the string.</param>
        /// <returns>True if safe to use, otherwise false.</returns>
        public static bool IsSafeForFilename(this string fileName, out LocalisableString errorMessage, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                errorMessage = StringCheckStrings.EmptyName;
                return false;
            }

            if (fileName.Length > maxLength)
            {
                errorMessage = StringCheckStrings.MaximumLengthExceeded(maxLength);
                return false;
            }

            Regex invalidCharsPattern = new Regex(@"[\\/:*?""<>|]");

            if (invalidCharsPattern.IsMatch(fileName))
            {
                errorMessage = StringCheckStrings.InvalidCharsFound;
                return false;
            }

            foreach (UnicodeCategory category in fileName.Select(char.GetUnicodeCategory))
            {
                if (category is UnicodeCategory.OtherSymbol or UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
                    or UnicodeCategory.ModifierSymbol or UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark
                    or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.Surrogate)
                {
                    errorMessage = StringCheckStrings.SpecialCharsFound;
                    return false;
                }
            }

            return true;
        }
    }
}
