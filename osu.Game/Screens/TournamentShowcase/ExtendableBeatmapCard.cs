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
using osu.Game.Models;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ExtendableBeatmapCard : CompositeDrawable
    {
        private const float star_rating_y_expanded = 0.075f;
        private const float centre_offset = -0.1f;

        private readonly string iconBaseDir;

        private readonly ShowcaseBeatmap beatmap;
        private WorkingBeatmap? workingBeatmap;
        private IBeatmapInfo? beatmapInfo;
        private Sprite setCover = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private Sprite modIcon = null!;
        private Container difficultyIconContainer = null!;
        private DifficultyIcon difficultyIcon = null!;
        private OsuTextFlowContainer beatmapInfoFlow = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        public ExtendableBeatmapCard(ShowcaseBeatmap beatmap, ShowcaseConfig config)
        {
            this.beatmap = beatmap;
            iconBaseDir = config.TournamentName.Value;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore)
        {
            Width = 400;
            Height = 300;
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.8f)
                },
                setCover = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Y = centre_offset,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.8f,
                    FillMode = FillMode.Fill
                },
                modIcon = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.25f,
                    Y = -star_rating_y_expanded + centre_offset,
                    FillMode = FillMode.Fit,
                    Texture = textureStore.Get($"{iconBaseDir}/{beatmap.ModString}{beatmap.ModIndex.Value}")
                },
                starRatingDisplay = new StarRatingDisplay(new StarDifficulty())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Y = star_rating_y_expanded + centre_offset,
                    Scale = new Vector2(1.75f)
                },
                difficultyIconContainer = new Container
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 100,
                    X = 0.07f,
                    Y = 0.9f
                },
                beatmapInfoFlow = new OsuTextFlowContainer(t => t.Font = OsuFont.Torus.With(weight: FontWeight.SemiBold))
                {
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 100,
                    Width = 0.88f,
                    X = 0.14f,
                    Y = 0.9f
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(async () =>
            {
                try
                {
                    workingBeatmap = beatmapManager.GetWorkingBeatmap(new BeatmapInfo { ID = beatmap.BeatmapGuid }, true);

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
            setCover.FadeTo(0.6f, duration * 0.5f, Easing.OutQuint);
            setCover.MoveToY(0, duration, Easing.OutQuint);
            setCover.ResizeHeightTo(1f, duration, Easing.OutQuint);
            this.ResizeHeightTo(80, duration, Easing.OutQuint);
            difficultyIconContainer.MoveToY(0.5f, duration, Easing.OutQuint);
            beatmapInfoFlow.MoveToY(0.5f, duration, Easing.OutQuint);
            modIcon.ResizeWidthTo(0.15f, duration, Easing.OutQuint);
            modIcon.MoveTo(new Vector2(0.4f, -0.15f), duration, Easing.OutQuint);
            starRatingDisplay.MoveTo(new Vector2(0.4f, 0.15f), duration, Easing.OutQuint);
            starRatingDisplay.ScaleTo(1.05f, duration, Easing.OutQuint);
        }

        public void Expand(int duration = 800)
        {
            setCover.FadeIn(duration * 0.5f, Easing.OutQuint);
            setCover.MoveToY(centre_offset, duration, Easing.OutQuint);
            setCover.ResizeHeightTo(0.8f, duration, Easing.OutQuint);
            this.ResizeHeightTo(400, duration, Easing.OutQuint);
            difficultyIconContainer.MoveToY(0.9f, duration, Easing.OutQuint);
            beatmapInfoFlow.MoveToY(0.9f, duration, Easing.OutQuint);
            modIcon.ResizeWidthTo(0.25f, duration, Easing.OutQuint);
            modIcon.MoveTo(new Vector2(0, -star_rating_y_expanded + centre_offset), duration, Easing.OutQuint);
            starRatingDisplay.ScaleTo(1.75f, duration, Easing.OutQuint);
            starRatingDisplay.MoveTo(new Vector2(0, star_rating_y_expanded + centre_offset), duration, Easing.OutQuint);
        }
    }
}
