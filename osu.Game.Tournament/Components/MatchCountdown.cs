// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class MatchCountdown : CompositeDrawable
    {
        /// <summary>
        /// The target time we should compare to. Used for calculating the remaining time.
        /// </summary>
        public readonly Bindable<DateTimeOffset?> Target = new Bindable<DateTimeOffset?>();

        /// <summary>
        /// If the countdown is in progress.
        /// </summary>
        public bool OnGoing { get; private set; }

        public static Color4 NormalColour = Color4.White;
        public static Color4 AccentColour = FumoColours.SeaBlue.Regular;
        public static Color4 NormalContentColour = Color4.Black;
        public static Color4 AccentContentColour = Color4.White;

        private readonly Box bottomBox;
        private readonly Box topBox;
        private readonly FillFlowContainer contentFlow;
        private readonly TrianglesV2 triangles;
        private readonly Container iconContainer;

        // Elements specifically used for cases with target time unset.
        private readonly SpriteIcon indicatorIcon;
        private readonly OsuSpriteText waitingText;
        private readonly FillFlowContainer timerFlow;

        private OsuSpriteText? countdownHourPart;
        private OsuSpriteText? countdownMinutePart;
        private OsuSpriteText? countdownSecondPart;
        private OsuSpriteText? countdownMSecondPart;

        private string lastHour = string.Empty;
        private string lastMinute = string.Empty;
        private string lastSecond = string.Empty;

        private TimeSpan? remainingTime;
        private bool placeholderLoaded;

        // If the complete animation is triggered since last countdown reset.
        private bool endTriggered;

        public MatchCountdown()
        {
            Width = 360;
            Height = 150;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = "Bottom Layer",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 10,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        bottomBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = AccentColour,
                        },
                    },
                },
                new Container
                {
                    Name = "Top Layer",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.95f,
                    CornerRadius = 10,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        topBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = NormalColour,
                        },
                        triangles = new TrianglesV2
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.3f,
                            Colour = AccentColour,
                        },
                        contentFlow = new FillFlowContainer
                        {
                            Name = "Countdown Content",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 300,
                            AutoSizeEasing = Easing.OutQuint,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10),
                        },
                    },
                },
            };

            // Previously the text will move when Icon is spinning.
            // Now we use a container to hold the icon and make it spin (
            iconContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.None,
                Size = new Vector2(40),
                Child = indicatorIcon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.HourglassHalf,
                    Size = new Vector2(32),
                    Colour = NormalContentColour,
                }
            };

            waitingText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 32, weight: FontWeight.SemiBold),
                Colour = NormalContentColour,
                Shadow = false,
            };

            timerFlow = new FillFlowContainer
            {
                Name = "Timer",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                AutoSizeDuration = 300,
                AutoSizeEasing = Easing.OutQuint,
                Direction = FillDirection.Horizontal,
            };
        }

        /// <summary>
        /// If the countdown takes a long time. In this case it means more than one day.
        /// </summary>
        /// <param name="timeOffset">a <see cref="DateTimeOffset"/> representing the target time</param>
        /// <returns>true if taking more than one day, otherwise false.</returns>
        private bool countdownLongProgress(DateTimeOffset timeOffset) =>
            timeOffset.ToLocalTime() - DateTimeOffset.Now >= TimeSpan.FromDays(1);

        /// <summary>
        /// If the countdown just ended, meaning the target time is earlier than current time by less than 3 minutes.
        /// </summary>
        /// <param name="timeOffset">a <see cref="DateTimeOffset"/> representing the target time</param>
        /// <returns>true if just ended, otherwise false.</returns>
        private bool countdownJustEnded(DateTimeOffset timeOffset) =>
            countdownEnded(timeOffset) && DateTimeOffset.Now - timeOffset.ToLocalTime() <= TimeSpan.FromMinutes(3);

        private LocalisableString getWaitingString() => Target.Value.HasValue
            ? !countdownEnded(Target.Value.Value)
                ? countdownLongProgress(Target.Value.Value)
                    ? CountdownStrings.LongWaitingPrompt
                    : (Target.Value.Value - DateTimeOffset.Now).ToString()
                : countdownJustEnded(Target.Value.Value)
                    ? CountdownStrings.JustEndedPrompt(HumanizerUtils.Humanize(Target.Value.Value - DateTimeOffset.Now))
                    : CountdownStrings.LongEndedPrompt
            : CountdownStrings.EmptyTimePrompt;

        private void updateTimerTextParts(bool init = false)
        {
            if (!remainingTime.HasValue) return;

            if (init)
            {
                timerFlow.Clear();

                countdownHourPart = new CountdownSpriteText();
                countdownMinutePart = new CountdownSpriteText();
                countdownSecondPart = new CountdownSpriteText();
                countdownMSecondPart = new CountdownSpriteText
                {
                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold, fixedWidth: true),
                };

                timerFlow.AddRange(new Drawable[]
                {
                    countdownHourPart,
                    countdownMinutePart,
                    countdownSecondPart,
                    countdownMSecondPart,
                });
            }

            if (countdownHourPart == null || countdownMinutePart == null || countdownSecondPart == null || countdownMSecondPart == null)
                return;

            countdownHourPart.Text = remainingTime.Value.ToString(@"hh\:");
            countdownMinutePart.Text = remainingTime.Value.ToString(@"mm\:");
            countdownSecondPart.Text = remainingTime.Value.ToString(@"ss");
            countdownMSecondPart.Text = $".{remainingTime.Value.Milliseconds:000}";

            countdownHourPart.FadeTo(remainingTime.Value.Hours > 0 ? 1 : 0, 100, Easing.OutQuint);
            countdownMSecondPart.FadeTo(remainingTime.Value.TotalMinutes <= 1f ? 1 : 0, 100, Easing.OutQuint);

            if ((remainingTime.Value - TimeSpan.FromMinutes(1)).NearlyEqualsZero())
            {
                bottomBox.FadeColour(NormalColour, 1000, Easing.OutQuint);
                topBox.FadeColour(AccentColour, 1000, Easing.OutQuint);
                triangles.FadeColour(AccentContentColour, 1000, Easing.OutQuint);

                foreach (var t in timerFlow)
                {
                    t?.FadeColour(AccentContentColour, 1000, Easing.OutQuint);
                }
            }
            else if ((lastHour != countdownHourPart.Text && remainingTime.Value.Hours == 0)
                     || (lastMinute != countdownMinutePart.Text && remainingTime.Value.Minutes == 0)
                     || (remainingTime.Value.TotalMinutes <= 30 && lastSecond != countdownSecondPart.Text && remainingTime.Value.Seconds == 0))
            {
                bottomBox.FlashColour(NormalColour, 1000, Easing.OutQuint);
                topBox.FlashColour(AccentColour, 1000, Easing.OutQuint);
                triangles.FlashColour(AccentContentColour, 1000, Easing.OutQuint);

                foreach (var t in timerFlow)
                {
                    t?.FlashColour(AccentContentColour, 1000, Easing.OutQuint);
                }
            }

            lastHour = countdownHourPart.Text.ToString();
            lastMinute = countdownMinutePart.Text.ToString();
            lastSecond = countdownSecondPart.Text.ToString();
        }

        /// <summary>
        /// If the countdown is ended.
        /// </summary>
        /// <param name="timeOffset">a <see cref="DateTimeOffset"/> representing the target time</param>
        /// <returns>true if the countdown is ended, otherwise false.</returns>
        private bool countdownEnded(DateTimeOffset timeOffset) => DateTimeOffset.Now - timeOffset.ToLocalTime() >= TimeSpan.Zero;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Target.BindValueChanged(targetTimeChanged);
            Target.TriggerChange();
        }

        protected override void Update()
        {
            base.Update();

            if (!Target.Value.HasValue || !OnGoing)
                return;

            remainingTime = Target.Value.Value - DateTimeOffset.Now;

            if (remainingTime.Value.NearlyEqualsZero())
            {
                OnGoing = false;
                fillDoneContent();
            }
            else updateTimerTextParts();
        }

        private void targetTimeChanged(ValueChangedEvent<DateTimeOffset?> target)
        {
            if (target.NewValue == null || countdownEnded(target.NewValue.Value) || countdownLongProgress(target.NewValue.Value))
                fillPlaceholderContent();
            else fillTimerContent((target.NewValue.Value - DateTimeOffset.Now).TotalMinutes <= 1);
        }

        /// <summary>
        /// Fade out the main content flow and ensure everything is in place.
        /// </summary>
        /// <remarks>Note that this method takes 450ms to complete. You may need to add a delay manually.</remarks>
        private void reset(bool fastMode = false)
        {
            endTriggered = false;
            int transformTime = fastMode ? 150 : 300;

            Scheduler.Add(() =>
            {
                contentFlow.FadeOut(150, Easing.OutQuint)
                           .Delay(transformTime).FadeIn();

                bottomBox.FadeColour(AccentColour, transformTime, Easing.OutQuint);
                topBox.FadeColour(NormalColour, transformTime, Easing.OutQuint);
                triangles.FadeColour(AccentColour, transformTime, Easing.OutQuint);
            });

            Scheduler.AddDelayed(() =>
            {
                contentFlow.Clear(false);
                contentFlow.ScaleTo(1);

                iconContainer.FadeOut();

                // Reset the icon container so Hourglass icon can be shown correctly again
                iconContainer.ClearTransforms();
                iconContainer.Position = Vector2.Zero;
                iconContainer.Scale = Vector2.One;

                waitingText.FadeOut();
                timerFlow.FadeOut();
                indicatorIcon.ClearTransforms();
                indicatorIcon.RotateTo(0);
                indicatorIcon.Icon = FontAwesome.Solid.HourglassHalf;
                indicatorIcon.Colour = NormalContentColour;
                waitingText.Colour = NormalContentColour;
                waitingText.ClearTransforms();
                timerFlow.ClearTransforms();

                if (Target.Value.HasValue && !countdownEnded(Target.Value.Value) && !countdownLongProgress(Target.Value.Value))
                    updateTimerTextParts(true);
                else
                    waitingText.Text = getWaitingString();
            }, transformTime);
        }

        private void showHourglassIcon()
        {
            contentFlow.Direction = FillDirection.Horizontal;
            contentFlow.Anchor = Anchor.Centre;
            contentFlow.Origin = Anchor.Centre;

            contentFlow.Clear();

            contentFlow.Add(iconContainer);

            iconContainer.Alpha = 0;
            iconContainer.Scale = new Vector2(2.5f);

            iconContainer.Anchor = Anchor.Centre;
            iconContainer.Origin = Anchor.Centre;

            iconContainer.FadeIn(100, Easing.OutQuint);

            iconContainer.ScaleTo(0.9f, 350, Easing.OutQuint)
                         .Then(-90)
                         .ScaleTo(1f, 400, Easing.OutCubic);
        }

        private void fillDoneContent()
        {
            endTriggered = true;

            if (countdownMSecondPart != null)
            {
                countdownMSecondPart.Text = ".000";
            }

            contentFlow.FadeOut().Delay(500).FadeIn().Delay(500).Loop(0, 3);
            contentFlow.Delay(2000).ScaleTo(1.2f, 1000, Easing.InQuint);

            Scheduler.AddDelayed(() =>
            {
                contentFlow.Clear(false);
                indicatorIcon.ClearTransforms();
                indicatorIcon.RotateTo(0);
                indicatorIcon.Icon = FontAwesome.Solid.Bell;
                indicatorIcon.Colour = AccentContentColour;
                waitingText.Text = CountdownStrings.EndedPrompt;
                waitingText.Colour = AccentContentColour;

                contentFlow.FadeInFromZero(500, Easing.InOutQuint);
                contentFlow.ScaleTo(1.45f, 1500, Easing.OutQuint)
                           .Then().ScaleTo(1, 10000, Easing.InOutQuint);

                iconContainer.Show();
                waitingText.Show();

                contentFlow.AddRange(new Drawable[]
                {
                    iconContainer,
                    waitingText,
                });

                for (int i = 1; i <= 3; i++)
                {
                    using (BeginDelayedSequence(4500 * (i - 1)))
                    {
                        indicatorIcon.RotateTo(-15, 50, Easing.OutQuint).Then()
                                     .RotateTo(15, 50, Easing.OutQuint).Loop(0, 20);
                        indicatorIcon.Delay(2000).RotateTo(0, 1000, Easing.OutQuint);
                    }
                }
            }, 3000);
        }

        private void fillTimerContent(bool fastMode = false)
        {
            if (!OnGoing || !Target.Value.HasValue)
            {
                reset(fastMode);

                // When time is not enough, don't play hourglass animation
                if (!fastMode)
                    Scheduler.AddDelayed(showHourglassIcon, 500);

                Scheduler.AddDelayed(() =>
                {
                    // Add the placeholder text after a little delay to make it look better
                    contentFlow.Add(timerFlow);
                    timerFlow.Delay(fastMode ? 0 : 500).FadeIn(300, Easing.OutQuint);

                    if (!fastMode)
                    {
                        using (BeginDelayedSequence(250))
                        {
                            iconContainer.FadeOut(300, Easing.InQuint);
                        }
                    }
                }, fastMode ? 200 : 1000);
            }
            else
            {
                if (Target.Value.HasValue && Target.Value.Value - DateTimeOffset.Now > TimeSpan.FromMinutes(1))
                {
                    bottomBox.FadeColour(AccentColour, 1000, Easing.OutQuint);
                    topBox.FadeColour(NormalColour, 1000, Easing.OutQuint);
                    triangles.FadeColour(AccentColour, 1000, Easing.OutQuint);

                    foreach (var t in timerFlow)
                    {
                        t?.FadeColour(NormalContentColour, 1000, Easing.OutQuint);
                    }
                }

                foreach (var t in timerFlow)
                {
                    t?.FlashColour(AccentContentColour, 500, Easing.OutQuint);
                }
            }

            OnGoing = true;
        }

        private void fillPlaceholderContent()
        {
            // No need to fade the whole content when countdown state is unchanged
            if (OnGoing || !Target.Value.HasValue || !placeholderLoaded || endTriggered)
            {
                reset();

                Scheduler.AddDelayed(() =>
                {
                    contentFlow.Direction = FillDirection.Horizontal;
                    showHourglassIcon();
                }, 500);

                Scheduler.AddDelayed(() =>
                {
                    // Add the placeholder text after a little delay to make it look better
                    contentFlow.Direction = FillDirection.Horizontal;
                    contentFlow.Spacing = new Vector2(10);

                    contentFlow.Add(waitingText);
                    waitingText.Text = getWaitingString();
                    waitingText.Delay(200).FadeIn(500, Easing.OutQuint);

                    using (BeginDelayedSequence(1000))
                    {
                        indicatorIcon.RotateTo(0)
                                     .Then().RotateTo(360 * 5, 3000, Easing.InOutQuint)
                                     .Loop(3000);
                    }
                }, 1000);

                placeholderLoaded = true;
            }
            else
            {
                waitingText.FadeOut(150, Easing.OutQuint);

                Scheduler.AddDelayed(() =>
                {
                    waitingText.Text = getWaitingString();
                    waitingText.FadeIn(150, Easing.OutQuint);
                }, 150);
            }

            OnGoing = false;
        }

        private partial class CountdownSpriteText : OsuSpriteText
        {
            public CountdownSpriteText()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                Font = OsuFont.Torus.With(size: 60, weight: FontWeight.SemiBold, fixedWidth: true);
                Colour = NormalContentColour;
                Shadow = false;
            }
        }
    }
}
