// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class GameplayScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.GameplayScreen";

        /// <summary>
        /// "Warmup stage"
        /// </summary>
        public static LocalisableString WarmupStage => new TranslatableString(getKey(@"warmup_stage"), @"Warmup stage");

        /// <summary>
        /// "Toggle chat"
        /// </summary>
        public static LocalisableString ToggleChat => new TranslatableString(getKey(@"toggle_chat"), @"Toggle chat");

        /// <summary>
        /// "Chroma width"
        /// </summary>
        public static LocalisableString ChromaWidth => new TranslatableString(getKey(@"chroma_width"), @"Chroma width");

        /// <summary>
        /// "Players per team"
        /// </summary>
        public static LocalisableString PlayersPerTeam => new TranslatableString(getKey(@"players_per_team"), @"Players per team");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
