// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapInfoWedge : CompositeDrawable
    {
        private const float border_weight = 2;

        public Bindable<ShowcaseBeatmap?> Target = new Bindable<ShowcaseBeatmap?>();

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private ModSettingChangeTracker? settingChangeTracker;

        private MarqueeContainer titleLabel = null!;
        private MarqueeContainer artistLabel = null!;
        private OsuSpriteText modText = null!;
        private BeatmapTitleWedge.Statistic lengthStatistic = null!;
        private BeatmapTitleWedge.Statistic bpmStatistic = null!;

        private Container showcaseInfoContainer = null!;
        private BeatmapTitleWedge.Statistic difficultyAreaText = null!;
        private BeatmapTitleWedge.Statistic commentText = null!;

        private FillFlowContainer statisticsFlow = null!;

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
                    Padding = new MarginPadding
                    {
                        Left = SongSelect.WEDGE_CONTENT_MARGIN,
                    },
                    Children = new Drawable[]
                    {
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Title.Size,
                            Margin = new MarginPadding { Top = 5f, Bottom = -4f },
                            Child = titleLabel = new MarqueeContainer
                            {
                                OverflowSpacing = 50,
                            },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Heading2.Size,
                            Margin = new MarginPadding { Left = 1f },
                            Child = artistLabel = new MarqueeContainer
                            {
                                OverflowSpacing = 50,
                            },
                        }),
                        new ShearAligningWrapper(statisticsFlow = new FillFlowContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AutoSizeAxes = Axes.X,
                            Height = 30,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2f, 0f),
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
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Padding = new MarginPadding { Right = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Child = new BeatmapTitleWedge.DifficultyDisplay(),
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
                                Padding = new MarginPadding { Bottom = border_weight, Right = border_weight },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Shear = OsuGame.SHEAR,
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background5.Opacity(0.8f),
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            difficultyAreaText = new BeatmapTitleWedge.Statistic(FontAwesome.Solid.Star),
                                            commentText = new BeatmapTitleWedge.Statistic(FontAwesome.Solid.CommentAlt)
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

            working.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            Target.BindValueChanged(_ => updateShowcaseInformation());

            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                updateLengthAndBpmStatistics();

                settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                settingChangeTracker.SettingChanged += _ => updateLengthAndBpmStatistics();
            }, true);

            updateDisplay();

            statisticsFlow.AutoSizeDuration = 100;
            statisticsFlow.AutoSizeEasing = Easing.OutQuint;
        }

        private void updateDisplay()
        {
            var metadata = working.Value.Metadata;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            titleLabel.CreateContent = () => new OsuSpriteText
            {
                Text = titleText,
                Shadow = true,
                Font = OsuFont.Style.Title,
            };

            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            artistLabel.CreateContent = () => new OsuSpriteText
            {
                Text = artistText,
                Shadow = true,
                Font = OsuFont.Style.Heading2,
            };

            updateLengthAndBpmStatistics();
            updateShowcaseInformation();
        }

        private void updateShowcaseInformation() => Scheduler.AddOnce(() =>
        {
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
                difficultyAreaText.Text = Target.Value.DiffField.Value;
            }

            if (!string.IsNullOrWhiteSpace(Target.Value.BeatmapComment.Value))
            {
                commentText.Text = Target.Value.BeatmapComment.Value;
            }
        });

        private CancellationTokenSource? lengthBpmCancellationSource;

        private void updateLengthAndBpmStatistics()
        {
            lengthBpmCancellationSource?.Cancel();
            lengthBpmCancellationSource = new CancellationTokenSource();

            var token = lengthBpmCancellationSource.Token;

            Task.Run(() =>
            {
                var beatmapInfo = working.Value.BeatmapInfo;
                // This can take time as it is a synchronous task.
                var beatmap = working.Value.Beatmap;

                double rate = ModUtils.CalculateRateWithMods(mods.Value);

                int bpmMax = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMaximum, rate);
                int bpmMin = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMinimum, rate);
                int mostCommonBPM = FormatUtils.RoundBPM(60000 / beatmap.GetMostCommonBeatLength(), rate);

                double drainLength = Math.Round(beatmap.CalculateDrainLength() / rate);
                double hitLength = Math.Round(beatmapInfo.Length / rate);

                Schedule(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    lengthStatistic.Text = hitLength.ToFormattedDuration();
                    lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

                    bpmStatistic.Text = bpmMin == bpmMax
                        ? $"{bpmMin}"
                        : LocalisableString.Interpolate($"{bpmMin}-{bpmMax} ({SongSelectStrings.MostlyBPM(mostCommonBPM)})");
                });
            }, token);
        }
    }
}
