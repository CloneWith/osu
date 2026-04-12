// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneMatchScoreDisplay : TournamentTestScene
    {
        private const int max_score = 4000000;

        [Cached(Type = typeof(LegacyMatchIPCInfo))]
        private LegacyMatchIPCInfo legacyMatchInfo = new LegacyMatchIPCInfo();

        [Cached(Type = typeof(MatchIPCInfo))]
        private MatchIPCInfo matchInfo = new MatchIPCInfo();

        public TestSceneMatchScoreDisplay()
        {
            Add(new TournamentMatchScoreDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [Test]
        public void TestStaticScore()
        {
            AddStep("Clear score", () =>
            {
                matchInfo.Score1.Value = 0;
                matchInfo.Score2.Value = 0;
            });

            AddStep("Set random red score", () => matchInfo.Score1.Value = RNG.Next(0, max_score));
            AddStep("Set random blue score", () => matchInfo.Score2.Value = RNG.Next(0, max_score));
        }

        [Test]
        public void TestRandomRolling()
        {
            AddStep("Start rolling", () => Scheduler.AddDelayed(() =>
            {
                int amount = (int)((RNG.NextDouble() - 0.5) * 10000);
                if (amount < 0)
                    legacyMatchInfo.Score1.Value -= amount;
                else
                    legacyMatchInfo.Score2.Value += amount;
            }, 100, true));
        }
    }
}
