// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ExtendableBeatmapCard : CompositeDrawable
    {
        private readonly ShowcaseBeatmap beatmap;
        private WorkingBeatmap? workingBeatmap;
        private IBeatmapInfo? beatmapInfo;
        private Sprite setCover = null!;
        private Box infoMask = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private OsuSpriteText modText = null!;
        private Container difficultyIconContainer = null!;
        private DifficultyIcon difficultyIcon = null!;
        private GridContainer infoContainer = null!;
        private OsuTextFlowContainer beatmapInfoFlow = null!;
        private SpriteIcon statusIcon = null!;
        private Container floatingContainer = null!;
        private Box floatingBox = null!;
        private OsuSpriteText instructText = null!;

        private readonly OverlayColourProvider? colourProvider;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        public ExtendableBeatmapCard(ShowcaseBeatmap beatmap, OverlayColourScheme? colourScheme = null)
        {
            Width = 400;
            Height = 300;
            CornerRadius = 10;
            Masking = true;

            this.beatmap = beatmap;

            if (colourScheme != null)
                colourProvider = new OverlayColourProvider(colourScheme.Value);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f)
                },
                setCover = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Alpha = 0.8f,
                },
                infoMask = new Box
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.4f,
                    Colour = ColourInfo.GradientVertical(Color4.Transparent, Color4.Black.Opacity(0.5f)),
                },
                infoContainer = new GridContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.2f,
                    ColumnDimensions = [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            difficultyIconContainer = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                AutoSizeEasing = Easing.OutQuint,
                                AutoSizeDuration = 100,
                                Padding = new MarginPadding { Right = 10 },
                            },
                            beatmapInfoFlow = new OsuTextFlowContainer(t => t.Font = OsuFont.Torus.With(weight: FontWeight.SemiBold))
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                ParagraphSpacing = 0,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    modText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = $@"{beatmap.ModString.Value}{beatmap.ModIndex.Value}",
                                        Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                                        Colour = ModColours.FromModString(beatmap.ModString.Value).Accent,
                                    },
                                    starRatingDisplay = new StarRatingDisplay(new StarDifficulty())
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    },
                                },
                            },
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
                        instructText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                            Text = @"Tournament Original!",
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (beatmap.IsOriginal.Value)
            {
                Scheduler.AddDelayed(() =>
                {
                    prepareFloatingBox();
                    runAnimation();
                }, 3000);
            }

            Task.Run(async () =>
            {
                try
                {
                    workingBeatmap = beatmapManager.GetWorkingBeatmap(new BeatmapInfo { Hash = beatmap.BeatmapHash }, true);

                    if (ReferenceEquals(workingBeatmap, beatmapManager.DefaultBeatmap))
                    {
                        beatmapInfo = await beatmapLookupCache.GetBeatmapAsync(beatmap.BeatmapId).ConfigureAwait(false);
                    }
                    else
                    {
                        beatmapInfo = workingBeatmap.BeatmapInfo;
                    }

                    if (beatmapInfo != null)
                    {
                        setCover.Texture = workingBeatmap.GetBackground();

                        var diff = await difficultyCache.GetDifficultyAsync(beatmapInfo).ConfigureAwait(false);
                        starRatingDisplay.Current.Value = diff ?? new StarDifficulty();
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Error while populating showcase item {e}");
                }
            }).ContinueWith(_ =>
            {
                // Ensure we are working on the correct thread!
                Scheduler.AddOnce(_ =>
                {
                    difficultyIconContainer.Child = difficultyIcon = new DifficultyIcon(beatmapInfo ?? new BeatmapInfo(),
                        rulesets.GetRuleset(beatmap.RulesetId), beatmap.RequiredMods.ToArray())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TooltipType = DifficultyIconTooltipType.None,
                        Scale = new Vector2(1.25f),
                        Alpha = 0
                    };

                    if (beatmapInfo != null)
                    {
                        beatmapInfoFlow.AddParagraph(beatmapInfo.GetDisplayTitleRomanisable(false, false));
                        beatmapInfoFlow.AddParagraph(beatmapInfo.DifficultyName);
                        beatmapInfoFlow.AddParagraph("Mapped by ");
                        beatmapInfoFlow.AddText(beatmapInfo.Metadata.Author.Username, t => t.Colour = colourProvider?.Highlight1 ?? Color4.SkyBlue);
                    }

                    difficultyIcon.ScaleTo(1.75f, 250, Easing.OutQuint);
                    difficultyIcon.FadeIn(300, Easing.OutQuint);
                }, true);
            });
        }

        public void Shrink(int duration = 800)
        {
            this.ResizeHeightTo(80, duration, Easing.OutQuint);
            infoContainer.ResizeHeightTo(1f, duration, Easing.OutQuint);
            infoMask.FadeOut(duration, Easing.OutQuint);
            setCover.FadeTo(0.6f, duration * 0.5f, Easing.OutQuint);
            difficultyIconContainer.MoveToY(0.5f, duration, Easing.OutQuint);
            beatmapInfoFlow.MoveToY(0.5f, duration, Easing.OutQuint);
            modText.ScaleTo(0.75f, duration, Easing.OutQuint);
            starRatingDisplay.ScaleTo(1.05f, duration, Easing.OutQuint);
        }

        public void Expand(int duration = 800)
        {
            this.ResizeHeightTo(400, duration, Easing.OutQuint);
            infoContainer.ResizeHeightTo(0.2f, duration, Easing.OutQuint);
            infoMask.FadeIn(duration, Easing.OutQuint);
            setCover.FadeTo(0.8f, duration * 0.5f, Easing.OutQuint);
            difficultyIconContainer.MoveToY(0.9f, duration, Easing.OutQuint);
            beatmapInfoFlow.MoveToY(0.9f, duration, Easing.OutQuint);
            modText.ScaleTo(1f, duration, Easing.OutQuint);
            starRatingDisplay.ScaleTo(1f, duration, Easing.OutQuint);
        }

        private void prepareFloatingBox() => Scheduler.AddDelayed(() =>
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
        }, 200 + 100 + 2000);

        private void runAnimation()
        {
            // Reset the state of the floating container
            floatingContainer.Anchor = Anchor.BottomCentre;
            floatingContainer.Origin = Anchor.BottomCentre;
            floatingContainer.Height = 0;

            // Colours may change halfway, using transforms to handle them.
            statusIcon.FadeColour(colourProvider?.Content1 ?? Color4.White, 300, Easing.OutQuint);
            instructText.FadeColour(colourProvider?.Content1 ?? Color4.White, 300, Easing.OutQuint);
            floatingBox.FadeColour(colourProvider?.Colour2 ?? Color4.SkyBlue, 300, Easing.OutQuint);

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

            // Use a separate scheduler to handle other things around floating container.
            prepareFloatingBox();
        }
    }
}
