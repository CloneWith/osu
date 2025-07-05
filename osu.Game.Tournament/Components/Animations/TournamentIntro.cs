// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Overlays;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components.Animations
{
    public partial class TournamentIntro : CompositeDrawable, IAnimation
    {
        private readonly RoundBeatmap map;
        private readonly string mod;
        private readonly TeamColour colour;
        private readonly ColourInfo themeColour;
        private readonly ColourInfo modColour;

        private Container introContent = null!;
        private Container topTitleDisplay = null!;
        private Container secondDisplay = null!;
        private Container beatmapBackground = null!;
        private FillFlowContainer authorDisplay = null!;
        private Box flash = null!;
        private EmptyBox dummyBackground = null!;
        private OsuSpriteText modText = null!;
        private StarRatingDisplay starRatingDisplay = null!;

        private FillFlowContainer beatmapContent = null!;

        private FumoChessPiece chessPiece = null!;
        private Container titleContainer = null!;

        private readonly OverlayColourProvider colourProvider;

        public event Action? OnAnimationComplete;
        public AnimationStatus Status { get; private set; } = AnimationStatus.Loading;

        public TournamentIntro(RoundBeatmap map, TeamColour colour = TeamColour.Neutral)
        {
            this.map = map;
            this.colour = colour;
            colourProvider = new OverlayColourProvider(colour == TeamColour.Red ? OverlayColourScheme.Red : OverlayColourScheme.Blue);
            themeColour = colour == TeamColour.Red ? FumoColours.FlandreRed.Regular : colour == TeamColour.Blue ? FumoColours.SeaBlue.Regular : Color4.White;
            mod = map.Mods + map.ModIndex;
            modColour = ModColours.FromModString(map.Mods).Accent;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float horizontal_info_size = 500f;

            InternalChildren = new Drawable[]
            {
                dummyBackground = new EmptyBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = 1366,
                    Height = (int)(1366 * 9f / 16f),
                    Colour = Color4.Black.Opacity(0.6f),
                    Alpha = 0f,
                },
                introContent = new Container
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = OsuGame.SHEAR,
                    Children = new Drawable[]
                    {
                        titleContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                topTitleDisplay = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    X = -75,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "You've picked...",
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = -OsuGame.SHEAR,
                                            Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                        },
                                    }
                                },
                                secondDisplay = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    X = 75,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        modText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = $"...{mod}:",
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = -OsuGame.SHEAR,
                                            Font = OsuFont.GetFont(size: 45, weight: FontWeight.SemiBold, typeface: Typeface.TorusAlternate),
                                            Colour = colourProvider.Background3,
                                        },
                                    }
                                },
                            }
                        },
                        chessPiece = new FumoChessPiece(map.Mods, map.ModIndex)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Shear = -OsuGame.SHEAR,
                            Scale = new Vector2(2f),
                        },
                        beatmapContent = new FillFlowContainer
                        {
                            AlwaysPresent = true, // so we can get the size ahead of time
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Scale = new Vector2(0.001f),
                            Spacing = new Vector2(10),
                            Shear = -OsuGame.SHEAR,
                            Children = new Drawable[]
                            {
                                beatmapBackground = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Size = new Vector2(horizontal_info_size, 250f),
                                    CornerRadius = 10f,
                                    BorderColour = colourProvider.Content2,
                                    BorderThickness = 3f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        flash = new Box
                                        {
                                            Colour = Color4.White,
                                            Blending = BlendingParameters.Additive,
                                            RelativeSizeAxes = Axes.Both,
                                            Depth = float.MinValue,
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Width = horizontal_info_size,
                                    AutoSizeAxes = Axes.Y,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Direction = FillDirection.Vertical,
                                            Padding = new MarginPadding(5f),
                                            Children = new Drawable[]
                                            {
                                                new TruncatingSpriteText
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    MaxWidth = horizontal_info_size,
                                                    Text = map.Beatmap != null ? map.Beatmap.Metadata.GetDisplayTitleRomanisable(false) : "This beatmap!",
                                                    Padding = new MarginPadding { Horizontal = 5f },
                                                    Font = OsuFont.TorusAlternate.With(size: 26, weight: FontWeight.SemiBold),
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"Difficulty: {(map.Beatmap != null ? map.Beatmap.DifficultyName : "A Random Difficulty")}",
                                                    Font = OsuFont.GetFont(size: 20, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"by {(map.Beatmap != null ? map.Beatmap.Metadata.Author.Username : "A Random Mapper")}",
                                                    Font = OsuFont.GetFont(size: 16, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                },
                                                starRatingDisplay = new StarRatingDisplay(new StarDifficulty(map.Beatmap?.StarRating ?? 0, map.MaxCombo), animated: true)
                                                {
                                                    Margin = new MarginPadding(5),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                }
                                            }
                                        },
                                        authorDisplay = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                            Direction = FillDirection.Horizontal,
                                            Padding = new MarginPadding(5f),
                                            Spacing = new Vector2(3),
                                            Alpha = 0,
                                            Children = new Drawable[]
                                            {
                                                new SpriteIcon
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Icon = FontAwesome.Solid.Check,
                                                    Size = new Vector2(20),
                                                    Colour = themeColour,
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = BoardStrings.RoundActionPrompt(TournamentGame.GetTeamString(colour), InstructionsStrings.PickShort),
                                                    Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                                    MaxWidth = horizontal_info_size,
                                                    Colour = themeColour,
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                }
                                            }
                                        },
                                    }
                                },
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new OnlineBeatmapSetCover(map.Beatmap)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                Shear = -OsuGame.SHEAR,
                Alpha = 0,
            }, c =>
            {
                beatmapBackground.Add(c);
                c.FadeIn(300, Easing.OutQuint);
            });
        }

        public void Fire()
        {
            beginAnimation();
        }

        private void beginAnimation()
        {
            this.FadeInFromZero(500, Easing.OutExpo);

            Scheduler.AddDelayed(() =>
            {
                if (map.StarRatingWithMod != null)
                {
                    starRatingDisplay.Current.Value = new StarDifficulty(map.StarRatingWithMod.Value, map.MaxCombo);
                }
            }, 4000);

            using (BeginDelayedSequence(1500))
            {
                introContent.FadeIn(500, Easing.OutQuint);

                const float y_offset_start = 260;
                const float y_offset_end = 20;

                dummyBackground
                    .FadeInFromZero(300, Easing.OutQuint);

                chessPiece.FadeIn(500, Easing.OutQuint)
                          .ScaleTo(1f, 500, Easing.OutQuint);

                topTitleDisplay
                    .FadeInFromZero(400, Easing.OutQuint);

                topTitleDisplay.MoveToY(-y_offset_start)
                               .MoveToY(-y_offset_end, 300, Easing.OutQuint)
                               .Then()
                               .MoveToY(0, 4000);

                modText.Delay(200)
                       .Then().FadeColour(modColour, 500, Easing.OutQuint);

                secondDisplay.MoveToY(y_offset_start)
                             .MoveToY(y_offset_end, 300, Easing.OutQuint)
                             .Then()
                             .MoveToY(0, 4000);

                using (BeginDelayedSequence(1000))
                {
                    beatmapContent
                        .ScaleTo(3)
                        .ScaleTo(1.15f, 500, Easing.In)
                        .Then()
                        .ScaleTo(1.3f, 2000, Easing.OutCubic);

                    using (BeginDelayedSequence(100))
                    {
                        chessPiece
                            .ScaleTo(0.4f, 400, Easing.In)
                            .FadeOut(500, Easing.OutQuint);

                        titleContainer
                            .ScaleTo(0.4f, 400, Easing.In)
                            .FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(240))
                    {
                        beatmapContent.FadeInFromZero(280, Easing.InQuad);

                        using (BeginDelayedSequence(200))
                            authorDisplay.FadeInFromZero(200, Easing.InQuad);

                        using (BeginDelayedSequence(400))
                            flash.FadeOutFromOne(5000, Easing.OutQuint);
                    }
                }

                using (BeginDelayedSequence(6000))
                {
                    introContent.ScaleTo(1.25f, 900, Easing.InOutQuint);

                    this.FadeOutFromOne(750, Easing.OutQuint).Then().Finally(_ =>
                    {
                        Status = AnimationStatus.Complete;
                        OnAnimationComplete?.Invoke();
                        Expire();
                    });
                }
            }
        }
    }
}
