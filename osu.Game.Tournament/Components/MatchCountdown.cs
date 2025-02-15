// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
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

        private const string long_waiting_string = @"还要等很久呢...";
        private const string empty_time_string = @"在等着什么呢 >.<";
        private const string ended_string = @"开赛啦 开赛啦！";
        private const string just_ended_string = @"前就开始了";
        private const string long_ended_string = @"早就开始啦 >.<";

        private readonly Box bottomBox;
        private readonly Box topBox;
        private readonly FillFlowContainer contentFlow;

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

            indicatorIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.HourglassHalf,
                Size = new Vector2(32),
                Colour = NormalContentColour,
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

        private string getWaitingString() => Target.Value.HasValue
            ? !countdownEnded(Target.Value.Value)
                ? countdownLongProgress(Target.Value.Value)
                    ? long_waiting_string
                    : (Target.Value.Value - DateTimeOffset.Now).ToString()
                : countdownJustEnded(Target.Value.Value)
                    ? @$"{(Target.Value.Value - DateTimeOffset.Now).ToHumanizedString()}{just_ended_string}"
                    : long_ended_string
            : empty_time_string;

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

                foreach (var t in timerFlow)
                {
                    t?.FadeColour(AccentContentColour, 1000, Easing.OutQuint);
                }
            }
            else if (lastHour != countdownHourPart.Text && remainingTime.Value.Hours == 0
                     || lastMinute != countdownMinutePart.Text && remainingTime.Value.Minutes == 0
                     || remainingTime.Value.TotalMinutes <= 30 && lastSecond != countdownSecondPart.Text && remainingTime.Value.Seconds == 0)
            {
                bottomBox.FlashColour(NormalColour, 1000, Easing.OutQuint);
                topBox.FlashColour(AccentColour, 1000, Easing.OutQuint);

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

            if (!Target.Value.HasValue || countdownEnded(Target.Value.Value) || countdownLongProgress(Target.Value.Value))
                fillPlaceholderContent();
            else fillTimerContent();

            Target.BindValueChanged(targetTimeChanged);
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
            else fillTimerContent();
        }

        /// <summary>
        /// Fade out the main content flow and ensure everything is in place.
        /// </summary>
        /// <remarks>Note that this method takes 450ms to complete. You may need to add a delay manually.</remarks>
        private void reset()
        {
            Scheduler.Add(() =>
            {
                contentFlow.FadeOut(150, Easing.OutQuint)
                           .Delay(300).FadeIn();

                bottomBox.FadeColour(AccentColour, 300, Easing.OutQuint);
                topBox.FadeColour(NormalColour, 300, Easing.OutQuint);
            });

            Scheduler.AddDelayed(() =>
            {
                contentFlow.Clear(false);
                contentFlow.ScaleTo(1);

                indicatorIcon.FadeOut();
                waitingText.FadeOut();
                timerFlow.FadeOut();
                indicatorIcon.ClearTransforms();
                indicatorIcon.RotateTo(0);
                indicatorIcon.Icon = FontAwesome.Solid.HourglassHalf;
                indicatorIcon.Colour = NormalContentColour;
                waitingText.Colour = NormalContentColour;
                waitingText.ClearTransforms();
                timerFlow.ClearTransforms();

                if (Target.Value.HasValue && !countdownJustEnded(Target.Value.Value) && !countdownLongProgress(Target.Value.Value))
                    updateTimerTextParts(true);
                else
                    waitingText.Text = getWaitingString();
            }, 300);
        }

        private void showHourglassIcon()
        {
            contentFlow.Add(indicatorIcon);
            indicatorIcon.ScaleTo(2.5f).Then().ScaleTo(1, 500, Easing.OutQuint);
            indicatorIcon.Delay(100).FadeIn(300, Easing.OutQuint);
        }

        private void fillDoneContent()
        {
            contentFlow.Clear(false);
            indicatorIcon.ClearTransforms();
            indicatorIcon.RotateTo(0);
            indicatorIcon.Icon = FontAwesome.Solid.Bell;
            indicatorIcon.Colour = AccentContentColour;
            waitingText.Text = ended_string;
            waitingText.Colour = AccentContentColour;

            indicatorIcon.Show();
            waitingText.Show();

            contentFlow.AddRange(new Drawable[]
            {
                indicatorIcon,
                waitingText,
            });

            contentFlow.ScaleTo(1.5f, 500, Easing.OutQuint);
            this.Shake(shakeMagnitude: 4f);
            contentFlow.FadeColour(AccentColour, 500, Easing.OutQuint)
                       .Then().FadeColour(AccentContentColour, 1000, Easing.OutQuint)
                       .Loop(500, 5);
        }

        private void fillTimerContent()
        {
            if (!OnGoing || !Target.Value.HasValue)
            {
                reset();

                Scheduler.AddDelayed(showHourglassIcon, 500);

                Scheduler.AddDelayed(() =>
                {
                    // Add the placeholder text after a little delay to make it look better
                    contentFlow.Add(timerFlow);
                    timerFlow.Delay(200).FadeIn(300, Easing.OutQuint);

                    using (BeginDelayedSequence(300))
                    {
                        indicatorIcon.FadeOut(300, Easing.OutQuint);
                    }
                }, 1000);
            }
            else
            {
                if (Target.Value.HasValue && Target.Value.Value - DateTimeOffset.Now > TimeSpan.FromMinutes(1))
                {
                    bottomBox.FadeColour(AccentColour, 1000, Easing.OutQuint);
                    topBox.FadeColour(NormalColour, 1000, Easing.OutQuint);

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
            if (OnGoing || !Target.Value.HasValue)
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
                    contentFlow.Add(waitingText);
                    waitingText.Delay(200).FadeIn(500, Easing.OutQuint);

                    using (BeginDelayedSequence(1000))
                    {
                        indicatorIcon.RotateTo(0)
                                     .Then().RotateTo(360 * 5, 3000, Easing.InOutQuint)
                                     .Loop(3000);
                    }
                }, 1000);
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
