// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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

        public Color4 NormalColour = Color4.White;
        public Color4 AccentColour = FumoColours.SeaBlue.Regular;
        public Color4 NormalContentColour = Color4.Black;
        public Color4 AccentContentColour = Color4.White;

        private readonly Container topLayer;
        private readonly Container bottomLayer;

        private readonly Box bottomBox;
        private readonly Box topBox;
        private readonly FillFlowContainer contentFlow;

        // Elements specifically used for cases with target time unset.
        private readonly SpriteIcon hourglassIcon;
        private readonly OsuSpriteText waitingText;

        public MatchCountdown()
        {
            Width = 360;
            Height = 150;

            InternalChildren = new Drawable[]
            {
                bottomLayer = new Container
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
                topLayer = new Container
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

            hourglassIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Regular.Hourglass,
                Size = new Vector2(32),
                Colour = NormalContentColour,
            };

            waitingText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 32, weight: FontWeight.SemiBold),
                Text = "Please wait...",
                Colour = NormalContentColour,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // TODO: Add code logic for real countdown
            if (!Target.Value.HasValue) fillPlaceholderContent();

            Target.BindValueChanged(targetTimeChanged);
        }

        private void targetTimeChanged(ValueChangedEvent<DateTimeOffset?> target)
        {
            if (target.NewValue == null) fillPlaceholderContent();
        }

        private void fillPlaceholderContent()
        {
            contentFlow.FadeOut(150, Easing.OutQuint)
                       .Delay(300).FadeIn();

            Scheduler.AddDelayed(_ =>
            {
                // Keep children for reusing purposes
                contentFlow.Clear(false);

                hourglassIcon.FadeOut();
                waitingText.FadeOut();
                hourglassIcon.ClearTransforms();
                hourglassIcon.RotateTo(0);
                waitingText.ClearTransforms();
            }, false, 150);

            Scheduler.AddDelayed(_ =>
            {
                contentFlow.Add(hourglassIcon);
                hourglassIcon.ScaleTo(2.5f).Then().ScaleTo(1, 500, Easing.OutQuint);
                hourglassIcon.Delay(100).FadeIn(300, Easing.OutQuint);
            }, false, 500);

            Scheduler.AddDelayed(_ =>
            {
                // Add the placeholder text after a little delay to make it look better
                contentFlow.Add(waitingText);
                waitingText.Delay(200).FadeIn(500, Easing.OutQuint);

                using (BeginDelayedSequence(1000))
                {
                    hourglassIcon.RotateTo(0)
                                 .Then().RotateTo(360 * 5, 3000, Easing.InOutQuint)
                                 .Loop(3000);
                }
            }, false, 1000);
        }
    }
}
