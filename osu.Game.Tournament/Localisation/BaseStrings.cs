// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class BaseStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Base";

        /// <summary>
        /// "Populating user stats ({0} / {1})"
        /// </summary>
        public static LocalisableString PopulatingUserStats(int current, int total) => new TranslatableString(getKey(@"populating_user_stats"),
            @"Populating user stats ({0} / {1})", current, total);

        /// <summary>
        /// "Populating round beatmaps ({0} / {1})"
        /// </summary>
        public static LocalisableString PopulatingRoundBeatmaps(int current, int total) => new TranslatableString(getKey(@"populating_round_beatmaps"),
            @"Populating round beatmaps ({0} / {1})", current, total);

        /// <summary>
        /// "Populating seeding beatmaps ({0} / {1})"
        /// </summary>
        public static LocalisableString PopulatingSeedingBeatmaps(int current, int total) => new TranslatableString(getKey(@"populating_seeding_beatmaps"),
            @"Populating seeding beatmaps ({0} / {1})", current, total);

        /// <summary>
        /// "Your {0} file could not be parsed. Please check runtime.log for more details."
        /// </summary>
        public static LocalisableString BracketErrorWarning(string bracketName) => new TranslatableString(getKey(@"bracket_error_warning"),
            @"Your {0} file could not be parsed. Please check runtime.log for more details.", bracketName);

        /// <summary>
        /// "Choose a match first from the brackets screen"
        /// </summary>
        public static LocalisableString NoMatchWarning => new TranslatableString(getKey(@"no_match_warning"),
            @"Choose a match first from the brackets screen");

        /// <summary>
        /// "Cannot access current match, sorry ;w;"
        /// </summary>
        public static LocalisableString MatchUnavailableWarning => new TranslatableString(getKey(@"match_unavailable_warning"),
            @"Cannot access current match, sorry ;w;");

        /// <summary>
        /// "This round isn't set up for board view..."
        /// </summary>
        public static LocalisableString BoardModeUnsetWarning => new TranslatableString(getKey(@"board_mode_unset_warning"),
            @"This round isn't set up for board view...");

        /// <summary>
        /// "Please adjust the aspect ratio. The minimum window width is {0}."
        /// </summary>
        public static LocalisableString AspectRatioWarning(int requiredWidth) => new TranslatableString(getKey(@"aspect_ratio_warning"),
            @"Please adjust the aspect ratio. The minimum window width is {0}.", requiredWidth);

        /// <summary>
        /// "Control Panel"
        /// </summary>
        public static LocalisableString ControlPanel => new TranslatableString(getKey(@"control_panel"), @"Control Panel");

        /// <summary>
        /// "Save Changes"
        /// </summary>
        public static LocalisableString SaveChanges => new TranslatableString(getKey(@"save_changes"), @"Save Changes");

        /// <summary>
        /// "Add New"
        /// </summary>
        public static LocalisableString AddNew => new TranslatableString(getKey(@"add_new"), @"Add New");

        /// <summary>
        /// "Clear All"
        /// </summary>
        public static LocalisableString Clear => new TranslatableString(getKey(@"clear"), @"Clear All");

        /// <summary>
        /// "Refresh"
        /// </summary>
        public static LocalisableString Refresh => new TranslatableString(getKey(@"refresh"), @"Refresh");

        /// <summary>
        /// "Reset"
        /// </summary>
        public static LocalisableString Reset => new TranslatableString(getKey(@"reset"), @"Reset");

        /// <summary>
        /// "Remove"
        /// </summary>
        public static LocalisableString Remove => new TranslatableString(getKey(@"remove"), @"Remove");

        /// <summary>
        /// "User ID"
        /// </summary>
        public static LocalisableString UserID => new TranslatableString(getKey(@"user_id"), @"User ID");

        /// <summary>
        /// "Beatmap ID"
        /// </summary>
        public static LocalisableString BeatmapID => new TranslatableString(getKey(@"beatmap_id"), @"Beatmap ID");

        /// <summary>
        /// "Seed"
        /// </summary>
        public static LocalisableString Seed => new TranslatableString(getKey(@"seed"), @"Seed");

        /// <summary>
        /// "Score"
        /// </summary>
        public static LocalisableString Score => new TranslatableString(getKey(@"score"), @"Score");

        /// <summary>
        /// "Round"
        /// </summary>
        public static LocalisableString Round => new TranslatableString(getKey(@"round"), @"Round");

        /// <summary>
        /// "Team Red"
        /// </summary>
        public static LocalisableString TeamRed => new TranslatableString(getKey(@"team_red"), @"Team Red");

        /// <summary>
        /// "Team Blue"
        /// </summary>
        public static LocalisableString TeamBlue => new TranslatableString(getKey(@"team_blue"), @"Team Blue");

        /// <summary>
        /// "Seeding Mod"
        /// </summary>
        public static LocalisableString BeatmapMod => new TranslatableString(getKey(@"beatmap_mod"), @"Mod");

        /// <summary>
        /// "Add beatmap"
        /// </summary>
        public static LocalisableString AddBeatmap => new TranslatableString(getKey(@"add_beatmap"), @"Add beatmap");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
