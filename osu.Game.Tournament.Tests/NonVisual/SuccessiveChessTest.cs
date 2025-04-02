// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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

        private void runOnce() => nums = match.GetMaximumSuccessiveChess();

        private void runWith(ChessPlacement[] chessList, bool clearList = true)
        {
            if (clearList)
                match.ChessPlacements.Clear();

            match.ChessPlacements.AddRange(chessList);

            runOnce();
        }

        private void updateStatusAt(int row, int col, TeamColour? newOwner, ChoiceType? newType)
        {
            var source = match.ChessPlacements.First(r => r.BoardRow == row && r.BoardColumn == col);
            source.Update(newOwner, newType);
        }

        private void undoAt(int row, int col)
        {
            var source = match.ChessPlacements.First(r => r.BoardRow == row && r.BoardColumn == col);
            source.Undo();
        }

        [Test]
        public void TestEmptyBoardChess()
        {
            runWith([]);

            Assert.AreEqual(0, nums.red, "Empty board test failed: Red chess");
            Assert.AreEqual(0, nums.blue, "Empty board test failed: Blue chess");
        }

        [Test]
        public void TestSingleWinColourChess()
        {
            runWith(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 1, type: ChoiceType.RedWin),
            });

            Assert.AreEqual(2, nums.red, "Dual vertical match test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 2, type: ChoiceType.RedWin),
            });

            Assert.AreEqual(0, nums.red, "Dual diagonal match test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(2, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 3, type: ChoiceType.RedWin),
            }, false);

            Assert.AreEqual(3, nums.red, "Vertical dual + horizontal triple matches test failed: Red chess");

            runWith(new[]
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
            runWith(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(1, 2, type: ChoiceType.RedWin),
                new ChessPlacement(3, 2, type: ChoiceType.BlueWin),
                new ChessPlacement(4, 1, type: ChoiceType.BlueWin),
            });

            Assert.AreEqual(2, nums.red, "Horizontal dual match test failed: Red chess");
            Assert.AreEqual(0, nums.blue, "Diagonal dual match test failed: Blue chess");

            runWith(new[]
            {
                new ChessPlacement(3, 4, type: ChoiceType.BlueWin),
                new ChessPlacement(4, 4, type: ChoiceType.BlueWin),
            }, false);

            Assert.AreEqual(2, nums.blue, "Dual vertical + diagonal match test failed: Blue chess");

            runWith(new[]
            {
                new ChessPlacement(1, 3, type: ChoiceType.RedWin),
                new ChessPlacement(2, 3, type: ChoiceType.BlueWin),
                new ChessPlacement(2, 4, type: ChoiceType.BlueWin),
            }, false);

            Assert.AreEqual(3, nums.red, "Horizontal triple match test failed: Red chess");
            Assert.AreEqual(3, nums.blue, "Triple vertical + diagonal match test failed: Blue chess");
        }

        [Test]
        public void TestComprehensiveBoard()
        {
            runWith(new[]
            {
                new ChessPlacement(1, 3, TeamColour.Red, ChoiceType.Pick),
                new ChessPlacement(3, 2, TeamColour.Blue, ChoiceType.Pick),
                new ChessPlacement(1, 4, TeamColour.Red),
            });

            Assert.AreEqual(0, nums.red, "Match without win types test failed: Red chess");
            Assert.AreEqual(0, nums.blue, "Initialization test failed: Blue chess");

            updateStatusAt(1, 3, null, ChoiceType.RedWin);
            updateStatusAt(1, 4, null, ChoiceType.RedWin);
            updateStatusAt(3, 2, null, ChoiceType.BlueWin);
            runOnce();

            Assert.AreEqual(2, nums.red, "Update status test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(2, 4, TeamColour.Red, ChoiceType.RedWin),
                new ChessPlacement(3, 4, TeamColour.Blue, ChoiceType.RedWin),
                new ChessPlacement(4, 4, TeamColour.Red, ChoiceType.RedWin),
            }, false);

            Assert.AreEqual(4, nums.red, "Vertical quad match test failed: Red chess");

            updateStatusAt(2, 4, null, ChoiceType.Consumed);
            updateStatusAt(3, 4, null, ChoiceType.Consumed);
            runOnce();

            Assert.AreEqual(2, nums.red, "Chess consumption test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(1, 2, TeamColour.Red, ChoiceType.RedWin),
                new ChessPlacement(2, 2, TeamColour.Blue, ChoiceType.BlueWin),
                new ChessPlacement(4, 2, TeamColour.Blue, ChoiceType.BlueWin),
            }, false);

            Assert.AreEqual(3, nums.red, "Horizontal triple match test failed: Red chess");
            Assert.AreEqual(3, nums.blue, "Vertical triple match test failed: Blue chess");

            updateStatusAt(1, 2, null, ChoiceType.BlueWin);
            undoAt(1, 2);
            runOnce();

            Assert.AreEqual(3, nums.red, "Chess status (1, 2) restore test failed");
        }
    }
}
