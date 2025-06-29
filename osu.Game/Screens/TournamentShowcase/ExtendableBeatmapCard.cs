// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Models;
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
        private StarRatingDisplay starRatingDisplay = null!;
        private OsuSpriteText modText = null!;
        private Container difficultyIconContainer = null!;
        private DifficultyIcon difficultyIcon = null!;
        private GridContainer infoContainer = null!;
        private OsuTextFlowContainer beatmapInfoFlow = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        public ExtendableBeatmapCard(ShowcaseBeatmap beatmap)
        {
            Width = 400;
            Height = 300;
            CornerRadius = 10;
            Masking = true;

            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore)
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
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.8f,
                    FillMode = FillMode.Fill,
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
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
                        beatmapInfoFlow.AddText(beatmapInfo.Metadata.Author.Username, t => t.Colour = Color4.SkyBlue);
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
            setCover.ResizeHeightTo(1f, duration, Easing.OutQuint);
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
            setCover.ResizeHeightTo(0.8f, duration, Easing.OutQuint);
            setCover.FadeIn(duration * 0.5f, Easing.OutQuint);
            difficultyIconContainer.MoveToY(0.9f, duration, Easing.OutQuint);
            beatmapInfoFlow.MoveToY(0.9f, duration, Easing.OutQuint);
            modText.ScaleTo(1f, duration, Easing.OutQuint);
            starRatingDisplay.ScaleTo(1f, duration, Easing.OutQuint);
        }
    }
}
