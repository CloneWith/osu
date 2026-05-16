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
        /// "Current"
        /// </summary>
        public static LocalisableString CurrentRound => new TranslatableString(getKey(@"current_round"), @"Current");

        /// <summary>
        /// "{0} {1}"
        /// </summary>
        public static LocalisableString RoundActionPrompt(LocalisableString team, LocalisableString action)
            => new TranslatableString(getKey(@"round_action_prompt"), @"{0} {1}", team, action);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
