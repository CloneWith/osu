// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private readonly FillFlowContainer? beatmapContainer;

        public ShowcaseBeatmapEditor(Bindable<ShowcaseConfig> config)
        {
            this.config.BindTo(config);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 200;
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
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 200,
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
        private FormDropdown<BeatmapType> mapTypeDropdown = null!;
        private DrawableShowcaseBeatmapItem drawableItem = null!;

        private ShowcaseConfig config { get; set; }

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
                new FormCheckBox
                {
                    Caption = @"Tournament Original",
                    HintText = @"This means the content of the beatmap is exclusively created for this tournament. Shows an indicator when turned on.",
                    Width = 0.49f,
                    Current = Beatmap.IsOriginal
                },
                new FormNumberBox
                {
                    Caption = @"Beatmap Chooser ID",
                    HintText = @"The user who chose or suggested this beatmap.",
                    Width = 0.49f,
                    Current = selectorId
                },
                mapTypeDropdown = new FormDropdown<BeatmapType>
                {
                    Caption = @"Beatmap Type",
                    HintText = @"Choose a type matching the beatmap mod best. Will be used to show a correct icon for the beatmap.",
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
                drawableItem.Refresh(true);
            });

            rulesetBindable.BindValueChanged(ruleset =>
            {
                Beatmap.RulesetId = ruleset.NewValue.OnlineID;
            });
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

                if (res == null)
                    return;

                Beatmap.Selector.Value.OnlineID = res.Id;

                Beatmap.Selector.Value.Username = res.Username;
                Beatmap.Selector.Value.Rank = res.Statistics?.GlobalRank;
            });
        }
    }
}
