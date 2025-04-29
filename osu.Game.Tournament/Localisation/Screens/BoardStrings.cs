// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class BoardStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.Board";

        /// <summary>
        /// "Cannot find a Pick / Ban record associated with the selected beatmap."
        /// </summary>
        public static LocalisableString PickBansUnavailable => new TranslatableString(getKey(@"pick_bans_unavailable"),
            @"Cannot find a Pick / Ban record associated with the selected beatmap.");

        /// <summary>
        /// "Remaining:"
        /// </summary>
        public static LocalisableString RemainingHeader => new TranslatableString(getKey(@"remaining_header"), @"Remaining:");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
