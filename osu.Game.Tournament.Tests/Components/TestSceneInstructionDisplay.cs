// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Tournament.Screens.Board.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneInstructionDisplay : TournamentTestScene
    {
        public TestSceneInstructionDisplay()
        {
            Add(new InstructionDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}
