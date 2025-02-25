// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class RoundEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.RoundEditor";

        /// <summary>
        /// "Round Information"
        /// </summary>
        public static LocalisableString RoundInfoHeader => new TranslatableString(getKey(@"round_info_header"), @"Round Information");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString RoundName => new TranslatableString(getKey(@"round_name"), @"Name");

        /// <summary>
        /// "Description"
        /// </summary>
        public static LocalisableString RoundDescription => new TranslatableString(getKey(@"round_description"), @"Description");

        /// <summary>
        /// "Start Time"
        /// </summary>
        public static LocalisableString StartTime => new TranslatableString(getKey(@"start_time"), @"Start Time");

        /// <summary>
        /// "# of Bans"
        /// </summary>
        public static LocalisableString NumOfBans => new TranslatableString(getKey(@"num_of_bans"), @"# of Bans");

        /// <summary>
        /// "Best of"
        /// </summary>
        public static LocalisableString BestOf => new TranslatableString(getKey(@"best_of"), @"Best of");

        /// <summary>
        /// "Board Mode"
        /// </summary>
        public static LocalisableString BoardMode => new TranslatableString(getKey(@"board_mode"), @"Board Mode");

        /// <summary>
        /// "Delete Round"
        /// </summary>
        public static LocalisableString DeleteRound => new TranslatableString(getKey(@"delete_round"), @"Delete Round");

        /// <summary>
        /// "Add referee"
        /// </summary>
        public static LocalisableString AddReferee => new TranslatableString(getKey(@"add_referee"), @"Add referee");

        /// <summary>
        /// "Referee List"
        /// </summary>
        public static LocalisableString RefereeList => new TranslatableString(getKey(@"referee_list"), @"Referee List");

        /// <summary>
        /// "Delete Referee"
        /// </summary>
        public static LocalisableString DeleteReferee => new TranslatableString(getKey(@"delete_referee"), @"Delete Referee");

        /// <summary>
        /// "Round Beatmaps"
        /// </summary>
        public static LocalisableString RoundBeatmapsHeader => new TranslatableString(getKey(@"round_beatmaps_header"), @"Round Beatmaps");

        /// <summary>
        /// "Mod Index"
        /// </summary>
        public static LocalisableString ModIndex => new TranslatableString(getKey(@"mod_index"), @"Mod Index");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

