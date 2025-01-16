// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    public partial class StateSwitchButton : FumoButton
    {
        public BindableBool Current = new BindableBool();

        public string Text
        {
            get => Current.Value ? ActiveText : IdleText;
            set => IdleText = ActiveText = value;
        }

        public StateSwitchButton(FillDirection direction = FillDirection.Horizontal)
            : base(direction)
        {
            Action = dummyAction;
        }

        private void dummyAction() { }

        protected override void LoadComplete()
        {
            IconSprite.Icon = Current.Value ? ActiveIcon ?? new IconUsage() : IdleIcon ?? new IconUsage();
            TextSprite.Text = Current.Value ? ActiveText : IdleText;
            Background.Colour = Current.Value ? ActiveBackgroundColour : IdleBackgroundColour;
            IconSprite.Colour = Current.Value ? ActiveForegroundColour : IdleForegroundColour;
            TextSprite.Colour = Current.Value ? ActiveForegroundColour : IdleForegroundColour;

            Current.BindValueChanged(switchChanged);
            base.LoadComplete();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value) Current.Value = !Current.Value;
            return base.OnClick(e);
        }

        private void switchChanged(ValueChangedEvent<bool> e)
        {
            IconSprite.Icon = e.NewValue ? ActiveIcon ?? new IconUsage() : IdleIcon ?? new IconUsage();
            TextSprite.Text = e.NewValue ? ActiveText : IdleText;
            Background.FadeColour(e.NewValue ? ActiveBackgroundColour : IdleBackgroundColour, TRANSFORM_DURATION, Easing.OutQuint);
            IconSprite.FadeColour(e.NewValue ? ActiveForegroundColour : IdleForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
            TextSprite.FadeColour(e.NewValue ? ActiveForegroundColour : IdleForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
        }
    }
}
