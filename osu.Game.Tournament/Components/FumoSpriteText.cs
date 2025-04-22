// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
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

        protected readonly Circle Background;

        public FumoSpriteText(
            string text = "", int fontSize = 24,
            Color4? backgroundColor = null, Color4? textColor = null, FontWeight? textWeight = FontWeight.SemiBold)
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                Background = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = backgroundColor.IsNotNull() ? backgroundColor.Value : Color4.White,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = textColor.IsNotNull() ? textColor.Value : Color4.Black,
                    Font = OsuFont.Torus.With(weight: textWeight, size: fontSize),
                    Padding = new MarginPadding { Horizontal = 50, Vertical = 10 },
                    Text = text,
                    Shadow = false,
                }
            };
        }
    }
}
