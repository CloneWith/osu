// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class GlitchingTriangles : CompositeDrawable
    {
        public GlitchingTriangles()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private double? lastGenTime;

        private const double time_between_triangles = 22;

        protected override void Update()
        {
            base.Update();

            if (lastGenTime == null || Time.Current - lastGenTime > time_between_triangles)
            {
                lastGenTime = (lastGenTime ?? Time.Current) + time_between_triangles;

                Drawable triangle = new OutlineTriangle(RNG.NextBool(), (RNG.NextSingle() + 0.2f) * 80)
                {
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                };

                AddInternal(triangle);

                triangle.FadeOutFromOne(120);
            }
        }

        /// <summary>
        /// Represents a sprite that is drawn in a triangle shape, instead of a rectangle shape.
        /// </summary>
        public partial class OutlineTriangle : BufferedContainer
        {
            public OutlineTriangle(bool outlineOnly, float size)
                : base(cachedFrameBuffer: true)
            {
                Size = new Vector2(size);

                InternalChildren = new Drawable[]
                {
                    new Triangle
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                };

                if (outlineOnly)
                {
                    AddInternal(new Triangle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Black,
                        Size = new Vector2(size - 5),
                        Blending = BlendingParameters.None,
                    });
                }

                Blending = BlendingParameters.Additive;
            }
        }
    }
}
