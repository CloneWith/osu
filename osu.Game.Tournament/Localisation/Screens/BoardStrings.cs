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
        /// "Cannot find a Pick record associated with the selected beatmap."
        /// </summary>
        public static LocalisableString PicksUnavailable => new TranslatableString(getKey(@"pick_unavailable"),
            @"Cannot find a Pick record associated with the selected beatmap.");

        /// <summary>
        /// "Remaining:"
        /// </summary>
        public static LocalisableString RemainingHeader => new TranslatableString(getKey(@"remaining_header"), @"Remaining:");

        /// <summary>
        /// "Chess area size"
        /// </summary>
        public static LocalisableString MainBoardAreaSize => new TranslatableString(getKey(@"main_board_area_size"), @"Chess area size");

        /// <summary>
        /// "Shiro Deployment"
        /// </summary>
        public static LocalisableString ShiroDeployment => new TranslatableString(getKey(@"shiro_deployment"), @"Shiro Deployment");

        /// <summary>
        /// "Deployment Mode"
        /// </summary>
        public static LocalisableString EnableDeployment => new TranslatableString(getKey(@"enable_deployment"), @"Deployment Mode");

        /// <summary>
        /// "Place"
        /// </summary>
        public static LocalisableString PlaceShiro => new TranslatableString(getKey(@"place_shiro"), @"Place");

        /// <summary>
        /// "Activate"
        /// </summary>
        public static LocalisableString ActivateShiro => new TranslatableString(getKey(@"activate_shiro"), @"Activate");

        /// <summary>
        /// "Update Owner"
        /// </summary>
        public static LocalisableString UpdateShiroOwner => new TranslatableString(getKey(@"update_shiro_owner"), @"Update Owner");

        /// <summary>
        /// "Clear Selection"
        /// </summary>
        public static LocalisableString ClearSelection => new TranslatableString(getKey(@"clear_selection"), @"Clear Selection");

        /// <summary>
        /// "The selected chess pieces must be in the same colour."
        /// </summary>
        public static LocalisableString SingleColourPrompt => new TranslatableString(getKey(@"single_colour_prompt"),
            @"The selected chess pieces must be in the same colour.");

        /// <summary>
        /// "Must select two chess pieces to activate Shiro."
        /// </summary>
        public static LocalisableString ShiroActivationPrompt => new TranslatableString(getKey(@"shiro_activation_prompt"),
            @"Must select two chess pieces to activate Shiro.");

        /// <summary>
        /// "Invalid combination for updating Shiro."
        /// </summary>
        public static LocalisableString ShiroOwnerUpdatePrompt => new TranslatableString(getKey(@"shiro_owner_update_prompt"),
            @"Invalid combination for updating Shiro.");

        /// <summary>
        /// "Cannot find an existing Shiro chess piece."
        /// </summary>
        public static LocalisableString ShiroMissingPrompt => new TranslatableString(getKey(@"shiro_missing_prompt"),
            @"Cannot find an existing Shiro chess piece.");

        /// <summary>
        /// "A Shiro chess piece already exists."
        /// </summary>
        public static LocalisableString ShiroExistsPrompt => new TranslatableString(getKey(@"shiro_exists_prompt"),
            @"A Shiro chess piece already exists.");

        /// <summary>
        /// "The Shiro chess piece has already been activated. Use Update Owner instead."
        /// </summary>
        public static LocalisableString ShiroActivatedPrompt => new TranslatableString(getKey(@"shiro_activated_prompt"),
            @"The Shiro chess piece has already been activated. Use Update Owner instead.");

        /// <summary>
        /// "Information about the selected action would be displayed here."
        /// </summary>
        public static LocalisableString ActionPlaceholder => new TranslatableString(getKey(@"action_placeholder"),
            @"Information about the selected action would be displayed here.");

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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
