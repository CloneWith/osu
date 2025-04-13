// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A general and rounded chess display for the chessboard.
    /// </summary>
    public partial class FumoChessPiece : Circle
    {
        /// <summary>
        /// The name of the chess's mod.
        /// </summary>
        public readonly string ModName;

        /// <summary>
        /// The index of the chess in its mod category.
        /// </summary>
        public readonly string ModIndex;

        /// <summary>
        /// Constructs a chess piece.
        /// </summary>
        /// <param name="mod">the mod name of the chess</param>
        /// <param name="index">the mod index</param>
        public FumoChessPiece(string mod, string index)
        {
            ModName = mod;
            ModIndex = index;

            Height = 100;
            Width = 100;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture? borderTexture = textures.Get(@"Board/chess-border");
            Texture? chessIcon = textures.Get(@$"Board/{ModName}{ModIndex}")
                                 ?? textures.Get(@$"Board/{ModName}");

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.3f),
                Offset = new Vector2(3),
                Radius = 3,
            };

            InternalChildren = new Drawable[]
            {
                borderTexture == null
                    ? new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    }
                    : Empty(),
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = FumoColours.SunshineYellow.Darkest,
                        },
                        new Triangles
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = FumoColours.SunshineYellow.Dark,
                            ColourDark = FumoColours.SunshineYellow.Darker,
                            TriangleScale = 1.25f,
                            Velocity = 0.75f,
                        },
                        new Circle
                        {
                            Name = "Dim mask",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.6f),
                        },
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = chessIcon,
                            Size = new Vector2(64),
                            Colour = FumoColours.SunshineYellow.Regular,
                        },
                    },
                },
                // Place the customized border at the top in case we have inner shadow.
                borderTexture != null
                    ? new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Texture = borderTexture,
                    }
                    : Empty(),
            };
        }
    }
}
