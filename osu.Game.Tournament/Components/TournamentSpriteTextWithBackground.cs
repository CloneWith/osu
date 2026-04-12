// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentSpriteTextWithBackground : Container
    {
        public readonly TournamentSpriteText InnerText;

        public LocalisableString Text
        {
            get => InnerText.Text;
            set => InnerText.Text = value;
        }

        public Color4 BackgroundColour
        {
            get => Background.Colour;
            set => Background.Colour = value;
        }

        public Color4 TextColour
        {
            get => InnerText.Colour;
            set => InnerText.Colour = value;
        }

        public FontUsage Font
        {
            get => InnerText.Font;
            set => InnerText.Font = value;
        }

        public MarginPadding InnerPadding
        {
            get => InnerText.Padding;
            set => InnerText.Padding = value;
        }

        protected readonly Box Background;

        public TournamentSpriteTextWithBackground(
            int fontSize = 50, FontWeight? textWeight = FontWeight.SemiBold)
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = TournamentGame.ELEMENT_BACKGROUND_COLOUR,
                    RelativeSizeAxes = Axes.Both,
                },
                InnerText = new TournamentSpriteText
                {
                    Colour = TournamentGame.ELEMENT_FOREGROUND_COLOUR,
                    Font = OsuFont.Torus.With(weight: textWeight, size: fontSize),
                    Padding = new MarginPadding { Left = 10, Right = 20 },
                }
            };
        }
    }
}
