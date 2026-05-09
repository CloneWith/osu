// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FumoColourStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.FumoColour";

        /// <summary>
        /// "Sea Blue"
        /// </summary>
        public static LocalisableString SeaBlue => new TranslatableString(getKey(@"sea_blue"), @"Sea Blue");

        /// <summary>
        /// "Sunshine Yellow"
        /// </summary>
        public static LocalisableString SunshineYellow => new TranslatableString(getKey(@"sunshine_yellow"), @"Sunshine Yellow");

        /// <summary>
        /// "Flandre Red"
        /// </summary>
        public static LocalisableString FlandreRed => new TranslatableString(getKey(@"flandre_red"), @"Flandre Red");

        /// <summary>
        /// "Deep Purple"
        /// </summary>
        public static LocalisableString DeepPurple => new TranslatableString(getKey(@"deep_purple"), @"Deep Purple");

        /// <summary>
        /// "Light Green"
        /// </summary>
        public static LocalisableString LightGreen => new TranslatableString(getKey(@"light_green"), @"Light Green");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
