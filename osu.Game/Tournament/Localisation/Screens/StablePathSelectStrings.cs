// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public static class StablePathSelectStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.StablePathSelect";

        /// <summary>
        /// "Please select a new location"
        /// </summary>
        public static LocalisableString PathSelectTitle => new TranslatableString(getKey(@"path_select_title"), @"Please select a new location");

        /// <summary>
        /// "Select stable path"
        /// </summary>
        public static LocalisableString SelectStablePath => new TranslatableString(getKey(@"select_stable_path"), @"Select stable path");

        /// <summary>
        /// "Auto detect"
        /// </summary>
        public static LocalisableString AutoDetect => new TranslatableString(getKey(@"auto_detect"), @"Auto detect");

        /// <summary>
        /// "This is an invalid IPC Directory"
        /// </summary>
        public static LocalisableString InvalidDirectoryTitle => new TranslatableString(getKey(@"invalid_directory_title"), @"This is an invalid IPC Directory");

        /// <summary>
        /// "Select a directory that contains an osu! stable cutting edge installation and make sure it has an empty ipc.txt file in it."
        /// </summary>
        public static LocalisableString InvalidDirectoryText => new TranslatableString(getKey(@"invalid_directory_text"),
            @"Select a directory that contains an osu! stable cutting edge installation and make sure it has an empty ipc.txt file in it.");

        /// <summary>
        /// "Failed to automatically detect"
        /// </summary>
        public static LocalisableString DetectFailureTitle => new TranslatableString(getKey(@"detect_failure_title"), @"Failed to automatically detect");

        /// <summary>
        /// "An osu! stable cutting-edge installation could not be detected. Please try and manually point to the directory."
        /// </summary>
        public static LocalisableString DetectFailureText => new TranslatableString(getKey(@"detect_failure_text"),
            @"An osu! stable cutting-edge installation could not be detected. Please try and manually point to the directory.");

        /// <summary>
        /// "Status"
        /// </summary>
        public static LocalisableString StatusHeader => new TranslatableString(getKey(@"status_header"), @"Status");

        /// <summary>
        /// "The selected directory is not an osu! stable installation (osu!.cfg or osu.exe is missing)."
        /// </summary>
        public static LocalisableString NotStableInstall => new TranslatableString(getKey(@"not_stable_install"),
            @"The selected directory is not an osu! stable installation (osu!.cfg or osu.exe is missing).");

        /// <summary>
        /// "The selected installation is on the "{0}" update stream, but tournament mode requires CuttingEdge."
        /// </summary>
        public static LocalisableString NotCuttingEdge(LocalisableString stream) => new TranslatableString(getKey(@"not_cutting_edge"),
            @"The selected installation is on the ""{0}"" update stream, but tournament mode requires CuttingEdge.", stream);

        /// <summary>
        /// "The selected installation is not set up for tournament mode (tournament.cfg, tournament_accepted or ipc.txt is missing)."
        /// </summary>
        public static LocalisableString NotTournamentConfigured => new TranslatableString(getKey(@"not_tournament_configured"),
            @"The selected installation is not set up for tournament mode (tournament.cfg, tournament_accepted or ipc.txt is missing).");

        /// <summary>
        /// "Resolve the issues above before using this directory."
        /// </summary>
        public static LocalisableString ResolveIssuesFirst => new TranslatableString(getKey(@"resolve_issues_first"),
            @"Resolve the issues above before using this directory.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

