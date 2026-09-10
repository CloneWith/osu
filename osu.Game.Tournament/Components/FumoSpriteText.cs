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
        protected readonly TournamentSpriteText Text;

        public Colour4 TextColour
        {
            get => Text.Colour;
            set => Text.Colour = value;
        }

        public Colour4 BackgroundColour
        {
            get => Background.Colour;
            set => Background.Colour = value;
        }

        protected readonly Circle Background;

        public FumoSpriteText(
            string text = "", int fontSize = 24,
            Colour4? backgroundColor = null, Colour4? textColor = null, FontWeight? textWeight = FontWeight.SemiBold)
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
