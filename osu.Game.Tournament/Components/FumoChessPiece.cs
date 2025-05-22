// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
using osu.Game.Tournament.Models;
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
        public string ModName { get; private set; } = string.Empty;

        /// <summary>
        /// The index of the chess in its mod category.
        /// </summary>
        public string ModIndex { get; private set; } = string.Empty;

        /// <summary>
        /// The ID of the beatmap.
        /// </summary>
        public readonly int BeatmapID;

        /// <inheritdoc cref="ChessPlacement.OwnerTeam"/>
        /// <remarks>Changing this will trigger an update of the chess.</remarks>
        public TeamColour OwnerTeam
        {
            get => ownerTeam;
            set
            {
                if (ownerTeam == value)
                    return;

                ownerTeam = value;
                updateChess();
            }
        }

        /// <inheritdoc cref="ChessPlacement.CurrentType"/>
        /// <remarks>Changing this will trigger an update of the chess.</remarks>
        public ChoiceType CurrentType
        {
            get => currentType;
            set
            {
                if (currentType == value)
                    return;

                currentType = value;
                updateChess();
            }
        }

        private TeamColour ownerTeam;
        private ChoiceType currentType;

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        private ModColourScheme colourScheme = ModColours.Empty;

        private Texture? chessIcon;
        private Circle backgroundCircle = null!;
        private Sprite topIcon = null!;
        private Sprite specialMask = null!;
        private Triangles triangles = null!;
        private Box dimMask = null!;

        private readonly bool requireFetch;

        public FumoChessPiece(int beatmapId)
        {
            BeatmapID = beatmapId;
            requireFetch = true;
        }

        public FumoChessPiece(ChessPlacement placement)
        {
            BeatmapID = placement.BeatmapID;
            ownerTeam = placement.OwnerTeam;
            currentType = placement.CurrentType;
            requireFetch = true;
        }

        /// <summary>
        /// Constructs a chess piece.
        /// </summary>
        /// <param name="mod">the mod name of the chess</param>
        /// <param name="index">the mod index</param>
        /// <param name="target">the <see cref="ChessPlacement"/> to provide relevant information.</param>
        public FumoChessPiece(string mod, string index, ChessPlacement? target = null)
        {
            ModName = mod;
            ModIndex = index;
            BeatmapID = target?.BeatmapID ?? TournamentGame.RESERVED_BEATMAP_ID;
            ownerTeam = target?.OwnerTeam ?? TeamColour.Neutral;
            currentType = target?.CurrentType ?? ChoiceType.Neutral;

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
        private void load(LadderInfo ladder)
        {
            if (requireFetch)
            {
                var beatmap = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b => b.ID == BeatmapID);

                if (beatmap != null)
                {
                    ModName = beatmap.Mods;
                    ModIndex = beatmap.ModIndex;
                }
            }

            colourScheme = ModColours.FromModString(ModName);

            Texture? borderTexture = textures.Get(@"Board/chess-border");
            Texture? specialTexture = textures.Get(@"Board/special-mask");
            chessIcon = textures.Get(@$"Board/{ModName}{ModIndex}")
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
                        backgroundCircle = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourScheme.Background,
                        },
                        triangles = new Triangles
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = colourScheme.TriangleLight,
                            ColourDark = colourScheme.TriangleDark,
                            TriangleScale = 1.25f,
                            Velocity = 0.75f,
                        },
                        specialMask = new Sprite
                        {
                            Name = @"Special mask",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = specialTexture,
                            FillMode = FillMode.Fit,
                            Colour = colourScheme.Accent.Opacity(0.3f),
                            Alpha = 0,
                        },
                        topIcon = new Sprite
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
                dimMask = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0,
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateChess();
        }

        /// <summary>
        /// Remove this chess piece and do proper cleaning.
        /// </summary>
        public void Remove()
        {
            this.ScaleTo(1.5f, 500, Easing.OutQuint);
            this.FadeOut(400, Easing.OutQuint);
            Expire();
        }

        private void updateChess()
        {
            if (!IsLoaded)
                return;

            if (currentType is ChoiceType.RedWin or ChoiceType.BlueWin or ChoiceType.Consumed)
            {
                ModColourScheme specialScheme = currentType switch
                {
                    ChoiceType.RedWin => ModColours.RedWin,
                    ChoiceType.BlueWin => ModColours.BlueWin,
                    ChoiceType.Consumed => ModColours.Consumed,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                backgroundCircle.FadeColour(specialScheme.Background, 500, Easing.OutQuint);
                specialMask.FadeColour(specialScheme.Accent.Opacity(0.3f));
                specialMask.FadeTo(currentType is ChoiceType.RedWin or ChoiceType.BlueWin or ChoiceType.Consumed ? 1 : 0, 500, Easing.OutQuint);
                topIcon.FadeColour(specialScheme.Accent, 500, Easing.OutQuint);
                triangles.TransformTo(nameof(triangles.ColourLight), specialScheme.TriangleLight, 500, Easing.OutQuint);
                triangles.TransformTo(nameof(triangles.ColourDark), specialScheme.TriangleDark, 500, Easing.OutQuint);
                dimMask.FadeTo(currentType is ChoiceType.Consumed ? 0.3f : 0, 500, Easing.OutQuint);
            }
            else
            {
                backgroundCircle.FadeColour(colourScheme.Background, 500, Easing.OutQuint);
                specialMask.FadeColour(colourScheme.Accent.Opacity(0.3f));
                specialMask.FadeOut(500, Easing.OutQuint);
                topIcon.FadeColour(colourScheme.Accent, 500, Easing.OutQuint);
                triangles.TransformTo(nameof(triangles.ColourLight), colourScheme.TriangleLight, 500, Easing.OutQuint);
                triangles.TransformTo(nameof(triangles.ColourDark), colourScheme.TriangleDark, 500, Easing.OutQuint);
                dimMask.FadeOut(500, Easing.OutQuint);
            }

            topIcon.Texture = currentType is ChoiceType.RedWin or ChoiceType.BlueWin or ChoiceType.Consumed
                ? textures.Get(@"Board/chess-win")
                : chessIcon;

            topIcon.ScaleTo(1.5f).Then().ScaleTo(1, 500, Easing.OutQuint);
        }

        protected override bool OnMouseDown(MouseDownEvent e) => false;
    }
}
