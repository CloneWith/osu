// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class BaseStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Base";

        /// <summary>
        /// "OFFC Tournament Client"
        /// </summary>
        public static LocalisableString ClientName => new TranslatableString(getKey(@"client_name"), @"OFFC Tournament Client");

        /// <summary>
        /// "Populating user stats"
        /// </summary>
        public static LocalisableString PopulatingUserStats => new TranslatableString(getKey(@"populating_user_stats"),
            @"Populating user stats");

        /// <summary>
        /// "Populating round beatmaps"
        /// </summary>
        public static LocalisableString PopulatingRoundBeatmaps => new TranslatableString(getKey(@"populating_round_beatmaps"),
            @"Populating round beatmaps");

        /// <summary>
        /// "Populating seeding beatmaps"
        /// </summary>
        public static LocalisableString PopulatingSeedingBeatmaps => new TranslatableString(getKey(@"populating_seeding_beatmaps"),
            @"Populating seeding beatmaps");

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
        /// "Please make the window wider for better control."
        /// </summary>
        public static LocalisableString AspectRatioWarning => new TranslatableString(getKey(@"aspect_ratio_warning"), @"Please make the window wider for better control.");

        /// <summary>
        /// "Control Panel"
        /// </summary>
        public static LocalisableString ControlPanel => new TranslatableString(getKey(@"control_panel"), @"Control Panel");

        /// <summary>
        /// "Save Changes"
        /// </summary>
        public static LocalisableString SaveChanges => new TranslatableString(getKey(@"save_changes"), @"Save Changes");

        /// <summary>
        /// "Fetch Data"
        /// </summary>
        public static LocalisableString FetchData => new TranslatableString(getKey(@"fetch_data"), @"Fetch Data");

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

        /// <summary>
        /// "Unknown Round"
        /// </summary>
        public static LocalisableString UnknownRound => new TranslatableString(getKey(@"unknown_round"), @"Unknown Round");

        /// <summary>
        /// "Task completed!"
        /// </summary>
        public static LocalisableString TaskCompleted => new TranslatableString(getKey(@"task_completed"), @"Task completed!");

        /// <summary>
        /// "This task has failed."
        /// </summary>
        public static LocalisableString TaskFailed => new TranslatableString(getKey(@"task_failed"), @"This task has failed.");

        /// <summary>
        /// "Please wait..."
        /// </summary>
        public static LocalisableString PleaseWait => new TranslatableString(getKey(@"please_wait"), @"Please wait...");

        /// <summary>
        /// "Fetching..."
        /// </summary>
        public static LocalisableString FetchingHeader => new TranslatableString(getKey(@"fetching_header"), @"Fetching...");

        /// <summary>
        /// "Fetching data from server, please wait..."
        /// </summary>
        public static LocalisableString FetchingDescription => new TranslatableString(getKey(@"fetching_description"), @"Fetching data from server, please wait...");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
