// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Toolbar;
using osu.Game.Tournament.Localisation;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class FooterButtonNew
    {
        public partial class NewProfilePopover : OsuPopover
        {
            private readonly ShowcaseConfig config;

            private readonly FooterButtonNew footerButton;

            [Cached]
            private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [Resolved]
            private ShowcaseStorage storage { get; set; } = null!;

            private LabelledTextBox filenameTextBox = null!;
            private LabelledTextBox tournamentNameTextBox = null!;
            private LabelledTextBox roundNameTextBox = null!;
            private ToolbarRulesetSelector rulesetSelector = null!;
            private ShearedButton createButton = null!;

            public NewProfilePopover(FooterButtonNew footerButton)
            {
                this.footerButton = footerButton;

                config = new ShowcaseConfig();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                const float content_width = 500;

                Child = new FillFlowContainer
                {
                    Width = content_width,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(7),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        filenameTextBox = new LabelledTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Label = TournamentShowcaseStrings.NewFilenameLabel,
                            TabbableContentContainer = this,
                        },
                        new SectionHeader(TournamentShowcaseStrings.TournamentInfoHeader),
                        tournamentNameTextBox = new LabelledTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Label = TournamentShowcaseStrings.TournamentName,
                            Description = TournamentShowcaseStrings.TournamentNameDescription,
                            PlaceholderText = TournamentShowcaseStrings.TournamentNamePlaceholder,
                            Current = config.TournamentName,
                            TabbableContentContainer = this,
                        },
                        roundNameTextBox = new LabelledTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Label = TournamentShowcaseStrings.TournamentRound,
                            Description = TournamentShowcaseStrings.TournamentRoundDescription,
                            PlaceholderText = TournamentShowcaseStrings.TournamentRoundPlaceholder,
                            Current = config.RoundName,
                            TabbableContentContainer = this,
                        },
                        new SectionHeader(TournamentShowcaseStrings.DefaultRuleset),
                        new OsuScrollContainer(Direction.Horizontal)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            ScrollbarVisible = false,
                            Margin = new MarginPadding { Top = -10 },
                            Child = rulesetSelector = new ToolbarRulesetSelector
                            {
                                Current = config.FallbackRuleset,
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(7),
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                createButton = new ShearedButton
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = content_width,
                                    Text = BaseStrings.AddNew,
                                    Enabled = { Value = false },
                                    DarkerColour = colourProvider.Colour1,
                                    LighterColour = colourProvider.Colour0,
                                    TextColour = colourProvider.Background6,
                                    Action = createProfile,
                                },
                            },
                        },
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ScheduleAfterChildren(() => GetContainingFocusManager()!.ChangeFocus(filenameTextBox));

                filenameTextBox.Current.BindValueChanged(name =>
                {
                    config.Filename.Value = name.NewValue + @".json";
                    checkInput();
                });
                tournamentNameTextBox.Current.BindValueChanged(_ => checkInput());
                roundNameTextBox.Current.BindValueChanged(_ => checkInput());
                rulesetSelector.Current.BindValueChanged(_ => checkInput());
            }

            private void checkInput()
            {
                bool isValid = filenameTextBox.Current.Value.IsSafeForFilename(50)
                               && !string.IsNullOrWhiteSpace(tournamentNameTextBox.Current.Value)
                               && !string.IsNullOrWhiteSpace(roundNameTextBox.Current.Value)
                               && config.FallbackRuleset.Value != null;

                createButton.Enabled.Value = isValid;
            }

            private void createProfile()
            {
                bool success = false;

                updateStatesForAll(false);

                Task.Run(() =>
                {
                    try
                    {
                        storage.SaveChanges(config);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, @"An error occurred while creating new showcase profile.");
                    }
                }).ContinueWith(_ =>
                {
                    updateStatesForAll(true);

                    if (success)
                    {
                        footerButton.OnCreate?.Invoke(config);
                        Schedule(Hide);
                    }
                });
            }

            private void updateStatesForAll(bool interactive)
            {
                filenameTextBox.Current.Disabled = !interactive;
                tournamentNameTextBox.Current.Disabled = !interactive;
                roundNameTextBox.Current.Disabled = !interactive;
                rulesetSelector.Current.Disabled = !interactive;
                createButton.Enabled.Value = interactive;
            }

            protected override void UpdateState(ValueChangedEvent<Visibility> state)
            {
                base.UpdateState(state);
                footerButton.OverlayState.Value = state.NewValue;
            }
        }
    }
}
