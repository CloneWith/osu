// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneMatchCountdown : TournamentTestScene
    {
        private readonly MatchCountdown countdown;

        public TestSceneMatchCountdown()
        {
            Add(countdown = new MatchCountdown());
        }

        [Test]
        public void TestCountdown()
        {
            AddAssert("Target time is null (unset)", () => !countdown.Target.Value.HasValue);

            AddStep("Set countdown time to 5s later",
                () => countdown.Target.Value = new DateTimeOffset(DateTime.Now.AddSeconds(5)));

            // TODO: Wait for class-side work to complete, then rewrite these steps
            AddAssert("Countdown started", () => countdown.OnGoing);
            AddUntilStep("Wait for complete", () => !countdown.OnGoing);

            AddStep("Set a target time in the past",
                () => countdown.Target.Value = new DateTimeOffset(DateTime.Now.AddMinutes(-1)));
            AddStep("Set a target time long ago",
                () => countdown.Target.Value = new DateTimeOffset(DateTime.Now.AddDays(-3)));
            AddStep("Unset target time", () => countdown.Target.Value = null);
        }
    }
}
