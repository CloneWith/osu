// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class InstructionsStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Instructions";

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString DefaultName => new TranslatableString(getKey(@"default_name"), @"Welcome to OFFC!");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString DefaultDescription => new TranslatableString(getKey(@"default_description"), @"Enjoy the game!");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString BanName(LocalisableString team) => new TranslatableString(getKey(@"ban_name"),
            @"Ban Maps ({0})", team);

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString BanDescription => new TranslatableString(getKey(@"ban_description"),
            @"Banned maps cannot be chosen.");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString PickName(LocalisableString team) => new TranslatableString(getKey(@"pick_name"),
            @"Pick Maps ({0})", team);

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString PickDescription => new TranslatableString(getKey(@"pick_description"),
            @"The picked map would be played later.");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString WinName(LocalisableString team) => new TranslatableString(getKey(@"win_name"),
            @"Winner Colouring ({0})", team);

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString WinDescription => new TranslatableString(getKey(@"win_description"),
            @"The chess would be tinted the winner's colour.");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString TieBreakerName => new TranslatableString(getKey(@"tie_breaker_name"),
            @"Entering TieBreaker Mode...");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString TieBreakerDescription => new TranslatableString(getKey(@"tie_breaker_description"),
            @"Yes, the final round coming now.");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString FinalWinName(LocalisableString team) => new TranslatableString(getKey(@"final_win_name"),
            @"{0} Won!", team);

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString FinalWinDescription => new TranslatableString(getKey(@"final_win_description"),
            @"Congratulations!");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString HaltName => new TranslatableString(getKey(@"halt_name"),
            @"Please Wait...");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString HaltDescription => new TranslatableString(getKey(@"halt_description"),
            @"Wait for referees' reply.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
