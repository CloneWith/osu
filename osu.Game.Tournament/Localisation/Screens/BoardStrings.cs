// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class BoardStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.Board";

        /// <summary>
        /// "Cannot find a Pick record associated with the selected beatmap."
        /// </summary>
        public static LocalisableString PicksUnavailable => new TranslatableString(getKey(@"pick_unavailable"),
            @"Cannot find a Pick record associated with the selected beatmap.");

        /// <summary>
        /// "Cannot add a Win status to a banned beatmap."
        /// </summary>
        public static LocalisableString WinOnBanNotAllowed => new TranslatableString(getKey(@"win_on_ban_not_allowed"),
            @"Cannot add a Win status to a banned beatmap.");

        /// <summary>
        /// "Remaining:"
        /// </summary>
        public static LocalisableString RemainingHeader => new TranslatableString(getKey(@"remaining_header"), @"Remaining:");

        /// <summary>
        /// "Chess area size"
        /// </summary>
        public static LocalisableString MainBoardAreaSize => new TranslatableString(getKey(@"main_board_area_size"), @"Chess area size");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
