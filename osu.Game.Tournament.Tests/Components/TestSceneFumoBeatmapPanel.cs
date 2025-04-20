// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneFumoBeatmapPanel : TournamentTestScene
    {
        private readonly FumoBeatmapPanel panel;

        public TestSceneFumoBeatmapPanel()
        {
            Add(panel = new FumoBeatmapPanel(new RoundBeatmap
            {
                Beatmap = CreateSampleBeatmap(),
                Mods = @"DT",
                ModIndex = @"3",
                DifficultyField = @"选图测试",
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [Test]
        public void TestStateChange()
        {
            AddToggleStep("Toggle selected", s => panel.Selected = s);
        }
    }
}
