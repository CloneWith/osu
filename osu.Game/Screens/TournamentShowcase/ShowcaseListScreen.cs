using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    // TODO: Refreshing and other utilities
    public partial class ShowcaseListScreen : OsuScreen, ISubScreenWithTitle
    {
        public string ShortTitle => @"Profiles";

        public LocalisableString LocalisableTitle => TournamentShowcaseStrings.Profiles;

        public override bool ShowFooter => true;

        [Resolved]
        private ShowcaseStorage storage { get; set; } = null!;

        private readonly FillFlowContainer profileListFlow;

        public ShowcaseListScreen()
        {
            InternalChildren = new Drawable[]
            {
                new OsuScrollContainer(Direction.Vertical)
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = false,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 0.8f,
                    ScrollbarOverlapsContent = false,
                    Padding = new MarginPadding
                    {
                        Top = Header.HEIGHT + 20,
                        Right = 5,
                    },
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            // TODO: Add profile creation card
                            profileListFlow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(10),
                            },
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            reloadProfiles();
        }

        private void reloadProfiles()
        {
            profileListFlow.Clear();

            var results = storage.ListTournaments();

            foreach (string result in results)
            {
                try
                {
                    var config = storage.GetConfig(result);

                    if (config != null)
                    {
                        profileListFlow.Add(new ShowcaseProfileItem(result, config)
                        {
                            // TODO: Add more actions
                            OnEdit = () => this.Push(new ShowcaseConfigScreen(result, config)),
                        });
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

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            // After navigating back to this screen, the file might have been updated.
            reloadProfiles();
        }
    }
}
