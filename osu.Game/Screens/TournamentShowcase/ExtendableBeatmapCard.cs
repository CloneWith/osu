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
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ExtendableBeatmapCard : CompositeDrawable
    {
        private const float star_rating_y_expanded = 0.075f;
        private const float centre_offset = -0.1f;
        private const float star_rating_x = 0.3f;
        private const float star_rating_y = 0.4f;

        private readonly string iconBaseDir;

        private readonly ShowcaseBeatmap beatmap;
        private IBeatmapInfo? beatmapInfo;
        private UpdateableOnlineBeatmapSetCover setCover = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private Container difficultyIconContainer = null!;
        private DifficultyIcon difficultyIcon = null!;
        private FillFlowContainer beatmapInfoFlow = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

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
                setCover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.Card, timeBeforeLoad: 0)
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.8f,
                },
                new Sprite
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
                beatmapInfoFlow = new FillFlowContainer
                {
                    Origin = Anchor.CentreLeft,
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 100,
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
                    beatmapInfo = await beatmapLookupCache.GetBeatmapAsync(beatmap.BeatmapId).ConfigureAwait(false);

                    if (beatmapInfo != null)
                    {
                        setCover.OnlineInfo = beatmapInfo.BeatmapSet as APIBeatmapSet;

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
                        beatmapInfo?.Ruleset, beatmap.RequiredMods.ToArray())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TooltipType = DifficultyIconTooltipType.None,
                        Scale = new Vector2(1.25f),
                        Alpha = 0
                    };
                    difficultyIcon.ScaleTo(1.75f, 250, Easing.OutQuint);
                    difficultyIcon.FadeIn(300, Easing.OutQuint);
                }, true);
            });
        }

        public void Shrink()
        {
            // TODO: Animation
        }

        public void Expand()
        {
            // TODO: Animation
        }
    }
}
