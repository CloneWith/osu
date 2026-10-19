// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osuTK;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class StablePathSelectScreen : TournamentScreen
    {
        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        private OsuDirectorySelector directorySelector = null!;
        private DialogOverlay? dialogOverlay;
        private RoundedButton useStablePathButton = null!;

        private readonly Bindable<SettingsNote.Data?> installNoteData = new Bindable<SettingsNote.Data?>();
        private readonly BindableBool canUseStablePath = new BindableBool();

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OverlayColourProvider colourProvider)
        {
            var initialStorage = (ipc as FileBasedIPC)?.IPCStorage ?? storage;
            string? initialPath = new DirectoryInfo(initialStorage.GetFullPath(string.Empty)).Parent?.FullName;

            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Name = @"Title",
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(10),
                                        Padding = new MarginPadding { Horizontal = 25, Vertical = 10 },
                                        Children = new Drawable[]
                                        {
                                            new SpriteIcon
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Icon = OsuIcon.Debug,
                                                Size = new Vector2(32),
                                            },
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Text = StablePathSelectStrings.PathSelectTitle,
                                                Font = OsuFont.Default.With(size: 32, weight: FontWeight.Bold),
                                            },
                                        },
                                    },
                                },
                                new Drawable[]
                                {
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.Relative, 0.5f),
                                            new Dimension(GridSizeMode.Relative, 0.5f),
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                directorySelector = new OsuDirectorySelector(initialPath)
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    ShowHiddenToggle = false,
                                                },
                                                new GridContainer
                                                {
                                                    Name = @"Settings section",
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    RelativeSizeAxes = Axes.Both,
                                                    RowDimensions = [new Dimension(), new Dimension(GridSizeMode.AutoSize)],
                                                    ColumnDimensions = [new Dimension()],
                                                    Padding = new MarginPadding { Horizontal = 20, Vertical = 20 },
                                                    Content = new Drawable[][]
                                                    {
                                                        [
                                                            new FillFlowContainer
                                                            {
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                                RelativeSizeAxes = Axes.Both,
                                                                Masking = true,
                                                                Direction = FillDirection.Vertical,
                                                                Spacing = new Vector2(10),
                                                                Children = new Drawable[]
                                                                {
                                                                    new SectionHeader(StablePathSelectStrings.StatusHeader)
                                                                    {
                                                                        Anchor = Anchor.TopCentre,
                                                                        Origin = Anchor.TopCentre,
                                                                    },
                                                                    new IPCStatusDisplay(),
                                                                },
                                                            },
                                                        ],
                                                        [
                                                            new FillFlowContainer
                                                            {
                                                                Anchor = Anchor.BottomCentre,
                                                                Origin = Anchor.BottomCentre,
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Direction = FillDirection.Vertical,
                                                                Spacing = new Vector2(5),
                                                                Children = new Drawable[]
                                                                {
                                                                    new SettingsNote
                                                                    {
                                                                        Anchor = Anchor.TopCentre,
                                                                        Origin = Anchor.TopCentre,
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Current = { BindTarget = installNoteData },
                                                                    },
                                                                    useStablePathButton = new RoundedButton
                                                                    {
                                                                        Anchor = Anchor.TopCentre,
                                                                        Origin = Anchor.TopCentre,
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Text = StablePathSelectStrings.SelectStablePath,
                                                                        Action = ChangePath,
                                                                    },
                                                                    new RoundedButton
                                                                    {
                                                                        Anchor = Anchor.TopCentre,
                                                                        Origin = Anchor.TopCentre,
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Text = StablePathSelectStrings.AutoDetect,
                                                                        Action = AutoDetect,
                                                                    },
                                                                },
                                                            },
                                                        ]
                                                    }
                                                },

                                            },
                                        }
                                    },
                                },
                            },
                        },
                    },
                },
                new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    State = { Value = Visibility.Visible },
                    Action = () => sceneManager?.SetScreen(typeof(SetupScreen)),
                },
                dialogOverlay = new DialogOverlay(),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            canUseStablePath.BindValueChanged(usable =>
            {
                useStablePathButton.Enabled.Value = usable.NewValue;
                useStablePathButton.TooltipText = usable.NewValue ? string.Empty : StablePathSelectStrings.ResolveIssuesFirst;
            }, true);

            directorySelector.CurrentPath.BindValueChanged(_ => verifyStablePath(), true);
        }

        /// <summary>
        /// Checks the currently selected directory for the requirements of tournament usage.
        /// </summary>
        private void verifyStablePath()
        {
            DirectoryInfo? selectedPath = directorySelector.CurrentPath.Value;

            // the selector has not resolved a path yet.
            if (selectedPath == null)
            {
                canUseStablePath.Value = false;
                installNoteData.Value = null;
                return;
            }

            StableInstallStatus status = StableInstall.Inspect(selectedPath.FullName);

            canUseStablePath.Value = status.IsUsable;
            installNoteData.Value = createInstallNote(status);
        }

        /// <summary>
        /// Describes all problems preventing the supplied installation from being used, or <c>null</c> if it can be used as-is.
        /// </summary>
        private static SettingsNote.Data? createInstallNote(StableInstallStatus status)
        {
            if (!status.IsInstall)
                return new SettingsNote.Data(StablePathSelectStrings.NotStableInstall, SettingsNote.Type.Critical);

            var issues = new List<LocalisableString>();

            if (!status.IsCuttingEdge)
            {
                string? releaseStream = status.ReleaseStream;
                LocalisableString stream = string.IsNullOrEmpty(releaseStream) ? IPCStatusStrings.Unknown : releaseStream;
                issues.Add(StablePathSelectStrings.NotCuttingEdge(stream));
            }

            if (!status.IsTournamentReady)
                issues.Add(StablePathSelectStrings.NotTournamentConfigured);

            if (issues.Count == 0)
                return null;

            LocalisableString text = issues.Count == 1
                ? issues[0]
                : LocalisableString.Format("{0}\n{1}", issues[0], issues[1]);

            return new SettingsNote.Data(text, SettingsNote.Type.Warning);
        }

        protected virtual void ChangePath()
        {
            string target = directorySelector.CurrentPath.Value.FullName;
            var fileBasedIpc = ipc as FileBasedIPC;
            Logger.Log($"Changing Stable CE location to {target}");

            if (!fileBasedIpc?.SetIPCLocation(target) ?? true)
            {
                dialogOverlay?.Push(new IPCErrorDialog(StablePathSelectStrings.InvalidDirectoryTitle, StablePathSelectStrings.InvalidDirectoryText));
                Logger.Log("Folder is not an osu! stable CE directory");
                return;
            }

            sceneManager?.SetScreen(typeof(SetupScreen));
        }

        protected virtual void AutoDetect()
        {
            var fileBasedIpc = ipc as FileBasedIPC;
            string? ipcLocation = fileBasedIpc?.AutoDetectIPCLocation();

            if (ipcLocation == null)
            {
                dialogOverlay?.Push(new IPCErrorDialog(StablePathSelectStrings.DetectFailureTitle, StablePathSelectStrings.DetectFailureText));
            }
            else
            {
                directorySelector.CurrentPath.Value = new DirectoryInfo(ipcLocation);
            }
        }
    }
}
