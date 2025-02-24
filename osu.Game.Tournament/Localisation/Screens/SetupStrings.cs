// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class SetupStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.Setup";

        /// <summary>
        /// "General"
        /// </summary>
        public static LocalisableString GeneralHeader => new TranslatableString(getKey(@"general_header"), @"General");

        /// <summary>
        /// "Not found"
        /// </summary>
        public static LocalisableString NotFound => new TranslatableString(getKey(@"not_found"), @"Not found");

        /// <summary>
        /// "Current IPC source"
        /// </summary>
        public static LocalisableString CurrentIPCSource => new TranslatableString(getKey(@"current_ipc_source"), @"Current IPC source");

        /// <summary>
        /// "Change..."
        /// </summary>
        public static LocalisableString Change => new TranslatableString(getKey(@"change"), @"Change...");

        /// <summary>
        /// "The osu!stable installation which is currently being used as a data source. If a source is not found, make sure you have created an empty ipc.txt in your stable cutting-edge installation."
        /// </summary>
        public static LocalisableString IPCSourceDescription => new TranslatableString(getKey(@"ipc_source_description"),
            @"The osu!stable installation which is currently being used as a data source. If a source is not found, make sure you have created an empty ipc.txt in your stable cutting-edge installation.");

        /// <summary>
        /// "Current user"
        /// </summary>
        public static LocalisableString CurrentUser => new TranslatableString(getKey(@"current_user"), @"Current user");

        /// <summary>
        /// "Show profile"
        /// </summary>
        public static LocalisableString ShowProfile => new TranslatableString(getKey(@"show_profile"), @"Show profile");

        /// <summary>
        /// "In order to access the API and display metadata, signing in is required."
        /// </summary>
        public static LocalisableString CurrentUserDescription => new TranslatableString(getKey(@"current_user_description"),
            @"In order to access the API and display metadata, signing in is required.");

        /// <summary>
        /// "Current tournament"
        /// </summary>
        public static LocalisableString CurrentTournament => new TranslatableString(getKey(@"current_tournament"), @"Current tournament");

        /// <summary>
        /// "Changes the background videos and bracket to match the selected tournament. This requires a restart to apply changes."
        /// </summary>
        public static LocalisableString CurrentTournamentDescription => new TranslatableString(getKey(@"current_tournament_description"),
            @"Changes the background videos and bracket to match the selected tournament. This requires a restart to apply changes.");

        /// <summary>
        /// "Stream area resolution"
        /// </summary>
        public static LocalisableString Resolution => new TranslatableString(getKey(@"resolution"), @"Stream area resolution");

        /// <summary>
        /// "Set height"
        /// </summary>
        public static LocalisableString SetResolution => new TranslatableString(getKey(@"set_resolution"), @"Set height");

        /// <summary>
        /// "Show time in UTC"
        /// </summary>
        public static LocalisableString ShowGlobalTime => new TranslatableString(getKey(@"show_global_time"), @"Show time in UTC");

        /// <summary>
        /// "Show Coordinated Universal Time instead of local time for schedules."
        /// </summary>
        public static LocalisableString ShowGlobalTimeDescription => new TranslatableString(getKey(@"show_global_time_description"),
            @"Show Coordinated Universal Time instead of local time for schedules.");

        /// <summary>
        /// "Tournament Specific"
        /// </summary>
        public static LocalisableString TournamentSpecificHeader => new TranslatableString(getKey(@"tournament_specific_header"), @"Tournament Specific");

        /// <summary>
        /// "Ruleset"
        /// </summary>
        public static LocalisableString Ruleset => new TranslatableString(getKey(@"ruleset"), @"Ruleset");

        /// <summary>
        /// "Decides what stats are displayed and which ranks are retrieved for players. This requires a restart to reload data for an existing bracket."
        /// </summary>
        public static LocalisableString RulesetDescription => new TranslatableString(getKey(@"ruleset_description"),
            @"Decides what stats are displayed and which ranks are retrieved for players. This requires a restart to reload data for an existing bracket.");

        /// <summary>
        /// "Background settings"
        /// </summary>
        public static LocalisableString BackgroundSettings => new TranslatableString(getKey(@"background_settings"), @"Background settings");

        /// <summary>
        /// "Set paths and behaviour of background display."
        /// </summary>
        public static LocalisableString BackgroundSettingsDescription => new TranslatableString(getKey(@"background_settings_description"),
            @"Set paths and behaviour of background display.");

        /// <summary>
        /// "Display team seeds"
        /// </summary>
        public static LocalisableString DisplaySeeds => new TranslatableString(getKey(@"display_seeds"), @"Display team seeds");

        /// <summary>
        /// "Team seeds will display alongside each team at the top in gameplay/map pool screens."
        /// </summary>
        public static LocalisableString DisplaySeedsDescription => new TranslatableString(getKey(@"display_seeds_description"),
            @"Team seeds will display alongside each team at the top in gameplay/map pool screens.");

        /// <summary>
        /// "Automation"
        /// </summary>
        public static LocalisableString AutomationHeader => new TranslatableString(getKey(@"automation_header"), @"Automation");

        /// <summary>
        /// "Auto advance screens"
        /// </summary>
        public static LocalisableString AutoAdvance => new TranslatableString(getKey(@"auto_advance"), @"Auto advance screens");

        /// <summary>
        /// "Screens will progress automatically from gameplay -> results -> map pool"
        /// </summary>
        public static LocalisableString AutoAdvanceDescription => new TranslatableString(getKey(@"auto_advance_description"),
            @"Screens will progress automatically from gameplay -> results -> map pool");

        /// <summary>
        /// "Open folder"
        /// </summary>
        public static LocalisableString OpenFolder => new TranslatableString(getKey(@"open_folder"), @"Open folder");

        /// <summary>
        /// "Close osu!"
        /// </summary>
        public static LocalisableString CloseOsu => new TranslatableString(getKey(@"close_osu"), @"Close osu!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
