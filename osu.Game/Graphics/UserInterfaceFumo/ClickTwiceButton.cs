// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    public partial class ClickTwiceButton : FumoButton
    {
        private double? activateTime;

        public string Text
        {
            get => activateTime != null ? ActiveText : IdleText;
            set => IdleText = ActiveText = value;
        }

        public ClickTwiceButton(FillDirection direction = FillDirection.Horizontal)
            : base(direction)
        {
        }

        protected override void LoadComplete()
        {
            Enabled.BindValueChanged(e =>
            {
                if (!e.NewValue && activateTime != null) deactivate();
            });

            base.LoadComplete();
        }

        protected override void Update()
        {
            if (activateTime != null && Clock.CurrentTime - activateTime > 5000)
            {
                activateTime = null;
                deactivate();
            }

            base.Update();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Enabled.Value) return base.OnClick(e);

            if (activateTime == null)
            {
                activateTime = Clock.CurrentTime;
                activate();
            }
            else
            {
                Action?.Invoke();
                activateTime = null;
                deactivate();
            }

            return base.OnClick(e);
        }

        private void activate()
        {
            IconSprite.Icon = ActiveIcon ?? IdleIcon ?? new IconUsage();
            TextSprite.Text = ActiveText != string.Empty ? ActiveText : IdleText;
            Background.FadeColour(ActiveBackgroundColour, TRANSFORM_DURATION, Easing.OutQuint);
            IconSprite.FadeColour(ActiveForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
            TextSprite.FadeColour(ActiveForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
        }

        private void deactivate()
        {
            IconSprite.Icon = IdleIcon ?? new IconUsage();
            TextSprite.Text = IdleText;
            Background.FadeColour(IdleBackgroundColour, TRANSFORM_DURATION, Easing.OutQuint);
            IconSprite.FadeColour(IdleForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
            TextSprite.FadeColour(IdleForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
        }
    }
}
