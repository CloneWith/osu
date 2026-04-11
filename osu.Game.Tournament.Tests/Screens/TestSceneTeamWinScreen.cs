// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Tournament.Screens.TeamWin;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneTeamWinScreen : TournamentScreenTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("set up match", () =>
            {
                Ladder.Use1V1Mode.Value = false;
                var match = Ladder.CurrentMatch.Value!;

                match.Round.Value = Ladder.Rounds.First(g => g.Name.Value == "Quarterfinals");
                match.Completed.Value = true;
            });

            AddStep("clear screen", () =>
            {
                var existing = this.ChildrenOfType<TeamWinScreen>();

                foreach (var s in existing)
                    Remove(s, true);
            });

            AddStep("create screen", () => Add(new TeamWinScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));
        }

        [Test]
        public void Test1V1()
        {
            AddStep("set up match (1v1)", () =>
            {
                Ladder.Use1V1Mode.Value = true;
                var match = Ladder.CurrentMatch.Value!;

                match.Round.Value = Ladder.Rounds.First(g => g.Name.Value == "Quarterfinals");
                match.Completed.Value = true;
            });

            AddStep("clear screen", () =>
            {
                var existing = this.ChildrenOfType<TeamWinScreen>();

                foreach (var s in existing)
                    Remove(s, true);
            });

            AddStep("create screen", () => Add(new TeamWinScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));
        }
    }
}
