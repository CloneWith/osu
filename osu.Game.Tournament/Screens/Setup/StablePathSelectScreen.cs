// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.IPC;
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

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private OsuDirectorySelector directorySelector = null!;
        private DialogOverlay? dialogOverlay;

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OsuColour colours)
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
                    Size = new Vector2(0.5f, 0.8f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeaFoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.8f),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = StablePathSelectStrings.PathSelectTitle,
                                        Font = OsuFont.Default.With(size: 40),
                                    },
                                },
                                new Drawable[]
                                {
                                    directorySelector = new OsuDirectorySelector(initialPath)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(20),
                                        Children = new Drawable[]
                                        {
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 300,
                                                Text = StablePathSelectStrings.SelectStablePath,
                                                Action = ChangePath,
                                            },
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 300,
                                                Text = StablePathSelectStrings.AutoDetect,
                                                Action = AutoDetect,
                                            },
                                        }
                                    }
                                }
                            }
                        }
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

            if (!fileBasedIpc?.AutoDetectIPCLocation() ?? true)
            {
                dialogOverlay?.Push(new IPCErrorDialog(StablePathSelectStrings.DetectFailureTitle, StablePathSelectStrings.DetectFailureText));
            }
            else
            {
                sceneManager?.SetScreen(typeof(SetupScreen));
            }
        }
    }
}
