// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Tournament.Components;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneChessPiece : TournamentTestScene
    {
        private readonly FumoChessPiece chessPiece;

        public TestSceneChessPiece()
        {
            Add(chessPiece = new FumoChessPiece("HR", "1"));
        }

        [Test]
        public void TestChessBorderOffset()
        {
            AddSliderStep("Scale", 0.5f, 3f, 1f, v => chessPiece.Scale = new Vector2(v));
        }
    }
}
