// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapEditor : FillFlowContainer
    {
        public Bindable<ShowcaseConfig> Config { get; } = new Bindable<ShowcaseConfig>();

        private FormCheckBox showListCheckBox = null!;
        private DrawableShowcaseBeatmapList beatmapList = null!;

        /// <summary>
        /// Re-entrance guard for the bidirectional sync between <see cref="ShowcaseConfig.Beatmaps"/> and the list's Items.
        /// </summary>
        private bool syncing;

        public ShowcaseBeatmapEditor()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 200;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(5);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SectionHeader(TournamentShowcaseStrings.BeatmapQueueHeader),
                showListCheckBox = new FormCheckBox
                {
                    Caption = TournamentShowcaseStrings.ShowBeatmapListInShowcase,
                    HintText = TournamentShowcaseStrings.ShowBeatmapListInShowcaseDescription,
                    Current = Config.Value.ShowMapPool,
                },
                beatmapList = new DrawableShowcaseBeatmapList(Config),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Config.BindValueChanged(onConfigChanged, true);

            // Sync Items to Beatmaps (for drag reorder in the list container).
            beatmapList.Items.BindCollectionChanged(onItemsChanged);
        }

        private void onConfigChanged(ValueChangedEvent<ShowcaseConfig> conf)
        {
            showListCheckBox.Current = conf.NewValue.ShowMapPool;

            // Re-bind Beatmaps: Items sync to the new config's beatmap list.
            conf.OldValue.Beatmaps.CollectionChanged -= onBeatmapsChanged;

            conf.NewValue.Beatmaps.CollectionChanged += onBeatmapsChanged;

            // Populate the list from the new config.
            syncing = true;
            beatmapList.Items.ReplaceRange(0, beatmapList.Items.Count, conf.NewValue.Beatmaps);
            syncing = false;
        }

        /// <summary>
        /// Handles changes originating from <see cref="ShowcaseConfig.Beatmaps"/> (e.g. add button, item deletion).
        /// Syncs the external beatmap list to the list container's Items.
        /// </summary>
        private void onBeatmapsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (syncing) return;

            syncing = true;
            beatmapList.Items.ReplaceRange(0, beatmapList.Items.Count, Config.Value.Beatmaps);
            syncing = false;
        }

        /// <summary>
        /// Handles changes originating from the list container's Items (e.g. drag-to-reorder).
        /// Syncs the list container's items back to <see cref="ShowcaseConfig.Beatmaps"/>.
        /// </summary>
        private void onItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (syncing) return;

            syncing = true;
            Config.Value.Beatmaps.ReplaceRange(0, Config.Value.Beatmaps.Count, beatmapList.Items);
            syncing = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (Config.Value != null)
                Config.Value.Beatmaps.CollectionChanged -= onBeatmapsChanged;
        }
    }
}
