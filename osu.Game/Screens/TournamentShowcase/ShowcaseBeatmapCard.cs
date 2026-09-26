// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    /// <summary>
    /// The card of the transient beatmap information display, kept on its face-down side.
    /// </summary>
    public partial class ShowcaseBeatmapCard : CompositeDrawable
    {
        public static readonly Vector2 SIZE = new Vector2(180, 300);

        public static readonly float CORNER_RADIUS = 6;

        /// <summary>
        /// The size of the chess piece at the centre of the card.
        /// </summary>
        private const float chess_icon_size = 64;

        /// <summary>
        /// The gap between the chess piece and the mod text below it.
        /// </summary>
        private const float mod_text_spacing = 12;

        public ShowcaseBeatmapCard(ShowcaseBeatmap showcaseBeatmap, OverlayColourScheme? colourScheme = null)
        {
            var colourProvider = new OverlayColourProvider(colourScheme ?? OverlayColourScheme.Blue);

            string mod = showcaseBeatmap.ModString.Value;
            string modIndex = showcaseBeatmap.ModIndex.Value;

            Size = SIZE;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Radius = 5,
                            Colour = Color4.Black.Opacity(0.1f),
                        },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        }
                    },
                    new Container
                    {
                        Name = @"Card back",
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS,
                        Children =
                        [
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(colourProvider.Background3, colourProvider.Background4),
                            },
                            new TrianglesV2
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background4,
                                SpawnRatio = 1.2f,
                                Velocity = 0.1f,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(0, mod_text_spacing),
                                Children = new Drawable[]
                                {
                                    new FumoChessIcon(mod, modIndex)
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Size = new Vector2(chess_icon_size),
                                    },
                                    new TruncatingSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = $"{mod}{modIndex}",
                                        MaxWidth = SIZE.X - 40,
                                        Font = OsuFont.GetFont(size: 32, weight: FontWeight.Bold),
                                        Colour = ModColours.FromModString(mod).Accent,
                                    },
                                },
                            },
                        ],
                    },
                ]
            };
        }
    }
}
