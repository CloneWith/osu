// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class BackgroundSelectScreen : TournamentScreen
    {
        private BackgroundTypeDropdown backgroundDropdown = null!;
        private TournamentSpriteText backgroundInfo = null!;

        private TourneyBackground backgroundPreview = null!;

        private Container backgroundContainer = null!;

        private SpriteIcon currentFileIcon = null!;
        private OsuTextFlowContainer currentFileText = null!;

        private RoundedButton saveButton = null!;

        private string initialPath = null!;
        private string videoPath = null!;
        private string imagePath = null!;
        private BackgroundInfo availableInfo;
        private bool pathValid;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private TournamentGame game { get; set; } = null!;

        private OsuFileSelector fileSelector = null!;
        private DialogOverlay? overlay;

        [BackgroundDependencyLoader(true)]
        private void load(TournamentStorage storage)
        {
            initialPath = new DirectoryInfo(storage.GetFullPath(string.Empty)).FullName;
            videoPath = new DirectoryInfo(storage.GetFullPath("./Videos")).FullName;
            imagePath = new DirectoryInfo(storage.GetFullPath("./Backgrounds")).FullName;

            InternalChildren = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Main)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f, 0.8f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = new OsuColour().GreySeaFoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.4f, 1f),
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
                                        Text = "Background Settings",
                                        Font = OsuFont.Default.With(size: 32, weight: FontWeight.SemiBold)
                                    },
                                },
                                new Drawable[]
                                {
                                    fileSelector = new OsuFileSelector(initialPath)
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
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            currentFileIcon = new SpriteIcon
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Icon = FontAwesome.Solid.Edit,
                                                Size = new Vector2(16),
                                            },
                                            currentFileText = new OsuTextFlowContainer
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                AutoSizeAxes = Axes.Both,
                                                AutoSizeEasing = Easing.OutQuint,
                                                AutoSizeDuration = 100
                                            },
                                        }
                                    }
                                },
                            }
                        },
                        new GridContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.6f, 1f),
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.2f),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(10),
                                                AutoSizeAxes = Axes.Both,
                                                Margin = new MarginPadding { Top = 10 },
                                                Children = new Drawable[]
                                                {
                                                    new SpriteIcon
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        Icon = FontAwesome.Solid.InfoCircle,
                                                        Size = new Vector2(24),
                                                        Colour = Color4.White,
                                                    },
                                                    backgroundInfo = new TournamentSpriteText
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        Text = "Unknown",
                                                        Font = OsuFont.Default.With(size: 24),
                                                    },
                                                },
                                            },
                                            new SettingsSlider<float>
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                LabelText = @"Background Dim",
                                                DisplayAsPercentage = true,
                                                Current = LadderInfo.BackgroundDim,
                                            },
                                            backgroundDropdown = new BackgroundTypeDropdown
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                LabelText = "Select background for",
                                                Margin = new MarginPadding { Top = 10 },
                                            },
                                            backgroundContainer = new Container
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                RelativeSizeAxes = Axes.Both,
                                                Child = backgroundPreview = new TourneyBackground(LadderInfo.BackgroundMap.LastOrDefault(v => v.Key == backgroundDropdown.Current.Value).Value,
                                                    showError: true, fillMode: FillMode.Fit)
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    Loop = true,
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                            },
                                        }
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
                                            saveButton = new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 200,
                                                Text = "Set and save",
                                                Action = saveSetting
                                            },
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 200,
                                                Text = "Reset...",
                                                Colour = Color4.Orange,
                                                Action = () => overlay?.Push(new ResetVideoDialog
                                                (
                                                    resetOneAction: () =>
                                                    {
                                                        string defaultVideo = BackgroundProps.PATHS.First(v => v.Key == backgroundDropdown.Current.Value).Value.Name;
                                                        LadderInfo.BackgroundMap.RemoveAll(v => v.Key == backgroundDropdown.Current.Value);
                                                        LadderInfo.BackgroundMap.Add(new KeyValuePair<BackgroundType, BackgroundInfo>(backgroundDropdown.Current.Value, new BackgroundInfo
                                                        (
                                                            name: defaultVideo,
                                                            source: BackgroundSource.Video
                                                        )));
                                                        game.SaveChanges();
                                                    },
                                                    resetAllAction: () =>
                                                    {
                                                        LadderInfo.BackgroundMap.Clear();
                                                        LadderInfo.BackgroundMap.AddRange(BackgroundProps.PATHS);
                                                        game.SaveChanges();
                                                    }
                                                )),
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
                    Action = () => sceneManager?.SetScreen(typeof(SetupScreen))
                },
                overlay = new DialogOverlay(),
            };

            currentFileText.AddText(@"Select a file!", t => t.Font = OsuFont.Default.With(size: 16));

            saveButton.Enabled.Value = false;

            if (!LadderInfo.BackgroundMap.Any())
            {
                LadderInfo.BackgroundMap.AddRange(BackgroundProps.PATHS);
            }

            backgroundInfo.Text = LadderInfo.BackgroundMap.LastOrDefault(v => v.Key == backgroundDropdown.Current.Value).Value.Name;
            backgroundInfo.Colour = backgroundPreview.BackgroundAvailable ? Color4.SkyBlue : Color4.Orange;

            backgroundDropdown.Current.BindValueChanged(e =>
            {
                backgroundContainer.Child = backgroundPreview = new TourneyBackground(LadderInfo.BackgroundMap.LastOrDefault(v => v.Key == e.NewValue).Value,
                    showError: true, fillMode: FillMode.Fit)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                };

                backgroundInfo.Text = $"Using: {LadderInfo.BackgroundMap.LastOrDefault(v => v.Key == e.NewValue).Value.Name}";
                backgroundInfo.Colour = backgroundPreview.BackgroundAvailable ? Color4.SkyBlue : Color4.Orange;
            }, true);

            fileSelector.CurrentPath.BindValueChanged(pathChanged, true);
            fileSelector.CurrentFile.BindValueChanged(fileChanged, true);

            LadderInfo.BackgroundDim.BindValueChanged(_ =>
            {
                saveButton.Enabled.Value = true;
            });
        }

        private void pathChanged(ValueChangedEvent<DirectoryInfo> e)
        {
            // This can be null initially, and throws an exception.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (e.NewValue == null)
            {
                pathValid = false;
            }
        }

        private void fileChanged(ValueChangedEvent<FileInfo> selectedFile)
        {
            // This can be null initially, and throws an exception.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (selectedFile.NewValue == null)
            {
                currentFileText.Text = "Select a file!";
                currentFileIcon.Icon = FontAwesome.Solid.Edit;
                return;
            }

            string lowerFileName = selectedFile.NewValue.Name.ToLowerInvariant();

            bool validVideo = lowerFileName.EndsWith(".mp4", StringComparison.Ordinal)
                              || lowerFileName.EndsWith(".avi", StringComparison.Ordinal)
                              || lowerFileName.EndsWith(".m4v", StringComparison.Ordinal);
            bool validImage = lowerFileName.EndsWith(".png", StringComparison.Ordinal)
                              || lowerFileName.EndsWith(".jpg", StringComparison.Ordinal)
                              || lowerFileName.EndsWith(".jpeg", StringComparison.Ordinal)
                              || lowerFileName.EndsWith(".bmp", StringComparison.Ordinal);

            if (!validVideo && !validImage)
            {
                saveButton.Enabled.Value = false;

                currentFileText.Clear();
                currentFileText.AddText($"{selectedFile.NewValue.Name}",
                    t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));
                currentFileText.AddText(@": Invalid file type.", t => t.Colour = Color4.Orange);
                currentFileIcon.Icon = FontAwesome.Solid.ExclamationCircle;
                currentFileIcon.Colour = Color4.Orange;
            }
            else
            {
                pathValid = (validVideo && fileSelector.CurrentPath.Value.FullName == videoPath)
                            || (validImage && fileSelector.CurrentPath.Value.FullName == imagePath);

                if (pathValid)
                {
                    saveButton.Enabled.Value = true;

                    currentFileText.Clear();
                    currentFileText.AddText($"{selectedFile.NewValue.Name}",
                        t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));
                    currentFileText.AddText(": Preview on the right!", t => t.Colour = Color4.SkyBlue);
                    currentFileIcon.Icon = FontAwesome.Solid.CheckCircle;
                    currentFileIcon.Colour = Color4.SkyBlue;
                    backgroundContainer.Child = new TourneyBackground(availableInfo = new BackgroundInfo
                    (
                        source: validVideo ? BackgroundSource.Video : BackgroundSource.Image,
                        name: selectedFile.NewValue.Name.Split('.')[0]
                    ), showError: true, fillMode: FillMode.Fit)
                    {
                        Loop = true,
                        RelativeSizeAxes = Axes.Both,
                    };
                }
                else
                {
                    (string, string) prompt = validVideo ? ("Videos", "Videos") : ("Images", "Backgrounds");

                    saveButton.Enabled.Value = false;
                    currentFileText.Clear();
                    currentFileText.AddText($"{prompt.Item1} must be selected from current \"{prompt.Item2}\" directory.",
                        t => t.Colour = Color4.Orange);
                    currentFileIcon.Icon = FontAwesome.Solid.ExclamationCircle;
                    currentFileIcon.Colour = Color4.Orange;
                }
            }
        }

        private void saveSetting()
        {
            BackgroundType currentType = backgroundDropdown.Current.Value;
            LadderInfo.BackgroundMap.RemoveAll(v => v.Key == currentType);

            LadderInfo.BackgroundMap.Add(new KeyValuePair<BackgroundType, BackgroundInfo>(
                currentType,
                availableInfo
            ));

            game.SaveChanges();

            saveButton.FlashColour(Color4.White, 500);
            saveButton.Enabled.Value = false;
        }
    }
}
