// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneFumoTeamDisplay : TournamentTestScene
    {
        private FumoTeamDisplay team1Display = null!;
        private FumoTeamDisplay team2Display = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    team1Display = new FumoTeamDisplay(TeamColour.Red),
                    team2Display = new FumoTeamDisplay(TeamColour.Blue),
                }
            });
        }

        [Test]
        public void TestTeamChange()
        {
            AddStep("Change Red team name", () => Ladder.CurrentMatch.Value!.Team1.Value!.FullName.Value = "赢了曹飞对面电脑显示屏");
            AddStep("Change Blue team name", () => Ladder.CurrentMatch.Value!.Team2.Value!.FullName.Value = "弱队");
        }

        [Test]
        public void TestActivate()
        {
            AddToggleStep("Is Red team active", v => team1Display.IsActive = v);
            AddToggleStep("Is Blue team active", v => team2Display.IsActive = v);
        }
    }
}
