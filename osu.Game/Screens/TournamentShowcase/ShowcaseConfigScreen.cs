// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
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

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        private FillFlowContainer innerFlow = null!;

        private FormDropdown<string> profileDropdown = null!;

        private FormDropdown<RulesetInfo?> rulesetDropdown = null!;
        private FormTextBox tournamentNameInput = null!;
        private FormTextBox roundNameInput = null!;
        private FormTextBox dateTimeInput = null!;
        private FormTextBox commentInput = null!;
        private FormSliderBar<int> transformDurationInput = null!;

        private BeatmapInfoWedgeV2 introMapWedge = null!;

        private Bindable<ShowcaseConfig> currentProfile = new Bindable<ShowcaseConfig>();

        public ShowcaseConfigScreen()
        {
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var availableProfiles = storage.ListTournaments();
            currentProfile = new Bindable<ShowcaseConfig>(storage.GetConfig(availableProfiles.First()));

            InternalChildren = new Drawable[]
            {
                new OsuScrollContainer
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
                                    rulesetDropdown = new FormDropdown<RulesetInfo?>
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
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Showcase Settings"),
                                    transformDurationInput = new FormSliderBar<int>
                                    {
                                        Caption = @"Transform duration",
                                        HintText = @"The length of the transform animation between screens, in milliseconds.",
                                        Current = currentProfile.Value.TransformDuration,
                                        TransferValueOnCommit = true,
                                        TabbableContentContainer = this,
                                    },
                                    new FormTextBox(),
                                    new FormTextBox(),
                                }
                            },
                            new ShowcaseTeamEditor(currentProfile),
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Beatmap Queue"),
                                    new FormTextBox(),
                                    new FormTextBox(),
                                }
                            },
                            new ShowcaseStaffEditor(currentProfile),
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Intro Beatmap"),
                                    new FormCheckBox
                                    {
                                        Caption = @"Use custom intro beatmap",
                                        HintText = @"If enabled, we will use the beatmap below as a fixed intro song for the showcase.",
                                        Current = currentProfile.Value.UseCustomIntroBeatmap
                                    },
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = @"Select beatmap",
                                        Action = () =>
                                        {
                                            Bindable<BeatmapInfo> introMapBindable = currentProfile.Value.IntroBeatmap.Value != null
                                                ? new Bindable<BeatmapInfo>(currentProfile.Value.IntroBeatmap.Value.BeatmapInfo)
                                                : new Bindable<BeatmapInfo>();

                                            Schedule(() => performer?.PerformFromScreen(s =>
                                                    s.Push(new ShowcaseSongSelect(introMapBindable)),
                                                new[] { typeof(ShowcaseConfigScreen) }));

                                            introMapBindable.BindValueChanged(e =>
                                                currentProfile.Value.IntroBeatmap.Value = new ShowcaseBeatmap(e.NewValue));
                                        }
                                    },
                                    introMapWedge = new BeatmapInfoWedgeV2
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.95f,
                                        Height = 90,
                                        State = { Value = Visibility.Visible },
                                        AlwaysPresent = true,
                                        Beatmap = beatmapManager.GetWorkingBeatmap(
                                            currentProfile.Value.IntroBeatmap.Value != null
                                                ? new BeatmapInfo
                                                {
                                                    ID = currentProfile.Value.IntroBeatmap.Value.BeatmapGuid,
                                                }
                                                : null, true)
                                    }
                                }
                            },
                        }
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
                            Text = "Save",
                            Action = () => storage.SaveChanges(currentProfile.Value),
                        },
                        new RoundedButton
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.4f,
                            Text = "Show",
                        }
                    }
                }
            };

            currentProfile.BindValueChanged(_ => updateForm());
            profileDropdown.Current.BindValueChanged(e =>
                currentProfile.Value = storage.GetConfig(e.NewValue));

            currentProfile.Value.IntroBeatmap.BindValueChanged(map =>
            {
                introMapWedge.Beatmap = beatmapManager.GetWorkingBeatmap(map.NewValue?.BeatmapInfo);
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
            // AddInternal(new ShowcaseCountdownOverlay(10000));
        }

        private void updateForm()
        {
            rulesetDropdown.Current = currentProfile.Value.Ruleset;
            tournamentNameInput.Current = currentProfile.Value.TournamentName;
            roundNameInput.Current = currentProfile.Value.RoundName;
            dateTimeInput.Current = currentProfile.Value.DateTime;
            commentInput.Current = currentProfile.Value.Comment;
            transformDurationInput.Current = currentProfile.Value.TransformDuration;
        }
    }
}
