// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public static class IPCStatusStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.IPCStatus";

        /// <summary>
        /// "Beatmap"
        /// </summary>
        public static LocalisableString Beatmap => new TranslatableString(getKey(@"beatmap"), @"Beatmap");

        /// <summary>
        /// "Mods"
        /// </summary>
        public static LocalisableString Mods => new TranslatableString(getKey(@"mods"), @"Mods");

        /// <summary>
        /// "State"
        /// </summary>
        public static LocalisableString State => new TranslatableString(getKey(@"state"), @"State");

        /// <summary>
        /// "Score"
        /// </summary>
        public static LocalisableString Score => new TranslatableString(getKey(@"score"), @"Score");

        /// <summary>
        /// "Channel"
        /// </summary>
        public static LocalisableString Channel => new TranslatableString(getKey(@"channel"), @"Channel");

        /// <summary>
        /// "Last update"
        /// </summary>
        public static LocalisableString LastUpdate => new TranslatableString(getKey(@"last_update"), @"Last update");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString None => new TranslatableString(getKey(@"none"), @"None");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString Unknown => new TranslatableString(getKey(@"unknown"), @"Unknown");

        /// <summary>
        /// "{0} ({1} ago)"
        /// </summary>
        public static LocalisableString LastUpdateTime(string time, string elapsed) => new TranslatableString(getKey(@"last_update_time"),
            @"{0} ({1} ago)", time, elapsed);

        /// <summary>
        /// "No IPC data has been received for {0}. Please check that the stable client is running in tournament mode."
        /// </summary>
        public static LocalisableString StaleHint(string elapsed) => new TranslatableString(getKey(@"stale_hint"),
            @"No IPC data has been received for {0}. Please check that the stable client is running in tournament mode.", elapsed);

        /// <summary>
        /// "{0} [{1}] ({2})"
        /// </summary>
        public static LocalisableString BeatmapValue(string title, string difficulty, int onlineID) => new TranslatableString(getKey(@"beatmap_value"),
            @"{0} [{1}] ({2})", title, difficulty, onlineID);

        /// <summary>
        /// "Red {0} - Blue {1}"
        /// </summary>
        public static LocalisableString ScoreValue(long score1, long score2) => new TranslatableString(getKey(@"score_value"),
            @"Red {0} - Blue {1}", score1, score2);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
