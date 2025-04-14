// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

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
        /// Triggered when the chess piece is requested to be removed (typically via user interaction).
        /// </summary>
        public event ChessRemovalHandler? OnRemovalRequested;

        public delegate void ChessRemovalHandler(string mod, string index);

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

        /// <summary>
        /// Constructs a chess piece with no mod information, usually for empty chess pieces or test purposes.
        /// </summary>
        public FumoChessPiece()
            : this(string.Empty, string.Empty)
        {
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            ModColourScheme colourScheme = ModColours.FromModString(ModName);

            Texture? borderTexture = textures.Get(@"Board/chess-border");
            Texture? specialMask = textures.Get(@"Board/special-mask");
            Texture? chessIcon = textures.Get(@$"Board/{ModName}{ModIndex}")
                                 ?? textures.Get(@$"Board/{ModName}");

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.3f),
                Offset = new Vector2(3),
                Radius = 3,
            };

            InternalChildren =
            [
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
                    Scale = borderTexture != null ? Vector2.One : new Vector2(0.8f),
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourScheme.Background,
                        },
                        new Triangles
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = colourScheme.TriangleLight,
                            ColourDark = colourScheme.TriangleDark,
                            TriangleScale = 1.25f,
                            Velocity = 0.75f,
                        },
                        new Sprite
                        {
                            Name = @"Special mask",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = specialMask,
                            FillMode = FillMode.Fit,
                            Colour = colourScheme.Accent.Opacity(0.3f),
                        },
                        new Sprite
                        {
                            Name = @"Chess icon",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.65f,
                            Texture = chessIcon,
                            FillMode = FillMode.Fit,
                            Colour = colourScheme.Accent,
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
            ];
        }

        /// <summary>
        /// Remove this chess piece and do proper cleaning.
        /// </summary>
        public void Remove()
        {
            OnRemovalRequested?.Invoke(ModName, ModIndex);

            this.ScaleTo(1.5f, 500, Easing.OutQuint);
            this.FadeOut(400, Easing.OutQuint);
            Expire();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
            {
                Remove();
                return true;
            }

            return base.OnMouseDown(e);
        }
    }
}
