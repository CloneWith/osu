// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public static class RandomPickStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.RandomPick";

        /// <summary>
        /// "Start draw"
        /// </summary>
        public static LocalisableString StartDraw => new TranslatableString(getKey(@"start_draw"), @"Start draw");

        /// <summary>
        /// "Stop draw"
        /// </summary>
        public static LocalisableString StopDraw => new TranslatableString(getKey(@"stop_draw"), @"Stop draw");

        /// <summary>
        /// "Tier {0} / {1}"
        /// </summary>
        public static LocalisableString TierSwitch(int tier, int tiers) => new TranslatableString(getKey(@"tier_switch"),
            @"Tier {0} / {1}", tier, tiers);

        /// <summary>
        /// "Write to bracket"
        /// </summary>
        public static LocalisableString WriteToBracket => new TranslatableString(getKey(@"write_to_bracket"), @"Write to bracket");

        /// <summary>
        /// "{0} takes the next draw."
        /// </summary>
        public static LocalisableString SelectInfo(string group) => new TranslatableString(getKey(@"select_info"),
            @"{0} takes the next draw.", group);

        /// <summary>
        /// "Tier {0}"
        /// </summary>
        public static LocalisableString PoolHeader(int tier) => new TranslatableString(getKey(@"pool_header"), @"Tier {0}", tier);

        /// <summary>
        /// "No tier {0} player is left to draw."
        /// </summary>
        public static LocalisableString NoTierPlayerWarning(int tier) => new TranslatableString(getKey(@"no_tier_player_warning"),
            @"No tier {0} player is left to draw.", tier);

        /// <summary>
        /// "Every group already has a tier {0} player."
        /// </summary>
        /// <remarks>
        /// Distinct from <see cref="NoTierPlayerWarning"/>: there are still players of the tier in the pool, but
        /// nowhere left to put them, so switching tier is the fix rather than fetching the list again.
        /// </remarks>
        public static LocalisableString NoTierSlotWarning(int tier) => new TranslatableString(getKey(@"no_tier_slot_warning"),
            @"Every group already has a tier {0} player.", tier);

        /// <summary>
        /// "{0} already has a tier {1} player."
        /// </summary>
        /// <remarks>
        /// Distinct from <see cref="NoTierSlotWarning"/>: only the group the operator pointed at is in the way,
        /// so pointing somewhere else is the fix rather than switching tier.
        /// </remarks>
        public static LocalisableString SelectedTierFilledWarning(string group, int tier) => new TranslatableString(getKey(@"selected_tier_filled_warning"),
            @"{0} already has a tier {1} player.", group, tier);

        /// <summary>
        /// "Every front row player has been drawn."
        /// </summary>
        public static LocalisableString AllDrawnWarning => new TranslatableString(getKey(@"all_drawn_warning"),
            @"Every front row player has been drawn.");

        /// <summary>
        /// "Drew {0} into {1}."
        /// </summary>
        public static LocalisableString DrawnInfo(string player, string group) => new TranslatableString(getKey(@"drawn_info"),
            @"Drew {0} into {1}.", player, group);

        /// <summary>
        /// "{0} players written to the bracket."
        /// </summary>
        public static LocalisableString WrittenInfo(int count) => new TranslatableString(getKey(@"written_info"),
            @"{0} players written to the bracket.", count);

        /// <summary>
        /// "Random pick saved."
        /// </summary>
        public static LocalisableString SavedInfo => new TranslatableString(getKey(@"saved_info"), @"Random pick saved.");

        /// <summary>
        /// "Assignments cleared, the draw starts over."
        /// </summary>
        public static LocalisableString ResetInfo => new TranslatableString(getKey(@"reset_info"), @"Assignments cleared, the draw starts over.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
