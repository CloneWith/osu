// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class StringCheckStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.StringCheck";

        /// <summary>
        /// "Empty name."
        /// </summary>
        public static LocalisableString EmptyName => new TranslatableString(getKey(@"empty_name"), @"Empty name.");

        /// <summary>
        /// "Total length of the string is too long (maximum {0})."
        /// </summary>
        public static LocalisableString MaximumLengthExceeded(int maximumLength) => new TranslatableString(getKey(@"maximum_length_exceeded"),
            @"Total length of the string is too long (maximum {0}).", maximumLength);

        /// <summary>
        /// "The name contains invalid characters for the filesystem. Please remove them."
        /// </summary>
        public static LocalisableString InvalidCharsFound => new TranslatableString(getKey(@"invalid_chars_found"),
            @"The name contains invalid characters for the filesystem. Please remove them.");

        /// <summary>
        /// "The name contains special characters (etc. control characters and emojis). Please remove them."
        /// </summary>
        public static LocalisableString SpecialCharsFound => new TranslatableString(getKey(@"special_chars_found"),
            @"The name contains special characters (etc. control characters and emojis). Please remove them.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

