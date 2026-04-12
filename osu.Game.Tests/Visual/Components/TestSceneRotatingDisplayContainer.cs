// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Tests.Visual.Components
{
    public partial class TestSceneRotatingDisplayContainer : OsuTestScene
    {
        private readonly RotatingDisplayContainer display;

        public TestSceneRotatingDisplayContainer()
        {
            Add(display = new RotatingDisplayContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 500,
                Height = 500,
            });
        }

        [Test]
        public void TestAddLayer()
        {
            AddStep("Add text layer", () => display.AddLayer(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = @"Hello Rotating Display!",
                Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 24),
            }));

            AddStep("Start rotating", () => display.Start());

            AddStep("Add another text layer", () => display.AddLayer(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = @"It is absolutely fun!",
                Colour = new OsuColour().Sky,
                Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 24),
            }));

            AddStep("Stop rotating", () => display.Pause());
        }

        [Test]
        public void TestDisplayProperty()
        {
            AddSliderStep("Display length", 500, 5000, 5000, d => display.DisplayLength = d);
            AddSliderStep("Transform duration", 0, 1000, 500, d => display.TransformLength = d);
            AddSliderStep("Interval", 0, 1000, 1000, d => display.Interval = d);

            AddToggleStep("Cross animation", t => display.CrossAnimation = t);
            AddToggleStep("Random order", t => display.Random = t);
            AddToggleStep("Lopped display", t => display.Looped = t);
        }
    }
}
