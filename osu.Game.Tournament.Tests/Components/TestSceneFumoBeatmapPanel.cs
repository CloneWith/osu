// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.ObjectModel;
using System.Linq;
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

        private void addPlacement(ObservableCollection<ChessPlacement> src, TeamColour? owner, ChoiceType? type)
        {
            src.Add(src.Any(r => r.BeatmapID == panel.Beatmap.Beatmap!.OnlineID)
                ? src.Last(r => r.BeatmapID == panel.Beatmap.Beatmap!.OnlineID)
                     .CreateUpdate(owner, type)
                : new ChessPlacement(1, 1, owner ?? TeamColour.Red, type ?? ChoiceType.Neutral,
                    panel.Beatmap.Beatmap!.OnlineID));
        }

        [Test]
        public void TestStateChange()
        {
            AddToggleStep("Toggle selected", s => panel.Selected = s);
        }

        [Test]
        public void TestPlacement()
        {
            ObservableCollection<ChessPlacement> placements = new ObservableCollection<ChessPlacement>();
            AddAssert("Current match not null", () => Ladder.CurrentMatch.Value != null);
            AddStep("Bind placement list reference", () => placements = Ladder.CurrentMatch.Value!.ChessPlacements);

            AddStep("Add red ban record", () =>
            {
                placements.Add(new ChessPlacement(1, 1,
                    TeamColour.Red, ChoiceType.Ban, panel.Beatmap.Beatmap!.OnlineID));
            });

            AddStep("Remove latest record", () =>
            {
                if (placements.Any(r => r.BeatmapID == panel.Beatmap.Beatmap!.OnlineID))
                    placements.Remove(placements.Last(r => r.BeatmapID == panel.Beatmap.Beatmap!.OnlineID));
            });

            AddStep("Add pick record", () => addPlacement(placements, null, ChoiceType.Pick));
            AddStep("Add blue win record", () => addPlacement(placements, null, ChoiceType.BlueWin));

            AddStep("Add consume record", () => addPlacement(placements, null, ChoiceType.Consumed));
        }
    }
}
