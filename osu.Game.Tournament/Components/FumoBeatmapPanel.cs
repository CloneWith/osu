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
using osu.Framework.Localisation;
using osu.Framework.Threading;
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

        public const float INNER_BORDER = 6;

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
        private CircularContainer banPill = null!;
        private Box pillBg = null!;
        private CircularContainer trophyIcon = null!;
        private Box trophyBg = null!;

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
                new EmptyBox(5)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = WIDTH,
                    Height = HEIGHT,
                    BoxColour = Color4.White,
                },
                beatmapInfoContainer = new Container
                {
                    Name = @"Beatmap information",
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
                    Name = @"Star rating pill",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(0.75f),
                },
                banPill = new CircularContainer
                {
                    Name = @"Ban Pill",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Horizontal = -5, Bottom = -8.5f },
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        pillBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new TournamentSpriteText
                        {
                            Text = @"Ban",
                            Padding = new MarginPadding { Horizontal = 5, Top = 0.3f, Bottom = 2.5f },
                            Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                },
                trophyIcon = new CircularContainer
                {
                    Name = @"Win Circle",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Horizontal = -5, Bottom = -11 },
                    Size = new Vector2(24),
                    Masking = true,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        trophyBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.Trophy,
                            Size = new Vector2(14),
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
            updateState(false);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
                match.OldValue.ChessPlacements.CollectionChanged -= onPlacementChanged;

            if (match.NewValue != null)
                match.NewValue.ChessPlacements.CollectionChanged += onPlacementChanged;

            Scheduler.AddOnce(() => updateState(false));
        }

        private void onPlacementChanged(object? _, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState, e.Action == NotifyCollectionChangedAction.Add);

        private ChessPlacement? lastPlacement;
        private ScheduledDelegate? scheduledFloatingBoxAnimation;

        private void updateBorder()
        {
            if (!IsLoaded)
                return;

            if (selected)
            {
                beatmapInfoContainer.ResizeTo(new Vector2(WIDTH - INNER_BORDER, HEIGHT - INNER_BORDER), 500, Easing.OutQuint);
                backgroundAddition.FlashColour(Color4.White.Opacity(0.9f), 1000, Easing.OutQuint);
            }
            else
            {
                beatmapInfoContainer.ResizeTo(new Vector2(WIDTH, HEIGHT), 500, Easing.OutQuint);
            }
        }

        private void updateState(bool playFullAnimation = true)
        {
            // Match unavailable: Clean up
            if (currentMatch.Value == null)
            {
                FinishTransforms(true);
                banPill.FadeOut(300, Easing.OutQuint);
                trophyIcon.FadeOut(300, Easing.OutQuint);
                lastPlacement = null;
                return;
            }

            var newPlacement = currentMatch.Value.ChessPlacements.LastOrDefault(p => p.BeatmapID == Beatmap.Beatmap?.OnlineID);

            // Relevant placement unchanged: don't update
            if (lastPlacement == newPlacement)
                return;

            bool shouldAnimate = playFullAnimation
                                 && (newPlacement?.OwnerTeam != lastPlacement?.OwnerTeam
                                     || newPlacement?.CurrentType != lastPlacement?.CurrentType);

            // Always finish transforms first!
            // Do this at the very beginning of animation.
            // RunTask must run before FinishTransforms.
            // If the delegate had been executed, initialize it again then.
            if (scheduledFloatingBoxAnimation == null || scheduledFloatingBoxAnimation.Completed)
                prepareFloatingBox();

            scheduledFloatingBoxAnimation?.RunTask();
            FinishTransforms(true);

            topMask.FadeTo(newPlacement != null && newPlacement.CurrentType != ChoiceType.Pick ? 0.5f : 0,
                300, Easing.OutQuint);
            banPill.FadeTo(!playFullAnimation && newPlacement?.CurrentType == ChoiceType.Ban ? 1 : 0, 300, Easing.OutQuint);
            trophyIcon.FadeTo(!playFullAnimation && newPlacement?.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin ? 1 : 0,
                300, Easing.OutQuint);

            if (newPlacement != null)
            {
                // First: Change colour of pills
                switch (newPlacement.CurrentType)
                {
                    case ChoiceType.Ban:
                        pillBg.FadeColour(TournamentGame.GetTeamColour(newPlacement.OwnerTeam), 300, Easing.OutQuint);
                        break;

                    case ChoiceType.RedWin or ChoiceType.BlueWin:
                        trophyBg.FadeColour(TournamentGame.GetTypeColour(newPlacement.CurrentType), 300, Easing.OutQuint);
                        break;
                }

                // Second: Optional animation
                if (shouldAnimate)
                {
                    runAnimation(newPlacement);
                }
            }

            lastPlacement = newPlacement;
        }

        /// <summary>
        /// Start the animation sequence for the beatmap card.
        /// </summary>
        /// <param name="placement">The chess placement.</param>
        private void runAnimation(ChessPlacement placement)
        {
            LocalisableString choiceText = TournamentGame.GetTeamString(placement.OwnerTeam, true, @"Map");

            instructText.Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold);

            // Initialize animation
            switch (placement.CurrentType)
            {
                case ChoiceType.Pick:
                    instructText.Text = $"{choiceText} picked!";
                    statusIcon.Icon = FontAwesome.Solid.CheckCircle;
                    break;

                case ChoiceType.Ban:
                    instructText.Text = $"{choiceText} banned!";
                    statusIcon.Icon = FontAwesome.Solid.Ban;
                    instructText.Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold);
                    break;

                case ChoiceType.RedWin:
                case ChoiceType.BlueWin:
                    statusIcon.Icon = FontAwesome.Solid.Trophy;
                    instructText.Text = placement.CurrentType == ChoiceType.RedWin ? "Red wins!" : "Blue wins!";
                    break;

                default:
                    // don't do anything
                    return;
            }

            banPill.FadeOut(300, Easing.OutQuint);
            banPill.MoveToY(0, 300, Easing.OutQuint);

            trophyIcon.FadeOut(300, Easing.OutQuint);
            trophyIcon.MoveToY(0, 300, Easing.OutQuint);

            ColourInfo useColour = placement.CurrentType switch
            {
                ChoiceType.Pick => Color4.White,
                ChoiceType.Ban => Color4.Gray,
                ChoiceType.RedWin => TournamentGame.GetTeamColour(TeamColour.Red),
                ChoiceType.BlueWin => TournamentGame.GetTeamColour(TeamColour.Blue),
                _ => Color4.White,
            };

            ColourInfo fadeColour = useColour == Color4.White ? Color4.Black : Color4.White;

            // Reset the state of the floating container
            floatingContainer.Anchor = Anchor.BottomCentre;
            floatingContainer.Origin = Anchor.BottomCentre;
            floatingContainer.Height = 0;

            // Colours may change halfway, using transforms to handle them.
            statusIcon.FadeColour(fadeColour, 300, Easing.OutQuint);
            instructText.FadeColour(fadeColour, 300, Easing.OutQuint);
            floatingBox.FadeColour(useColour, 300, Easing.OutQuint);

            statusIcon.Y = 1.5f;
            statusIcon.Alpha = 0f;

            instructText.Y = 1.5f;
            instructText.Alpha = 0f;

            // Expand floating container and show instructions.
            using (BeginDelayedSequence(200))
            {
                floatingContainer.ResizeHeightTo(1, 700, Easing.OutQuint);

                statusIcon.FadeIn(300, Easing.OutQuint);
                instructText.FadeIn(300, Easing.OutQuint);

                using (BeginDelayedSequence(100))
                {
                    statusIcon.MoveToY(-0.175f, 800, Easing.OutExpo);
                    instructText.Delay(50).MoveToY(0.175f, 800, Easing.OutExpo);
                }
            }

            // Show pills when possible.
            using (BeginDelayedSequence(300 + 1000 + 500))
            {
                if (placement.CurrentType == ChoiceType.Ban)
                {
                    banPill.FadeIn(600, Easing.OutExpo);
                    banPill.MoveToY(15)
                           .Then().MoveToY(0, 600, Easing.OutExpo);
                }
                else if (placement.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin)
                {
                    trophyIcon.FadeIn(600, Easing.OutExpo);
                    trophyIcon.MoveToY(15)
                              .Then().MoveToY(0, 600, Easing.OutExpo);
                }
            }

            // Use a separate scheduler to handle other things around floating container.
            prepareFloatingBox();
        }

        private void prepareFloatingBox() => scheduledFloatingBoxAnimation = Scheduler.AddDelayed(() =>
        {
            floatingContainer.Anchor = Anchor.TopCentre;
            floatingContainer.Origin = Anchor.TopCentre;

            floatingContainer.ResizeHeightTo(0, 1300, Easing.InOutQuint);

            statusIcon.MoveToY(-2f, 1350, Easing.InExpo);
            instructText.MoveToY(-2f, 1450, Easing.InExpo);

            using (BeginDelayedSequence(500))
            {
                statusIcon.FadeOut(600, Easing.OutQuint);
                instructText.FadeOut(600, Easing.OutQuint);
            }
        }, 200 + 100 + 1000);
    }
}
