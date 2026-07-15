using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseProfileItem
    {
        public partial class CloneRenamePopover : OsuPopover
        {
            [Resolved]
            private ShowcaseStorage storage { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            private readonly ShowcaseConfig source;

            private string targetName => targetTextBox.Current.Value + @".json";

            private readonly LabelledTextBox targetTextBox;
            private readonly LabelledSwitchButton cloneSwitchButton;
            private readonly SpriteIcon promptIcon;
            private readonly OsuSpriteText promptText;
            private readonly ShearedButton confirmButton;

            private readonly CancellationTokenSource cts = new CancellationTokenSource();

            public CloneRenamePopover(ShowcaseConfig source, bool cloneAsDefault = true)
            {
                this.source = source;

                const float content_width = 500;

                Child = new FillFlowContainer
                {
                    Width = content_width,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(7),
                    Children = new Drawable[]
                    {
                        targetTextBox = new LabelledTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Label = TournamentShowcaseStrings.NewFilenameLabel,
                            TabbableContentContainer = this,
                        },
                        cloneSwitchButton = new LabelledSwitchButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Label = TournamentShowcaseStrings.MakeDuplicateLabel,
                            Current = { Value = cloneAsDefault },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(7),
                            Children = new Drawable[]
                            {
                                promptIcon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(20),
                                },
                                promptText = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.Style.Body,
                                },
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
                                confirmButton = new ShearedButton
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = content_width,
                                    Text = cloneSwitchButton.Current.Value ? CommonStrings.Clone : CommonStrings.Rename,
                                    Action = performAction,
                                },
                            },
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Body.BorderThickness = 3;
                Body.BorderColour = colourProvider.Colour1;

                confirmButton.DarkerColour = colourProvider.Colour1;
                confirmButton.LighterColour = colourProvider.Colour0;
                confirmButton.TextColour = colourProvider.Background6;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ScheduleAfterChildren(() => GetContainingFocusManager()!.ChangeFocus(targetTextBox));

                targetTextBox.Current.BindValueChanged(_ => checkTarget(), true);
                cloneSwitchButton.Current.BindValueChanged(e =>
                {
                    checkTarget();
                    confirmButton.Text = e.NewValue ? CommonStrings.Clone : CommonStrings.Rename;
                });
            }

            private void checkTarget()
            {
                if (!targetTextBox.Current.Value.IsSafeForFilename(out LocalisableString errorMessage, 50))
                {
                    showWarning(errorMessage);
                    return;
                }

                if (storage.Exists(targetName))
                {
                    showWarning(TournamentShowcaseStrings.FilenameTakenPrompt);
                    return;
                }

                confirmButton.Enabled.Value = true;
                promptIcon.Colour = Color4.White;
                promptIcon.Icon = cloneSwitchButton.Current.Value ? FontAwesome.Solid.PlusCircle : FontAwesome.Solid.ArrowCircleRight;
                promptText.Colour = Color4.White;
                promptText.Text = TournamentShowcaseStrings.NewFilenamePrompt(targetTextBox.Current.Value.TruncateMiddleWithEllipsis(20) + @".json");
            }

            private void showWarning(LocalisableString message)
            {
                confirmButton.Enabled.Value = false;
                promptIcon.Colour = colours.Orange1;
                promptIcon.Icon = FontAwesome.Solid.ExclamationTriangle;
                promptText.Colour = colours.Orange1;
                promptText.Text = message;
            }

            private void performAction()
            {
                bool success = false;

                targetTextBox.Current.Disabled = true;
                cloneSwitchButton.Current.Disabled = true;
                confirmButton.Enabled.Value = false;

                Task.Run(() =>
                {
                    try
                    {
                        if (cloneSwitchButton.Current.Value)
                            storage.SaveChangesTo(source, targetName);
                        else
                            storage.Move(source.Filename.Value, targetName);

                        success = true;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"An error occurred while saving the showcase profile: {e.Message}", LoggingTarget.Runtime, LogLevel.Error);
                        showWarning(e.Message);
                    }
                }, cts.Token).ContinueWith(_ => Scheduler.Add(() =>
                {
                    targetTextBox.Current.Disabled = false;
                    cloneSwitchButton.Current.Disabled = false;
                    confirmButton.Enabled.Value = true;

                    if (success)
                        Hide();
                }), cts.Token);
            }

            protected override void Dispose(bool isDisposing)
            {
                cts.Cancel();
                base.Dispose(isDisposing);
            }
        }
    }
}
