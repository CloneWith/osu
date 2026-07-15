using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseProfileListing : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// Profile items which should be displayed. Should be managed externally.
        /// </summary>
        public readonly BindableList<ShowcaseConfig> Profiles = new BindableList<ShowcaseConfig>();

        /// <summary>
        /// The current filter string. Should be managed externally.
        /// </summary>
        public readonly Bindable<string?> FilterString = new Bindable<string?>();

        /// <summary>
        /// The currently user-selected item.
        /// </summary>
        public IBindable<ShowcaseConfig?> SelectedConfig => selectedConfig;

        private readonly Bindable<ShowcaseConfig?> selectedConfig = new Bindable<ShowcaseConfig?>();

        public IReadOnlyList<ShowcaseProfileItem> DrawableProfiles => profileFlow.FlowingChildren.Cast<ShowcaseProfileItem>().ToArray();

        private readonly ScrollContainer<Drawable> scroll;
        private readonly FillFlowContainer<ShowcaseProfileItem> profileFlow;

        // handle deselection
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public ShowcaseProfileListing()
        {
            InternalChild = scroll = new Scroll
            {
                Masking = false,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Width = 0.8f,
                ScrollbarOverlapsContent = false,
                Padding = new MarginPadding { Right = 5 },
                Child = profileFlow = new FillFlowContainer<ShowcaseProfileItem>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Vertical = 10 },
                },
            };
        }

        private partial class Scroll : OsuScrollContainer
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        }

        protected override void LoadComplete()
        {
            SelectedConfig.BindValueChanged(onSelectedProfileChanged, true);
            Profiles.BindCollectionChanged(profilesChanged, true);
            FilterString.BindValueChanged(s => applyFilterCriteria(s.NewValue), true);
        }

        private void applyFilterCriteria(string? criteria)
        {
            profileFlow.Children.ForEach(r =>
            {
                if (criteria == null)
                    r.MatchingFilter = true;
                else
                {
                    bool matchingFilter = true;

                    string[] filterTerms = r.FilterTerms.Select(t => t.ToString()).ToArray();
                    string[] searchTerms = criteria.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    matchingFilter &= searchTerms.All(searchTerm => filterTerms.Any(filterTerm => checkTerm(filterTerm, searchTerm)));

                    r.MatchingFilter = matchingFilter;
                }
            });

            // Lifted from SearchContainer.
            static bool checkTerm(string haystack, string needle)
            {
                int index = 0;

                for (int i = 0; i < needle.Length; i++)
                {
                    int found = CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle[i], index, CompareOptions.OrdinalIgnoreCase);
                    if (found < 0)
                        return false;

                    index = found + 1;
                }

                return true;
            }
        }

        private void onSelectedProfileChanged(ValueChangedEvent<ShowcaseConfig?> room)
        {
            // scroll selected room into view on selection.
            var drawable = DrawableProfiles.FirstOrDefault(r => r.Config == room.NewValue);
            if (drawable != null)
                scroll.ScrollIntoView(drawable);
        }

        private void profilesChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(args.NewItems != null);

                    addItems(args.NewItems.Cast<ShowcaseConfig>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(args.OldItems != null);

                    if (args.OldItems.Count == profileFlow.Count)
                        clearItems();
                    else
                        removeItems(args.OldItems.Cast<ShowcaseConfig>());

                    break;
            }

            this.FadeInFromZero(300, Easing.OutQuint);
        }

        private void addItems(IEnumerable<ShowcaseConfig> rooms)
        {
            foreach (var room in rooms)
            {
                var drawableRoom = new ShowcaseProfileItem(room)
                {
                    SelectedConfig = selectedConfig,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                profileFlow.Add(drawableRoom);
            }

            applyFilterCriteria(FilterString.Value);
        }

        private void removeItems(IEnumerable<ShowcaseConfig> rooms)
        {
            foreach (var r in rooms)
            {
                profileFlow.RemoveAll(d => d.Config == r, true);

                // selection may have a lease due to being in a sub screen.
                if (SelectedConfig.Value == r && !SelectedConfig.Disabled)
                    selectedConfig.Value = null;
            }
        }

        private void clearItems()
        {
            profileFlow.Clear();

            // selection may have a lease due to being in a sub screen.
            if (!SelectedConfig.Disabled)
                selectedConfig.Value = null;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!SelectedConfig.Disabled)
                selectedConfig.Value = null;
            return base.OnClick(e);
        }

        #region Key selection logic (shared with BeatmapCarousel and DrawableRoomPlaylist)

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.SelectNext:
                    selectNext(1);
                    return true;

                case GlobalAction.SelectPrevious:
                    selectNext(-1);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void selectNext(int direction)
        {
            if (SelectedConfig.Disabled)
                return;

            var visibleItems = DrawableProfiles.AsEnumerable().Where(r => r.IsPresent);

            ShowcaseConfig? config;

            if (SelectedConfig.Value == null)
                config = visibleItems.FirstOrDefault()?.Config;
            else
            {
                if (direction < 0)
                    visibleItems = visibleItems.Reverse();

                config = visibleItems.SkipWhile(r => r.Config != SelectedConfig.Value).Skip(1).FirstOrDefault()?.Config;
            }

            if (config != null)
                selectedConfig.Value = config;
        }

        #endregion
    }
}
