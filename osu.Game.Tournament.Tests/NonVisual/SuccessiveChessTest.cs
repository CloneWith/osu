// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Extensions;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class SuccessiveChessTest
    {
        private readonly TournamentMatch match = new TournamentMatch();
        private (int red, int blue) nums = (0, 0);

        private void runTest(ChessPlacement[] chessList, bool clearList = true)
        {
            if (clearList)
                match.ChessPlacements.Clear();

            match.ChessPlacements.AddRange(chessList);

            nums = match.GetMaximumSuccessiveChess();
        }

        [Test]
        public void TestEmptyBoardChess()
        {
            runTest([]);

            Assert.AreEqual(0, nums.red, "Empty board test failed: Red chess");
            Assert.AreEqual(0, nums.blue, "Empty board test failed: Blue chess");
        }

        [Test]
        public void TestSingleWinColourChess()
        {
            runTest(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 1, type: ChoiceType.RedWin),
            });

            Assert.AreEqual(2, nums.red, "Dual vertical match test failed: Red chess");

            runTest(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 2, type: ChoiceType.RedWin),
            });

            Assert.AreEqual(0, nums.red, "Dual diagonal match test failed: Red chess");

            runTest(new[]
            {
                new ChessPlacement(2, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 3, type: ChoiceType.RedWin),
            }, false);

            Assert.AreEqual(3, nums.red, "Vertical dual + horizontal triple matches test failed: Red chess");

            runTest(new[]
            {
                new ChessPlacement(1, 4, type: ChoiceType.RedWin),
                new ChessPlacement(3, 2, type: ChoiceType.RedWin),
                new ChessPlacement(4, 1, type: ChoiceType.RedWin),
            }, false);

            Assert.AreEqual(4, nums.red, "Diagonal quad (maximum) match test failed: Red chess");
        }

        [Test]
        public void TestDualWinColourChess()
        {
            runTest(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(1, 2, type: ChoiceType.RedWin),
                new ChessPlacement(3, 2, type: ChoiceType.BlueWin),
                new ChessPlacement(4, 1, type: ChoiceType.BlueWin),
            });

            Assert.AreEqual(2, nums.red, "Horizontal dual match test failed: Red chess");
            Assert.AreEqual(0, nums.blue, "Diagonal dual match test failed: Blue chess");

            runTest(new[]
            {
                new ChessPlacement(3, 4, type: ChoiceType.BlueWin),
                new ChessPlacement(4, 4, type: ChoiceType.BlueWin),
            }, false);

            Assert.AreEqual(2, nums.blue, "Dual vertical + diagonal match test failed: Blue chess");

            runTest(new[]
            {
                new ChessPlacement(1, 3, type: ChoiceType.RedWin),
                new ChessPlacement(2, 3, type: ChoiceType.BlueWin),
                new ChessPlacement(2, 4, type: ChoiceType.BlueWin),
            }, false);

            Assert.AreEqual(3, nums.red, "Horizontal triple match test failed: Red chess");
            Assert.AreEqual(3, nums.blue, "Triple vertical + diagonal match test failed: Blue chess");
        }
    }
}
