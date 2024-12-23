// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osuTK;

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

        [Resolved(canBeNull: true)]
        private IDialogOverlay? dialogOverlay { get; set; }

        private const float sizing_duration = 200;

        private FillFlowContainer innerFlow = null!;

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

        private Bindable<ShowcaseConfig> currentProfile = new Bindable<ShowcaseConfig>();

        public ShowcaseConfigScreen()
        {
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var availableProfiles = storage.ListTournaments();
            if (!availableProfiles.Any())
                currentProfile = new Bindable<ShowcaseConfig>();
            else
                currentProfile = new Bindable<ShowcaseConfig>(storage.GetConfig(availableProfiles.First()));

            currentProfile.Value ??= new ShowcaseConfig();

            // In case an exception prevents this variable from initializing...
            currentProfile.Value.IntroBeatmap.Value ??= new ShowcaseBeatmap();

            InternalChildren = new Drawable[]
            {
                new ExtendableBeatmapCard(currentProfile.Value.IntroBeatmap.Value, currentProfile.Value),
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new OsuScrollContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.75f,
                        Height = 0.8f,
                        Child = innerFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(10),
                            Direction = FillDirection.Full,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    AutoSizeEasing = Easing.OutQuint,
                                    AutoSizeDuration = sizing_duration,
                                    Spacing = new Vector2(10),
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new SectionHeader(@"Tournament Information"),
                                        profileDropdown = new FormDropdown<string>
                                        {
                                            Caption = "Load set",
                                            Items = availableProfiles
                                        },
                                        rulesetDropdown = new FormDropdown<RulesetInfo>
                                        {
                                            Caption = "Ruleset",
                                            HintText = @"The ruleset we should use for showcase and replays.",
                                            Items = rulesets.AvailableRulesets,
                                            Current = currentProfile.Value.Ruleset,
                                        },
                                        tournamentNameInput = new FormTextBox
                                        {
                                            Caption = "Name",
                                            PlaceholderText = "Tournament series name (e.g. osu! World Cup)",
                                            HintText = "This would be shown at the intro screen.",
                                            Current = currentProfile.Value.TournamentName,
                                            TabbableContentContainer = this,
                                        },
                                        roundNameInput = new FormTextBox
                                        {
                                            Caption = "Round",
                                            PlaceholderText = "Tournament round (e.g. Semifinals)",
                                            Current = currentProfile.Value.RoundName,
                                            TabbableContentContainer = this,
                                        },
                                        dateTimeInput = new FormTextBox
                                        {
                                            Caption = "Date and Time",
                                            PlaceholderText = "2024/11/4 5:14:19:191 UTC+8",
                                            HintText = "This would stay the same in the showcase. So use your own preferred format!",
                                            Current = currentProfile.Value.DateTime,
                                            TabbableContentContainer = this,
                                        },
                                        commentInput = new FormTextBox
                                        {
                                            Caption = "Comment",
                                            PlaceholderText = "Welcome to osu!",
                                            HintText = "In fact you can write anything here.\nThis is also part of the intro screen.",
                                            Current = currentProfile.Value.Comment,
                                            TabbableContentContainer = this,
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    AutoSizeEasing = Easing.OutQuint,
                                    AutoSizeDuration = sizing_duration,
                                    Spacing = new Vector2(10),
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new SectionHeader(@"Showcase Settings"),
                                        layoutDropdown = new FormDropdown<ShowcaseLayout>
                                        {
                                            Caption = @"Interface layout",
                                            HintText = @"The layout of the showcases screen.",
                                            Current = currentProfile.Value.Layout,
                                            Items = new List<ShowcaseLayout>
                                            {
                                                ShowcaseLayout.Immersive,
                                                ShowcaseLayout.SimpleControl,
                                                ShowcaseLayout.DetailedControl
                                            }
                                        },
                                        aspectRatioInput = new FormSliderBar<float>
                                        {
                                            Caption = @"Aspect ratio",
                                            HintText = @"Defines the ratio of the showcase area. Change this when you need to record a video with specific sizes.",
                                            Current = currentProfile.Value.AspectRatio,
                                            TransferValueOnCommit = true,
                                            TabbableContentContainer = this,
                                        },
                                        transformDurationInput = new FormSliderBar<int>
                                        {
                                            Caption = @"Transform duration",
                                            HintText = @"The length of the transform animation between screens, in milliseconds.",
                                            Current = currentProfile.Value.TransformDuration,
                                            TransferValueOnCommit = true,
                                            TabbableContentContainer = this,
                                        },
                                        startCountdownInput = new FormSliderBar<int>
                                        {
                                            Caption = @"Start countdown",
                                            HintText = @"A duration to wait for before the showcase starts. Get prepared this time!",
                                            Current = currentProfile.Value.StartCountdown,
                                            TransferValueOnCommit = true,
                                            TabbableContentContainer = this,
                                        },
                                        outroTitleInput = new FormTextBox
                                        {
                                            Caption = @"Outro title",
                                            PlaceholderText = @"Thanks for watching!",
                                            Current = currentProfile.Value.OutroTitle,
                                            TabbableContentContainer = this,
                                        },
                                        outroSubtitleInput = new FormTextBox
                                        {
                                            Caption = @"Outro subtitle",
                                            HintText = @"This would be shown in one line, so shouldn't be too long!",
                                            PlaceholderText = @"Take care of yourself, and be well.",
                                            Current = currentProfile.Value.OutroSubtitle,
                                            TabbableContentContainer = this,
                                        },
                                    }
                                },
                                new ShowcaseTeamEditor(currentProfile),
                                new ShowcaseBeatmapEditor(currentProfile),
                                new ShowcaseStaffEditor(currentProfile),
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
                                        new SectionHeader(@"Intro Beatmap"),
                                        useCustomIntroSwitch = new FormCheckBox
                                        {
                                            Caption = @"Use custom intro beatmap",
                                            HintText = @"If enabled, we will use the beatmap below as a fixed intro song for the showcase."
                                                       + @" Otherwise the first beatmap will be used.",
                                            Current = currentProfile.Value.UseCustomIntroBeatmap
                                        },
                                        introBeatmapRow = new BeatmapRow(currentProfile.Value.IntroBeatmap.Value, currentProfile.Value)
                                        {
                                            AllowDeletion = false
                                        },
                                    }
                                },
                            }
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.6f,
                    Y = -30,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new RoundedButton
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.4f,
                            Text = @"Save",
                            Action = () =>
                            {
                                if (checkConfig())
                                    storage.SaveChanges(currentProfile.Value);
                            },
                        },
                        new RoundedButton
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.4f,
                            Text = @"Start Showcase",
                            Action = () =>
                            {
                                if (checkConfig())
                                    this.Push(new ShowcaseScreen(currentProfile.Value));
                            },
                        }
                    }
                }
            };

            currentProfile.BindValueChanged(_ => updateForm());
            profileDropdown.Current.BindValueChanged(e =>
            {
                currentProfile.Value = storage.GetConfig(e.NewValue);
            });

            foreach (var f in innerFlow.Children)
            {
                f.Width = 0.49f;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(500, Easing.OutQuint);
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
                    HeaderText = @"Beatmap list empty",
                    BodyText = @"Consider adding one here."
                });

                return false;
            }

            if (useCustomIntroSwitch.Current.Value && !currentProfile.Value.IntroBeatmap.Value.IsValid())
            {
                dialogOverlay?.Push(new ProfileCheckFailedDialog
                {
                    HeaderText = @"Custom null intro map?",
                    BodyText = @"Specify a custom intro beatmap, or turn off the switch to use the first beatmap in the queue."
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
            rulesetDropdown.Current = currentProfile.Value.Ruleset;
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

            introBeatmapRow.Expire();
            introEditor.Add(introBeatmapRow = new BeatmapRow(currentProfile.Value.IntroBeatmap.Value, currentProfile.Value)
            {
                AllowDeletion = false
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
                    dialogOverlay.Push(new ConfirmDialog("Are you sure to exit this screen?", () =>
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
}
