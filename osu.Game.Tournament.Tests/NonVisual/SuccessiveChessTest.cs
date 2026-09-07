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
            var source = match.ChessPlacements.Last(r => r.BoardRow == row && r.BoardColumn == col);
            match.ChessPlacements.Add(new ChessPlacement(source.BoardRow, source.BoardColumn,
                newOwner ?? source.OwnerTeam, newType ?? source.CurrentType));
        }

        private void undoAt(int row, int col)
        {
            var source = match.ChessPlacements.Last(r => r.BoardRow == row && r.BoardColumn == col);
            match.ChessPlacements.Remove(source);
        }

        [Test]
        public void TestEmptyBoardChess()
        {
            runWith([]);

            Assert.That(nums.red, Is.EqualTo(0), "Empty board test failed: Red chess");
            Assert.That(nums.blue, Is.EqualTo(0), "Empty board test failed: Blue chess");
        }

        [Test]
        public void TestSingleWinColourChess()
        {
            runWith(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 1, type: ChoiceType.RedWin),
            });

            Assert.That(nums.red, Is.EqualTo(2), "Dual vertical match test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(1, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 2, type: ChoiceType.RedWin),
            });

            Assert.That(nums.red, Is.EqualTo(1), "Dual diagonal match test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(2, 1, type: ChoiceType.RedWin),
                new ChessPlacement(2, 3, type: ChoiceType.RedWin),
            }, false);

            Assert.That(nums.red, Is.EqualTo(3), "Vertical dual + horizontal triple matches test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(1, 4, type: ChoiceType.RedWin),
                new ChessPlacement(3, 2, type: ChoiceType.RedWin),
                new ChessPlacement(4, 1, type: ChoiceType.RedWin),
            }, false);

            Assert.That(nums.red, Is.EqualTo(4), "Diagonal quad (maximum) match test failed: Red chess");
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

            Assert.That(nums.red, Is.EqualTo(2), "Horizontal dual match test failed: Red chess");
            Assert.That(nums.blue, Is.EqualTo(1), "Diagonal dual match test failed: Blue chess");

            runWith(new[]
            {
                new ChessPlacement(3, 4, type: ChoiceType.BlueWin),
                new ChessPlacement(4, 4, type: ChoiceType.BlueWin),
            }, false);

            Assert.That(nums.blue, Is.EqualTo(2), "Dual vertical + diagonal match test failed: Blue chess");

            runWith(new[]
            {
                new ChessPlacement(1, 3, type: ChoiceType.RedWin),
                new ChessPlacement(2, 3, type: ChoiceType.BlueWin),
                new ChessPlacement(2, 4, type: ChoiceType.BlueWin),
            }, false);

            Assert.That(nums.red, Is.EqualTo(3), "Horizontal triple match test failed: Red chess");
            Assert.That(nums.blue, Is.EqualTo(3), "Triple vertical + diagonal match test failed: Blue chess");
        }

        [Test]
        public void TestOutOfBoundsChess()
        {
            match.ChessPlacements.Clear();
            for (int i = -5; i <= 15; i++)
                match.ChessPlacements.Add(new ChessPlacement(i, i, type: ChoiceType.RedWin));

            runOnce();
            Assert.That(nums.red, Is.EqualTo(4), "Out of bound chess test failed: Red chess");
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

            Assert.That(nums.red, Is.Zero, "Match without win types test failed: Red chess");
            Assert.That(nums.blue, Is.Zero, "Initialization test failed: Blue chess");

            updateStatusAt(1, 3, null, ChoiceType.RedWin);
            updateStatusAt(1, 4, null, ChoiceType.RedWin);
            updateStatusAt(3, 2, null, ChoiceType.BlueWin);
            runOnce();

            Assert.That(nums.red, Is.EqualTo(2), "Update status test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(2, 4, TeamColour.Red, ChoiceType.RedWin),
                new ChessPlacement(3, 4, TeamColour.Blue, ChoiceType.RedWin),
                new ChessPlacement(4, 4, TeamColour.Red, ChoiceType.RedWin),
            }, false);

            Assert.That(nums.red, Is.EqualTo(4), "Vertical quad match test failed: Red chess");

            updateStatusAt(2, 4, null, ChoiceType.Consumed);
            updateStatusAt(3, 4, null, ChoiceType.Consumed);
            runOnce();

            Assert.That(nums.red, Is.EqualTo(2), "Chess consumption test failed: Red chess");

            runWith(new[]
            {
                new ChessPlacement(1, 2, TeamColour.Red, ChoiceType.RedWin),
                new ChessPlacement(2, 2, TeamColour.Blue, ChoiceType.BlueWin),
                new ChessPlacement(4, 2, TeamColour.Blue, ChoiceType.BlueWin),
            }, false);

            Assert.That(nums.red, Is.EqualTo(3), "Horizontal triple match test failed: Red chess");
            Assert.That(nums.blue, Is.EqualTo(3), "Vertical triple match test failed: Blue chess");

            updateStatusAt(1, 2, null, ChoiceType.BlueWin);
            undoAt(1, 2);
            runOnce();

            Assert.That(nums.red, Is.EqualTo(3), "Chess status (1, 2) restore test failed");
        }
    }
}
