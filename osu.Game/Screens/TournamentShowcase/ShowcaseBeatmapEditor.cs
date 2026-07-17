// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Models;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapEditor : FillFlowContainer
    {
        public Bindable<ShowcaseConfig> Config { get; } = new Bindable<ShowcaseConfig>();

        private FormCheckBox showListCheckBox = null!;
        private FillFlowContainer beatmapFlow = null!;

        public ShowcaseBeatmapEditor()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 200;
            Direction = FillDirection.Full;
            Spacing = new Vector2(5);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = 200,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
            };

            Children = new Drawable[]
            {
                new SectionHeader(TournamentShowcaseStrings.BeatmapQueueHeader),
                showListCheckBox = new FormCheckBox
                {
                    Caption = TournamentShowcaseStrings.ShowBeatmapListInShowcase,
                    HintText = TournamentShowcaseStrings.ShowBeatmapListInShowcaseDescription,
                    Current = Config.Value.ShowMapPool,
                },
                // TODO: Remove this and use a more initiative method, like the footer button
                new ShowcaseAddButton(TournamentShowcaseStrings.AddBeatmap, () =>
                {
                    var addedBeatmap = new ShowcaseBeatmap();
                    Config.Value.Beatmaps.Add(addedBeatmap);

                    beatmapFlow.Add(new DrawableShowcaseBeatmapItem(addedBeatmap, Config.Value));
                }),
                beatmapFlow,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Config.BindValueChanged(conf =>
            {
                showListCheckBox.Current = conf.NewValue.ShowMapPool;
                beatmapFlow.ChildrenEnumerable = conf.NewValue.Beatmaps.Select(t => new DrawableShowcaseBeatmapItem(t, Config.Value));
            }, true);
        }
    }
}
