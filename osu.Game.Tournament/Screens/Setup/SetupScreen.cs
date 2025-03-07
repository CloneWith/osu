// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class SetupScreen : TournamentScreen
    {
        private FillFlowContainer fillFlow = null!;

        private LoginOverlay? loginOverlay;
        private ResolutionSelector resolution = null!;

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        [Resolved]
        private StableInfo stableInfo { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private Bindable<Size> windowSize = null!;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            windowSize = frameworkConfig.GetBindable<Size>(FrameworkSetting.WindowedSize);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = fillFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(10),
                    },
                },
                new ControlPanel(true)
            };

            api.LocalUser.BindValueChanged(_ => Schedule(reload));
            stableInfo.OnStableInfoSaved += () => Schedule(reload);
            reload();
        }

        private void reload()
        {
            var fileBasedIpc = ipc as FileBasedIPC;
            fillFlow.Children = new Drawable[]
            {
                new SectionHeader(SetupStrings.GeneralHeader),
                new LanguageSwitcher
                {
                    Label = GeneralSettingsStrings.LanguageHeader,
                },
                new ActionableInfo
                {
                    Label = SetupStrings.CurrentIPCSource,
                    ButtonText = SetupStrings.Change,
                    Action = () => sceneManager?.SetScreen(new StablePathSelectScreen()),
                    Value = fileBasedIpc?.IPCStorage?.GetFullPath(string.Empty) ?? SetupStrings.NotFound,
                    Failing = fileBasedIpc?.IPCStorage == null,
                    Description = SetupStrings.IPCSourceDescription,
                },
                new ActionableInfo
                {
                    Label = SetupStrings.CurrentUser,
                    ButtonText = SetupStrings.ShowProfile,
                    Action = () =>
                    {
                        if (loginOverlay == null)
                        {
                            AddInternal(loginOverlay = new LoginOverlay
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            });
                        }

                        loginOverlay.State.Value = Visibility.Visible;
                    },
                    Value = api.LocalUser.Value.Username,
                    Failing = api.IsLoggedIn != true,
                    Description = SetupStrings.CurrentUserDescription,
                },
                new TournamentSwitcher
                {
                    Label = SetupStrings.CurrentTournament,
                    Description = SetupStrings.CurrentTournamentDescription,
                },
                resolution = new ResolutionSelector
                {
                    Label = SetupStrings.Resolution,
                    ButtonText = SetupStrings.SetResolution,
                    Action = height =>
                    {
                        windowSize.Value = new Size((int)(height * aspect_ratio / TournamentSceneManager.STREAM_AREA_WIDTH * TournamentSceneManager.REQUIRED_WIDTH), height);
                    },
                },
                new LabelledSwitchButton
                {
                    Label = SetupStrings.ShowGlobalTime,
                    Description = SetupStrings.ShowGlobalTimeDescription,
                    Current = LadderInfo.UseUtcTime,
                },
                new SectionHeader(SetupStrings.TournamentSpecificHeader),
                new LabelledDropdown<RulesetInfo?>
                {
                    Label = SetupStrings.Ruleset,
                    Description = SetupStrings.RulesetDescription,
                    Items = rulesets.AvailableRulesets,
                    Current = LadderInfo.Ruleset,
                },
                new ActionableInfo
                {
                    Label = SetupStrings.BackgroundSettings,
                    ButtonText = SetupStrings.Change,
                    Description = SetupStrings.BackgroundSettingsDescription,
                    Action = () => sceneManager?.SetScreen(new BackgroundSelectScreen()),
                },
                new LabelledSwitchButton
                {
                    Label = SetupStrings.DisplaySeeds,
                    Description = SetupStrings.DisplaySeedsDescription,
                    Current = LadderInfo.DisplayTeamSeeds,
                },
                new SectionHeader(SetupStrings.AutomationHeader),
                new LabelledSwitchButton
                {
                    Label = SetupStrings.AutoAdvance,
                    Description = SetupStrings.AutoAdvanceDescription,
                    Current = LadderInfo.AutoProgressScreens,
                }
            };
        }

        private const float aspect_ratio = 16f / 9f;

        protected override void Update()
        {
            base.Update();

            resolution.Value = $"{ScreenSpaceDrawQuad.Width:N0}x{ScreenSpaceDrawQuad.Height:N0}";
        }
    }
}
