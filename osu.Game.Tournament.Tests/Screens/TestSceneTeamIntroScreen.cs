// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneTeamIntroScreen : TournamentScreenTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        [SetUp]
        public void Setup()
        {
            ladder.CurrentMatch.Value = new TournamentMatch
            {
                Team1 = { Value = Ladder.Teams.FirstOrDefault(t => t.Acronym.Value == "USA") },
                Team2 = { Value = Ladder.Teams.FirstOrDefault(t => t.Acronym.Value == "JPN") },
                Round = { Value = Ladder.Rounds.FirstOrDefault(g => g.Name.Value == "Finals") }
            };
        }

        [Test]
        public void TestTeamIntro()
        {
            AddStep("disable 1v1 mode", () => ladder.Use1V1Mode.Value = false);

            AddStep("clear screen", () =>
            {
                var existing = this.ChildrenOfType<TeamIntroScreen>();

                foreach (var s in existing)
                    Remove(s, true);
            });

            AddStep("create screen", () => Add(new TeamIntroScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));
        }

        [Test]
        public void Test1V1()
        {
            AddStep("enable 1v1 mode", () => ladder.Use1V1Mode.Value = true);

            AddStep("clear screen", () =>
            {
                var existing = this.ChildrenOfType<TeamIntroScreen>();

                foreach (var s in existing)
                    Remove(s, true);
            });

            AddStep("create screen", () => Add(new TeamIntroScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));
        }
    }
}
