// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A general chess icon display for usages outside the normal board.
    /// </summary>
    /// <remarks>
    /// Shared between the tournament client and the showcase, which is why it lives in the game assembly alongside the
    /// tournament models it consumes.
    /// </remarks>
    public partial class FumoChessIcon : ConstrainedIconContainer
    {
        /// <summary>
        /// The name of the chess's mod.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        /// The index of the chess in its mod category.
        /// </summary>
        public string ModIndex { get; }

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        /// <summary>
        /// The theme of the round currently being played, if there is one.
        /// </summary>
        /// <remarks>
        /// Only the tournament client provides this. The showcase is started from the main menu without an active
        /// round, so tiebreakers keep the fixed tiebreaker palette there.
        /// </remarks>
        [Resolved(CanBeNull = true)]
        private TournamentThemeProvider? themeProvider { get; set; }

        private ModColourScheme colourScheme = ModColours.Empty;
        private bool isTieBreaker;

        private Sprite iconSprite = null!;

        private Texture? chessIcon;

        /// <summary>
        /// Constructs a chess icon.
        /// </summary>
        /// <param name="mod">the mod name of the icon</param>
        /// <param name="index">the mod index</param>
        public FumoChessIcon(string mod, string index)
        {
            ModName = mod;
            ModIndex = index;
        }

        /// <summary>
        /// Constructs a chess icon with no mod information.
        /// </summary>
        public FumoChessIcon()
            : this(string.Empty, string.Empty)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            isTieBreaker = ModName.Equals(@"TB", StringComparison.OrdinalIgnoreCase);

            // tiebreaker icons follow the current round's theme, which is tracked by the theme provider
            // (see LoadComplete). Every other icon keeps the fixed palette of its mod.
            colourScheme = isTieBreaker ? ModColours.TieBreaker : ModColours.FromModString(ModName);

            // Use win icon for TB maps, subject to change
            if (isTieBreaker)
                chessIcon = textures.Get(@"Board/chess-win");
            else
            {
                chessIcon = textures.Get(@$"Board/{ModName}{ModIndex}")
                            ?? textures.Get(@$"Board/{ModName}");
            }

            InternalChildren =
            [
                iconSprite = new Sprite
                {
                    Name = @"Chess icon",
                    RelativeSizeAxes = Axes.Both,
                    Texture = chessIcon,
                    FillMode = FillMode.Fit,
                    Colour = colourScheme.Accent,
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!isTieBreaker || themeProvider == null)
                return;

            themeProvider.Current.BindValueChanged(theme =>
                iconSprite.FadeColour(theme.NewValue.TieBreaker.Accent, 500, Easing.OutQuint), true);
        }
    }
}
