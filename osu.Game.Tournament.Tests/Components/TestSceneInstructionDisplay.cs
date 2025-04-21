// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneInstructionDisplay : TournamentTestScene
    {
        private readonly InstructionDisplay instructionDisplay;

        public TestSceneInstructionDisplay()
        {
            Add(instructionDisplay = new InstructionDisplay
            {
                Anchor = Anchor.Centre, Origin = Anchor.Centre,
            });
        }

        [Test]
        public void TestStepChange()
        {
            AddStep("Change to red win", () =>
            {
                instructionDisplay.Team = TeamColour.Red;
                instructionDisplay.Step = RoundStep.Win;
            });
        }
    }
}
