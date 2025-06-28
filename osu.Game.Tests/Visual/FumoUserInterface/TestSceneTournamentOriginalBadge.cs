// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.TournamentShowcase;
using osuTK;

namespace osu.Game.Tests.Visual.FumoUserInterface
{
    public partial class TestSceneTournamentOriginalBadge : OsuTestScene
    {
        private readonly TournamentOriginalBadge badge;

        public TestSceneTournamentOriginalBadge()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300),
                Masking = true,
                Children = new Drawable[]
                {
                    badge = new TournamentOriginalBadge
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                },
            };
        }

        [Test]
        public void TestBadgeAnimation()
        {
            AddStep("Start animation", badge.Animate);
        }
    }
}
