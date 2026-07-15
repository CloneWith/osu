using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
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
using osuTK.Graphics;

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
                    Padding = new MarginPadding
                    {
                        Top = Header.HEIGHT + 20,
                        Bottom = ScreenFooter.HEIGHT + 20,
                    },
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = profileListing = new ShowcaseProfileListing
                        {
                            RelativeSizeAxes = Axes.Both,
                            FilterString = { BindTarget = filter },
                        },
                    },
                },
                new Box
                {
                    Name = @"Gradient area",
                    RelativeSizeAxes = Axes.X,
                    Height = Header.HEIGHT + 25,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.6f), Color4.Black.Opacity(0)),
                },
                new Container
                {
                    Name = @"Search area",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.6f,
                    Height = Header.HEIGHT,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING + 10 },
                    Child = searchTextBox = new BasicSearchTextBox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.X,
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            storage.OnProfileChange += reloadProfiles;

            searchTextBox.Current.BindValueChanged(_ => updateFilterDebounced());

            updateFilter();
            reloadProfiles();
        }

        protected override void Dispose(bool isDisposing)
        {
            storage.OnProfileChange -= reloadProfiles;
            base.Dispose(isDisposing);
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
            new FooterButtonNew
            {
                OnCreate = newConfig => Schedule(() => this.Push(new ShowcaseConfigScreen(newConfig))),
            },
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
