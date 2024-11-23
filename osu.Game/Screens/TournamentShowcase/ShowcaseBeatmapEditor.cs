// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapEditor : FillFlowContainer
    {
        private readonly Bindable<ShowcaseConfig> config = new Bindable<ShowcaseConfig>();

        private readonly FillFlowContainer? beatmapContainer;

        public ShowcaseBeatmapEditor(Bindable<ShowcaseConfig> config)
        {
            this.config.BindTo(config);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(10);
            Children = new Drawable[]
            {
                new SectionHeader(@"Beatmap Queue"),
                new ShowcaseAddButton(@"Add beatmap", () =>
                {
                    var addedBeatmap = new ShowcaseBeatmap();
                    config.Value.Beatmaps.Add(addedBeatmap);
                    beatmapContainer?.Add(new BeatmapRow(addedBeatmap, config.Value));
                }),
                beatmapContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    ChildrenEnumerable = config.Value.Beatmaps.Select(t => new BeatmapRow(t, config.Value))
                }
            };

            config.BindValueChanged(conf =>
            {
                beatmapContainer.Clear();
                beatmapContainer.ChildrenEnumerable = conf.NewValue.Beatmaps.Select(t => new BeatmapRow(t, config.Value));
            });
        }
    }

    public partial class BeatmapRow : FillFlowContainer
    {
        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private DialogOverlay? dialogOverlay { get; set; }

        private ScoreManager scoreManager = null!;

        public ShowcaseBeatmap Beatmap { get; }
        private readonly Bindable<BeatmapInfo> beatmapInfoBindable = new Bindable<BeatmapInfo>();
        private readonly Bindable<ScoreInfo?> scoreInfoBindable = new Bindable<ScoreInfo?>();

        private ShowcaseConfig config { get; set; }

        private readonly Bindable<string> selectorId = new Bindable<string>();

        public BeatmapRow(ShowcaseBeatmap beatmap, ShowcaseConfig config)
        {
            FormDropdown<BeatmapType> mapTypeDropdown;
            DrawableShowcaseBeatmapItem drawableItem;
            Beatmap = beatmap;
            beatmapInfoBindable.Value = Beatmap.BeatmapInfo;

            this.config = config;
            selectorId.Value = beatmap.SelectorId.ToString();

            Masking = true;
            CornerRadius = 10;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Spacing = new Vector2(5);
            Padding = new MarginPadding(10);
            Direction = FillDirection.Full;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new FormCheckBox
                {
                    Caption = @"Tournament Original",
                    HintText = @"This means the content of the beatmap is exclusively created for this tournament. Shows an indicator when turned on.",
                    Width = 0.49f,
                    Current = Beatmap.IsOriginal
                },
                new FormNumberBox
                {
                    Caption = @"Selector ID",
                    Width = 0.49f,
                    Current = selectorId
                },
                mapTypeDropdown = new FormDropdown<BeatmapType>
                {
                    Caption = @"Beatmap Type",
                    HintText = @"Choose a type matching the beatmap mod best. Will be used to show the current icon for the showcase.",
                    Width = 0.49f,
                    Current = Beatmap.ModType,
                    Items = new[]
                    {
                        BeatmapType.NoMod,
                        BeatmapType.HardRock,
                        BeatmapType.DoubleTime,
                        BeatmapType.Hidden,
                        BeatmapType.FlashLight,
                        BeatmapType.FreeMod,
                        BeatmapType.Tiebreaker,
                        BeatmapType.Extra
                    }
                },
                new FormTextBox
                {
                    Caption = @"Mod Index",
                    HintText = @"The index of the beatmap in this type of mod.",
                    Width = 0.49f,
                    Current = Beatmap.ModIndex,
                },
                new FormTextBox
                {
                    Caption = @"Difficulty Area",
                    HintText = @"The major area this beatmap lays difficulty on.",
                    Width = 1f,
                    Current = Beatmap.BeatmapArea
                },
                new FormTextBox
                {
                    Caption = @"Comment",
                    HintText = @"Have something else to show on the showcase screen?",
                    Width = 1f,
                    Current = Beatmap.BeatmapComment
                },
                drawableItem = new DrawableShowcaseBeatmapItem(Beatmap, config)
                {
                    RelativeSizeAxes = Axes.X,
                    AllowSelection = false,
                    AllowReordering = false,
                    AllowEditing = true,
                    AllowDeletion = true,
                    RequestResults = _ =>
                    {
                        if (Beatmap.ShowcaseScore == null)
                        {
                            dialogOverlay?.Push(new ProfileCheckFailedDialog
                            {
                                HeaderText = @"No score selected!",
                                BodyText = @"An Autoplay-generated score would be used for showcase."
                            });
                        }
                        else
                        {
                            Schedule(() => performer?.PerformFromScreen(s => s.Push(new SoloResultsScreen(Beatmap.ShowcaseScore)),
                                new[] { typeof(ShowcaseConfigScreen) }));
                        }
                    },
                    RequestEdit = _ =>
                    {
                        Schedule(() => performer?.PerformFromScreen(s =>
                                s.Push(new ShowcaseSongSelect(beatmapInfoBindable, scoreInfoBindable)),
                            new[] { typeof(ShowcaseConfigScreen) }));
                    },
                    RequestDeletion = _ =>
                    {
                        config.Beatmaps.Remove(beatmap);
                        Expire();
                    }
                }
            };

            mapTypeDropdown.Current.BindValueChanged(type =>
            {
                switch (type.NewValue)
                {
                    case BeatmapType.NoMod:
                        Beatmap.ModString = "NM";
                        break;

                    case BeatmapType.Tiebreaker:
                        Beatmap.ModString = "TB";
                        break;

                    case BeatmapType.Extra:
                        Beatmap.ModString = "EX";
                        break;

                    case BeatmapType.HardRock:
                        Beatmap.ModString = "HR";
                        break;

                    case BeatmapType.DoubleTime:
                        Beatmap.ModString = "DT";
                        break;

                    case BeatmapType.Hidden:
                        Beatmap.ModString = "HD";
                        break;

                    case BeatmapType.FlashLight:
                        Beatmap.ModString = "FL";
                        break;

                    case BeatmapType.FreeMod:
                        Beatmap.ModString = "FM";
                        break;
                }
            });

            selectorId.BindValueChanged(id =>
            {
                bool idValid = int.TryParse(id.NewValue, out int newId) && newId >= 0;
                Beatmap.SelectorId.Value = idValid ? newId : 0;
            }, true);

            beatmapInfoBindable.BindValueChanged(info =>
            {
                Beatmap.BeatmapInfo = info.NewValue;
                Beatmap.BeatmapGuid = info.NewValue.ID;
                Beatmap.BeatmapId = info.NewValue.OnlineID;

                // Is there a better solution?
                drawableItem.Expire();
                Add(drawableItem = new DrawableShowcaseBeatmapItem(Beatmap, config)
                {
                    RelativeSizeAxes = Axes.X,
                    AllowSelection = false,
                    AllowReordering = false,
                    AllowEditing = true,
                    AllowDeletion = true,
                    RequestResults = _ =>
                    {
                        if (Beatmap.ShowcaseScore == null)
                        {
                            dialogOverlay?.Push(new ProfileCheckFailedDialog
                            {
                                HeaderText = @"No score selected!",
                                BodyText = @"An Autoplay-generated score would be used for showcase."
                            });
                        }
                        else
                        {
                            Schedule(() => performer?.PerformFromScreen(s => s.Push(new SoloResultsScreen(Beatmap.ShowcaseScore)),
                                new[] { typeof(ShowcaseConfigScreen) }));
                        }
                    },
                    RequestEdit = _ =>
                    {
                        Schedule(() => performer?.PerformFromScreen(s =>
                                s.Push(new ShowcaseSongSelect(beatmapInfoBindable, scoreInfoBindable)),
                            new[] { typeof(ShowcaseConfigScreen) }));
                    },
                    RequestDeletion = _ =>
                    {
                        config.Beatmaps.Remove(beatmap);
                        Expire();
                    }
                });
            });

            scoreInfoBindable.BindValueChanged(score =>
            {
                Beatmap.ShowcaseScore = score.NewValue;
                Beatmap.ScoreHash = score.NewValue?.Hash ?? string.Empty;
            });
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scoreManager)
        {
            this.scoreManager = scoreManager;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreInfoBindable.Value = scoreManager.GetScore(new ScoreInfo
            {
                Hash = Beatmap.ScoreHash
            })?.ScoreInfo;
        }
    }
}
