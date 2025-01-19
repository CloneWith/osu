// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    /// <summary>
    /// A general button class for those with two statuses and optional icons and texts.
    /// </summary>
    public partial class FumoButton : OsuAnimatedButton
    {
        protected const int TRANSFORM_DURATION = 600;

        protected readonly FillDirection LayoutDirection;

        protected Box Background = null!;
        protected SpriteIcon IconSprite = null!;
        protected SpriteText TextSprite = null!;

        public new Action? Action;

        public string IdleText = string.Empty;
        public string ActiveText = string.Empty;
        public IconUsage? IdleIcon;
        public IconUsage? ActiveIcon;

        public ColourInfo IdleBackgroundColour = Color4Extensions.FromHex("#232323");
        public ColourInfo IdleForegroundColour = Color4.White;
        public ColourInfo ActiveBackgroundColour = Color4.SkyBlue;
        public ColourInfo ActiveForegroundColour = Color4.Black;

        public static void DummyAction() { }

        public FumoButton(FillDirection direction = FillDirection.Horizontal)
        {
            // Overriding this to avoid accidentally triggering clicks.
            base.Action = DummyAction;

            LayoutDirection = direction;
            AutoSizeAxes = Axes.Both;
            AutoSizeDuration = TRANSFORM_DURATION;
            AutoSizeEasing = Easing.OutQuint;

            if (ActiveText == string.Empty) ActiveText = IdleText;
            ActiveIcon ??= IdleIcon;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.AddRange(new Drawable[]
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = IdleBackgroundColour
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = TRANSFORM_DURATION,
                    AutoSizeEasing = Easing.OutQuint,
                    Direction = LayoutDirection,
                    Spacing = new Vector2(3),
                    Margin = new MarginPadding { Horizontal = 8, Vertical = 6 },
                    Children = new Drawable[]
                    {
                        IconSprite = new SpriteIcon
                        {
                            Anchor = LayoutDirection == FillDirection.Horizontal ? Anchor.CentreLeft : Anchor.Centre,
                            Origin = LayoutDirection == FillDirection.Horizontal ? Anchor.CentreLeft : Anchor.Centre,
                            Icon = IdleIcon ?? new IconUsage(),
                            Size = new Vector2(20)
                        },
                        TextSprite = new OsuSpriteText
                        {
                            Anchor = LayoutDirection == FillDirection.Horizontal ? Anchor.CentreLeft : Anchor.Centre,
                            Origin = LayoutDirection == FillDirection.Horizontal ? Anchor.CentreLeft : Anchor.Centre,
                            Text = IdleText,
                            Colour = IdleForegroundColour,
                            Font = OsuFont.Torus.With(weight: FontWeight.SemiBold)
                        }
                    }
                }
            });
        }
    }
}
