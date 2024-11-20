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

        public ShowcaseBeatmap Beatmap { get; }
        private readonly Bindable<BeatmapInfo> beatmapInfoBindable = new Bindable<BeatmapInfo>();

        private ShowcaseConfig config { get; set; }

        private readonly Bindable<string> selectorId = new Bindable<string>();

        private DrawableShowcaseBeatmapItem drawableItem;

        public BeatmapRow(ShowcaseBeatmap beatmap, ShowcaseConfig config)
        {
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
                new FormDropdown<BeatmapType>
                {
                    Caption = @"Beatmap Type",
                    HintText = @"Choose a type matching the beatmap mod best. Will be used to show the current icon for the showcase.",
                    Width = 1f,
                    Current = Beatmap.ModType,
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
                    AllowReordering = false,
                    AllowEditing = true,
                    AllowDeletion = true,
                    RequestEdit = _ =>
                    {
                        Schedule(() => performer?.PerformFromScreen(s =>
                                s.Push(new ShowcaseSongSelect(beatmapInfoBindable)),
                            new[] { typeof(ShowcaseConfigScreen) }));
                    },
                    RequestDeletion = _ =>
                    {
                        config.Beatmaps.Remove(beatmap);
                        Expire();
                    }
                }
            };

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

                // TODO: Self updating, or use playlist item's solution.
                drawableItem = new DrawableShowcaseBeatmapItem(Beatmap, config);
            });
        }
    }
}
