// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.SelectV2;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapInfoWedge : CompositeDrawable
    {
        private const float border_weight = 2;

        public Bindable<ShowcaseBeatmap?> Target = new Bindable<ShowcaseBeatmap?>();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private ModSettingChangeTracker? settingChangeTracker;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private StarRatingDisplay starRatingDisplay = null!;
        private FillFlowContainer nameLine = null!;
        private OsuSpriteText modText = null!;
        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText mappedByText = null!;
        private OsuSpriteText mapperText = null!;
        private BeatmapTitleWedge.Statistic lengthStatistic = null!;
        private BeatmapTitleWedge.Statistic bpmStatistic = null!;

        private GridContainer ratingAndNameContainer = null!;
        private AdjustableDifficultyStatisticsDisplay difficultyStatisticsDisplay = null!;

        private Container showcaseInfoContainer = null!;
        private OsuTextFlowContainer difficultyAreaText = null!;
        private OsuTextFlowContainer commentText = null!;

        private CancellationTokenSource? cancellationSource;

        private bool shouldShowShowcaseInfo => Target.Value != null
                                               && (!string.IsNullOrWhiteSpace(Target.Value.DiffField.Value) || !string.IsNullOrWhiteSpace(Target.Value.BeatmapComment.Value));

        public ShowcaseBeatmapInfoWedge()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 10;
            Shear = OsuGame.SHEAR;

            InternalChildren = new Drawable[]
            {
                new WedgeBackground(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new ShearAligningWrapper(titleText = new TruncatingSpriteText
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Shadow = true,
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Top = 5 },
                            Font = OsuFont.Style.Heading2,
                        }),
                        new ShearAligningWrapper(artistText = new TruncatingSpriteText
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Shadow = true,
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            Font = OsuFont.Style.Body,
                        }),
                        new ShearAligningWrapper(new FillFlowContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AutoSizeAxes = Axes.X,
                            Height = 30,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2f, 0f),
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            Children = new Drawable[]
                            {
                                lengthStatistic = new BeatmapTitleWedge.Statistic(OsuIcon.Clock),
                                bpmStatistic = new BeatmapTitleWedge.Statistic(OsuIcon.Metronome)
                                {
                                    TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                    Margin = new MarginPadding { Left = 5f },
                                },
                            },
                        }),
                        new ShearAligningWrapper(ratingAndNameContainer = new GridContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AlwaysPresent = true,
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Margin = new MarginPadding { Vertical = 5f },
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 6),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    Empty(),
                                    nameLine = new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Margin = new MarginPadding { Bottom = 2f },
                                        Children = new Drawable[]
                                        {
                                            difficultyText = new TruncatingSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                            },
                                            mappedByText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Text = " mapped by ",
                                                Font = OsuFont.Style.Body,
                                            },
                                            mapperText = new TruncatingSpriteText
                                            {
                                                Shadow = true,
                                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                                Colour = FumoColours.SeaBlue.Regular,
                                            },
                                        },
                                    },
                                },
                            },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = 53,
                            Padding = new MarginPadding { Bottom = border_weight, Right = border_weight },
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Masking = true,
                                CornerRadius = 10 - border_weight,
                                Shear = OsuGame.SHEAR,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background5.Opacity(0.8f),
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f, Vertical = 7.5f },
                                        Shear = -OsuGame.SHEAR,
                                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                difficultyStatisticsDisplay = new AdjustableDifficultyStatisticsDisplay(autoSize: true),
                                            }
                                        },
                                    },
                                },
                            },
                        }),
                        new ShearAligningWrapper(showcaseInfoContainer = new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Masking = true,
                                CornerRadius = 10 - border_weight,
                                Shear = OsuGame.SHEAR,
                                Padding = new MarginPadding { Bottom = border_weight, Right = border_weight },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background5.Opacity(0.8f),
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(3),
                                        Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Vertical = 7.5f },
                                        Children = new Drawable[]
                                        {
                                            difficultyAreaText = new OsuTextFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Shear = -OsuGame.SHEAR,
                                            },
                                            commentText = new OsuTextFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Shear = -OsuGame.SHEAR,
                                            },
                                        },
                                    },
                                },
                            },
                        }),
                    },
                },
                modText = new OsuSpriteText
                {
                    Name = "Mod text",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Shear = -OsuGame.SHEAR,
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                    Margin = new MarginPadding(20),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            Target.BindValueChanged(_ => updateShowcaseInformation());

            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                updateDifficultyStatistics();

                if (m.NewValue.Any())
                {
                    settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                    settingChangeTracker.SettingChanged += _ => updateDifficultyStatistics();
                }
            }, true);

            updateDisplay();
        }

        private void updateDisplay()
        {
            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            if (beatmap.IsDefault)
            {
                ratingAndNameContainer.FadeOut(300, Easing.OutQuint);
            }
            else
            {
                ratingAndNameContainer.FadeIn(300, Easing.OutQuint);
                titleText.Text = beatmap.Value.Metadata.TitleUnicode;
                artistText.Text = beatmap.Value.Metadata.ArtistUnicode;
                difficultyText.Text = beatmap.Value.BeatmapInfo.DifficultyName;
                mapperText.Text = beatmap.Value.Metadata.Author.Username;
            }

            starRatingDisplay.Current = (Bindable<StarDifficulty>)difficultyCache.GetBindableDifficulty(beatmap.Value.BeatmapInfo, cancellationSource.Token, SongSelect.SELECTION_DEBOUNCE);

            updateDifficultyStatistics();
            updateLengthAndBpmStatistics();
            updateShowcaseInformation();
        }

        private static void formatText(SpriteText t) => t.Font = OsuFont.Style.Body;

        private void updateShowcaseInformation() => Scheduler.AddOnce(() =>
        {
            difficultyAreaText.Clear();
            commentText.Clear();

            showcaseInfoContainer.FadeTo(shouldShowShowcaseInfo ? 1 : 0);

            if (Target.Value == null)
            {
                difficultyAreaText.Hide();
                commentText.Hide();
                return;
            }

            modText.Text = $"{Target.Value.ModString.Value}{Target.Value.ModIndex.Value}";
            modText.FadeColour(ModColours.FromModString(Target.Value.ModString.Value).Accent);

            difficultyAreaText.FadeTo(string.IsNullOrWhiteSpace(Target.Value.DiffField.Value) ? 0 : 1);
            commentText.FadeTo(string.IsNullOrWhiteSpace(Target.Value.BeatmapComment.Value) ? 0 : 1);

            if (!string.IsNullOrWhiteSpace(Target.Value.DiffField.Value))
            {
                difficultyAreaText.AddIcon(FontAwesome.Solid.Star, icon =>
                {
                    icon.Colour = FumoColours.SunshineYellow.Regular;
                    icon.Margin = new MarginPadding { Right = 5 };
                });

                difficultyAreaText.AddText(Target.Value.DiffField.Value, formatText);
            }

            if (!string.IsNullOrWhiteSpace(Target.Value.BeatmapComment.Value))
            {
                commentText.AddIcon(FontAwesome.Solid.CommentAlt, icon =>
                {
                    icon.Colour = FumoColours.SeaBlue.Regular;
                    icon.Margin = new MarginPadding { Right = 5 };
                });
                commentText.AddText(Target.Value.BeatmapComment.Value, formatText);
            }
        });

        private void updateDifficultyStatistics() => Scheduler.AddOnce(() =>
        {
            if (beatmap.IsDefault || ruleset.Value == null)
            {
                difficultyStatisticsDisplay.TooltipContent = null;
                difficultyStatisticsDisplay.Statistics = Array.Empty<BeatmapTitleWedge.StatisticDifficulty.Data>();
                return;
            }

            BeatmapDifficulty originalDifficulty = beatmap.Value.BeatmapInfo.Difficulty;
            BeatmapDifficulty adjustedDifficulty = new BeatmapDifficulty(originalDifficulty);

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(adjustedDifficulty);

            Ruleset rulesetInstance = ruleset.Value.CreateInstance();

            adjustedDifficulty = rulesetInstance.GetAdjustedDisplayDifficulty(adjustedDifficulty, mods.Value);
            difficultyStatisticsDisplay.TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, adjustedDifficulty);

            BeatmapTitleWedge.StatisticDifficulty.Data firstStatistic;

            switch (ruleset.Value.OnlineID)
            {
                case 3:
                    // Account for mania differences locally for now.
                    // Eventually this should be handled in a more modular way, allowing rulesets to return arbitrary difficulty attributes.
                    ILegacyRuleset legacyRuleset = (ILegacyRuleset)rulesetInstance;

                    // For the time being, the key count is static no matter what, because:
                    // - The method doesn't have knowledge of the active keymods. Doing so may require considerations for filtering.
                    // - Using the difficulty adjustment mod to adjust OD doesn't have an effect on conversion.
                    int keyCount = legacyRuleset.GetKeyCount(beatmap.Value.BeatmapInfo, mods.Value);

                    firstStatistic = new BeatmapTitleWedge.StatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsCsMania, keyCount, keyCount, 10);
                    break;

                default:
                    firstStatistic = new BeatmapTitleWedge.StatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsCs, originalDifficulty.CircleSize, adjustedDifficulty.CircleSize, 10);
                    break;
            }

            difficultyStatisticsDisplay.Statistics = new[]
            {
                firstStatistic,
                new BeatmapTitleWedge.StatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsAr, originalDifficulty.ApproachRate, adjustedDifficulty.ApproachRate, 10),
                new BeatmapTitleWedge.StatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsAccuracy, originalDifficulty.OverallDifficulty, adjustedDifficulty.OverallDifficulty, 10),
                new BeatmapTitleWedge.StatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsDrain, originalDifficulty.DrainRate, adjustedDifficulty.DrainRate, 10),
            };
        });

        private CancellationTokenSource? lengthBpmCancellationSource;

        private void updateLengthAndBpmStatistics()
        {
            lengthBpmCancellationSource?.Cancel();
            lengthBpmCancellationSource = new CancellationTokenSource();

            var token = lengthBpmCancellationSource.Token;

            Task.Run(() =>
            {
                var beatmapInfo = beatmap.Value.BeatmapInfo;
                // This can take time as it is a synchronous task.
                var underlyingBeatmap = beatmap.Value.Beatmap;

                double rate = ModUtils.CalculateRateWithMods(mods.Value);

                int bpmMax = FormatUtils.RoundBPM(underlyingBeatmap.ControlPointInfo.BPMMaximum, rate);
                int bpmMin = FormatUtils.RoundBPM(underlyingBeatmap.ControlPointInfo.BPMMinimum, rate);
                int mostCommonBPM = FormatUtils.RoundBPM(60000 / underlyingBeatmap.GetMostCommonBeatLength(), rate);

                double drainLength = Math.Round(underlyingBeatmap.CalculateDrainLength() / rate);
                double hitLength = Math.Round(beatmapInfo.Length / rate);

                Schedule(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    lengthStatistic.Text = hitLength.ToFormattedDuration();
                    lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

                    bpmStatistic.Text = bpmMin == bpmMax
                        ? $"{bpmMin}"
                        : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})";
                });
            }, token);
        }

        protected override void Update()
        {
            base.Update();

            difficultyText.MaxWidth = Math.Max((int)(nameLine.DrawWidth - mappedByText.DrawWidth - mapperText.DrawWidth - 20), 0);

            // Use difficulty colour until it gets too dark to be visible against dark backgrounds.
            Color4 col = starRatingDisplay.DisplayedStars.Value >= OsuColour.STAR_DIFFICULTY_DEFINED_COLOUR_CUTOFF ? colours.Orange1 : starRatingDisplay.DisplayedDifficultyColour;

            difficultyText.Colour = col;
            mappedByText.Colour = col;
            difficultyStatisticsDisplay.AccentColour = col;
        }

        private partial class AdjustableDifficultyStatisticsDisplay : BeatmapTitleWedge.DifficultyStatisticsDisplay, IHasCustomTooltip<AdjustedAttributesTooltip.Data>
        {
            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ITooltip<AdjustedAttributesTooltip.Data> GetCustomTooltip() => new AdjustedAttributesTooltip(colourProvider);

            public AdjustedAttributesTooltip.Data? TooltipContent { get; set; }

            public AdjustableDifficultyStatisticsDisplay(bool autoSize)
                : base(autoSize)
            {
            }
        }
    }
}
