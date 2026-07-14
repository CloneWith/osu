using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseListScreen : OsuScreen, ISubScreenWithTitle
    {
        public string ShortTitle => @"Profiles";

        public LocalisableString LocalisableTitle => TournamentShowcaseStrings.Profiles;

        public override bool ShowFooter => true;

        [Resolved]
        private ShowcaseStorage storage { get; set; } = null!;

        private readonly Bindable<string?> filter = new Bindable<string?>();

        private readonly ShowcaseProfileListing profileListing;
        private readonly PopoverContainer popoverContainer;
        private readonly SearchTextBox searchTextBox;

        public ShowcaseListScreen()
        {
            Masking = true;

            InternalChildren = new Drawable[]
            {
                popoverContainer = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT + 20 },
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            // TODO: Add profile creation card
                            profileListing = new ShowcaseProfileListing
                            {
                                RelativeSizeAxes = Axes.X,
                                FilterString = { BindTarget = filter },
                            },
                        },
                    },
                },
                new Container
                {
                    Name = @"Search area",
                    RelativeSizeAxes = Axes.X,
                    Height = Header.HEIGHT,
                    Child = searchTextBox = new BasicSearchTextBox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.6f,
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            searchTextBox.Current.BindValueChanged(_ => updateFilterDebounced());

            updateFilter();
            reloadProfiles();
        }

        public void UpdateFilter() => Scheduler.AddOnce(updateFilter);

        private ScheduledDelegate? scheduledFilterUpdate;

        private void updateFilterDebounced()
        {
            scheduledFilterUpdate?.Cancel();
            scheduledFilterUpdate = Scheduler.AddDelayed(UpdateFilter, 200);
        }

        private void updateFilter()
        {
            scheduledFilterUpdate?.Cancel();
            filter.Value = searchTextBox.Current.Value;
        }

        private void reloadProfiles()
        {
            profileListing.Profiles.Clear();

            var results = storage.ListTournaments();

            foreach (string result in results)
            {
                try
                {
                    var config = storage.GetConfig(result);

                    if (config != null)
                    {
                        profileListing.Profiles.Add(config);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Unable to load \"{result}\" as a valid showcase config: {e.Message}", level: LogLevel.Verbose);
                }
            }
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new FooterButtonRefresh
            {
                Action = reloadProfiles,
            },
            new FooterButtonOpenExternally
            {
                Action = () => storage.PresentExternally(),
            },
        };

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            profileListing.FadeIn(300, Easing.OutQuint);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            onLeaving();
            return base.OnExiting(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            onLeaving();
            base.OnSuspending(e);
            profileListing.FadeOut(300, Easing.OutQuint);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            onReturning();
            profileListing.FadeIn(300, Easing.OutQuint);

            // After navigating back to this screen, the file might have been updated.
            reloadProfiles();
        }

        protected override void OnFocus(FocusEvent e)
        {
            searchTextBox.TakeFocus();
        }

        private void onReturning()
        {
            searchTextBox.HoldFocus = true;
        }

        private void onLeaving()
        {
            searchTextBox.HoldFocus = false;
            popoverContainer.HidePopover();
        }
    }
}
