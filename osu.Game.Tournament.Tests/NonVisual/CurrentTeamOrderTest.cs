// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class CurrentTeamOrderTest
    {
        private static TournamentMatch createMatch(bool useBoard, int banCount, TeamColour firstBanSide, TeamColour firstPickSide)
            => new TournamentMatch
            {
                Round =
                {
                    Value = new TournamentRound
                    {
                        UseBoard = { Value = useBoard },
                        BanCount = { Value = banCount },
                    }
                },
                FirstBanSide = { Value = firstBanSide },
                FirstPickSide = { Value = firstPickSide },
            };

        private static TeamColour[] getBanOrder(bool useBoard, int banCount, TeamColour firstBanSide)
        {
            var match = createMatch(useBoard, banCount, firstBanSide, TeamColour.Red);

            return Enumerable.Range(match.CurrentRoundIndex.MinValue, banCount * 2)
                             .Select(index =>
                             {
                                 match.CurrentRoundIndex.Value = index;
                                 return match.CurrentTeam;
                             })
                             .ToArray();
        }

        [Test]
        public void TestAlternatingBanOrderWithoutBoard()
        {
            Assert.That(getBanOrder(false, 1, TeamColour.Red), Is.EqualTo(new[] { TeamColour.Red, TeamColour.Blue }));
            Assert.That(getBanOrder(false, 2, TeamColour.Red), Is.EqualTo(new[] { TeamColour.Red, TeamColour.Blue, TeamColour.Red, TeamColour.Blue }));
            Assert.That(getBanOrder(false, 2, TeamColour.Blue), Is.EqualTo(new[] { TeamColour.Blue, TeamColour.Red, TeamColour.Blue, TeamColour.Red }));
        }

        [Test]
        public void TestSnakeBanOrderWithBoard()
        {
            Assert.That(getBanOrder(true, 1, TeamColour.Red), Is.EqualTo(new[] { TeamColour.Red, TeamColour.Blue }));

            Assert.That(getBanOrder(true, 2, TeamColour.Red), Is.EqualTo(new[]
            {
                TeamColour.Red,
                TeamColour.Blue,
                TeamColour.Blue,
                TeamColour.Red,
            }));

            Assert.That(getBanOrder(true, 2, TeamColour.Blue), Is.EqualTo(new[]
            {
                TeamColour.Blue,
                TeamColour.Red,
                TeamColour.Red,
                TeamColour.Blue,
            }));

            Assert.That(getBanOrder(true, 3, TeamColour.Red), Is.EqualTo(new[]
            {
                TeamColour.Red,
                TeamColour.Blue,
                TeamColour.Blue,
                TeamColour.Red,
                TeamColour.Red,
                TeamColour.Blue,
            }));
        }

        [Test]
        public void TestPickOrderUnaffectedByBoard()
        {
            var match = createMatch(true, 2, TeamColour.Red, TeamColour.Blue);

            match.CurrentRoundIndex.Value = 1;
            Assert.That(match.CurrentTeam, Is.EqualTo(TeamColour.Blue));

            match.CurrentRoundIndex.Value = 2;
            Assert.That(match.CurrentTeam, Is.EqualTo(TeamColour.Red));

            match.CurrentRoundIndex.Value = 3;
            Assert.That(match.CurrentTeam, Is.EqualTo(TeamColour.Blue));
        }
    }
}
