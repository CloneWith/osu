// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class BoardStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.Board";

        /// <summary>
        /// "Current Mode"
        /// </summary>
        public static LocalisableString CurrentMode => new TranslatableString(getKey(@"current_mode"), @"Current Mode");

        /// <summary>
        /// "Round Counter"
        /// </summary>
        public static LocalisableString RoundCounter => new TranslatableString(getKey(@"round_counter"), @"Round Counter");

        /// <summary>
        /// "Preparation Mode"
        /// </summary>
        public static LocalisableString PreparationMode => new TranslatableString(getKey(@"preparation_mode"), @"Preparation Mode");

        /// <summary>
        /// "Tiebreaker Control"
        /// </summary>
        public static LocalisableString TiebreakerControl => new TranslatableString(getKey(@"tiebreaker_control"), @"Tiebreaker Control");

        /// <summary>
        /// "TB Status"
        /// </summary>
        public static LocalisableString TiebreakerIndicator => new TranslatableString(getKey(@"tiebreaker_indicator"), @"TB Status");

        /// <summary>
        /// "Control Override"
        /// </summary>
        public static LocalisableString OverrideTiebreakerControl => new TranslatableString(getKey(@"override_tiebreaker_control"), @"Control Override");

        /// <summary>
        /// "Enter TB"
        /// </summary>
        public static LocalisableString EnterTiebreaker => new TranslatableString(getKey(@"enter_tiebreaker"), @"Enter TB");

        /// <summary>
        /// "Clear Special State"
        /// </summary>
        public static LocalisableString ClearSpecialState => new TranslatableString(getKey(@"clear_special_state"), @"Clear Special State");

        /// <summary>
        /// "Cannot find a Pick record associated with the selected beatmap."
        /// </summary>
        public static LocalisableString PicksUnavailable => new TranslatableString(getKey(@"pick_unavailable"),
            @"Cannot find a Pick record associated with the selected beatmap.");

        /// <summary>
        /// "Remaining:"
        /// </summary>
        public static LocalisableString RemainingHeader => new TranslatableString(getKey(@"remaining_header"), @"Remaining:");

        /// <summary>
        /// "Clear Selection"
        /// </summary>
        public static LocalisableString ClearSelection => new TranslatableString(getKey(@"clear_selection"), @"Clear Selection");

        /// <summary>
        /// "Intro Animation"
        /// </summary>
        public static LocalisableString EnableIntroAnimation => new TranslatableString(getKey(@"enable_intro_animation"),
            @"Intro Animation");

        /// <summary>
        /// "Warning: Board Reset"
        /// </summary>
        public static LocalisableString ResetBoardTitle => new TranslatableString(getKey(@"reset_board_title"),
            @"Warning: Board Reset");

        /// <summary>
        /// "This would reset the board to the initial state, and you will lose all chess placements and ban / pick data in this match. Are you sure?"
        /// </summary>
        public static LocalisableString ResetBoardDescription => new TranslatableString(getKey(@"reset_board_description"),
            @"This would reset the board to the initial state, and you will lose all chess placements and ban / pick data in this match. Are you sure?");

        /// <summary>
        /// "Yes, reset to the initial state."
        /// </summary>
        public static LocalisableString AgreeReset => new TranslatableString(getKey(@"agree_reset"),
            @"Yes, reset to the initial state.");

        /// <summary>
        /// "I'd rather stay the same."
        /// </summary>
        public static LocalisableString KeepCurrentState => new TranslatableString(getKey(@"keep_current_state"),
            @"I'd rather stay the same.");

        /// <summary>
        /// "Current"
        /// </summary>
        public static LocalisableString CurrentRound => new TranslatableString(getKey(@"current_round"), @"Current");

        /// <summary>
        /// "Advance rounds"
        /// </summary>
        public static LocalisableString AutoAdvanceRounds => new TranslatableString(getKey(@"auto_advance_rounds"), @"Advance rounds");

        /// <summary>
        /// "Advance screens"
        /// </summary>
        public static LocalisableString AutoAdvanceScreens => new TranslatableString(getKey(@"auto_advance_screens"), @"Advance screens");

        /// <summary>
        /// "{0} {1}"
        /// </summary>
        public static LocalisableString RoundActionPrompt(LocalisableString team, LocalisableString action)
            => new TranslatableString(getKey(@"round_action_prompt"), @"{0} {1}", team, action);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
