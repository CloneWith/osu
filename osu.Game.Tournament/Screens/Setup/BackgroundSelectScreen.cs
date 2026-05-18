// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class BackgroundSelectScreen : TournamentScreen
    {
        private readonly string[] supportedVideoExtensions = [@".mp4", @".avi", @".m4v"];
        private readonly string[] supportedImageExtensions = [@".png", @".jpg", @".jpeg", @".bmp"];

        private string[] supportedExtensions => supportedImageExtensions.Concat(supportedVideoExtensions).ToArray();

        private BackgroundTypeDropdown typeDropdown = null!;
        private SpriteIcon infoIcon = null!;
        private TournamentSpriteText infoText = null!;

        private Container previewContainer = null!;
        private TourneyBackground preview = null!;

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

        private readonly BindableFloat backgroundDim = new BindableFloat
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01f,
        };

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider colourProvider, TournamentStorage storage)
        {
            initialPath = new DirectoryInfo(storage.GetFullPath(string.Empty)).FullName;
            videoPath = new DirectoryInfo(storage.GetFullPath(@"./Videos", true)).FullName;
            imagePath = new DirectoryInfo(storage.GetFullPath(@"./Backgrounds", true)).FullName;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
                                new Dimension(GridSizeMode.Absolute, 50),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new GridContainer
                                    {
                                        Name = @"Top bar",
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Horizontal = 25, Vertical = 10 },
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.AutoSize),
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
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10),
                                                    Children = new Drawable[]
                                                    {
                                                        new SpriteIcon
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Icon = OsuIcon.Graphics,
                                                            Size = new Vector2(32),
                                                        },
                                                        new OsuSpriteText
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Text = BackgroundSelectStrings.BackgroundSettingsTitle,
                                                            Font = OsuFont.Default.With(size: 32, weight: FontWeight.Bold),
                                                        },
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    Name = @"Action buttons",
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(15),
                                                    Children = new Drawable[]
                                                    {
                                                        saveButton = new RoundedButton
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            Width = 150,
                                                            Text = BaseStrings.SaveChanges,
                                                            Action = () => saveSetting(),
                                                        },
                                                        new RoundedButton
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            Width = 150,
                                                            Text = BackgroundSelectStrings.SaveAll,
                                                            Action = () => saveSetting(true),
                                                        },
                                                        new RoundedButton
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            Width = 150,
                                                            Text = BaseStrings.Reset,
                                                            Colour = Color4.Orange,
                                                            Action = () => overlay?.Push(new BackgroundResetDialog
                                                            (
                                                                resetOneAction: () =>
                                                                {
                                                                    BackgroundInfo defaultInfo = BackgroundProps.GetDefaultBackgroundInfo(typeDropdown.Current.Value);
                                                                    LadderInfo.BackgroundMap.SetBackgroundInfo(typeDropdown.Current.Value, defaultInfo);
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
                                                    },
                                                },
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
                                            new Dimension(GridSizeMode.Relative, 0.4f),
                                            new Dimension(GridSizeMode.Relative, 0.6f),
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                fileSelector = new OsuFileSelector(initialPath, supportedExtensions)
                                                {
                                                    ShowHiddenToggle = false,
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                                new FillFlowContainer
                                                {
                                                    Name = @"Settings section",
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Masking = true,
                                                    Direction = FillDirection.Vertical,
                                                    Spacing = new Vector2(10),
                                                    Padding = new MarginPadding { Horizontal = 20 },
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
                                                                infoIcon = new SpriteIcon
                                                                {
                                                                    Anchor = Anchor.TopCentre,
                                                                    Origin = Anchor.TopCentre,
                                                                    Icon = FontAwesome.Solid.Play,
                                                                    Size = new Vector2(24),
                                                                },
                                                                infoText = new TournamentSpriteText
                                                                {
                                                                    Anchor = Anchor.TopCentre,
                                                                    Origin = Anchor.TopCentre,
                                                                    Text = BackgroundSelectStrings.Unknown,
                                                                    Font = OsuFont.Default.With(size: 24),
                                                                },
                                                            },
                                                        },
                                                        new FormSliderBar<float>
                                                        {
                                                            Anchor = Anchor.TopCentre,
                                                            Origin = Anchor.TopCentre,
                                                            Caption = BackgroundSelectStrings.BackgroundDim,
                                                            DisplayAsPercentage = true,
                                                            Current = backgroundDim,
                                                        },
                                                        typeDropdown = new BackgroundTypeDropdown
                                                        {
                                                            Anchor = Anchor.TopCentre,
                                                            Origin = Anchor.TopCentre,
                                                            Caption = BackgroundSelectStrings.SelectBackgroundFor,
                                                        },
                                                        previewContainer = new Container
                                                        {
                                                            Name = @"Preview",
                                                            Anchor = Anchor.TopCentre,
                                                            Origin = Anchor.TopCentre,
                                                            FillMode = FillMode.Fit,
                                                            RelativeSizeAxes = Axes.Both,
                                                            FillAspectRatio = TournamentExtensions.ASPECT_RATIO,
                                                            Masking = true,
                                                            CornerRadius = 10,
                                                            Child = preview = new TourneyBackground(availableInfo = LadderInfo.BackgroundMap.GetBackgroundInfo(typeDropdown.Current.Value), showError: true)
                                                            {
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                                Loop = true,
                                                                RelativeSizeAxes = Axes.Both,
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Name = @"Status bar",
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
                                            },
                                        },
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
                    Action = () => sceneManager?.SetScreen(typeof(SetupScreen))
                },
                overlay = new DialogOverlay(),
            };

            currentFileText.AddText(BackgroundSelectStrings.PromptSelectFile, t => t.Font = OsuFont.Default.With(size: 16));

            saveButton.Enabled.Value = false;

            void updateSelectedBackground(BackgroundType backgroundType, bool resetSelectedFile = true)
            {
                if (resetSelectedFile)
                    fileSelector.CurrentFile.SetDefault();

                availableInfo = LadderInfo.BackgroundMap.GetBackgroundInfo(backgroundType);

                previewContainer.Child = preview = new TourneyBackground(availableInfo, showError: true)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                };

                infoText.Text = LocalisableString.Interpolate($"{BackgroundSelectStrings.PromptFileUsing}{availableInfo.Name}");
                backgroundDim.Value = availableInfo.Dim;

                updatePreviewStatus(preview.BackgroundAvailable);
            }

            updateSelectedBackground(typeDropdown.Current.Value, false);

            typeDropdown.Current.BindValueChanged(e =>
            {
                updateSelectedBackground(e.NewValue);
            }, true);

            fileSelector.CurrentPath.BindValueChanged(pathChanged, true);
            fileSelector.CurrentFile.BindValueChanged(fileChanged, true);

            backgroundDim.BindValueChanged(e =>
            {
                saveButton.Enabled.Value = true;
                preview.Dim = e.NewValue;
                availableInfo.Dim = e.NewValue;
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
                currentFileText.Text = BackgroundSelectStrings.PromptSelectFile;
                currentFileIcon.Icon = FontAwesome.Solid.Pen;
                return;
            }

            string lowerFileName = selectedFile.NewValue.Name.ToLowerInvariant();

            bool validVideo = supportedVideoExtensions.Any(ext => lowerFileName.EndsWith(ext, StringComparison.Ordinal));
            bool validImage = supportedImageExtensions.Any(ext => lowerFileName.EndsWith(ext, StringComparison.Ordinal));

            Debug.Assert(validVideo || validImage);

            pathValid = (validVideo && fileSelector.CurrentPath.Value.FullName == videoPath)
                        || (validImage && fileSelector.CurrentPath.Value.FullName == imagePath);

            if (pathValid)
            {
                saveButton.Enabled.Value = true;

                currentFileText.Clear();
                currentFileText.AddText($"{selectedFile.NewValue.Name}",
                    t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));
                currentFileText.AddText(LocalisableString.Interpolate($": {BackgroundSelectStrings.PromptFilePreview}"), t => t.Colour = Color4.SkyBlue);
                currentFileIcon.Icon = FontAwesome.Solid.CheckCircle;
                currentFileIcon.Colour = Color4.SkyBlue;
                previewContainer.Child = preview = new TourneyBackground(availableInfo = new BackgroundInfo
                (
                    source: validVideo ? BackgroundSource.Video : BackgroundSource.Image,

                    // Display the file name with the extension.
                    name: selectedFile.NewValue.Name,
                    dim: backgroundDim.Value
                ), showError: true)
                {
                    Loop = true,
                    Dim = backgroundDim.Value,
                    RelativeSizeAxes = Axes.Both,
                };
            }
            else
            {
                (LocalisableString, string) prompt = validVideo
                    ? (BackgroundSelectStrings.FileTypeVideo, @"Videos")
                    : (BackgroundSelectStrings.FileTypeImage, @"Backgrounds");

                saveButton.Enabled.Value = false;
                currentFileText.Clear();
                currentFileText.AddText(BackgroundSelectStrings.PromptFilePath(prompt.Item1, prompt.Item2),
                    t => t.Colour = Color4.Orange);
                currentFileIcon.Icon = FontAwesome.Solid.Folder;
                currentFileIcon.Colour = Color4.Orange;
            }
        }

        private void saveSetting(bool applyToAll = false)
        {
            BackgroundType currentType = typeDropdown.Current.Value;
            // If the user has selected a new file, use that instead of the current mapping.
            BackgroundInfo currentMapping = LadderInfo.BackgroundMap.GetBackgroundInfo(currentType);
            BackgroundInfo infoToSave = EqualityComparer<BackgroundInfo>.Default.Equals(availableInfo, default)
                ? currentMapping
                : availableInfo;

            if (applyToAll)
            {
                // Update all background mappings with the selected type.
                // Use enum members as reference in case of missing entries from the ladder.
                LadderInfo.BackgroundMap.Clear();

                foreach (var bg in Enum.GetValues(typeof(BackgroundType)).Cast<BackgroundType>())
                {
                    LadderInfo.BackgroundMap.Add(new KeyValuePair<BackgroundType, BackgroundInfo>(bg, infoToSave));
                }
            }
            else
            {
                LadderInfo.BackgroundMap.SetBackgroundInfo(currentType, infoToSave);
            }

            game.SaveChanges();

            saveButton.FlashColour(Color4.White, 500);
            saveButton.Enabled.Value = false;
            updatePreviewStatus(preview.BackgroundAvailable);
        }

        private void updatePreviewStatus(bool backgroundAvailable)
        {
            infoText.Colour = backgroundAvailable ? Color4.SkyBlue : Color4.Orange;
            infoIcon.Icon = backgroundAvailable ? FontAwesome.Solid.Play : FontAwesome.Solid.Unlink;
        }
    }
}
