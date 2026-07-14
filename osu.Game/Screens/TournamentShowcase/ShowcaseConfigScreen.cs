// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Toolbar;
using osu.Game.Scoring;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseConfigScreen : OsuScreen, ISubScreenWithTitle, IKeyBindingHandler<PlatformAction>
    {
        public string ShortTitle => @"Configuration";

        public LocalisableString LocalisableTitle => TournamentShowcaseStrings.Configuration;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private ShowcaseStorage storage { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        private const float sizing_duration = 200;

        public override bool ShowFooter => true;

        #region Drawable variables

        private FillFlowContainer tournamentInfoSection = null!;
        private FillFlowContainer settingsSection = null!;
        private ShowcaseBeatmapEditor beatmapSection = null!;

        private ToolbarRulesetSelector rulesetSelector = null!;
        private FormTextBox tournamentNameInput = null!;
        private FormTextBox roundNameInput = null!;
        private FormEnumDropdown<OverlayColourScheme> colourSchemeDropdown = null!;
        private FormCheckBox useCustomIntroSwitch = null!;
        private FillFlowContainer introEditor = null!;

        private FooterButtonSave saveButton = null!;

        #endregion

        private readonly string filename;
        private readonly Bindable<ShowcaseConfig> currentProfile = new Bindable<ShowcaseConfig>();
        private readonly Bindable<ShowcaseConfigTab> currentTab = new Bindable<ShowcaseConfigTab>();

        public ShowcaseConfigScreen(string filename, ShowcaseConfig config)
        {
            Alpha = 0;

            this.filename = filename;
            currentProfile.Value = config;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentProfile.Value.IntroBeatmap.Value ??= new ShowcaseBeatmap();

            #region Sections

            tournamentInfoSection = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = sizing_duration,
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new SectionHeader(TournamentShowcaseStrings.DefaultRuleset),
                    new OsuScrollContainer(Direction.Horizontal)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Margin = new MarginPadding { Vertical = -10 },
                        ScrollbarVisible = false,
                        Child = rulesetSelector = new ToolbarRulesetSelector
                        {
                            Current = currentProfile.Value.FallbackRuleset,
                        },
                    },
                    new SectionHeader(TournamentShowcaseStrings.TournamentInfoHeader),
                    tournamentNameInput = new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.TournamentName,
                        PlaceholderText = TournamentShowcaseStrings.TournamentNamePlaceholder,
                        HintText = TournamentShowcaseStrings.TournamentNameDescription,
                        Current = currentProfile.Value.TournamentName,
                        TabbableContentContainer = this,
                    },
                    roundNameInput = new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.TournamentRound,
                        PlaceholderText = TournamentShowcaseStrings.TournamentRoundPlaceholder,
                        HintText = TournamentShowcaseStrings.TournamentRoundDescription,
                        Current = currentProfile.Value.RoundName,
                        TabbableContentContainer = this,
                    },
                    new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.DateAndTime,
                        PlaceholderText = "2024/11/4 5:14:19:191 UTC+8",
                        HintText = TournamentShowcaseStrings.DateAndTimeDescription,
                        Current = currentProfile.Value.DateTime,
                        TabbableContentContainer = this,
                    },
                    new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.Comment,
                        PlaceholderText = "Welcome to osu!",
                        HintText = TournamentShowcaseStrings.IntroCommentDescription,
                        Current = currentProfile.Value.Comment,
                        TabbableContentContainer = this,
                    },
                },
            };
            settingsSection = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = sizing_duration,
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new SectionHeader(TournamentShowcaseStrings.ShowcaseSettingsHeader),
                    colourSchemeDropdown = new FormEnumDropdown<OverlayColourScheme>
                    {
                        Caption = TournamentShowcaseStrings.ColourScheme,
                        HintText = TournamentShowcaseStrings.ColourSchemeDescription,
                        Current = currentProfile.Value.ColourScheme,
                    },
                    new FormEnumDropdown<ShowcaseLayout>
                    {
                        Caption = TournamentShowcaseStrings.InterfaceLayout,
                        HintText = TournamentShowcaseStrings.InterfaceLayoutDescription,
                        Current = currentProfile.Value.Layout,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = TournamentShowcaseStrings.AspectRatio,
                        HintText = TournamentShowcaseStrings.AspectRatioDescription,
                        Current = currentProfile.Value.AspectRatio,
                        TransferValueOnCommit = true,
                        TabbableContentContainer = this,
                    },
                    new FormSliderBar<int>
                    {
                        Caption = TournamentShowcaseStrings.TransformDuration,
                        HintText = TournamentShowcaseStrings.TransformDurationDescription,
                        Current = currentProfile.Value.TransformDuration,
                        TransferValueOnCommit = true,
                        TabbableContentContainer = this,
                    },
                    new FormSliderBar<int>
                    {
                        Caption = TournamentShowcaseStrings.StartCountdownDuration,
                        HintText = TournamentShowcaseStrings.StartCountdownDurationDescription,
                        Current = currentProfile.Value.StartCountdown,
                        TransferValueOnCommit = true,
                        TabbableContentContainer = this,
                    },
                    new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.OutroTitle,
                        PlaceholderText = @"Thanks for watching!",
                        Current = currentProfile.Value.OutroTitle,
                        TabbableContentContainer = this,
                    },
                    new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.OutroSubtitle,
                        PlaceholderText = @"Take care of yourself, and be well.",
                        Current = currentProfile.Value.OutroSubtitle,
                        TabbableContentContainer = this,
                    },
                },
            };
            beatmapSection = new ShowcaseBeatmapEditor(currentProfile);
            introEditor = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = sizing_duration,
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new SectionHeader(TournamentShowcaseStrings.IntroBeatmapHeader),
                    useCustomIntroSwitch = new FormCheckBox
                    {
                        Caption = TournamentShowcaseStrings.UseCustomIntroBeatmap,
                        HintText = TournamentShowcaseStrings.UseCustomIntroBeatmapDescription,
                        Current = currentProfile.Value.UseCustomIntroBeatmap,
                    },
                    new BeatmapRow(currentProfile.Value.IntroBeatmap.Value, currentProfile.Value)
                    {
                        AllowDeletion = false,
                    },
                },
            };

            #endregion

            #region Basic Layout

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.6f),
                },
                new GridContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = ScreenFooter.HEIGHT },
                    Width = 0.8f,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.1f),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new OsuTabControl<ShowcaseConfigTab>
                            {
                                Name = @"Top Tab Control",
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                Current = { BindTarget = currentTab },
                            },
                        },
                        new Drawable[]
                        {
                            new OsuContextMenuContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new OsuScrollContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarOverlapsContent = false,
                                    Child = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(10),
                                        Direction = FillDirection.Full,
                                        Children = new Drawable[]
                                        {
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
                                                    new Dimension(),
                                                    // Add a 10px gap between two columns
                                                    new Dimension(GridSizeMode.Absolute, 10),
                                                    new Dimension(),
                                                },
                                                Content = new[]
                                                {
                                                    new[]
                                                    {
                                                        tournamentInfoSection,
                                                        Empty(),
                                                        settingsSection,
                                                    },
                                                },
                                            },
                                            introEditor,
                                            beatmapSection,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            #endregion
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            saveButton = new FooterButtonSave
            {
                Action = () =>
                {
                    if (checkConfig())
                        storage.SaveChangesTo(currentProfile.Value, filename);
                },
            },
            new FooterButtonStartShowcase
            {
                Hotkey = GlobalAction.ShowcaseStart,
                Action = startShowcase,
            },
            new FooterButtonOpenExternally
            {
                Action = () => storage.PresentFileExternally(filename),
            },
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentTab.BindValueChanged(currentTabChanged);
            colourSchemeDropdown.Current.BindValueChanged(e => colourProvider.ChangeColourScheme(e.NewValue), true);

            this.FadeInFromZero(500, Easing.OutQuint);

            currentTab.TriggerChange();
        }

        /// <summary>
        /// Check all necessary fields to ensure that the profile can be saved and used properly.
        /// In case an issue is found, a popup prompt will appear.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool checkConfig()
        {
            bool isValid = rulesetSelector.Current.Value != null
                           && tournamentNameInput.Current.Value != null
                           && roundNameInput.Current.Value != null
                           && tournamentNameInput.Current.Value.Trim() != string.Empty
                           && roundNameInput.Current.Value.Trim() != string.Empty;

            if (!isValid)
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Text = TournamentShowcaseStrings.ProfileErrorDialogText,
                });

                return false;
            }

            if (!filename.IsSafeForFilename(out LocalisableString error, 50))
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Text = error,
                });

                return false;
            }

            if (currentProfile.Value.Beatmaps.Count == 0)
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Text = TournamentShowcaseStrings.EmptyBeatmapListDialogText,
                });

                return false;
            }

            if (useCustomIntroSwitch.Current.Value && !currentProfile.Value.IntroBeatmap.Value.IsValid())
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Text = TournamentShowcaseStrings.NullIntroMapDialogText,
                });

                return false;
            }

            return isValid;
        }

        private bool checkAndFetchScores()
        {
            if (currentProfile.Value.Beatmaps.All(b => b.ShowcaseScore != null))
                return true;

            // Try fetching missing scores once
            currentProfile.Value.Beatmaps.Where(b => b.ShowcaseScore == null)
                          .ForEach(b => b.ShowcaseScore = scoreManager.GetScore(new ScoreInfo
                          {
                              Hash = b.ScoreHash
                          })?.ScoreInfo);

            return currentProfile.Value.Beatmaps.All(b => b.ShowcaseScore != null);
        }

        private void startShowcase()
        {
            Action launchAction = () => this.Push(new ShowcaseViewScreen(currentProfile.Value));

            if (!checkConfig())
                return;

            if (!checkAndFetchScores())
            {
                int missing = currentProfile.Value.Beatmaps.Count(b => b.ShowcaseScore == null);
                dialogOverlay?.Push(new ScoreMissingDialog(missing, launchAction));
            }
            else
            {
                launchAction.Invoke();
            }
        }

        private void currentTabChanged(ValueChangedEvent<ShowcaseConfigTab> e)
        {
            // <comment>
            // The old implementation mechanism will destroy and rebuild the component tree on each switch
            // Causes a lot of unnecessary overhead and more importantly it does not work well with GridContainers
            // they tried to reuse the components and it violates osu!framework's component ownership
            // So I decided to use the Alpha property to control the visibility of the components :)
            // </comment>
            bool generalVisible = currentTab.Value == ShowcaseConfigTab.General;
            tournamentInfoSection.Alpha = generalVisible ? 1 : 0;
            settingsSection.Alpha = generalVisible ? 1 : 0;
            introEditor.Alpha = generalVisible ? 1 : 0;

            beatmapSection.Alpha = currentTab.Value == ShowcaseConfigTab.Beatmaps ? 1 : 0;
        }

        private bool exitConfirmed;

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!exitConfirmed && dialogOverlay != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                    confirmDialog.PerformOkAction();
                else
                {
                    dialogOverlay.Push(new ConfirmDialog(TournamentShowcaseStrings.ExitScreenDialogTitle, () =>
                    {
                        exitConfirmed = true;
                        if (this.IsCurrentScreen())
                            this.Exit();
                    }));
                }

                return true;
            }

            return base.OnExiting(e);
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Save:
                    return e.Repeat || saveButton.TriggerClick();

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }

    public enum ShowcaseConfigTab
    {
        General,
        Beatmaps,
    }
}
