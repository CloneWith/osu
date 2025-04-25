// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A round-corner beatmap panel with star rating display, mainly used in the board screen (right area).
    /// </summary>
    public partial class FumoBeatmapPanel : CompositeDrawable
    {
        /// <summary>
        /// The relevant <see cref="RoundBeatmap"/> of the beatmap panel.
        /// </summary>
        public readonly RoundBeatmap Beatmap;

        public const float WIDTH = 80;
        public const float HEIGHT = 60;

        public const float INNER_BORDER = 3;

        /// <summary>
        /// Whether this panel is in a selected state.
        /// </summary>
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                    return;

                selected = value;
                updateBorder();
            }
        }

        private bool selected;

        private SpriteIcon statusIcon = null!;
        private Container beatmapInfoContainer = null!;
        private Container floatingContainer = null!;
        private Box floatingBox = null!;
        private Box topMask = null!;
        private Box backgroundAddition = null!;
        private TournamentSpriteText instructText = null!;
        private Container banPill = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public FumoBeatmapPanel(RoundBeatmap beatmap)
        {
            Beatmap = beatmap;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            InternalChildren = new Drawable[]
            {
                beatmapInfoContainer = new Container
                {
                    Name = "Beatmap information",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = WIDTH,
                    Height = HEIGHT,
                    Masking = true,
                    CornerRadius = 5,
                    Children = new Drawable[]
                    {
                        new NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = OsuColour.Gray(0.5f),
                            OnlineInfo = Beatmap.Beatmap,
                        },
                        backgroundAddition = new Box
                        {
                            Name = @"Background addition",
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.3f,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5f),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = @$"{Beatmap.Mods}{Beatmap.ModIndex}",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 18),
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = Beatmap.DifficultyField,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 12),
                                },
                            },
                        },
                        topMask = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0,
                        },
                    },
                },
                floatingContainer = new Container
                {
                    Name = @"Floating container",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0,
                    Masking = true,
                    CornerRadius = 5,
                    Children = new Drawable[]
                    {
                        floatingBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                        },
                        statusIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Height = 1f,
                            Icon = FontAwesome.Solid.Heart,
                            Size = new Vector2(20),
                            Colour = Color4.White,
                            Alpha = 0,
                        },
                        instructText = new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                            Text = @"Undefined",
                        }
                    }
                },
                new StarRatingDisplay(starDifficulty: new StarDifficulty(Beatmap.Beatmap?.StarRating ?? 0, 0), animated: true)
                {
                    Name = "Star rating pill",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(0.75f),
                },
                banPill = new Container
                {
                    Name = "Ban Pill",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Horizontal = -5, Bottom = -8.5f },
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 8,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new TournamentSpriteText
                        {
                            Text = "Ban",
                            Padding = new MarginPadding { Horizontal = 5, Top = 0.3f, Bottom = 2.5f },
                            Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateBorder();
            updateState();
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
                match.OldValue.ChessPlacements.CollectionChanged -= onPlacementChanged;

            if (match.NewValue != null)
                match.NewValue.ChessPlacements.CollectionChanged += onPlacementChanged;

            Scheduler.AddOnce(updateState);
        }

        private void onPlacementChanged(object? _, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState);

        private ChessPlacement? lastPlacement;

        private void updateBorder()
        {
            if (!IsLoaded)
                return;

            if (selected)
            {
                beatmapInfoContainer.BorderColour = Colour4.White;
                beatmapInfoContainer.TransformTo(nameof(beatmapInfoContainer.BorderThickness), INNER_BORDER, 500, Easing.OutQuint);
                backgroundAddition.FlashColour(Color4.White.Opacity(0.9f), 1000, Easing.OutQuint);
            }
            else
            {
                beatmapInfoContainer.TransformTo(nameof(beatmapInfoContainer.BorderThickness), 0f, 500, Easing.OutQuint);
            }
        }

        private void updateState()
        {
            if (currentMatch.Value == null)
            {
                banPill?.FadeOut(300, Easing.OutQuint);
                return;
            }

            var newPlacement = currentMatch.Value.ChessPlacements.LastOrDefault(p => p.BeatmapID == Beatmap.Beatmap?.OnlineID);

            string choiceText = newPlacement?.OwnerTeam switch
            {
                TeamColour.Red => @"Red",
                TeamColour.Blue => @"Blue",
                _ => @"Map",
            };

            bool shouldAnimate = newPlacement?.OwnerTeam != lastPlacement?.OwnerTeam
                                 || newPlacement?.CurrentType != lastPlacement?.CurrentType;

            if (newPlacement != null)
            {
                if (newPlacement.CurrentType == ChoiceType.Ban)
                {
                    var pillBg = banPill.Children.OfType<Box>().First();
                    pillBg.Colour = newPlacement.OwnerTeam == TeamColour.Red ? TournamentGame.COLOUR_RED : TournamentGame.COLOUR_BLUE;
                }
                else
                {
                    banPill.FinishTransforms(true);
                    banPill.FadeOut(300, Easing.OutQuint);
                }

                if (shouldAnimate)
                {
                    topMask.FadeTo(newPlacement.CurrentType == ChoiceType.Ban ? 0.5f : 0f, 300, Easing.OutQuint);

                    switch (newPlacement.CurrentType)
                    {
                        case ChoiceType.Pick:
                            statusIcon.Icon = FontAwesome.Solid.CheckCircle;
                            instructText.Text = "Map picked!";
                            banPill.FinishTransforms(true);
                            banPill.FadeOut(300, Easing.OutQuint);
                            runAnimation();
                            break;

                        case ChoiceType.Ban:
                            instructText.Text = $"{choiceText} banned!";
                            statusIcon.Icon = FontAwesome.Solid.Ban;
                            instructText.Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold);

                            runAnimation(Color4.Gray);
                            break;

                        case ChoiceType.RedWin:
                        case ChoiceType.BlueWin:
                            statusIcon.Icon = FontAwesome.Solid.Trophy;
                            instructText.Text = newPlacement.CurrentType == ChoiceType.RedWin ? "Red wins!" : "Blue wins!";
                            banPill.FinishTransforms(true);
                            banPill.FadeOut(300, Easing.OutQuint);
                            runAnimation(newPlacement.CurrentType == ChoiceType.RedWin ? TournamentGame.COLOUR_RED : TournamentGame.COLOUR_BLUE);
                            break;
                    }
                }
                else if (newPlacement.CurrentType != ChoiceType.Ban)
                {
                    banPill.FinishTransforms(true);
                    banPill.FadeOut(300, Easing.OutQuint);
                }
            }
            else
            {
                banPill.FinishTransforms(true);
                banPill.FadeOut(300, Easing.OutQuint);
                topMask.FadeOut(300, Easing.OutQuint);
                statusIcon.FadeOut(200, Easing.OutQuint);
                Alpha = 1;
            }

            lastPlacement = newPlacement;
        }

        /// <summary>
        /// Start the animation sequence for the beatmap card.
        /// </summary>
        /// <param name="colour">The background colour.</param>
        private void runAnimation(ColourInfo? colour = null)
        {
            // Stop any transform process (if exists) first
            FinishTransforms(true);

            banPill.FinishTransforms(true);
            banPill.Alpha = 0;
            banPill.Y = 0;

            ColourInfo useColour = colour ?? Color4.White;
            ColourInfo fadeColour = useColour == Color4.White ? Color4.Black : Color4.White;

            statusIcon.Colour = Color4.Black;
            instructText.Colour = Color4.Black;

            // Reset the state of the floating container
            floatingContainer.Anchor = Anchor.BottomCentre;
            floatingContainer.Origin = Anchor.BottomCentre;
            floatingContainer.ResizeHeightTo(0);
            floatingBox.Colour = useColour;

            statusIcon.Y = 1.5f;
            statusIcon.FadeOut();

            instructText.Y = 1.5f;
            instructText.FadeOut();

            using (BeginDelayedSequence(200))
            {
                floatingContainer.ResizeHeightTo(1, 700, Easing.OutQuint);

                statusIcon.FadeIn(300, Easing.OutQuint);
                instructText.FadeIn(300, Easing.OutQuint);

                using (BeginDelayedSequence(100))
                {
                    statusIcon.MoveToY(-0.175f, 800, Easing.OutExpo);
                    instructText.Delay(50).MoveToY(0.175f, 800, Easing.OutExpo);
                    statusIcon.FadeColour(fadeColour, 1000, Easing.OutQuint);
                    instructText.FadeColour(fadeColour, 1000, Easing.OutQuint);

                    Scheduler.AddDelayed(() =>
                    {
                        floatingContainer.Anchor = Anchor.TopCentre;
                        floatingContainer.Origin = Anchor.TopCentre;

                        floatingContainer.ResizeHeightTo(0, 1300, Easing.InOutQuint);

                        statusIcon.MoveToY(-2f, 1350, Easing.InExpo);
                        instructText.MoveToY(-2f, 1450, Easing.InExpo);

                        if (useColour == Color4.Gray)
                        {
                            var currentPlacement = currentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == Beatmap.Beatmap?.OnlineID);
                            if (currentPlacement?.CurrentType == ChoiceType.Ban)
                            {
                                banPill.Y = 15;
                                banPill.Alpha = 0;

                                using (BeginDelayedSequence(500))
                                {
                                    banPill.FadeIn(600, Easing.OutExpo);
                                    banPill.MoveToY(0, 600, Easing.OutExpo);
                                }
                            }
                        }

                        using (BeginDelayedSequence(500))
                        {
                            statusIcon.FadeOut(600, Easing.OutQuint);
                            instructText.FadeOut(600, Easing.OutQuint);
                        }
                    }, 3000);
                }
            }
        }
    }
}
