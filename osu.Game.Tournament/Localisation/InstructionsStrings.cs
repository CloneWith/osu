// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class InstructionsStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Instructions";

        /// <summary>
        /// "Welcome to OFFC!"
        /// </summary>
        public static LocalisableString DefaultName => new TranslatableString(getKey(@"default_name"), @"Welcome to OFFC!");

        /// <summary>
        /// "Enjoy the game!"
        /// </summary>
        public static LocalisableString DefaultDescription => new TranslatableString(getKey(@"default_description"), @"Enjoy the game!");

        /// <summary>
        /// "Ban Maps ({0})"
        /// </summary>
        public static LocalisableString BanName(LocalisableString team) => new TranslatableString(getKey(@"ban_name"),
            @"Ban Maps ({0})", team);

        /// <summary>
        /// "Banned maps cannot be chosen."
        /// </summary>
        public static LocalisableString BanDescription => new TranslatableString(getKey(@"ban_description"),
            @"Banned maps cannot be chosen.");

        /// <summary>
        /// "Pick Maps ({0})"
        /// </summary>
        public static LocalisableString PickName(LocalisableString team) => new TranslatableString(getKey(@"pick_name"),
            @"Pick Maps ({0})", team);

        /// <summary>
        /// "The picked map would be played later."
        /// </summary>
        public static LocalisableString PickDescription => new TranslatableString(getKey(@"pick_description"),
            @"The picked map would be played later.");

        /// <summary>
        /// "Round winner ({0})"
        /// </summary>
        public static LocalisableString WinName(LocalisableString team) => new TranslatableString(getKey(@"win_name"),
            @"Round winner ({0})", team);

        /// <summary>
        /// "Paint the chess with winner's colour."
        /// </summary>
        public static LocalisableString WinDescription => new TranslatableString(getKey(@"win_description"),
            @"Paint the chess with winner's colour.");

        /// <summary>
        /// "Deploy Shiro ({0})"
        /// </summary>
        public static LocalisableString ShiroName(LocalisableString team) => new TranslatableString(getKey(@"shiro_name"),
            @"Deploy Shiro ({0})", team);

        /// <summary>
        /// "Consume couplets to deploy one."
        /// </summary>
        public static LocalisableString ShiroDescription => new TranslatableString(getKey(@"shiro_description"),
            @"Consume couplets to deploy one.");

        /// <summary>
        /// "Place Shiro"
        /// </summary>
        public static LocalisableString ShiroPlacingName => new TranslatableString(getKey(@"shiro_placing_name"),
            @"Place Shiro");

        /// <summary>
        /// "Place an empty Shiro chess piece."
        /// </summary>
        public static LocalisableString ShiroPlacingDescription => new TranslatableString(getKey(@"shiro_placing_description"),
            @"Place an empty Shiro chess piece.");

        /// <summary>
        /// "Get Owner ({0})"
        /// </summary>
        public static LocalisableString UpdateOwnerName(LocalisableString team) => new TranslatableString(getKey(@"update_owner_name"),
            @"Get Owner ({0})", team);

        /// <summary>
        /// "Consume couplets to take a win chess."
        /// </summary>
        public static LocalisableString UpdateOwnerDescription => new TranslatableString(getKey(@"update_owner_description"),
            @"Consume couplets to take a win chess.");

        /// <summary>
        /// "Entering TB Mode..."
        /// </summary>
        public static LocalisableString TieBreakerName => new TranslatableString(getKey(@"tie_breaker_name"),
            @"Entering TB Mode...");

        /// <summary>
        /// "Yes, the final round coming now."
        /// </summary>
        public static LocalisableString TieBreakerDescription => new TranslatableString(getKey(@"tie_breaker_description"),
            @"Yes, the final round coming now.");

        /// <summary>
        /// "{0} Won!"
        /// </summary>
        public static LocalisableString FinalWinName(LocalisableString team) => new TranslatableString(getKey(@"final_win_name"),
            @"{0} Won!", team);

        /// <summary>
        /// "Congratulations!"
        /// </summary>
        public static LocalisableString FinalWinDescription => new TranslatableString(getKey(@"final_win_description"),
            @"Congratulations!");

        /// <summary>
        /// "Do you want smoke?"
        /// </summary>
        public static LocalisableString OnFireName => new TranslatableString(getKey(@"on_fire_name"), @"Do you want smoke?");

        /// <summary>
        /// "You need Ding Zhen's Ruike V5!"
        /// </summary>
        public static LocalisableString OnFireDescription => new TranslatableString(getKey(@"on_fire_description"), @"You need Ding Zhen's Ruike V5!");

        /// <summary>
        /// "Please Wait..."
        /// </summary>
        public static LocalisableString HaltName => new TranslatableString(getKey(@"halt_name"),
            @"Please Wait...");

        /// <summary>
        /// "Waiting for referees' reply..."
        /// </summary>
        public static LocalisableString HaltDescription => new TranslatableString(getKey(@"halt_description"),
            @"Waiting for referees' reply...");

        /// <summary>
        /// "Ban"
        /// </summary>
        public static LocalisableString BanShort => new TranslatableString(getKey(@"ban_short"), @"Ban");

        /// <summary>
        /// "Pick"
        /// </summary>
        public static LocalisableString PickShort => new TranslatableString(getKey(@"pick_short"), @"Pick");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
