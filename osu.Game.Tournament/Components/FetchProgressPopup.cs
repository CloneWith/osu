// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A modal popup that shows the progress of an ongoing task.
    /// </summary>
    /// <remarks>The popup doesn't have a listener for tasks, and you should call <see cref="SetTaskCompleted"/> manually.</remarks>
    public partial class FetchProgressPopup : CompositeDrawable
    {
        /// <summary>
        /// The index of the current item being handled.
        /// </summary>
        public int CurrentCount
        {
            get => currentCount;
            set
            {
                currentCount = value;
                countText.Text = $"{value}/{totalCount}";
                progressBar.CurrentTime = value;
            }
        }

        /// <summary>
        /// The total count of items associated with the task.
        /// </summary>
        public int TotalCount
        {
            get => totalCount;
            set
            {
                totalCount = value;
                countText.Text = $"{currentCount}/{value}";
                progressBar.EndTime = value;
            }
        }

        /// <summary>
        /// The text to show below the popup title.
        /// </summary>
        public LocalisableString PromptString
        {
            get => promptText.Text;
            set => promptText.Text = value;
        }

        /// <summary>
        /// The text to show beside the counter.
        /// </summary>
        public LocalisableString StatusString
        {
            get => statusText.Text;
            set => statusText.Text = value;
        }

        private readonly bool closeOnComplete;
        private readonly Action? cancelAction;

        private ProgressBar progressBar = null!;

        private TournamentSpriteText promptText = null!;
        private TruncatingSpriteText statusText = null!;
        private TournamentSpriteText countText = null!;
        private ClickTwiceButton cancelButton = null!;

        private int currentCount;
        private int totalCount;

        /// <inheritdoc cref="FetchProgressPopup"/>
        /// <param name="cancelAction">the action to trigger when click the cancel button</param>
        /// <param name="closeOnComplete">whether to close the popup automatically after the task is completed</param>
        public FetchProgressPopup(Action? cancelAction = null, bool closeOnComplete = false)
        {
            this.cancelAction = cancelAction;
            this.closeOnComplete = closeOnComplete;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.2f),
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 500,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeaFoamDark,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(15),
                            Padding = new MarginPadding(10),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Fetching...",
                                    Font = OsuFont.Torus.With(size: 32, weight: FontWeight.SemiBold),
                                },
                                promptText = new TournamentSpriteText
                                {
                                    Name = @"Prompt text",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Fetching data from server, please wait...",
                                    Font = OsuFont.Torus.With(size: 20),
                                },
                                new GridContainer
                                {
                                    Name = @"Progress text grid",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
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
                                            statusText = new TruncatingSpriteText
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                RelativeSizeAxes = Axes.X,
                                                Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                                Text = @"Please wait...",
                                            },
                                            countText = new TournamentSpriteText
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                                Text = @"???",
                                            },
                                        },
                                    },
                                },
                                progressBar = new ProgressBar(false)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    EndTime = 1,
                                    Height = 6,
                                    Masking = true,
                                    CornerRadius = 3,
                                    FillColour = colours.Sky,
                                },
                                cancelButton = new ClickTwiceButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    IdleIcon = FontAwesome.Regular.StopCircle,
                                    ActiveIcon = FontAwesome.Solid.Stop,
                                    IdleText = @"Cancel",
                                    ActiveText = @"Click again to cancel",
                                    Action = () => invokeAndExpire(cancelAction),
                                },
                            },
                        },
                    },
                },
            };

            cancelButton.Enabled.Value = cancelAction != null;
        }

        /// <summary>
        /// Tell the popup that the task has completed.
        /// </summary>
        /// <param name="faulted">whether the task completed successfully</param>
        /// <remarks>The popup will display corresponding texts for the task's status.</remarks>
        public void SetTaskCompleted(bool faulted = false)
        {
            progressBar.CurrentTime = progressBar.EndTime = 1;
            statusText.Text = faulted ? @"This task has failed." : @"Task completed!";
            statusText.FlashColour(faulted ? FumoColours.SunshineYellow.Regular : FumoColours.SeaBlue.Light, 1000, Easing.OutSine);
            countText.FlashColour(faulted ? FumoColours.SunshineYellow.Regular : FumoColours.SeaBlue.Light, 1000, Easing.OutSine);

            cancelButton.Enabled.Value = true;
            cancelButton.Text = @"Close";
            cancelButton.IdleIcon = cancelButton.ActiveIcon = FontAwesome.Solid.Times;
            cancelButton.Action = () => invokeAndExpire();

            if (closeOnComplete)
                this.Delay(2000).FadeOut(500, Easing.OutQuint).Then().Expire();
        }

        private void invokeAndExpire(Action? action = null)
        {
            action?.Invoke();
            cancelButton.Enabled.Value = false;
            this.FadeOut(500, Easing.OutQuint).Then().Expire();
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnScroll(ScrollEvent e) => true;
    }
}
