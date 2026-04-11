// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.TournamentIpc;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneGameplayScreen : TournamentScreenTestScene
    {
        [Cached]
        private TournamentMatchChatDisplay chat = new TournamentMatchChatDisplay { Width = 0.5f };

        [Test]
        public void TestWarmup()
        {
            createScreen();

            checkScoreVisibility(false);

            toggleWarmup();
            checkScoreVisibility(true);

            toggleWarmup();
            checkScoreVisibility(false);
        }

        [Test]
        public void TestScoreAdd()
        {
            AddStep("disable cumulative score", () => Ladder.CumulativeScore.Value = false);

            createScreen();
            toggleWarmup();

            AddStep("set state: lobby", () => LazerIPCInfo.State.Value = TourneyState.Lobby);

            AddStep("set state: playing", () => LazerIPCInfo.State.Value = TourneyState.Playing);
            AddStep("add score", () =>
            {
                LazerIPCInfo.Score1.Value = 127_727;
                LazerIPCInfo.Score2.Value = 63_727;
            });
            AddStep("set state: ranking", () => LazerIPCInfo.State.Value = TourneyState.Ranking);
        }

        [Test]
        public void TestScoreAddCumulative()
        {
            AddStep("disable cumulative score", () => Ladder.CumulativeScore.Value = false);
            AddStep("enable cumulative score", () => Ladder.CumulativeScore.Value = true);

            createScreen();
            toggleWarmup();

            AddStep("add set with maps 1 & 2", () => Ladder.CurrentMatch.Value!.Sets.Add(new MatchSet(false) { Map1Id = { Value = 1 }, Map2Id = { Value = 2 } }));
            AddStep("add set with maps 3 & 4", () => Ladder.CurrentMatch.Value!.Sets.Add(new MatchSet(false) { Map1Id = { Value = 3 }, Map2Id = { Value = 4 } }));

            for (int i = 0; i < 2; i++)
            {
                int i1 = i;
                AddStep($"switch to map {i + 1}", () => LazerIPCInfo.Beatmap.Value = new TournamentBeatmap { OnlineID = i1 + 1 });

                AddStep("set state: lobby", () => LazerIPCInfo.State.Value = TourneyState.Lobby);

                AddStep("set state: playing", () => LazerIPCInfo.State.Value = TourneyState.Playing);

                int iteration = i;
                AddStep("add score", () =>
                {
                    LazerIPCInfo.Score1.Value = iteration == 0 ? 127_727 : 492_000;
                    LazerIPCInfo.Score2.Value = iteration == 0 ? 63_000 : 613_727;
                });
                AddStep("set state: ranking", () => LazerIPCInfo.State.Value = TourneyState.Ranking);

                AddWaitStep("wait a bit", 8);

                AddStep("clear scores", () =>
                {
                    LazerIPCInfo.Score1.Value = 0;
                    LazerIPCInfo.Score2.Value = 0;
                });
            }

            AddStep("set state: lobby", () => LazerIPCInfo.State.Value = TourneyState.Lobby);
            AddStep("switch to map 3", () => LazerIPCInfo.Beatmap.Value = new TournamentBeatmap { OnlineID = 3 });
        }

        [Test]
        public void TestScoreAddCumulativeTiebreaker()
        {
            AddStep("disable cumulative score", () => Ladder.CumulativeScore.Value = false);
            AddStep("enable cumulative score", () => Ladder.CumulativeScore.Value = true);

            createScreen();
            toggleWarmup();

            AddStep("add tiebreaker set with maps 3, 4, 5", () => Ladder.CurrentMatch.Value!.Sets.Add(new MatchSet(true) { Map1Id = { Value = 1 }, Map2Id = { Value = 2 }, Map3Id = { Value = 3 } }));

            for (int i = 0; i < 3; i++)
            {
                int i1 = i;
                AddStep($"switch to map {i + 1}", () => LazerIPCInfo.Beatmap.Value = new TournamentBeatmap { OnlineID = i1 + 1 });

                AddStep("set state: lobby", () => LazerIPCInfo.State.Value = TourneyState.Lobby);

                AddStep("set state: playing", () => LazerIPCInfo.State.Value = TourneyState.Playing);

                int iteration = i + 1;
                AddStep("add score", () =>
                {
                    LazerIPCInfo.Score1.Value = iteration * 1_000;
                    LazerIPCInfo.Score2.Value = iteration;
                });
                AddStep("set state: ranking", () => LazerIPCInfo.State.Value = TourneyState.Ranking);

                AddWaitStep("wait a bit", 8);

                AddStep("clear scores", () =>
                {
                    LazerIPCInfo.Score1.Value = 0;
                    LazerIPCInfo.Score2.Value = 0;
                });
            }
        }

        [Test]
        public void TestScoreCumulativeDelta()
        {
            AddStep("enable cumulative score", () => Ladder.CumulativeScore.Value = true);

            createScreen();
            toggleWarmup();

            for (int i = 0; i < 7; i++)
            {
                AddStep($"add map {i + 1} results", () =>
                {
                    Ladder.CurrentMatch.Value!.Team1Score.Value += RNG.Next(1_000_000);
                    Ladder.CurrentMatch.Value!.Team2Score.Value += RNG.Next(1_000_000);
                });

                AddUntilStep("wait for score delta to settle", () =>
                {
                    var scoreDeltaDrawable = this.ChildrenOfType<MatchHeader.MatchCumulativeScoreDiffCounter>().First();
                    return Math.Abs(scoreDeltaDrawable.DisplayedCount - scoreDeltaDrawable.Current.Value) < 0.0001f;
                });
                AddRepeatStep("wait a bit more", () => { }, 8);
            }
        }

        [Test]
        public void TestStartupState([Values] LegacyTourneyState state)
        {
            AddStep("set state", () => IPCInfo.State.Value = state);
            createScreen();
        }

        [Test]
        public void TestStartupStateNoCurrentMatch([Values] LegacyTourneyState state)
        {
            AddStep("set null current", () => Ladder.CurrentMatch.Value = null);
            AddStep("set state", () => IPCInfo.State.Value = state);
            createScreen();
        }

        private void createScreen()
        {
            AddStep("setup screen", () =>
            {
                Remove(chat, false);

                Children = new Drawable[]
                {
                    new GameplayScreen(),
                    chat,
                };
            });
        }

        private void checkScoreVisibility(bool visible)
            => AddUntilStep($"scores {(visible ? "shown" : "hidden")}",
                () => this.ChildrenOfType<TeamScore>().All(score => score.Alpha == (visible ? 1 : 0)));

        private void toggleWarmup()
            => AddStep("toggle warmup", () => this.ChildrenOfType<LabelledSwitchButton>().First().ChildrenOfType<SwitchButton>().First().TriggerClick());
    }
}
