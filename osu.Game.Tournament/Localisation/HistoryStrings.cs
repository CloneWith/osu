// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Localisation
{
    public class HistoryStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.History";

        /// <summary>
        /// "Match History"
        /// </summary>
        public static LocalisableString MatchHistory => new TranslatableString(getKey(@"match_history"), @"Match History");

        /// <summary>
        /// "Match in progress"
        /// </summary>
        public static LocalisableString MatchInProgress => new TranslatableString(getKey(@"match_in_progress"), @"Match in progress");

        /// <summary>
        /// "TieBreaker for the final stage..."
        /// </summary>
        public static LocalisableString MatchTiebreaker => new TranslatableString(getKey(@"match_tiebreaker"), @"TieBreaker for the final stage...");

        /// <summary>
        /// "Match concluded, {0} wins!"
        /// </summary>
        public static LocalisableString MatchEnded(TeamColour winner) => new TranslatableString(getKey(@"match_ended"),
            @"Match concluded, {0} wins!", TournamentExtensions.GetTeamString(winner));

        /// <summary>
        /// " banned "
        /// </summary>
        public static LocalisableString Banned => new TranslatableString(getKey(@"banned"), @" banned ");

        /// <summary>
        /// " picked "
        /// </summary>
        public static LocalisableString Picked => new TranslatableString(getKey(@"picked"), @" picked ");

        /// <summary>
        /// " won "
        /// </summary>
        public static LocalisableString Won => new TranslatableString(getKey(@"won"), @" won ");

        /// <summary>
        /// " placed a shiro"
        /// </summary>
        public static LocalisableString PlacedShiro => new TranslatableString(getKey(@"placed_shiro"), @" placed a shiro");

        /// <summary>
        /// " used {0} to take "
        /// </summary>
        public static LocalisableString UpdatedWinner(LocalisableString usedPieces) => new TranslatableString(getKey(@"updated_winner"),
            @" used {0} to take ", usedPieces);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
