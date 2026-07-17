// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Models;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    /// <summary>
    /// A scrollable list of <see cref="ShowcaseBeatmap"/>s that supports drag-to-reorder.
    /// Used by <see cref="ShowcaseBeatmapEditor"/> to display and manage the beatmap queue.
    /// </summary>
    public partial class DrawableShowcaseBeatmapList : OsuRearrangeableListContainer<ShowcaseBeatmap>
    {
        private readonly Bindable<ShowcaseConfig> config;

        public DrawableShowcaseBeatmapList(Bindable<ShowcaseConfig> config)
        {
            this.config = config;

            // The list lives inside a parent scroll container, so auto-size vertically
            // to show all items without a nested scrollbar.
            RelativeSizeAxes = Axes.X;
            Height = 600;
            // AutoSizeAxes = Axes.Y;

            // The base constructor sets RelativeSizeAxes = Axes.Both on the internal scroll container.
            // Override it to match — auto-size vertically so the container grows with its content.
            // ScrollContainer.RelativeSizeAxes = Axes.X;
            // ScrollContainer.AutoSizeAxes = Axes.Y;
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer
        {
            ScrollbarVisible = false,
        };

        protected override FillFlowContainer<RearrangeableListItem<ShowcaseBeatmap>> CreateListFillFlowContainer() =>
            new FillFlowContainer<RearrangeableListItem<ShowcaseBeatmap>>
            {
                LayoutDuration = 200,
                LayoutEasing = Easing.OutQuint,
                Spacing = new Vector2(5),
            };

        protected override OsuRearrangeableListItem<ShowcaseBeatmap> CreateOsuDrawable(ShowcaseBeatmap item) =>
            new DrawableShowcaseBeatmapItem(item, config.Value);
    }
}
