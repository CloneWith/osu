// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class SeedingScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.SeedingScreen";

        /// <summary>
        /// "Show first team"
        /// </summary>
        public static LocalisableString ShowFirstTeam => new TranslatableString(getKey(@"show_first_team"), @"Show first team");

        /// <summary>
        /// "Show second team"
        /// </summary>
        public static LocalisableString ShowSecondTeam => new TranslatableString(getKey(@"show_second_team"), @"Show second team");

        /// <summary>
        /// "Show specific team"
        /// </summary>
        public static LocalisableString ShowSpecificTeam => new TranslatableString(getKey(@"show_specific_team"), @"Show specific team");

        /// <summary>
        /// "by"
        /// </summary>
        public static LocalisableString By => new TranslatableString(getKey(@"by"), @"by");

        /// <summary>
        /// "Average Rank:"
        /// </summary>
        public static LocalisableString AverageRank => new TranslatableString(getKey(@"average_rank"), @"Average Rank:");

        /// <summary>
        /// "Seed:"
        /// </summary>
        public static LocalisableString Seed => new TranslatableString(getKey(@"seed"), @"Seed:");

        /// <summary>
        /// "Last year's placing:"
        /// </summary>
        public static LocalisableString LastYearsPlacing => new TranslatableString(getKey(@"last_years_placing"), @"Last year's placing:");

        /// <summary>
        /// "N/A"
        /// </summary>
        public static LocalisableString NotAvailable => new TranslatableString(getKey(@"not_available"), @"N/A");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
