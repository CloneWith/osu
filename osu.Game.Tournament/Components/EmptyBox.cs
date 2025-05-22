// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class EmptyBox : Container
    {
        private readonly Box box;

        public Color4 BoxColour
        {
            get => box.Colour;
            set => box.Colour = value;
        }

        public EmptyBox(int cornerRadius = 0)
        {
            Size = Vector2.One;
            Masking = true;
            CornerRadius = cornerRadius;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}
