// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseConfigScreen : OsuScreen
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private ShowcaseStorage storage { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        private const float sizing_duration = 200;

        #region Drawable variables

        private FillFlowContainer innerFlow = null!;
        private FillFlowContainer tournamentInfoSection = null!;
        private FillFlowContainer settingsSection = null!;
        private ShowcaseBeatmapEditor beatmapSection = null!;

        private FormDropdown<string> profileDropdown = null!;

        private FormDropdown<RulesetInfo> rulesetDropdown = null!;
        private FormTextBox tournamentNameInput = null!;
        private FormTextBox roundNameInput = null!;
        private FormTextBox dateTimeInput = null!;
        private FormTextBox commentInput = null!;
        private FormSliderBar<int> transformDurationInput = null!;
        private FormSliderBar<int> startCountdownInput = null!;
        private FormDropdown<ShowcaseLayout> layoutDropdown = null!;
        private FormSliderBar<float> aspectRatioInput = null!;
        private FormCheckBox useCustomIntroSwitch = null!;
        private FormTextBox outroTitleInput = null!;
        private FormTextBox outroSubtitleInput = null!;
        private FillFlowContainer introEditor = null!;
        private BeatmapRow introBeatmapRow = null!;

        #endregion

        private readonly Bindable<ShowcaseConfig> currentProfile = new Bindable<ShowcaseConfig>();
        private readonly Bindable<ShowcaseConfigTab> currentTab = new Bindable<ShowcaseConfigTab>();

        public ShowcaseConfigScreen()
        {
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var availableProfiles = storage.ListTournaments();
            var firstConfig = storage.GetConfig(availableProfiles.First());

            if (firstConfig != null)
                currentProfile.Value = firstConfig;

            // Enforce a non-null current profile and necessary properties.
            currentProfile.Value ??= new ShowcaseConfig();
            currentProfile.Value.IntroBeatmap.Value ??= new ShowcaseBeatmap();

            Debug.Assert(currentProfile.Value != null);

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
                    new SectionHeader(TournamentShowcaseStrings.TournamentInfoHeader),
                    profileDropdown = new FormDropdown<string>
                    {
                        Caption = TournamentShowcaseStrings.CurrentProfile, HintText = TournamentShowcaseStrings.CurrentProfileDescription, Items = availableProfiles,
                    },
                    rulesetDropdown = new FormDropdown<RulesetInfo>
                    {
                        Caption = TournamentShowcaseStrings.DefaultRuleset,
                        HintText = TournamentShowcaseStrings.DefaultRulesetDescription,
                        Items = rulesets.AvailableRulesets,
                        Current = currentProfile.Value.FallbackRuleset,
                    },
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
                    dateTimeInput = new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.DateAndTime,
                        PlaceholderText = "2024/11/4 5:14:19:191 UTC+8",
                        HintText = TournamentShowcaseStrings.DateAndTimeDescription,
                        Current = currentProfile.Value.DateTime,
                        TabbableContentContainer = this,
                    },
                    commentInput = new FormTextBox
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
                    layoutDropdown = new FormEnumDropdown<ShowcaseLayout>
                    {
                        Caption = TournamentShowcaseStrings.InterfaceLayout, HintText = TournamentShowcaseStrings.InterfaceLayoutDescription, Current = currentProfile.Value.Layout,
                    },
                    aspectRatioInput = new FormSliderBar<float>
                    {
                        Caption = TournamentShowcaseStrings.AspectRatio,
                        HintText = TournamentShowcaseStrings.AspectRatioDescription,
                        Current = currentProfile.Value.AspectRatio,
                        TransferValueOnCommit = true,
                        TabbableContentContainer = this,
                    },
                    transformDurationInput = new FormSliderBar<int>
                    {
                        Caption = TournamentShowcaseStrings.TransformDuration,
                        HintText = TournamentShowcaseStrings.TransformDurationDescription,
                        Current = currentProfile.Value.TransformDuration,
                        TransferValueOnCommit = true,
                        TabbableContentContainer = this,
                    },
                    startCountdownInput = new FormSliderBar<int>
                    {
                        Caption = TournamentShowcaseStrings.StartCountdownDuration,
                        HintText = TournamentShowcaseStrings.StartCountdownDurationDescription,
                        Current = currentProfile.Value.StartCountdown,
                        TransferValueOnCommit = true,
                        TabbableContentContainer = this,
                    },
                    outroTitleInput = new FormTextBox
                    {
                        Caption = TournamentShowcaseStrings.OutroTitle, PlaceholderText = @"Thanks for watching!", Current = currentProfile.Value.OutroTitle, TabbableContentContainer = this,
                    },
                    outroSubtitleInput = new FormTextBox
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
                    introBeatmapRow = new BeatmapRow(currentProfile.Value.IntroBeatmap.Value, currentProfile.Value)
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
                    Width = 0.8f,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.1f),
                        new Dimension(),
                        new Dimension(GridSizeMode.Relative, 0.1f),
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
                                    Child = innerFlow = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(10),
                                        Direction = FillDirection.Full,
                                        Children = new Drawable[]
                                        {
                                            tournamentInfoSection,
                                            settingsSection,
                                            introEditor,
                                        },
                                    },
                                },
                            },
                        },
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                Width = 0.6f,
                                Spacing = new Vector2(10),
                                Children = new Drawable[]
                                {
                                    new RoundedButton
                                    {
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.4f,
                                        Text = TournamentShowcaseStrings.SaveAction,
                                        Action = () =>
                                        {
                                            if (checkConfig())
                                                storage.SaveChanges(currentProfile.Value);

                                            refreshProfileList();
                                        },
                                    },
                                    new RoundedButton
                                    {
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.4f,
                                        Text = TournamentShowcaseStrings.StartShowcase,
                                        Action = () =>
                                        {
                                            if (checkConfig())
                                                this.Push(new ShowcaseScreen(currentProfile.Value));
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentProfile.BindValueChanged(_ => updateForm());
            currentTab.BindValueChanged(currentTabChanged);
            profileDropdown.Current.BindValueChanged(e =>
            {
                var newProfile = storage.GetConfig(e.NewValue);

                if (newProfile != null)
                    currentProfile.Value = newProfile;
                else
                {
                    Logger.Error(null, $"The given showcase configuration file \"{e.NewValue}\" cannot be loaded properly."
                                       + $" You are still editing \"{e.OldValue}\".");
                }
            });

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        private void refreshProfileList()
        {
            var availableProfiles = storage.ListTournaments();
            profileDropdown.Items = availableProfiles;
        }

        /// <summary>
        /// Check all necessary fields to ensure that the profile can be saved and used properly.
        /// In case an issue is found, a popup prompt will appear.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool checkConfig()
        {
            bool isValid = rulesetDropdown.Current.Value != null
                           && tournamentNameInput.Current.Value.Trim() != string.Empty
                           && roundNameInput.Current.Value.Trim() != string.Empty;

            if (!isValid)
            {
                dialogOverlay?.Push(new ProfileCheckFailedDialog());

                return false;
            }

            if (!currentProfile.Value.Beatmaps.Any())
            {
                dialogOverlay?.Push(new ProfileCheckFailedDialog
                {
                    HeaderText = TournamentShowcaseStrings.EmptyBeatmapListDialogTitle,
                    BodyText = TournamentShowcaseStrings.EmptyBeatmapListDialogText,
                });

                return false;
            }

            if (useCustomIntroSwitch.Current.Value && !currentProfile.Value.IntroBeatmap.Value.IsValid())
            {
                dialogOverlay?.Push(new ProfileCheckFailedDialog
                {
                    HeaderText = TournamentShowcaseStrings.NullIntroMapDialogTitle,
                    BodyText = TournamentShowcaseStrings.NullIntroMapDialogText,
                });

                return false;
            }

            return isValid;
        }

        /// <summary>
        /// Update the form components to match the new profile.
        /// </summary>
        private void updateForm()
        {
            rulesetDropdown.Current = currentProfile.Value.FallbackRuleset;
            tournamentNameInput.Current = currentProfile.Value.TournamentName;
            roundNameInput.Current = currentProfile.Value.RoundName;
            dateTimeInput.Current = currentProfile.Value.DateTime;
            commentInput.Current = currentProfile.Value.Comment;
            layoutDropdown.Current = currentProfile.Value.Layout;
            aspectRatioInput.Current = currentProfile.Value.AspectRatio;
            transformDurationInput.Current = currentProfile.Value.TransformDuration;
            startCountdownInput.Current = currentProfile.Value.StartCountdown;
            outroTitleInput.Current = currentProfile.Value.OutroTitle;
            outroSubtitleInput.Current = currentProfile.Value.OutroSubtitle;
            useCustomIntroSwitch.Current = currentProfile.Value.UseCustomIntroBeatmap;

            introBeatmapRow.Expire();
            introEditor.Add(introBeatmapRow = new BeatmapRow(currentProfile.Value.IntroBeatmap.Value, currentProfile.Value)
            {
                AllowDeletion = false
            });
        }

        private void currentTabChanged(ValueChangedEvent<ShowcaseConfigTab> e)
        {
            innerFlow.Clear(false);

            innerFlow.AddRange(currentTab.Value switch
            {
                ShowcaseConfigTab.General => new Drawable[]
                {
                    tournamentInfoSection,
                    settingsSection,
                    introEditor,
                },
                ShowcaseConfigTab.Beatmaps => new Drawable[]
                {
                    beatmapSection,
                },
                _ => throw new ArgumentOutOfRangeException(),
            });
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
    }

    public enum ShowcaseConfigTab
    {
        General,
        Beatmaps,
    }
}
