// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class FumoSpriteText : CompositeDrawable
    {
        public readonly TournamentSpriteText Text;

        public Color4 TextColour
        {
            get => Text.Colour;
            set => Text.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => Background.Colour;
            set => Background.Colour = value;
        }

        public new Color4 BorderColour
        {
            get => Background.BorderColour;
            set => Background.BorderColour = value;
        }

        public new float BorderThickness
        {
            get => Background.BorderThickness;
            set => Background.BorderThickness = value;
        }

        protected readonly Circle Background;

        public FumoSpriteText(
            string text = "", int fontSize = 24,
            Color4? backgroundColor = null, Color4? textColor = null, FontWeight? textWeight = FontWeight.SemiBold)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Background = new Circle
                {
                    Colour = backgroundColor.IsNotNull() ? backgroundColor.Value : Color4.White,
                    BorderColour = FumoColours.SeaBlue.Regular,
                    BorderThickness = 2,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Colour = textColor.IsNotNull() ? textColor.Value : Color4.Black,
                    Font = OsuFont.Torus.With(weight: textWeight, size: fontSize),
                    Padding = new MarginPadding { Horizontal = 50, Vertical = 10 },
                    Text = text,
                }
            };
        }
    }
}
