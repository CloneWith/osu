// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// The resolved colour theme of a tournament round.
    /// </summary>
    /// <remarks>
    /// This is the single place where a round's custom theme colour is translated into the palettes the
    /// tournament UI consumes, so that consumers never have to repeat the fallback logic themselves.
    /// Team colours (red / blue) are deliberately absent, as those are not a property of the round.
    /// </remarks>
    public sealed class RoundTheme : IEquatable<RoundTheme>
    {
        /// <summary>
        /// The theme applied when the current round does not define a custom theme colour.
        /// </summary>
        public static readonly RoundTheme DEFAULT = new RoundTheme(FumoColours.SeaBlue, false);

        /// <summary>
        /// A seven-shade colour scheme derived from the round's theme colour.
        /// </summary>
        public IFumoColour Scheme { get; }

        /// <summary>
        /// The primary theme colour, equivalent to <see cref="IFumoColour.Regular"/>.
        /// </summary>
        public Colour4 Accent => Scheme.Regular;

        /// <summary>
        /// Whether the round explicitly opted into a custom theme colour.
        /// </summary>
        public bool IsCustom { get; }

        private RoundTheme(IFumoColour scheme, bool isCustom)
        {
            Scheme = scheme;
            IsCustom = isCustom;
        }

        /// <summary>
        /// Resolves the theme of a round, falling back to <see cref="DEFAULT"/>.
        /// </summary>
        /// <param name="round">the round to resolve, may be <see langword="null"/>.</param>
        public static RoundTheme FromRound(TournamentRound? round)
        {
            if (round?.UseCustomThemeColour.Value != true)
                return DEFAULT;

            return new RoundTheme(round.ColourScheme, true);
        }

        public bool Equals(RoundTheme? other) => other != null && IsCustom == other.IsCustom && Accent == other.Accent;

        public override bool Equals(object? obj) => Equals(obj as RoundTheme);

        public override int GetHashCode() => HashCode.Combine(IsCustom, Accent);
    }
}
