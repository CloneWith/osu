// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components.Shapes;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneGraphPartTriangle : TournamentTestScene
    {
        private readonly GraphPartTriangle triangle;

        public TestSceneGraphPartTriangle()
        {
            Child = triangle = new GraphPartTriangle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 500,
                Height = 500,
            };
        }

        [Test]
        public void TestProperties()
        {
            AddSliderStep("Start angle", -180, 180, 0, v => triangle.StartAngle = v * (float)Math.PI / 180);
            AddSliderStep("End angle", -180, 180, 60, v => triangle.EndAngle = v * (float)Math.PI / 180);
            AddSliderStep("Start position", 0, 1, 0.5f, v => triangle.StartPosition = v);
            AddSliderStep("End position", 0, 1, 0.5f, v => triangle.EndPosition = v);
        }
    }
}
