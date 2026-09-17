// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public static class DraftStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.Draft";

        /// <summary>
        /// "No {0} file found. Create it with one user ID per line, then press the fetch players button."
        /// </summary>
        public static LocalisableString PlayerListMissingWarning(string filename) => new TranslatableString(getKey(@"player_list_missing_warning"),
            @"No {0} file found. Create it with one user ID per line, then press the fetch players button.", filename);

        /// <summary>
        /// "Could not read {0}. Please check runtime.log for more details."
        /// </summary>
        public static LocalisableString PlayerListUnreadableWarning(string filename) => new TranslatableString(getKey(@"player_list_unreadable_warning"),
            @"Could not read {0}. Please check runtime.log for more details.", filename);

        /// <summary>
        /// "No valid user ID found in {0}. Expected one user ID per line."
        /// </summary>
        public static LocalisableString PlayerListParseWarning(string filename) => new TranslatableString(getKey(@"player_list_parse_warning"),
            @"No valid user ID found in {0}. Expected one user ID per line.", filename);

        /// <summary>
        /// "Every player past the front row has been drafted."
        /// </summary>
        public static LocalisableString NoPlayerWarning => new TranslatableString(getKey(@"no_player_warning"),
            @"Every player past the front row has been drafted.");

        /// <summary>
        /// "Pool is empty. Fill {0}."
        /// </summary>
        public static LocalisableString PoolEmptyWarning(string filename) => new TranslatableString(getKey(@"pool_empty_warning"),
            @"Pool is empty. Fill {0}.", filename);

        /// <summary>
        /// "No players past the front row in {0}."
        /// </summary>
        public static LocalisableString FrontRowOnlyWarning(string filename) => new TranslatableString(getKey(@"front_row_only_warning"),
            @"No players past the front row in {0}.", filename);

        /// <summary>
        /// "Select a player from the draft pool first."
        /// </summary>
        public static LocalisableString NoSelectionWarning => new TranslatableString(getKey(@"no_selection_warning"),
            @"Select a player from the draft pool first.");

        /// <summary>
        /// "This group is already full."
        /// </summary>
        public static LocalisableString GroupFullWarning => new TranslatableString(getKey(@"group_full_warning"),
            @"This group is already full.");

        /// <summary>
        /// "Draft group"
        /// </summary>
        public static LocalisableString GroupHeader => new TranslatableString(getKey(@"group_header"), @"Draft group");

        /// <summary>
        /// "Other players"
        /// </summary>
        public static LocalisableString PoolHeader => new TranslatableString(getKey(@"pool_header"), @"Other players");

        /// <summary>
        /// "Group count"
        /// </summary>
        public static LocalisableString GroupCount => new TranslatableString(getKey(@"group_count"), @"Group count");

        /// <summary>
        /// "Members per team"
        /// </summary>
        public static LocalisableString MembersPerTeam => new TranslatableString(getKey(@"members_per_team"), @"Members per team");

        /// <summary>
        /// "Fetch players"
        /// </summary>
        public static LocalisableString FetchPlayers => new TranslatableString(getKey(@"fetch_players"), @"Fetch players");

        /// <summary>
        /// "Write results"
        /// </summary>
        public static LocalisableString WriteResults => new TranslatableString(getKey(@"write_results"), @"Write results");

        /// <summary>
        /// "Reload"
        /// </summary>
        public static LocalisableString Reload => new TranslatableString(getKey(@"reload"), @"Reload");

        /// <summary>
        /// "Empty"
        /// </summary>
        public static LocalisableString EmptySlot => new TranslatableString(getKey(@"empty_slot"), @"Empty");

        /// <summary>
        /// "Assigned {0} to {1}."
        /// </summary>
        public static LocalisableString AssignedInfo(string player, string group) => new TranslatableString(getKey(@"assigned_info"),
            @"Assigned {0} to {1}.", player, group);

        /// <summary>
        /// "Removed {0} from {1}."
        /// </summary>
        public static LocalisableString RemovedInfo(string player, string group) => new TranslatableString(getKey(@"removed_info"),
            @"Removed {0} from {1}.", player, group);

        /// <summary>
        /// "Draft saved."
        /// </summary>
        public static LocalisableString SavedInfo => new TranslatableString(getKey(@"saved_info"), @"Draft saved.");

        /// <summary>
        /// "{0} players written to the bracket."
        /// </summary>
        public static LocalisableString WriteResultsInfo(int count) => new TranslatableString(getKey(@"write_results_info"),
            @"{0} players written to the bracket.", count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
