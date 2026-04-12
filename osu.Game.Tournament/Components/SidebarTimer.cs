// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A timer with a progress display and cancel button. Just a simple timer, mainly used for auto screen changing.
    /// </summary>
    public partial class SidebarTimer : CompositeDrawable
    {
        /// <summary>
        /// The text to display when the timer is inactive.
        /// </summary>
        public LocalisableString IdleText
        {
            get => idleText;
            set
            {
                idleText = value;

                if (!IsRunning)
                    descriptionText.Text = idleText;
            }
        }

        public LocalisableString ActiveText
        {
            get => activeText;
            set
            {
                activeText = value;

                if (IsRunning)
                    descriptionText.Text = activeText;
            }
        }

        public double TimerTime
        {
            get => timerTime;
            set
            {
                if (value < 0)
                    return;

                if (IsRunning)
                {
                    Logger.Log("Update of timer time while the timer is active. Stopping the timer.");
                    Stop();
                }

                timerTime = value;
                progressBar.EndTime = timerTime;
            }
        }

        public bool IsRunning { get; private set; }

        public event Action? OnCancel;

        private LocalisableString idleText = BaseStrings.IdleScreenTimer;
        private LocalisableString activeText = @"Running...";

        private double timerTime;
        private readonly BindableDouble currentTime = new BindableDouble();
        private readonly Bindable<Color4> progressColour = new Bindable<Color4>(FumoColours.SeaBlue.Light);

        private readonly TruncatingSpriteText descriptionText;
        private readonly ProgressBar progressBar;
        private readonly ClickTwiceButton cancelButton;

        public SidebarTimer()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(3),
                Padding = new MarginPadding { Horizontal = 5 },
                Children = new Drawable[]
                {
                    descriptionText = new TruncatingSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Text = IdleText,
                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                    },
                    progressBar = new ProgressBar(false)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = 6,
                        Masking = true,
                        CornerRadius = 3,
                        FillColour = FumoColours.SeaBlue.Light,
                    },
                    cancelButton = new ClickTwiceButton(sampleSet: HoverSampleSet.Muted)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Text = BaseStrings.Cancel,
                        Action = Stop,
                        Enabled = { Value = false },
                        IdleIcon = FontAwesome.Solid.Stop,
                    },
                },
            };

            currentTime.BindValueChanged(e =>
            {
                progressBar.CurrentTime = e.NewValue;

                if (Math.Abs(e.NewValue - timerTime) < double.Epsilon)
                {
                    this.TransformBindableTo(progressColour, FumoColours.SeaBlue.Light.Opacity(0.5f), 500, Easing.OutExpo);
                    cancelButton.Enabled.Value = false;
                    descriptionText.Text = BaseStrings.ScreenChanged;
                }
            });
            progressColour.BindValueChanged(e => progressBar.FillColour = e.NewValue);
        }

        public void Start()
        {
            if (timerTime == 0)
            {
                Logger.Log("Time unset or invalid. Won't start.");
                return;
            }

            IsRunning = true;
            cancelButton.Enabled.Value = true;
            currentTime.Value = 0;
            descriptionText.Text = activeText;
            this.TransformBindableTo(progressColour, FumoColours.SeaBlue.Light, 500, Easing.OutExpo);
            this.TransformBindableTo(currentTime, timerTime, timerTime);
        }

        public void Stop()
        {
            IsRunning = false;
            cancelButton.Enabled.Value = false;
            ClearTransforms();
            this.TransformBindableTo(progressColour, FumoColours.SeaBlue.Light.Opacity(0.5f), 500, Easing.OutExpo);
            OnCancel?.Invoke();
        }
    }
}
