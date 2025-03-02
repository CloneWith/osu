// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapEditor : FillFlowContainer
    {
        private readonly Bindable<ShowcaseConfig> config = new Bindable<ShowcaseConfig>();

        public ShowcaseBeatmapEditor(Bindable<ShowcaseConfig> config)
        {
            FormCheckBox showListCheckBox;
            this.config.BindTo(config);
            int leftNum = (int)Math.Ceiling(config.Value.Beatmaps.Count / 2f);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 200;
            Direction = FillDirection.Full;
            Spacing = new Vector2(5);

            var leftFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = 200,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                ChildrenEnumerable = config.Value.Beatmaps.Take(leftNum).Select(t => new BeatmapRow(t, config.Value)),
            };
            var rightFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = 200,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                ChildrenEnumerable = config.Value.Beatmaps.Skip(leftNum).Select(t => new BeatmapRow(t, config.Value)),
            };

            Children = new Drawable[]
            {
                new SectionHeader(TournamentShowcaseStrings.BeatmapQueueHeader),
                showListCheckBox = new FormCheckBox
                {
                    Caption = TournamentShowcaseStrings.ShowBeatmapListInShowcase,
                    HintText = TournamentShowcaseStrings.ShowBeatmapListInShowcaseDescription,
                    Current = this.config.Value.ShowMapPool
                },
                new ShowcaseAddButton(TournamentShowcaseStrings.AddBeatmap, () =>
                {
                    var addedBeatmap = new ShowcaseBeatmap();
                    config.Value.Beatmaps.Add(addedBeatmap);

                    if (leftFlow.Children.Count > rightFlow.Children.Count)
                    {
                        rightFlow.Add(new BeatmapRow(addedBeatmap, config.Value));
                    }
                    else
                    {
                        leftFlow.Add(new BeatmapRow(addedBeatmap, config.Value));
                    }
                }),
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.5f),
                        new Dimension(GridSizeMode.Relative, 0.5f),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            leftFlow,
                            rightFlow,
                        },
                    },
                },
            };

            config.BindValueChanged(conf =>
            {
                showListCheckBox.Current = conf.NewValue.ShowMapPool;
                leftFlow.ChildrenEnumerable = conf.NewValue.Beatmaps.Take(leftNum).Select(t => new BeatmapRow(t, config.Value));
                rightFlow.ChildrenEnumerable = conf.NewValue.Beatmaps.Skip(leftNum).Select(t => new BeatmapRow(t, config.Value));
            });
        }
    }

    public partial class BeatmapRow : FillFlowContainer
    {
        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        private ScoreManager scoreManager = null!;

        public bool AllowDeletion = true;

        public ShowcaseBeatmap Beatmap { get; }
        private readonly Bindable<BeatmapInfo> beatmapInfoBindable;
        private readonly Bindable<ScoreInfo?> scoreInfoBindable = new Bindable<ScoreInfo?>();
        private readonly BindableList<Mod> modListBindable = new BindableList<Mod>();
        private Bindable<RulesetInfo> rulesetBindable = new Bindable<RulesetInfo>();
        private DrawableShowcaseBeatmapItem drawableItem = null!;

        private ShowcaseConfig config { get; }

        private readonly Bindable<string> selectorId;

        public BeatmapRow(ShowcaseBeatmap beatmap, ShowcaseConfig config)
        {
            Beatmap = beatmap;
            this.config = config;

            beatmapInfoBindable = new Bindable<BeatmapInfo>(Beatmap.BeatmapInfo);
            modListBindable.BindTo(Beatmap.RequiredMods);
            selectorId = new Bindable<string>(beatmap.Selector.Value?.OnlineID.ToString() ?? string.Empty);

            Masking = true;
            CornerRadius = 10;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Spacing = new Vector2(5);
            Padding = new MarginPadding(10);
            Direction = FillDirection.Full;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scoreManager)
        {
            this.scoreManager = scoreManager;

            // RulesetStore needs to be resolved first.
            rulesetBindable = new Bindable<RulesetInfo>(rulesetStore.GetRuleset(Beatmap.RulesetId) ?? config.FallbackRuleset.Value);

            Children = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            new FormCheckBox
                            {
                                Caption = TournamentShowcaseStrings.TournamentOriginal,
                                HintText = TournamentShowcaseStrings.TournamentOriginalDescription,
                                Current = Beatmap.IsOriginal,
                            },
                            Empty(),
                            new FormNumberBox
                            {
                                Caption = TournamentShowcaseStrings.BeatmapChooserID,
                                HintText = TournamentShowcaseStrings.BeatmapChooserIDDescription,
                                Current = selectorId,
                            },
                        },
                        new[]
                        {
                            Empty(),
                        },
                        new[]
                        {
                            new FormTextBox
                            {
                                Caption = TournamentShowcaseStrings.BeatmapModType,
                                HintText = TournamentShowcaseStrings.BeatmapModTypeDescription,
                                Current = Beatmap.ModString,
                            },
                            Empty(),
                            new FormTextBox
                            {
                                Caption = TournamentShowcaseStrings.BeatmapModIndex,
                                HintText = TournamentShowcaseStrings.BeatmapModIndexDescription,
                                Current = Beatmap.ModIndex,
                            },
                        },
                    },
                },
                new FormTextBox
                {
                    Caption = TournamentShowcaseStrings.DifficultyField,
                    HintText = TournamentShowcaseStrings.DifficultyFieldDescription,
                    Current = Beatmap.DiffField,
                },
                new FormTextBox
                {
                    Caption = TournamentShowcaseStrings.Comment,
                    HintText = TournamentShowcaseStrings.BeatmapCommentDescription,
                    Current = Beatmap.BeatmapComment,
                },
                drawableItem = new DrawableShowcaseBeatmapItem(Beatmap, config)
                {
                    RelativeSizeAxes = Axes.X,
                    AllowEditing = true,
                    AllowDeletion = AllowDeletion,
                    RequestEdit = _ =>
                    {
                        Schedule(() => performer?.PerformFromScreen(s =>
                                s.Push(new ShowcaseSongSelect(beatmapInfoBindable, modListBindable, scoreInfoBindable, rulesetBindable)),
                            new[] { typeof(ShowcaseConfigScreen) }));
                    },
                    RequestDeletion = _ =>
                    {
                        config.Beatmaps.Remove(Beatmap);
                        Expire();
                    },
                },
            };

            selectorId.BindValueChanged(id =>
            {
                bool idValid = int.TryParse(id.NewValue, out int newId) && newId >= 0;

                if (Beatmap.Selector.Value != null)
                    Beatmap.Selector.Value.OnlineID = idValid ? newId : 0;
                else
                {
                    Beatmap.Selector.Value = new ShowcaseUser
                    {
                        OnlineID = idValid ? newId : 0
                    };
                }

                if (idValid)
                    Scheduler.AddOnce(populateSelector);
            }, true);

            beatmapInfoBindable.BindValueChanged(info =>
            {
                Beatmap.BeatmapInfo = info.NewValue;
                Beatmap.BeatmapGuid = info.NewValue.ID;
                Beatmap.BeatmapId = info.NewValue.OnlineID;

                // Reset the score to avoid conflict.
                Beatmap.ShowcaseScore = null;
                Beatmap.ScoreHash = string.Empty;

                drawableItem.Item = Beatmap;
            });

            scoreInfoBindable.BindValueChanged(score =>
            {
                Beatmap.ShowcaseScore = score.NewValue;
                Beatmap.ScoreHash = score.NewValue?.Hash ?? string.Empty;
                drawableItem.Refresh(refreshScoreOnly: true);
            });

            rulesetBindable.BindValueChanged(ruleset =>
            {
                Beatmap.RulesetId = ruleset.NewValue.OnlineID;
                drawableItem.Refresh(needPopulation: true);
            });

            modListBindable.BindCollectionChanged((_, _) => drawableItem.Refresh());

            Beatmap.ModString.BindValueChanged(_ => drawableItem.UpdateModIcon());
            Beatmap.ModIndex.BindValueChanged(_ => drawableItem.UpdateModIcon());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreInfoBindable.Value = scoreManager.GetScore(new ScoreInfo
            {
                Hash = Beatmap.ScoreHash
            })?.ScoreInfo;
        }

        private void populateSelector()
        {
            Task.Run(async () =>
            {
                var req = new GetUserRequest(Beatmap.Selector.Value.OnlineID);

                await api.PerformAsync(req).ConfigureAwait(true);

                var res = req.Response;

                Beatmap.Selector.Value.OnlineID = res?.Id ?? 0;

                Beatmap.Selector.Value.Username = res?.Username ?? string.Empty;
                Beatmap.Selector.Value.Rank = res?.Statistics?.GlobalRank;

                Scheduler.AddOnce(_ => drawableItem.Refresh(needPopulation: true), false);
            });
        }
    }
}
